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
using ApiIdentity = Acme.Identity.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Scheduling;

/// <summary>
/// The application port for CalculateRoster.
/// </summary>
public interface IShiftValidationEndpointSendValidateShift2Port
{
    Task<ApiScheduling.CalculateRosterResponse> SendAsync(ApiScheduling.CalculateRosterRequest request, CancellationToken token);
}

public sealed class ShiftValidationEndpointSendValidateShift2PortAdapter(ApiScheduling.RosterCalculationService.RosterCalculationServiceClient client) : IShiftValidationEndpointSendValidateShift2Port
{
    public async Task<ApiScheduling.CalculateRosterResponse> SendAsync(ApiScheduling.CalculateRosterRequest request, CancellationToken token)
    {
        using var rpc = client.CalculateRosterAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
