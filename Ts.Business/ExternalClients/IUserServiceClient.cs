using System.Collections.Generic;
using Tangent.CeviriDukkani.Domain.Common;
using Tangent.CeviriDukkani.Domain.Dto.System;

namespace Ts.Business.ExternalClients {
    public interface IUserServiceClient {
        ServiceResult<List<UserDto>> GetTranslatorsAccordingToOrderTranslationQuality(int orderId);
    }
}