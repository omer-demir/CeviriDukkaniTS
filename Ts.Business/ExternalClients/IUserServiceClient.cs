using Tangent.CeviriDukkani.Domain.Common;

namespace Ts.Business.ExternalClients {
    public interface IUserServiceClient {
        ServiceResult GetTranslatorsAccordingToOrderTranslationQuality(int orderId);
    }
}