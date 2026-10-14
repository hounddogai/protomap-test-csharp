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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiGiftcards = Acme.Giftcards.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPricing = Acme.Pricing.V1;
using ApiReturns = Acme.Returns.V1;

namespace Acme.Workforce.Itsm;

/// <summary>
/// The application port for RejectManagedDevice.
/// </summary>
public interface IProblemReportingEndpointSendStreamProblemMetrics0Port
{
    Task<ApiDevices.RejectManagedDeviceResponse> SendAsync(ApiDevices.RejectManagedDeviceRequest request, CancellationToken token);
}

public sealed class ProblemReportingEndpointSendStreamProblemMetrics0PortAdapter(ApiDevices.ManagedDeviceApprovalService.ManagedDeviceApprovalServiceClient client) : IProblemReportingEndpointSendStreamProblemMetrics0Port
{
    public async Task<ApiDevices.RejectManagedDeviceResponse> SendAsync(ApiDevices.RejectManagedDeviceRequest request, CancellationToken token)
    {
        using var rpc = client.RejectManagedDeviceAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class ProblemReportingEndpointRequests
{
    /// <summary>
    /// Builds the NormalizeIncident request from the current workflow state.
    /// </summary>
    public static ApiItsm.NormalizeIncidentRequest ShareProblemReportToNormalizeIncident3(this ApiItsm.ShareProblemReportRequest request, ApiDocuments.ValidateDocumentFolderResponse reply0, ApiGiftcards.CreateGiftCardResponse reply2)
    {
        return new ApiItsm.NormalizeIncidentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Incident = new ApiItsm.Incident
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Recipient?.Email ?? ""),
                },
                Customer = new ApiEmployees.EmployeeRef
                {
                    Email = (reply2.GiftCard?.Contact?.Email ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply0.DocumentFolder?.Manager?.Email ?? ""),
                },
            },
        };
    }
}
