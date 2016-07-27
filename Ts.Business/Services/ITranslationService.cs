using Tangent.CeviriDukkani.Domain.Common;

namespace Ts.Business.Services {
    public interface ITranslationService {
        ServiceResult GetAverageDocumentPartCount(int orderId);
    }
}