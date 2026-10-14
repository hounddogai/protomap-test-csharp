using Acme.Workforce.Platform;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiKeymanagement = Acme.Keymanagement.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Performance;

public static class FeedbackValidationEndpointRequests
{
    /// <summary>
    /// Builds the ExplainCryptoKeyCalculation request from the current workflow state.
    /// </summary>
    public static ApiKeymanagement.ExplainCryptoKeyCalculationRequest ValidateFeedbackToExplainCryptoKeyCalculation0(this ApiPerformance.ValidateFeedbackRequest request)
    {
        return new ApiKeymanagement.ExplainCryptoKeyCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CryptoKey = new ApiKeymanagement.CryptoKey
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
            },
            Details = new ApiKeymanagement.ExplainCryptoKeyCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Contact?.Phone ?? ""),
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
            },
        };
    }
}
