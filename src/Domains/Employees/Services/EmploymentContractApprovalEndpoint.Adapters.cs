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
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMaintenance = Acme.Maintenance.V1;
using ApiPromotions = Acme.Promotions.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Employees;

/// <summary>
/// The application port for SignalEmployeeWorkflow.
/// </summary>
public interface IEmploymentContractApprovalEndpointSendDelegateEmploymentContractApproval2Port
{
    Task<ApiEmployees.SignalEmployeeWorkflowResponse> SendAsync(ApiEmployees.SignalEmployeeWorkflowRequest request, CancellationToken token);
}

public sealed class EmploymentContractApprovalEndpointSendDelegateEmploymentContractApproval2PortAdapter(ApiEmployees.EmployeeWorkflowService.EmployeeWorkflowServiceClient client) : IEmploymentContractApprovalEndpointSendDelegateEmploymentContractApproval2Port
{
    public async Task<ApiEmployees.SignalEmployeeWorkflowResponse> SendAsync(ApiEmployees.SignalEmployeeWorkflowRequest request, CancellationToken token)
    {
        using var rpc = client.SignalEmployeeWorkflowAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class EmploymentContractApprovalEndpointRequests
{
    /// <summary>
    /// Builds the ListEmployeeWorkflows request from the current workflow state.
    /// </summary>
    public static ApiEmployees.ListEmployeeWorkflowsRequest DelegateEmploymentContractApprovalToListEmployeeWorkflows1(this ApiEmployees.DelegateEmploymentContractApprovalRequest request, ApiEmployees.GetEmploymentContractRulesResponse reply0)
    {
        return new ApiEmployees.ListEmployeeWorkflowsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.ListEmployeeWorkflowsRequest.Types.Filter
            {
                CreatedAfter = (request.EmploymentContract?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (reply0.EmploymentContract?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.EmploymentContract?.Counterparty?.EmployeeId ?? ""),
                Region = (request.EmploymentContract?.BillingAddress?.Region ?? ""),
            },
        };
    }
}
