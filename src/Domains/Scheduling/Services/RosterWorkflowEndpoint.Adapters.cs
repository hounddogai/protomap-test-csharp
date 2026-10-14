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
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiCustomers = Acme.Customers.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFeaturestore = Acme.Featurestore.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLabeling = Acme.Labeling.V1;
using ApiPayroll = Acme.Payroll.V1;
using ApiRatelimiting = Acme.Ratelimiting.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Scheduling;

/// <summary>
/// The application port for LintAvailabilityWindows.
/// </summary>
public interface IRosterWorkflowEndpointSendRetryRosterWorkflow2Port
{
    Task<ApiScheduling.LintAvailabilityWindowsResponse> SendAsync(ApiScheduling.LintAvailabilityWindowsRequest request, CancellationToken token);
}

public sealed class RosterWorkflowEndpointSendRetryRosterWorkflow2PortAdapter(ApiScheduling.AvailabilityWindowValidationService.AvailabilityWindowValidationServiceClient client) : IRosterWorkflowEndpointSendRetryRosterWorkflow2Port
{
    public async Task<ApiScheduling.LintAvailabilityWindowsResponse> SendAsync(ApiScheduling.LintAvailabilityWindowsRequest request, CancellationToken token)
    {
        using var rpc = client.LintAvailabilityWindowsAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class RosterWorkflowEndpointRequests
{
    /// <summary>
    /// Builds the CheckPositionEligibility request from the current workflow state.
    /// </summary>
    public static ApiEmployees.CheckPositionEligibilityRequest RetryRosterWorkflowToCheckPositionEligibility1(this ApiScheduling.RetryRosterWorkflowRequest request)
    {
        return new ApiEmployees.CheckPositionEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Roster?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Roster?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
    }
}
