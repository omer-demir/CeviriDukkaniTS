using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Tangent.CeviriDukkani.Domain.Common;
using Tangent.CeviriDukkani.Domain.Dto.Request;
using Tangent.CeviriDukkani.Domain.Dto.Translation;
using Tangent.CeviriDukkani.WebCore.BaseControllers;
using Ts.Business.Services;

namespace Ts.Api.Controllers {
    [RoutePrefix("api/translationapi")]
    public class TranslationApiController:BaseApiController {
        private readonly ITranslationService _translationService;

        public TranslationApiController(ITranslationService translationService) {
            _translationService = translationService;
        }

        [HttpGet, Route("getAverageDocumentPartCount")]
        public HttpResponseMessage GetAverageDocumentPartCount([FromUri] int orderId) {
            ServiceResult serviceResult = _translationService.GetAverageDocumentPartCount(orderId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }

        [HttpPost, Route("saveTranslationOperations")]
        public HttpResponseMessage SaveTranslationOperations([FromBody]List<TranslationOperationDto> translationOperations) {
            ServiceResult<List<TranslationOperationDto>> serviceResult = _translationService.SaveTranslationOperations(translationOperations);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }

        [HttpPost, Route("updateTranslatedDocumentPart")]
        public HttpResponseMessage UpdateTranslatedDocumentPart(UpdateDocumentPartContentRequestDto request) {
            var serviceResult = _translationService.UpdateTranslatedDocumentPart(request.ChangerId, request.TranslationDocumentPartId, request.Content);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }

        [HttpPost, Route("updateEditedDocumentPart")]
        public HttpResponseMessage UpdateEditedDocumentPart(UpdateDocumentPartContentRequestDto request) {
            var serviceResult = _translationService.UpdateEditedDocumentPart(request.ChangerId, request.TranslationDocumentPartId, request.Content);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }

        [HttpPost, Route("updateProofReadDocumentPart")]
        public HttpResponseMessage UpdateProofReadDocumentPart(UpdateDocumentPartContentRequestDto request) {
            var serviceResult = _translationService.UpdateProofReadDocumentPart(request.ChangerId, request.TranslationDocumentPartId, request.Content);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }

        [HttpPost, Route("finishTranslation")]
        public HttpResponseMessage FinishTranslation(FinishDocumentPartRequestDto request) {
            var serviceResult = _translationService.FinishTranslation(request.ChangerId, request.TranslationDocumentPartId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }

        [HttpPost, Route("finishEditing")]
        public HttpResponseMessage FinishEditing(FinishDocumentPartRequestDto request) {
            var serviceResult = _translationService.FinishEditing(request.ChangerId, request.TranslationDocumentPartId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }

        [HttpPost, Route("finishProofReading")]
        public HttpResponseMessage FinishProofReading(FinishDocumentPartRequestDto request) {
            var serviceResult = _translationService.FinishProofReading(request.ChangerId, request.TranslationDocumentPartId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }

        [HttpGet, Route("getTranslatedContent")]
        public HttpResponseMessage GetTranslatedContent([FromUri]int translationDocumentPartId,[FromUri] int userId) {
            var serviceResult = _translationService.GetTranslatedContent(translationDocumentPartId, userId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }

        [HttpGet, Route("getTranslatedContentForEditor")]
        public HttpResponseMessage GetTranslatedContentForEditor([FromUri]int translationDocumentPartId, [FromUri] int userId) {
            var serviceResult = _translationService.GetTranslatedContentForEditor(translationDocumentPartId, userId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }

        [HttpGet, Route("getEditedContent")]
        public HttpResponseMessage GetEditedContent([FromUri]int translationDocumentPartId,[FromUri] int userId) {
            var serviceResult = _translationService.GetEditedContent(translationDocumentPartId, userId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }

        [HttpGet, Route("getEditedContentForProofReader")]
        public HttpResponseMessage GetEditedContentForProofReader([FromUri]int translationDocumentPartId, [FromUri] int userId) {
            var serviceResult = _translationService.GetEditedContentForProofReader(translationDocumentPartId, userId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }

        [HttpGet, Route("getProofReadContent")]
        public HttpResponseMessage GetProofReadContent([FromUri]int translationDocumentPartId,[FromUri] int userId) {
            var serviceResult = _translationService.GetProofReadContent(translationDocumentPartId, userId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }

        [HttpPost, Route("addCommentToTranslationOperation")]
        public HttpResponseMessage AddCommentToTranslationOperation(CreateCommentRequestDto request) {
            var serviceResult = _translationService.AddCommentToTranslationOperation(request.TranslationDocumentPartId, request.CommentCreatorId,request.Content);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }

        [HttpPost, Route("addCommentToEditionOperation")]
        public HttpResponseMessage AddCommentToEditionOperation(CreateCommentRequestDto request) {
            var serviceResult = _translationService.AddCommentToEditionOperation(request.TranslationDocumentPartId, request.CommentCreatorId, request.Content);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }

        [HttpGet, Route("getTranslationOperationComments")]
        public HttpResponseMessage GetTranslationOperationComments([FromUri]int translationDocumentPartId) {
            var serviceResult = _translationService.GetTranslationOperationComments(translationDocumentPartId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }

        [HttpPost, Route("markTranslatingAsFinished")]
        public HttpResponseMessage MarkTranslatingAsFinished(MarkOperationAsFinishedRequestDto request) {
            var serviceResult = _translationService.MarkTranslatingAsFinished(request.TranslationDocumentPartId,request.UserId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }

        [HttpPost, Route("markEditingAsFinished")]
        public HttpResponseMessage MarkEditingAsFinished(MarkOperationAsFinishedRequestDto request) {
            var serviceResult = _translationService.MarkEditingAsFinished(request.TranslationDocumentPartId, request.UserId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }

        [HttpPost, Route("markProofReadingAsFinished")]
        public HttpResponseMessage MarkProofReadingAsFinished(MarkOperationAsFinishedRequestDto request) {
            var serviceResult = _translationService.MarkProofReadingAsFinished(request.TranslationDocumentPartId, request.UserId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }

        [HttpGet, Route("getAssignedJobsAsTranslator")]
        public HttpResponseMessage GetAssignedJobsAsTranslator([FromUri]int userId) {
            var serviceResult = _translationService.GetAssignedJobsAsTranslator(userId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }

        [HttpGet, Route("getAssignedJobsAsEditor")]
        public HttpResponseMessage GetAssignedJobsAsEditor([FromUri]int userId) {
            var serviceResult = _translationService.GetAssignedJobsAsEditor(userId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }

        [HttpGet, Route("getAssignedJobsAsProofReader")]
        public HttpResponseMessage GetAssignedJobsAsProofReader([FromUri]int userId) {
            var serviceResult = _translationService.GetAssignedJobsAsProofReader(userId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                return Error(serviceResult);
            }

            return OK(serviceResult);
        }

    }
}