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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFleet = Acme.Fleet.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Helpdesk;

/// <summary>
/// The application port for SuggestSoftwareLicenses.
/// </summary>
public interface ISlaPolicyValidationEndpointSendPreviewSlaPolicy3Port
{
    Task<ApiLicensing.SuggestSoftwareLicensesResponse> SendAsync(ApiLicensing.SuggestSoftwareLicensesRequest request, CancellationToken token);
}

public sealed class SlaPolicyValidationEndpointSendPreviewSlaPolicy3PortAdapter(ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient client) : ISlaPolicyValidationEndpointSendPreviewSlaPolicy3Port
{
    public async Task<ApiLicensing.SuggestSoftwareLicensesResponse> SendAsync(ApiLicensing.SuggestSoftwareLicensesRequest request, CancellationToken token)
    {
        using var rpc = client.SuggestSoftwareLicensesAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
