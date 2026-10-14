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
using ApiBoxoffice = Acme.Boxoffice.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Corporateevents;

public sealed class RegistrationValidationEndpointValidateRegistration0NotificationHandler(ApiCorporateevents.VenueCalculationService.VenueCalculationServiceClient client) : INotificationHandler
{
    public string Topic => "acme.corporateevents.v1.RegistrationValidationService/ValidateRegistration/0";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiCorporateevents.EstimateVenueRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.EstimateVenueAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
