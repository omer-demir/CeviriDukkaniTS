using System.Collections.Generic;
using Tangent.CeviriDukkani.Domain.Common;
using Tangent.CeviriDukkani.Domain.Dto.Common;
using Tangent.CeviriDukkani.Domain.Dto.Translation;

namespace Ts.Business.Services {
    public interface ITranslationService {
        ServiceResult GetAverageDocumentPartCount(int orderId);
        ServiceResult<List<TranslationOperationDto>> SaveTranslationOperations(List<TranslationOperationDto> translationOperations);
        ServiceResult UpdateTranslatedDocumentPart(int translatorId,int translationDocumentPartId, string content);
        ServiceResult UpdateEditedDocumentPart(int editorId, int translationDocumentPartId, string content);
        ServiceResult UpdateProofReadDocumentPart(int proofReaderId, int translationDocumentPartId, string content);
        ServiceResult FinishTranslation(int translatorId, int translationDocumentPartId);
        ServiceResult FinishEditing(int editorId, int translationDocumentPartId);
        ServiceResult FinishProofReading(int proofReaderId, int translationDocumentPartId);
        ServiceResult GetTranslatedContent(int translationDocumentPartId, int translatorId);
        ServiceResult GetEditedContent(int translationDocumentPartId, int editorId);
        ServiceResult GetProofReadContent(int translationDocumentPartId, int proofReaderId);
        ServiceResult AddCommentToTranslationOperation(int translationDocumentPartId, int commentCreatorId,string content);
        ServiceResult AddCommentToEditionOperation(int translationDocumentPartId, int commentCreatorId, string content);
        ServiceResult<List<CommentDto>> GetTranslationOperationComments(int translationDocumentPartId);
        ServiceResult MarkTranslatingAsFinished(int translationDocumentPartId, int translatorId);
        ServiceResult MarkEditingAsFinished(int translationDocumentPartId, int editorId);
        ServiceResult MarkProofReadingAsFinished(int translationDocumentPartId, int proofReaderId);

    }
}