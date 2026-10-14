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
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMerchandising = Acme.Merchandising.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWarehousing = Acme.Warehousing.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// The application port for PreviewEmploymentContract.
/// </summary>
public interface IItineraryReportingEndpointSendStreamItineraryMetrics0Port
{
    Task<ApiEmployees.PreviewEmploymentContractResponse> SendAsync(ApiEmployees.PreviewEmploymentContractRequest request, CancellationToken token);
}

public sealed class ItineraryReportingEndpointSendStreamItineraryMetrics0PortAdapter(ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient client) : IItineraryReportingEndpointSendStreamItineraryMetrics0Port
{
    public async Task<ApiEmployees.PreviewEmploymentContractResponse> SendAsync(ApiEmployees.PreviewEmploymentContractRequest request, CancellationToken token)
    {
        using var rpc = client.PreviewEmploymentContractAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class ItineraryReportingEndpointRequests
{
    /// <summary>
    /// Builds the BatchGetCollections request from the current workflow state.
    /// </summary>
    public static ApiMerchandising.BatchGetCollectionsRequest ShareItineraryReportToBatchGetCollections0(this ApiTravel.ShareItineraryReportRequest request)
    {
        return new ApiMerchandising.BatchGetCollectionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
    }
}
