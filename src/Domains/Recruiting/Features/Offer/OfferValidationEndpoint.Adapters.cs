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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiMedia = Acme.Media.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Recruiting;

/// <summary>
/// The application port for SimulateLearningPath.
/// </summary>
public interface IOfferValidationEndpointSendNormalizeOffer1Port
{
    Task<ApiLearning.SimulateLearningPathResponse> SendAsync(ApiLearning.SimulateLearningPathRequest request, CancellationToken token);
}

public sealed class OfferValidationEndpointSendNormalizeOffer1PortAdapter(ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient client) : IOfferValidationEndpointSendNormalizeOffer1Port
{
    public async Task<ApiLearning.SimulateLearningPathResponse> SendAsync(ApiLearning.SimulateLearningPathRequest request, CancellationToken token)
    {
        using var rpc = client.SimulateLearningPathAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class OfferValidationEndpointRequests
{
    /// <summary>
    /// Builds the SimulateOffer request from the current workflow state.
    /// </summary>
    public static ApiRecruiting.SimulateOfferRequest NormalizeOfferToSimulateOffer0(this ApiRecruiting.NormalizeOfferRequest request)
    {
        return new ApiRecruiting.SimulateOfferRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.SimulateOfferRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Offer?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Region = (request.Offer?.BillingAddress?.Region ?? ""),
                },
            },
            Offer = new ApiRecruiting.Offer
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Offer?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Offer?.Counterparty?.Email ?? ""),
                },
            },
        };
    }
}
