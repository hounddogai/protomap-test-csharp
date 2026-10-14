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
using ApiAdmissions = Acme.Admissions.V1;
using ApiAudiences = Acme.Audiences.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiPrescriptions = Acme.Prescriptions.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiSearch = Acme.Search.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Scheduling;

/// <summary>
/// The application port for NormalizeChangeRequest.
/// </summary>
public interface IRosterCalculationEndpointSendQuoteRoster2Port
{
    Task<ApiItsm.NormalizeChangeRequestResponse> SendAsync(ApiItsm.NormalizeChangeRequestRequest request, CancellationToken token);
}

public sealed class RosterCalculationEndpointSendQuoteRoster2PortAdapter(ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient client) : IRosterCalculationEndpointSendQuoteRoster2Port
{
    public async Task<ApiItsm.NormalizeChangeRequestResponse> SendAsync(ApiItsm.NormalizeChangeRequestRequest request, CancellationToken token)
    {
        using var rpc = client.NormalizeChangeRequestAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class RosterCalculationEndpointRequests
{
    /// <summary>
    /// Builds the QuoteDesk request from the current workflow state.
    /// </summary>
    public static ApiWorkplaces.QuoteDeskRequest QuoteRosterToQuoteDesk1(this ApiScheduling.QuoteRosterRequest request, ApiDocuments.SendDocumentTemplateTestNoticeResponse reply0)
    {
        return new ApiWorkplaces.QuoteDeskRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Desk = new ApiWorkplaces.Desk
            {
                Address = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                    Locality = (reply0.DocumentTemplate?.BillingAddress?.Locality ?? ""),
                    Recipient = (reply0.DocumentTemplate?.BillingAddress?.Recipient ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Roster?.Counterparty?.Email ?? ""),
                },
            },
        };
    }
}
