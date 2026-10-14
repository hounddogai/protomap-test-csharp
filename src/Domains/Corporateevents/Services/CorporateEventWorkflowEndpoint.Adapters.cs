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
using ApiCareplans = Acme.Careplans.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLocalization = Acme.Localization.V1;
using ApiSellers = Acme.Sellers.V1;

namespace Acme.Workforce.Corporateevents;

/// <summary>
/// The application port for EscalateServiceAsset.
/// </summary>
public interface ICorporateEventWorkflowEndpointSendAdvanceCorporateEventWorkflow1Port
{
    Task<ApiItsm.EscalateServiceAssetResponse> SendAsync(ApiItsm.EscalateServiceAssetRequest request, CancellationToken token);
}

public sealed class CorporateEventWorkflowEndpointSendAdvanceCorporateEventWorkflow1PortAdapter(ApiItsm.ServiceAssetApprovalService.ServiceAssetApprovalServiceClient client) : ICorporateEventWorkflowEndpointSendAdvanceCorporateEventWorkflow1Port
{
    public async Task<ApiItsm.EscalateServiceAssetResponse> SendAsync(ApiItsm.EscalateServiceAssetRequest request, CancellationToken token)
    {
        using var rpc = client.EscalateServiceAssetAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class CorporateEventWorkflowEndpointRequests
{
    /// <summary>
    /// Builds the CompareSellerOptions request from the current workflow state.
    /// </summary>
    public static ApiSellers.CompareSellerOptionsRequest RetryCorporateEventWorkflowToCompareSellerOptions3(this ApiCorporateevents.RetryCorporateEventWorkflowRequest request, ApiCorporateevents.DelegateRegistrationApprovalResponse reply0, ApiDocuments.RecordSignatureVersionResponse reply1)
    {
        return new ApiSellers.CompareSellerOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Seller = new ApiSellers.Seller
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.CorporateEvent?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (request.CorporateEvent?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (reply0.Registration?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.Signature?.Subject?.DisplayName ?? ""),
                },
            },
        };
    }
}
