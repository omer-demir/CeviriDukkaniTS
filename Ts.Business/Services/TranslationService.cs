using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using log4net;
using Newtonsoft.Json;
using Tangent.CeviriDukkani.Data.Model;
using Tangent.CeviriDukkani.Domain.Common;
using Tangent.CeviriDukkani.Domain.Dto.Common;
using Tangent.CeviriDukkani.Domain.Dto.Enums;
using Tangent.CeviriDukkani.Domain.Dto.System;
using Tangent.CeviriDukkani.Domain.Dto.Translation;
using Tangent.CeviriDukkani.Domain.Entities.Common;
using Tangent.CeviriDukkani.Domain.Entities.Translation;
using Tangent.CeviriDukkani.Domain.Exceptions;
using Tangent.CeviriDukkani.Domain.Exceptions.ExceptionCodes;
using Tangent.CeviriDukkani.Domain.Mappers;
using Tangent.CeviriDukkani.Event.OrderEvents;
using Tangent.CeviriDukkani.Messaging;
using Tangent.CeviriDukkani.Messaging.Producer;
using Ts.Business.ExternalClients;

namespace Ts.Business.Services {
    public class TranslationService : ITranslationService {
        private readonly CeviriDukkaniModel _model;
        private readonly IUserServiceClient _userServiceClient;
        private readonly CustomMapperConfiguration _mapper;
        private readonly ILog _logger;
        private readonly IDispatchCommits _dispatcher;

        public TranslationService(CeviriDukkaniModel model, IUserServiceClient userServiceClient, CustomMapperConfiguration mapper, ILog logger, IDispatchCommits dispatcher) {
            _model = model;
            _userServiceClient = userServiceClient;
            _mapper = mapper;
            _logger = logger;
            _dispatcher = dispatcher;
        }

        #region Implementation of ITranslationService

        public ServiceResult GetAverageDocumentPartCount(int orderId) {
            var serviceResult = new ServiceResult(ServiceResultType.NotKnown);
            try {
                var translatorServiceResult = _userServiceClient.GetTranslatorsAccordingToOrderTranslationQuality(orderId);
                if (translatorServiceResult.ServiceResultType != ServiceResultType.Success) {
                    throw translatorServiceResult.Exception;
                }

                var translators = translatorServiceResult.Data;
                var translatorsId = translators.Select(a => a.Id);
                var ceiledNumber = GetAverateUserRating(translatorsId);

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = ceiledNumber;
            } catch (Exception exc) {
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod()} with message {exc.Message}");
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                serviceResult.Exception = exc;
            }

            return serviceResult;
        }

        public ServiceResult<List<TranslationOperationDto>> SaveTranslationOperations(List<TranslationOperationDto> translationOperations) {
            var serviceResult = new ServiceResult<List<TranslationOperationDto>>();
            try {
                var translationOperationItems =
                    translationOperations.Select(
                        a => _mapper.GetMapEntity<TranslationOperation, TranslationOperationDto>(a)).ToList();

                _model.TranslationOperations.AddRange(translationOperationItems);

                var saveResult = _model.SaveChanges() > 0;
                if (!saveResult) {
                    throw new BusinessException(ExceptionCodes.UnableToInsert);
                }

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = translationOperationItems.Select(a => _mapper.GetMapDto<TranslationOperationDto, TranslationOperation>(a)).ToList();
            } catch (Exception exc) {
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod()} with message {exc.Message}");
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                serviceResult.Exception = exc;
            }

