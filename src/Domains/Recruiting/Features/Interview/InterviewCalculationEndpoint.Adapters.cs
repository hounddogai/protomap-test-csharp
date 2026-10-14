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
using ApiChat = Acme.Chat.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPrescriptions = Acme.Prescriptions.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Recruiting;

/// <summary>
/// The application port for ScoreItinerary.
/// </summary>
public interface IInterviewCalculationEndpointSendCompareInterviewOptions1Port
{
    Task<ApiTravel.ScoreItineraryResponse> SendAsync(ApiTravel.ScoreItineraryRequest request, CancellationToken token);
}

public sealed class InterviewCalculationEndpointSendCompareInterviewOptions1PortAdapter(ApiTravel.ItineraryValidationService.ItineraryValidationServiceClient client) : IInterviewCalculationEndpointSendCompareInterviewOptions1Port
{
    public async Task<ApiTravel.ScoreItineraryResponse> SendAsync(ApiTravel.ScoreItineraryRequest request, CancellationToken token)
    {
        using var rpc = client.ScoreItineraryAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class InterviewCalculationEndpointRequests
{
    /// <summary>
    /// Builds the PreviewItinerary request from the current workflow state.
    /// </summary>
    public static ApiTravel.PreviewItineraryRequest CompareInterviewOptionsToPreviewItinerary0(this ApiRecruiting.CompareInterviewOptionsRequest request)
    {
        return new ApiTravel.PreviewItineraryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Itinerary = new ApiTravel.Itinerary
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Interview?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Interview?.Location?.CountryCode ?? ""),
                    Lines = { (request.Interview?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Recipient = (request.Details?.Destination?.Recipient ?? ""),
                },
            },
        };
    }
}
