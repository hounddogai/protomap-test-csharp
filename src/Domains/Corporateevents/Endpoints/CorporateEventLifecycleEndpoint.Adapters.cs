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
using ApiCertificates = Acme.Certificates.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiVenues = Acme.Venues.V1;
using ApiWarehousing = Acme.Warehousing.V1;

namespace Acme.Workforce.Corporateevents;

/// <summary>
/// The application port for LintCorporateEvents.
/// </summary>
public interface ICorporateEventLifecycleEndpointSendExpireCorporateEvent2Port
{
    Task<ApiCorporateevents.LintCorporateEventsResponse> SendAsync(ApiCorporateevents.LintCorporateEventsRequest request, CancellationToken token);
}

public sealed class CorporateEventLifecycleEndpointSendExpireCorporateEvent2PortAdapter(ApiCorporateevents.CorporateEventValidationService.CorporateEventValidationServiceClient client) : ICorporateEventLifecycleEndpointSendExpireCorporateEvent2Port
{
    public async Task<ApiCorporateevents.LintCorporateEventsResponse> SendAsync(ApiCorporateevents.LintCorporateEventsRequest request, CancellationToken token)
    {
        using var rpc = client.LintCorporateEventsAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class CorporateEventLifecycleEndpointRequests
{
    /// <summary>
    /// Builds the BrowseCertificates request from the current workflow state.
    /// </summary>
    public static ApiCertificates.BrowseCertificatesRequest ExpireCorporateEventToBrowseCertificates1(this ApiCorporateevents.ExpireCorporateEventRequest request, ApiCorporateevents.GenerateRegistrationReportResponse reply0)
    {
        return new ApiCertificates.BrowseCertificatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiCertificates.BrowseCertificatesRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.ReportId ?? ""),
            },
        };
    }
}
