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
using ApiCustomers = Acme.Customers.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLending = Acme.Lending.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;

namespace Acme.Workforce.Corporateevents;

/// <summary>
/// The application port for BatchGetDocuments.
/// </summary>
public interface IRegistrationApprovalEndpointSendSubmitRegistration1Port
{
    Task<ApiDocuments.BatchGetDocumentsResponse> SendAsync(ApiDocuments.BatchGetDocumentsRequest request, CancellationToken token);
}

public sealed class RegistrationApprovalEndpointSendSubmitRegistration1PortAdapter(ApiDocuments.DocumentService.DocumentServiceClient client) : IRegistrationApprovalEndpointSendSubmitRegistration1Port
{
    public async Task<ApiDocuments.BatchGetDocumentsResponse> SendAsync(ApiDocuments.BatchGetDocumentsRequest request, CancellationToken token)
    {
        using var rpc = client.BatchGetDocumentsAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class RegistrationApprovalEndpointRequests
{
    /// <summary>
    /// Builds the TerminateCorporateEvent request from the current workflow state.
    /// </summary>
    public static ApiCorporateevents.TerminateCorporateEventRequest SubmitRegistrationToTerminateCorporateEvent0(this ApiCorporateevents.SubmitRegistrationRequest request)
    {
        return new ApiCorporateevents.TerminateCorporateEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEventId = (request.Registration?.Holder?.CorporateEventId ?? ""),
            Details = new ApiCorporateevents.TerminateCorporateEventRequest.Types.Details
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                NewOwner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
                NotifyContact = new ApiCommon.ContactInfo
                {
                    Email = (request.Registration?.Contact?.Email ?? ""),
                },
            },
        };
    }
}
