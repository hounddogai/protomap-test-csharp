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
using ApiCompliance = Acme.Compliance.V1;
using ApiConcessions = Acme.Concessions.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Corporateevents;

/// <summary>
/// The application port for CalculateDocumentTemplate.
/// </summary>
public interface IVenueLifecycleEndpointSendRenewVenue0Port
{
    Task<ApiDocuments.CalculateDocumentTemplateResponse> SendAsync(ApiDocuments.CalculateDocumentTemplateRequest request, CancellationToken token);
}

public sealed class VenueLifecycleEndpointSendRenewVenue0PortAdapter(ApiDocuments.DocumentTemplateCalculationService.DocumentTemplateCalculationServiceClient client) : IVenueLifecycleEndpointSendRenewVenue0Port
{
    public async Task<ApiDocuments.CalculateDocumentTemplateResponse> SendAsync(ApiDocuments.CalculateDocumentTemplateRequest request, CancellationToken token)
    {
        using var rpc = client.CalculateDocumentTemplateAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
