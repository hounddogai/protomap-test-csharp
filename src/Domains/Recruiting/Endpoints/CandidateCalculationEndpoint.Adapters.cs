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
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiShows = Acme.Shows.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Recruiting;

/// <summary>
/// The application port for RecalculateJobOpenings.
/// </summary>
public interface ICandidateCalculationEndpointSendCalculateCandidate0Port
{
    Task<ApiRecruiting.RecalculateJobOpeningsResponse> SendAsync(ApiRecruiting.RecalculateJobOpeningsRequest request, CancellationToken token);
}

public sealed class CandidateCalculationEndpointSendCalculateCandidate0PortAdapter(ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient client) : ICandidateCalculationEndpointSendCalculateCandidate0Port
{
    public async Task<ApiRecruiting.RecalculateJobOpeningsResponse> SendAsync(ApiRecruiting.RecalculateJobOpeningsRequest request, CancellationToken token)
    {
        using var rpc = client.RecalculateJobOpeningsAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

/// <summary>
/// The application port for EstimateJobOpening.
/// </summary>
public interface ICandidateCalculationEndpointSendQuoteCandidate1Port
{
    Task<ApiRecruiting.EstimateJobOpeningResponse> SendAsync(ApiRecruiting.EstimateJobOpeningRequest request, CancellationToken token);
}

public sealed class CandidateCalculationEndpointSendQuoteCandidate1PortAdapter(ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient client) : ICandidateCalculationEndpointSendQuoteCandidate1Port
{
    public async Task<ApiRecruiting.EstimateJobOpeningResponse> SendAsync(ApiRecruiting.EstimateJobOpeningRequest request, CancellationToken token)
    {
        using var rpc = client.EstimateJobOpeningAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class CandidateCalculationEndpointRequests
{
    /// <summary>
    /// Builds the ExplainJobOpeningCalculation request from the current workflow state.
    /// </summary>
    public static ApiRecruiting.ExplainJobOpeningCalculationRequest QuoteCandidateToExplainJobOpeningCalculation0(this ApiRecruiting.QuoteCandidateRequest request)
    {
        return new ApiRecruiting.ExplainJobOpeningCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.ExplainJobOpeningCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Candidate?.Contact?.Email ?? ""),
                },
            },
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Candidate?.Audit?.CreatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                },
            },
        };
    }
}
