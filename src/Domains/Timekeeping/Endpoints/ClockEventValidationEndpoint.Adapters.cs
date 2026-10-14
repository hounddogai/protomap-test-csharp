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
using ApiAppointments = Acme.Appointments.V1;
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Timekeeping;

/// <summary>
/// The application port for PreviewTimesheet.
/// </summary>
public interface IClockEventValidationEndpointSendPreviewClockEvent1Port
{
    Task<ApiTimekeeping.PreviewTimesheetResponse> SendAsync(ApiTimekeeping.PreviewTimesheetRequest request, CancellationToken token);
}

public sealed class ClockEventValidationEndpointSendPreviewClockEvent1PortAdapter(ApiTimekeeping.TimesheetValidationService.TimesheetValidationServiceClient client) : IClockEventValidationEndpointSendPreviewClockEvent1Port
{
    public async Task<ApiTimekeeping.PreviewTimesheetResponse> SendAsync(ApiTimekeeping.PreviewTimesheetRequest request, CancellationToken token)
    {
        using var rpc = client.PreviewTimesheetAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class ClockEventValidationEndpointRequests
{
    /// <summary>
    /// Builds the CalculateOvertime request from the current workflow state.
    /// </summary>
    public static ApiTimekeeping.CalculateOvertimeRequest PreviewClockEventToCalculateOvertime0(this ApiTimekeeping.PreviewClockEventRequest request)
    {
        return new ApiTimekeeping.CalculateOvertimeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Overtime = new ApiTimekeeping.Overtime
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
        };
    }
}
