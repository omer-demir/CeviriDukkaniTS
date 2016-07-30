using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using log4net;
using Newtonsoft.Json;
using Tangent.CeviriDukkani.Data.Model;
using Tangent.CeviriDukkani.Domain.Common;
using Tangent.CeviriDukkani.Domain.Dto.System;
using Tangent.CeviriDukkani.Domain.Dto.Translation;
using Tangent.CeviriDukkani.Domain.Entities.Translation;
using Tangent.CeviriDukkani.Domain.Exceptions;
using Tangent.CeviriDukkani.Domain.Exceptions.ExceptionCodes;
using Tangent.CeviriDukkani.Domain.Mappers;
using Ts.Business.ExternalClients;

namespace Ts.Business.Services {
    public class TranslationService : ITranslationService {
        private readonly CeviriDukkaniModel _model;
        private readonly IUserServiceClient _userServiceClient;
        private readonly CustomMapperConfiguration _mapper;
        private readonly ILog _logger;

        public TranslationService(CeviriDukkaniModel model, IUserServiceClient userServiceClient, CustomMapperConfiguration mapper, ILog logger) {
            _model = model;
            _userServiceClient = userServiceClient;
            _mapper = mapper;
            _logger = logger;
        }

        #region Implementation of ITranslationService

        public ServiceResult GetAverageDocumentPartCount(int orderId) {
            var serviceResult = new ServiceResult(ServiceResultType.NotKnown);
            try {
                var translatorServiceResult = _userServiceClient.GetTranslatorsAccordingToOrderTranslationQuality(orderId);
                if (translatorServiceResult.ServiceResultType != ServiceResultType.Success) {
                    throw translatorServiceResult.Exception;
                }

                var translators = JsonConvert.DeserializeObject<List<UserDto>>(translatorServiceResult.Data.ToString());
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

        public ServiceResult SaveTranslationOperations(List<TranslationOperationDto> translationOperations) {
            var serviceResult = new ServiceResult(ServiceResultType.NotKnown);
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
                serviceResult.Data = translationOperationItems.Select(a => _mapper.GetMapDto<TranslationOperationDto, TranslationOperation>(a));
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