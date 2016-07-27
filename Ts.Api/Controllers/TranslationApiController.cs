using System.Net;
using System.Net.Http;
using System.Web.Http;
using Tangent.CeviriDukkani.Domain.Common;
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
            var response = new HttpResponseMessage();

            ServiceResult serviceResult = _translationService.GetAverageDocumentPartCount(orderId);
            if (serviceResult.ServiceResultType != ServiceResultType.Success) {
                response.StatusCode = HttpStatusCode.Forbidden;
                response.Content = new ObjectContent(serviceResult.GetType(), serviceResult, Formatter);
                return response;
            }

            response.StatusCode = HttpStatusCode.OK;
            response.Content = new ObjectContent(serviceResult.GetType(), serviceResult, Formatter);
            return response;
        }
    }
}