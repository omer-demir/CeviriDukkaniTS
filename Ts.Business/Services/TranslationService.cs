using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Tangent.CeviriDukkani.Data.Model;
using Tangent.CeviriDukkani.Domain.Common;
using Tangent.CeviriDukkani.Domain.Dto.System;
using Ts.Business.ExternalClients;

namespace Ts.Business.Services {
    public class TranslationService : ITranslationService {
        private readonly CeviriDukkaniModel _model;
        private readonly IUserServiceClient _userServiceClient;

        public TranslationService(CeviriDukkaniModel model, IUserServiceClient userServiceClient) {
            _model = model;
            _userServiceClient = userServiceClient;
        }

        #region Implementation of ITranslationService

        public ServiceResult GetAverageDocumentPartCount(int orderId) {
            var serviceResult = new ServiceResult(ServiceResultType.NotKnown);
            try {
                var translatorServiceResult = _userServiceClient.GetTranslatorsAccordingToOrderTranslationQuality(orderId);
                if (translatorServiceResult.ServiceResultType != ServiceResultType.Success) {
                    throw translatorServiceResult.Exception;
                }

                var translators = translatorServiceResult.Data as List<UserDto>;
                var translatorsId = translators.Select(a => a.Id);
                var ceiledNumber = GetAverateUserRating(translatorsId);

                serviceResult.ServiceResultType = ServiceResultType.Success;
                serviceResult.Data = ceiledNumber;
            } catch (Exception exc) {
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