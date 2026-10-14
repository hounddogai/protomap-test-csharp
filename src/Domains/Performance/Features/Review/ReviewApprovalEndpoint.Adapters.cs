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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Performance;

/// <summary>
/// The application port for NormalizeGoal.
/// </summary>
public interface IReviewApprovalEndpointSendGetReviewApprovalStatus0Port
{
    Task<ApiPerformance.NormalizeGoalResponse> SendAsync(ApiPerformance.NormalizeGoalRequest request, CancellationToken token);
}

public sealed class ReviewApprovalEndpointSendGetReviewApprovalStatus0PortAdapter(ApiPerformance.GoalValidationService.GoalValidationServiceClient client) : IReviewApprovalEndpointSendGetReviewApprovalStatus0Port
{
    public async Task<ApiPerformance.NormalizeGoalResponse> SendAsync(ApiPerformance.NormalizeGoalRequest request, CancellationToken token)
    {
        using var rpc = client.NormalizeGoalAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class ReviewApprovalEndpointRequests
{
    /// <summary>
    /// Builds the LookupReview request from the current workflow state.
    /// </summary>
    public static ApiPerformance.LookupReviewRequest EscalateReviewToLookupReview3(this ApiPerformance.EscalateReviewRequest request)
    {
        return new ApiPerformance.LookupReviewRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ReviewId = (request.ReviewId ?? ""),
        };
    }
}
