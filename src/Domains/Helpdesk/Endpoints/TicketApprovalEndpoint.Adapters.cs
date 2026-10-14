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
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRatelimiting = Acme.Ratelimiting.V1;

namespace Acme.Workforce.Helpdesk;

/// <summary>
/// The application port for ValidateEmploymentContract.
/// </summary>
public interface ITicketApprovalEndpointSendGetTicketApprovalStatus0Port
{
    Task<ApiEmployees.ValidateEmploymentContractResponse> SendAsync(ApiEmployees.ValidateEmploymentContractRequest request, CancellationToken token);
}

public sealed class TicketApprovalEndpointSendGetTicketApprovalStatus0PortAdapter(ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient client) : ITicketApprovalEndpointSendGetTicketApprovalStatus0Port
{
    public async Task<ApiEmployees.ValidateEmploymentContractResponse> SendAsync(ApiEmployees.ValidateEmploymentContractRequest request, CancellationToken token)
    {
        using var rpc = client.ValidateEmploymentContractAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
