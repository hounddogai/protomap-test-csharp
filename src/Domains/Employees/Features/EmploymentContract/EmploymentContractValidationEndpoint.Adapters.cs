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
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Employees;

/// <summary>
/// The application port for ValidatePosition.
/// </summary>
public interface IEmploymentContractValidationEndpointSendNormalizeEmploymentContract3Port
{
    Task<ApiEmployees.ValidatePositionResponse> SendAsync(ApiEmployees.ValidatePositionRequest request, CancellationToken token);
}

public sealed class EmploymentContractValidationEndpointSendNormalizeEmploymentContract3PortAdapter(ApiEmployees.PositionValidationService.PositionValidationServiceClient client) : IEmploymentContractValidationEndpointSendNormalizeEmploymentContract3Port
{
    public async Task<ApiEmployees.ValidatePositionResponse> SendAsync(ApiEmployees.ValidatePositionRequest request, CancellationToken token)
    {
        using var rpc = client.ValidatePositionAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class EmploymentContractValidationEndpointRequests
{
    /// <summary>
    /// Builds the BrowseLedgerAccounts request from the current workflow state.
    /// </summary>
    public static ApiLedger.BrowseLedgerAccountsRequest NormalizeEmploymentContractToBrowseLedgerAccounts2(this ApiEmployees.NormalizeEmploymentContractRequest request, ApiEmployees.LintPositionsResponse reply1)
    {
        return new ApiLedger.BrowseLedgerAccountsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLedger.BrowseLedgerAccountsRequest.Types.Filter
            {
                CreatedAfter = (request.EmploymentContract?.IssuedAt ?? new Timestamp()),
                Labels = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.EmploymentContract?.Counterparty?.EmployeeId ?? ""),
                Region = (request.EmploymentContract?.BillingAddress?.Region ?? ""),
            },
        };
    }
}
