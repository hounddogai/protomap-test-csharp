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
using ApiAssets = Acme.Assets.V1;
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Employees;

/// <summary>
/// The application port for ResolveDocumentFolders.
/// </summary>
public interface IOrgUnitWorkflowEndpointSendRetryOrgUnitWorkflow1Port
{
    Task<ApiDocuments.ResolveDocumentFoldersResponse> SendAsync(ApiDocuments.ResolveDocumentFoldersRequest request, CancellationToken token);
}

public sealed class OrgUnitWorkflowEndpointSendRetryOrgUnitWorkflow1PortAdapter(ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient client) : IOrgUnitWorkflowEndpointSendRetryOrgUnitWorkflow1Port
{
    public async Task<ApiDocuments.ResolveDocumentFoldersResponse> SendAsync(ApiDocuments.ResolveDocumentFoldersRequest request, CancellationToken token)
    {
        using var rpc = client.ResolveDocumentFoldersAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class OrgUnitWorkflowEndpointRequests
{
    /// <summary>
    /// Builds the BrowseOrgUnits request from the current workflow state.
    /// </summary>
    public static ApiEmployees.BrowseOrgUnitsRequest RetryOrgUnitWorkflowToBrowseOrgUnits0(this ApiEmployees.RetryOrgUnitWorkflowRequest request)
    {
        return new ApiEmployees.BrowseOrgUnitsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.BrowseOrgUnitsRequest.Types.Filter
            {
                CreatedAfter = (request.OrgUnit?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (request.Details?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.OrgUnit?.Manager?.UserId ?? ""),
                Region = (request.OrgUnit?.Location?.Region ?? ""),
            },
        };
    }
}