            return serviceResult;
        }

        public ServiceResult UpdateTranslatedDocumentPart(int translatorId, int translationDocumentPartId, string content) {
            var serviceResult = new ServiceResult();
            try {
                var translationOperationItem =
                    _model.TranslationOperations.FirstOrDefault(
                        a => a.TranslatorId == translatorId && a.TranslationDocumentPartId == translationDocumentPartId);

                if (translationOperationItem == null) {
                    throw new BusinessException(ExceptionCodes.NoRelatedData);
                }

                translationOperationItem.TranslatedContent = content;
                translationOperationItem.TranslationProgressStatusId = (int)TranslationProgressStatusEnum.TranslatorInProgress;

                _model.Entry(translationOperationItem).State = EntityState.Modified;
                var saveResult = _model.SaveChanges() > 0;
                if (!saveResult) {
                    throw new BusinessException(ExceptionCodes.UnableToInsert);
                }

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = true;
            } catch (Exception exc) {
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod()} with message {exc.Message}");
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                serviceResult.Exception = exc;
            }

            return serviceResult;
        }

        public ServiceResult UpdateEditedDocumentPart(int editorId, int translationDocumentPartId, string content) {
            var serviceResult = new ServiceResult();
            try {
                var translationOperationItem =
                    _model.TranslationOperations.FirstOrDefault(
                        a => a.EditorId == editorId && a.TranslationDocumentPartId == translationDocumentPartId);

                if (translationOperationItem == null) {
                    throw new BusinessException(ExceptionCodes.NoRelatedData);
                }

                translationOperationItem.EditedContent = content;
                translationOperationItem.TranslationProgressStatusId = (int)TranslationProgressStatusEnum.EditorInProgress;

                _model.Entry(translationOperationItem).State = EntityState.Modified;
                var saveResult = _model.SaveChanges() > 0;
                if (!saveResult) {
                    throw new BusinessException(ExceptionCodes.UnableToInsert);
                }

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = true;
            } catch (Exception exc) {
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod()} with message {exc.Message}");
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                serviceResult.Exception = exc;
            }

            return serviceResult;
        }

        public ServiceResult UpdateProofReadDocumentPart(int proofReaderId, int translationDocumentPartId, string content) {
            var serviceResult = new ServiceResult();
            try {
                var translationOperationItem =
                    _model.TranslationOperations.FirstOrDefault(
                        a => a.ProofReaderId == proofReaderId && a.TranslationDocumentPartId == translationDocumentPartId);

                if (translationOperationItem == null) {
                    throw new BusinessException(ExceptionCodes.NoRelatedData);
                }

                translationOperationItem.ProofReadContent = content;
                translationOperationItem.TranslationProgressStatusId = (int)TranslationProgressStatusEnum.ProofReaderInProgress;

                _model.Entry(translationOperationItem).State = EntityState.Modified;
                var saveResult = _model.SaveChanges() > 0;
                if (!saveResult) {
                    throw new BusinessException(ExceptionCodes.UnableToInsert);
                }

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = true;
            } catch (Exception exc) {
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod()} with message {exc.Message}");
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                serviceResult.Exception = exc;
            }

            return serviceResult;
        }

        public ServiceResult FinishTranslation(int translatorId, int translationDocumentPartId) {
            var serviceResult = new ServiceResult();
            try {
                var translationOperationItem =
                    _model.TranslationOperations.FirstOrDefault(
                        a => a.TranslatorId == translatorId && a.TranslationDocumentPartId == translationDocumentPartId);

                if (translationOperationItem == null) {
                    throw new BusinessException(ExceptionCodes.NoRelatedData);
                }

                translationOperationItem.TranslationProgressStatusId = (int)TranslationProgressStatusEnum.TranslatorDone;

                _model.Entry(translationOperationItem).State = EntityState.Modified;
                var saveResult = _model.SaveChanges() > 0;
                if (!saveResult) {
                    throw new BusinessException(ExceptionCodes.UnableToInsert);
                }

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = true;
            } catch (Exception exc) {
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod()} with message {exc.Message}");
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                serviceResult.Exception = exc;
            }

            return serviceResult;
        }

        public ServiceResult FinishEditing(int editorId, int translationDocumentPartId) {
            var serviceResult = new ServiceResult();
            try {
                var translationOperationItem =
                    _model.TranslationOperations.FirstOrDefault(
                        a => a.EditorId == editorId && a.TranslationDocumentPartId == translationDocumentPartId);

                if (translationOperationItem == null) {
                    throw new BusinessException(ExceptionCodes.NoRelatedData);
                }

                translationOperationItem.TranslationProgressStatusId = (int)TranslationProgressStatusEnum.EditorDone;

                _model.Entry(translationOperationItem).State = EntityState.Modified;
                var saveResult = _model.SaveChanges() > 0;
                if (!saveResult) {
                    throw new BusinessException(ExceptionCodes.UnableToInsert);
                }

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = true;
            } catch (Exception exc) {
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod()} with message {exc.Message}");
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                serviceResult.Exception = exc;
            }

            return serviceResult;
        }

        public ServiceResult FinishProofReading(int proofReaderId, int translationDocumentPartId) {
            var serviceResult = new ServiceResult();
            try {
                var translationOperationItem =
                    _model.TranslationOperations.FirstOrDefault(
                        a => a.ProofReaderId == proofReaderId && a.TranslationDocumentPartId == translationDocumentPartId);

                if (translationOperationItem == null) {
                    throw new BusinessException(ExceptionCodes.NoRelatedData);
                }

                translationOperationItem.TranslationProgressStatusId = (int)TranslationProgressStatusEnum.ProofReaderDone;

                _model.Entry(translationOperationItem).State = EntityState.Modified;
                var saveResult = _model.SaveChanges() > 0;
                if (!saveResult) {
                    throw new BusinessException(ExceptionCodes.UnableToInsert);
                }

                _dispatcher.Dispatch(new List<EventMessage> {
                    new UpdateOrderStatusEvent {
                        Id = Guid.NewGuid(),
                        StatusId = (int)OrderStatusEnum.RevisionNeeded,
                        TranslationOperationId = translationOperationItem.Id
                    }.ToEventMessage()
                });

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = true;
            } catch (Exception exc) {
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod()} with message {exc.Message}");
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                serviceResult.Exception = exc;
            }

            return serviceResult;
        }

        public ServiceResult GetTranslatedContent(int translationDocumentPartId, int translatorId) {
            var serviceResult = new ServiceResult();
            try {
                var translationOperationItem =
                    _model.TranslationOperations.FirstOrDefault(
                        a => a.TranslatorId == translatorId && a.TranslationDocumentPartId == translationDocumentPartId);

                if (translationOperationItem == null) {
                    throw new BusinessException(ExceptionCodes.NoRelatedData);
                }

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = translationOperationItem.TranslatedContent;
            } catch (Exception exc) {
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod()} with message {exc.Message}");
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                serviceResult.Exception = exc;
            }

            return serviceResult;
        }

        public ServiceResult GetTranslatedContentForEditor(int translationDocumentPartId, int editorId) {
            var serviceResult = new ServiceResult();
            try {
                var translationOperationItem =
                    _model.TranslationOperations.FirstOrDefault(
                        a => a.EditorId == editorId && a.TranslationDocumentPartId == translationDocumentPartId);

                if (translationOperationItem == null) {
                    throw new BusinessException(ExceptionCodes.NoRelatedData);
                }

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = translationOperationItem.TranslatedContent;
            } catch (Exception exc) {
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod()} with message {exc.Message}");
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                serviceResult.Exception = exc;
            }

            return serviceResult;
        }

        public ServiceResult GetEditedContent(int translationDocumentPartId, int editorId) {
            var serviceResult = new ServiceResult();
            try {
                var translationOperationItem =
                    _model.TranslationOperations.FirstOrDefault(
                        a => a.EditorId == editorId && a.TranslationDocumentPartId == translationDocumentPartId);

                if (translationOperationItem == null) {
                    throw new BusinessException(ExceptionCodes.NoRelatedData);
                }

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = translationOperationItem.EditedContent;
            } catch (Exception exc) {
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod()} with message {exc.Message}");
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                serviceResult.Exception = exc;
            }

            return serviceResult;
        }

        public ServiceResult GetEditedContentForProofReader(int translationDocumentPartId, int proofReaderId) {
            var serviceResult = new ServiceResult();
            try {
                var translationOperationItem =
                    _model.TranslationOperations.FirstOrDefault(
                        a => a.ProofReaderId == proofReaderId && a.TranslationDocumentPartId == translationDocumentPartId);

                if (translationOperationItem == null) {
                    throw new BusinessException(ExceptionCodes.NoRelatedData);
                }

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = translationOperationItem.EditedContent;
            } catch (Exception exc) {
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod()} with message {exc.Message}");
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                serviceResult.Exception = exc;
            }

            return serviceResult;
        }

        public ServiceResult GetProofReadContent(int translationDocumentPartId, int proofReaderId) {
            var serviceResult = new ServiceResult();
            try {
                var translationOperationItem =
                    _model.TranslationOperations.FirstOrDefault(
                        a => a.ProofReaderId == proofReaderId && a.TranslationDocumentPartId == translationDocumentPartId);

                if (translationOperationItem == null) {
                    throw new BusinessException(ExceptionCodes.NoRelatedData);
                }

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = translationOperationItem.ProofReadContent;
            } catch (Exception exc) {
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod()} with message {exc.Message}");
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                serviceResult.Exception = exc;
            }

            return serviceResult;
        }

        public ServiceResult AddCommentToTranslationOperation(int translationDocumentPartId, int commentCreatorId, string content) {
            var serviceResult = new ServiceResult();
            try {
                var translationOperationItem = _model.TranslationOperations.FirstOrDefault(a => a.TranslationDocumentPartId == translationDocumentPartId);
                if (translationOperationItem == null) {
                    throw new BusinessException(ExceptionCodes.NoRelatedData);
                }
                translationOperationItem.Comments = new List<Comment> {new Comment {
                    Content = content,
                    Active = true,
                    CreatedAt = DateTime.Now,
                    FromUserId = commentCreatorId,
                    ToUserId = translationOperationItem.TranslatorId.Value,
                    TranslationOperationId = translationOperationItem.Id
                } };

                _model.Entry(translationOperationItem).State = EntityState.Modified;

                var result = _model.SaveChanges() > 0;
                if (!result) {
                    throw new BusinessException(ExceptionCodes.UnableToUpdate);
                }

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = true;
            } catch (Exception exc) {
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod()} with message {exc.Message}");
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                serviceResult.Exception = exc;
            }

            return serviceResult;
        }

        public ServiceResult AddCommentToEditionOperation(int translationDocumentPartId, int commentCreatorId, string content) {
            var serviceResult = new ServiceResult();
            try {
                var translationOperationItem = _model.TranslationOperations.FirstOrDefault(a => a.TranslationDocumentPartId == translationDocumentPartId);
                if (translationOperationItem == null) {
                    throw new BusinessException(ExceptionCodes.NoRelatedData);
                }
                translationOperationItem.Comments = new List<Comment> {new Comment {
                    Content = content,
                    Active = true,
                    CreatedAt = DateTime.Now,
                    FromUserId = commentCreatorId,
                    ToUserId = translationOperationItem.EditorId.Value,
                    TranslationOperationId = translationOperationItem.Id
                } };

                _model.Entry(translationOperationItem).State = EntityState.Modified;

                var result = _model.SaveChanges() > 0;
                if (!result) {
                    throw new BusinessException(ExceptionCodes.UnableToUpdate);
                }

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = true;
            } catch (Exception exc) {
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod()} with message {exc.Message}");
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                serviceResult.Exception = exc;
            }

            return serviceResult;
        }

        public ServiceResult<List<CommentDto>> GetTranslationOperationComments(int translationDocumentPartId) {
            var serviceResult = new ServiceResult<List<CommentDto>>();
            try {
                var translationOperationItem =
                    _model.TranslationOperations.Include(a => a.Comments.Select(b => b.FromUser)).FirstOrDefault(a => a.TranslationDocumentPartId == translationDocumentPartId);

                if (translationOperationItem == null) {
                    throw new BusinessException(ExceptionCodes.NoRelatedData);
                }

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = translationOperationItem.Comments.Select(a => _mapper.GetMapDto<CommentDto, Comment>(a)).ToList();
            } catch (Exception exc) {
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod()} with message {exc.Message}");
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                serviceResult.Exception = exc;
            }

            return serviceResult;
        }

        public ServiceResult MarkTranslatingAsFinished(int translationDocumentPartId, int translatorId) {
            var serviceResult = new ServiceResult();
            try {
                var translationOperationItem = _model.TranslationOperations.FirstOrDefault(a => a.TranslationDocumentPartId == translationDocumentPartId);
                if (translationOperationItem == null) {
                    throw new BusinessException(ExceptionCodes.NoRelatedData);
                }

                translationOperationItem.TranslationProgressStatusId = (int)TranslationProgressStatusEnum.TranslatorDone;

                _model.Entry(translationOperationItem).State = EntityState.Modified;

                var result = _model.SaveChanges() > 0;
                if (!result) {
                    throw new BusinessException(ExceptionCodes.UnableToUpdate);
                }

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = true;
            } catch (Exception exc) {
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod()} with message {exc.Message}");
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                serviceResult.Exception = exc;
            }

            return serviceResult;
        }

        public ServiceResult MarkEditingAsFinished(int translationDocumentPartId, int editorId) {
            var serviceResult = new ServiceResult();
            try {
                var translationOperationItem = _model.TranslationOperations.FirstOrDefault(a => a.TranslationDocumentPartId == translationDocumentPartId);
                if (translationOperationItem == null) {
                    throw new BusinessException(ExceptionCodes.NoRelatedData);
                }

                translationOperationItem.TranslationProgressStatusId = (int)TranslationProgressStatusEnum.EditorDone;

                _model.Entry(translationOperationItem).State = EntityState.Modified;

                var result = _model.SaveChanges() > 0;
                if (!result) {
                    throw new BusinessException(ExceptionCodes.UnableToUpdate);
                }

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = true;
            } catch (Exception exc) {
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod()} with message {exc.Message}");
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                serviceResult.Exception = exc;
            }

            return serviceResult;
        }

        public ServiceResult MarkProofReadingAsFinished(int translationDocumentPartId, int proofReaderId) {
            var serviceResult = new ServiceResult();
            try {
                var translationOperationItem = _model.TranslationOperations.FirstOrDefault(a => a.TranslationDocumentPartId == translationDocumentPartId);
                if (translationOperationItem == null) {
                    throw new BusinessException(ExceptionCodes.NoRelatedData);
                }

                translationOperationItem.TranslationProgressStatusId = (int)TranslationProgressStatusEnum.ProofReaderDone;

                _model.Entry(translationOperationItem).State = EntityState.Modified;

                var result = _model.SaveChanges() > 0;
                if (!result) {
                    throw new BusinessException(ExceptionCodes.UnableToUpdate);
                }


                var order = _model.OrderDetails.Include(a => a.TranslationOperation).FirstOrDefault(a => a.TranslationOperation.TranslationDocumentPartId == translationDocumentPartId);
                if (order == null) {
                    throw new BusinessException(ExceptionCodes.NoRelatedData);
                }

                var orderDetails =
                    _model.TranslatingOrders.Include(a => a.OrderDetails.Select(b => b.TranslationOperation))
                        .FirstOrDefault(a => a.Id == order.Id);

                if (orderDetails.OrderDetails.Count(a => a.TranslationOperation.TranslationProgressStatusId == (int)TranslationProgressStatusEnum.ProofReaderDone) == orderDetails.OrderDetails.Count) {
                    _dispatcher.Dispatch(new List<EventMessage> {
                    new UpdateOrderStatusEvent {
                        Id = Guid.NewGuid(),
                        StatusId = (int)OrderStatusEnum.RevisionNeeded,
                        TranslationOperationId = translationOperationItem.Id
                    }.ToEventMessage()
                    });
                }

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = true;
            } catch (Exception exc) {
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod()} with message {exc.Message}");
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                serviceResult.Exception = exc;
            }

            return serviceResult;
        }

        public ServiceResult<List<TranslationOperationDto>> GetAssignedJobsAsTranslator(int translatorId) {
            var serviceResult = new ServiceResult<List<TranslationOperationDto>>();
            try {
                var translationOperations = _model.TranslationOperations
                    .Include(a => a.TranslationProgressStatus)
                    .Include(a => a.TranslationDocumentPart)
                    .Include(a => a.TranslationOperationStatus)
                    .Where(a => a.TranslatorId == translatorId).ToList();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = translationOperations.Select(a=> _mapper.GetMapDto<TranslationOperationDto, TranslationOperation>(a)).ToList();
            } catch (Exception exc) {
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod()} with message {exc.Message}");
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                serviceResult.Exception = exc;
            }

            return serviceResult;
        }

        public ServiceResult<List<TranslationOperationDto>> GetAssignedJobsAsEditor(int editorId) {
            var serviceResult = new ServiceResult<List<TranslationOperationDto>>();
            try {
                var translationOperations = _model.TranslationOperations
                    .Include(a => a.TranslationProgressStatus)
                    .Include(a => a.TranslationDocumentPart)
                    .Include(a => a.TranslationOperationStatus)
                    .Where(a => a.EditorId == editorId).ToList();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = translationOperations.Select(a => _mapper.GetMapDto<TranslationOperationDto, TranslationOperation>(a)).ToList();
            } catch (Exception exc) {
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod()} with message {exc.Message}");
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                serviceResult.Exception = exc;
            }

            return serviceResult;
        }

        public ServiceResult<List<TranslationOperationDto>> GetAssignedJobsAsProofReader(int proofReaderId) {
            var serviceResult = new ServiceResult<List<TranslationOperationDto>>();
            try {
                var translationOperations = _model.TranslationOperations
                    .Include(a => a.TranslationProgressStatus)
                    .Include(a => a.TranslationDocumentPart)
                    .Include(a => a.TranslationOperationStatus)
                    .Where(a => a.ProofReaderId == proofReaderId).ToList();

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = translationOperations.Select(a => _mapper.GetMapDto<TranslationOperationDto, TranslationOperation>(a)).ToList();
            } catch (Exception exc) {
                _logger.Error($"Error occured in {MethodBase.GetCurrentMethod()} with message {exc.Message}");
                serviceResult.ServiceResultType = ServiceResultType.Fail;
                serviceResult.Exception = exc;
            }

            return serviceResult;
        }

        #endregion

        //TODO change this in order to eliminate cross reference. 
        //Instead use user service client to obtain average translating score
        private int GetAverateUserRating(IEnumerable<int> translatorsId) {
            var userRatings =
                _model.Users.Include(a => a.UserScore)
                    .Where(a => translatorsId.Any(x => x == a.Id))
                    .Select(a => a.UserScore.AverageTranslatingScore)
                    .Average();

            var ceiledNumber = (int)Math.Ceiling(userRatings);
            return ceiledNumber;
        }
    }
}