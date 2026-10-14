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
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiSearch = Acme.Search.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Timekeeping;

/// <summary>
/// The application port for UpdateEmployee.
/// </summary>
public interface IOvertimeLifecycleEndpointSendReinstateOvertime1Port
{
    Task<ApiEmployees.UpdateEmployeeResponse> SendAsync(ApiEmployees.UpdateEmployeeRequest request, CancellationToken token);
}

public sealed class OvertimeLifecycleEndpointSendReinstateOvertime1PortAdapter(ApiEmployees.EmployeeService.EmployeeServiceClient client) : IOvertimeLifecycleEndpointSendReinstateOvertime1Port
{
    public async Task<ApiEmployees.UpdateEmployeeResponse> SendAsync(ApiEmployees.UpdateEmployeeRequest request, CancellationToken token)
    {
        using var rpc = client.UpdateEmployeeAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class OvertimeLifecycleEndpointRequests
{
    /// <summary>
    /// Builds the QuoteEmploymentContract request from the current workflow state.
    /// </summary>
    public static ApiEmployees.QuoteEmploymentContractRequest ReinstateOvertimeToQuoteEmploymentContract0(this ApiTimekeeping.ReinstateOvertimeRequest request)
    {
        return new ApiEmployees.QuoteEmploymentContractRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
        };
    }
}
