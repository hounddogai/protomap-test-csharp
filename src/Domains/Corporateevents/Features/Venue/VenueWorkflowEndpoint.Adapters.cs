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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPersonalization = Acme.Personalization.V1;

namespace Acme.Workforce.Corporateevents;

/// <summary>
/// The application port for ScheduleSignatureExport.
/// </summary>
public interface IVenueWorkflowEndpointSendSignalVenueWorkflow0Port
{
    Task<ApiDocuments.ScheduleSignatureExportResponse> SendAsync(ApiDocuments.ScheduleSignatureExportRequest request, CancellationToken token);
}

public sealed class VenueWorkflowEndpointSendSignalVenueWorkflow0PortAdapter(ApiDocuments.SignatureExportService.SignatureExportServiceClient client) : IVenueWorkflowEndpointSendSignalVenueWorkflow0Port
{
    public async Task<ApiDocuments.ScheduleSignatureExportResponse> SendAsync(ApiDocuments.ScheduleSignatureExportRequest request, CancellationToken token)
    {
        using var rpc = client.ScheduleSignatureExportAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class VenueWorkflowEndpointRequests
{
    /// <summary>
    /// Builds the RejectCorporateEvent request from the current workflow state.
    /// </summary>
    public static ApiCorporateevents.RejectCorporateEventRequest RetryVenueWorkflowToRejectCorporateEvent0(this ApiCorporateevents.RetryVenueWorkflowRequest request)
    {
        return new ApiCorporateevents.RejectCorporateEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Venue?.Audit?.Etag ?? ""),
                },
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Venue?.Owner?.Email ?? ""),
                        PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                    },
                } },
            },
        };
    }
}
