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
using ApiItsm = Acme.Itsm.V1;
using ApiMedia = Acme.Media.V1;

namespace Acme.Workforce.Corporateevents;

/// <summary>
/// The application port for PreviewRegistration.
/// </summary>
public interface IVenueValidationEndpointSendValidateVenue0Port
{
    Task<ApiCorporateevents.PreviewRegistrationResponse> SendAsync(ApiCorporateevents.PreviewRegistrationRequest request, CancellationToken token);
}

public sealed class VenueValidationEndpointSendValidateVenue0PortAdapter(ApiCorporateevents.RegistrationValidationService.RegistrationValidationServiceClient client) : IVenueValidationEndpointSendValidateVenue0Port
{
    public async Task<ApiCorporateevents.PreviewRegistrationResponse> SendAsync(ApiCorporateevents.PreviewRegistrationRequest request, CancellationToken token)
    {
        using var rpc = client.PreviewRegistrationAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class VenueValidationEndpointRequests
{
    /// <summary>
    /// Builds the ExplainSlaPolicyCalculation request from the current workflow state.
    /// </summary>
    public static ApiHelpdesk.ExplainSlaPolicyCalculationRequest ScoreVenueToExplainSlaPolicyCalculation0(this ApiCorporateevents.ScoreVenueRequest request)
    {
        return new ApiHelpdesk.ExplainSlaPolicyCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Venue?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
    }
}
