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
using ApiFraud = Acme.Fraud.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Recruiting;

/// <summary>
/// The application port for SummarizeOrgUnits.
/// </summary>
public interface IOfferLifecycleEndpointSendTerminateOffer2Port
{
    Task<ApiEmployees.SummarizeOrgUnitsResponse> SendAsync(ApiEmployees.SummarizeOrgUnitsRequest request, CancellationToken token);
}

public sealed class OfferLifecycleEndpointSendTerminateOffer2PortAdapter(ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient client) : IOfferLifecycleEndpointSendTerminateOffer2Port
{
    public async Task<ApiEmployees.SummarizeOrgUnitsResponse> SendAsync(ApiEmployees.SummarizeOrgUnitsRequest request, CancellationToken token)
    {
        using var rpc = client.SummarizeOrgUnitsAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class OfferLifecycleEndpointRequests
{
    /// <summary>
    /// Builds the ListDocumentFolderExports request from the current workflow state.
    /// </summary>
    public static ApiDocuments.ListDocumentFolderExportsRequest TerminateOfferToListDocumentFolderExports1(this ApiRecruiting.TerminateOfferRequest request, ApiIdentity.PreviewUserResponse reply0)
    {
        return new ApiDocuments.ListDocumentFolderExportsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentFolderExportsRequest.Types.Filter
            {
                CreatedAfter = (reply0.User?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.User?.RelationshipManager?.UserId ?? ""),
                Region = (reply0.User?.PrimaryAddress?.Region ?? ""),
            },
        };
    }
}
