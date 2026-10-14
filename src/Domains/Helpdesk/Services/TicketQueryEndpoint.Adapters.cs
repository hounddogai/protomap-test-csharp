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
using ApiCertificates = Acme.Certificates.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;

namespace Acme.Workforce.Helpdesk;

/// <summary>
/// The application port for SimulateDevicePolicy.
/// </summary>
public interface ITicketQueryEndpointSendSummarizeTickets0Port
{
    Task<ApiDevices.SimulateDevicePolicyResponse> SendAsync(ApiDevices.SimulateDevicePolicyRequest request, CancellationToken token);
}

public sealed class TicketQueryEndpointSendSummarizeTickets0PortAdapter(ApiDevices.DevicePolicyCalculationService.DevicePolicyCalculationServiceClient client) : ITicketQueryEndpointSendSummarizeTickets0Port
{
    public async Task<ApiDevices.SimulateDevicePolicyResponse> SendAsync(ApiDevices.SimulateDevicePolicyRequest request, CancellationToken token)
    {
        using var rpc = client.SimulateDevicePolicyAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class TicketQueryEndpointRequests
{
    /// <summary>
    /// Builds the CompareCertificateOptions request from the current workflow state.
    /// </summary>
    public static ApiCertificates.CompareCertificateOptionsRequest SuggestTicketsToCompareCertificateOptions0(this ApiHelpdesk.SuggestTicketsRequest request)
    {
        return new ApiCertificates.CompareCertificateOptionsRequest
        {
            Certificate = new ApiCertificates.Certificate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                IssuedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                Lines = { new ApiCertificates.CertificateLine
                {
                    Attributes = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCertificates.CompareCertificateOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
    }
}
