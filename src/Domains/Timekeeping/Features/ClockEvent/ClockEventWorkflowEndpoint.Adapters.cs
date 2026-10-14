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
using ApiAudit = Acme.Audit.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Timekeeping;

/// <summary>
/// The application port for CheckOvertimeEligibility.
/// </summary>
public interface IClockEventWorkflowEndpointSendSignalClockEventWorkflow0Port
{
    Task<ApiTimekeeping.CheckOvertimeEligibilityResponse> SendAsync(ApiTimekeeping.CheckOvertimeEligibilityRequest request, CancellationToken token);
}

public sealed class ClockEventWorkflowEndpointSendSignalClockEventWorkflow0PortAdapter(ApiTimekeeping.OvertimeValidationService.OvertimeValidationServiceClient client) : IClockEventWorkflowEndpointSendSignalClockEventWorkflow0Port
{
    public async Task<ApiTimekeeping.CheckOvertimeEligibilityResponse> SendAsync(ApiTimekeeping.CheckOvertimeEligibilityRequest request, CancellationToken token)
    {
        using var rpc = client.CheckOvertimeEligibilityAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class ClockEventWorkflowEndpointRequests
{
    /// <summary>
    /// Builds the FindEmployees request from the current workflow state.
    /// </summary>
    public static ApiEmployees.FindEmployeesRequest RetryClockEventWorkflowToFindEmployees0(this ApiTimekeeping.RetryClockEventWorkflowRequest request)
    {
        return new ApiEmployees.FindEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.FindEmployeesRequest.Types.Filter
            {
                CreatedAfter = (request.ClockEvent?.OccurredAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.ClockEvent?.Subject?.TimesheetId ?? ""),
            },
        };
    }
}
