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
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Licensing;

public sealed class EntitlementApprovalEndpointSubmitEntitlement0NotificationHandler(ApiRecruiting.OfferValidationService.OfferValidationServiceClient client) : INotificationHandler
{
    public string Topic => "acme.licensing.v1.EntitlementApprovalService/SubmitEntitlement/0";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiRecruiting.LintOffersRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.LintOffersAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
