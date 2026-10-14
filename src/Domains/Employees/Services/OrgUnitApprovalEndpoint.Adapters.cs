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
using ApiExperiments = Acme.Experiments.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLoyalty = Acme.Loyalty.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Employees;

/// <summary>
/// The application port for ListDocumentFolderConflicts.
/// </summary>
public interface IOrgUnitApprovalEndpointSendEscalateOrgUnit2Port
{
    Task<ApiDocuments.ListDocumentFolderConflictsResponse> SendAsync(ApiDocuments.ListDocumentFolderConflictsRequest request, CancellationToken token);
}

public sealed class OrgUnitApprovalEndpointSendEscalateOrgUnit2PortAdapter(ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient client) : IOrgUnitApprovalEndpointSendEscalateOrgUnit2Port
{
    public async Task<ApiDocuments.ListDocumentFolderConflictsResponse> SendAsync(ApiDocuments.ListDocumentFolderConflictsRequest request, CancellationToken token)
    {
        using var rpc = client.ListDocumentFolderConflictsAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class OrgUnitApprovalEndpointRequests
{
    /// <summary>
    /// Builds the PreviewEmploymentContract request from the current workflow state.
    /// </summary>
    public static ApiEmployees.PreviewEmploymentContractRequest EscalateOrgUnitToPreviewEmploymentContract1(this ApiEmployees.EscalateOrgUnitRequest request)
    {
        return new ApiEmployees.PreviewEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
        };
    }
}
