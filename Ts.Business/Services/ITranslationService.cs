using System.Collections.Generic;
using Tangent.CeviriDukkani.Domain.Common;
using Tangent.CeviriDukkani.Domain.Dto.Translation;

namespace Ts.Business.Services {
    public interface ITranslationService {
        ServiceResult GetAverageDocumentPartCount(int orderId);
        ServiceResult SaveTranslationOperations(List<TranslationOperationDto> translationOperations);
    }
}