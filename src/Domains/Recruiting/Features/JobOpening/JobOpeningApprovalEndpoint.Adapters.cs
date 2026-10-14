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
using ApiBanking = Acme.Banking.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Recruiting;

/// <summary>
/// The application port for CheckEmploymentContractEligibility.
/// </summary>
public interface IJobOpeningApprovalEndpointSendListPendingJobOpenings0Port
{
    Task<ApiEmployees.CheckEmploymentContractEligibilityResponse> SendAsync(ApiEmployees.CheckEmploymentContractEligibilityRequest request, CancellationToken token);
}

public sealed class JobOpeningApprovalEndpointSendListPendingJobOpenings0PortAdapter(ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient client) : IJobOpeningApprovalEndpointSendListPendingJobOpenings0Port
{
    public async Task<ApiEmployees.CheckEmploymentContractEligibilityResponse> SendAsync(ApiEmployees.CheckEmploymentContractEligibilityRequest request, CancellationToken token)
    {
        using var rpc = client.CheckEmploymentContractEligibilityAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class JobOpeningApprovalEndpointRequests
{
    /// <summary>
    /// Builds the ScoreMetricSeries request from the current workflow state.
    /// </summary>
    public static ApiMetrics.ScoreMetricSeriesRequest GetJobOpeningApprovalStatusToScoreMetricSeries0(this ApiRecruiting.GetJobOpeningApprovalStatusRequest request)
    {
        return new ApiMetrics.ScoreMetricSeriesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            MetricSeries = new ApiMetrics.MetricSeries
            {
                Components = { { "primary", new ApiMetrics.MetricSeriesComponent
                {
                    ComponentId = (request.JobOpeningId ?? ""),
                } } },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
    }
}
