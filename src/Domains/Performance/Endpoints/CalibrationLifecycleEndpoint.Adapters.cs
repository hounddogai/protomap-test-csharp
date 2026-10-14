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
using ApiCatalog = Acme.Catalog.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiCustomers = Acme.Customers.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecommendations = Acme.Recommendations.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Performance;

/// <summary>
/// The application port for PreviewGoal.
/// </summary>
public interface ICalibrationLifecycleEndpointSendActivateCalibration2Port
{
    Task<ApiPerformance.PreviewGoalResponse> SendAsync(ApiPerformance.PreviewGoalRequest request, CancellationToken token);
}

public sealed class CalibrationLifecycleEndpointSendActivateCalibration2PortAdapter(ApiPerformance.GoalValidationService.GoalValidationServiceClient client) : ICalibrationLifecycleEndpointSendActivateCalibration2Port
{
    public async Task<ApiPerformance.PreviewGoalResponse> SendAsync(ApiPerformance.PreviewGoalRequest request, CancellationToken token)
    {
        using var rpc = client.PreviewGoalAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class CalibrationLifecycleEndpointRequests
{
    /// <summary>
    /// Builds the CountProducts request from the current workflow state.
    /// </summary>
    public static ApiCatalog.CountProductsRequest ActivateCalibrationToCountProducts1(this ApiPerformance.ActivateCalibrationRequest request)
    {
        return new ApiCatalog.CountProductsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiCatalog.CountProductsRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
    }
}
