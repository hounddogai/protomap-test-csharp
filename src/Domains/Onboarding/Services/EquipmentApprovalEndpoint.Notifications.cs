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
using ApiAudit = Acme.Audit.V1;
using ApiCommon = Acme.Common.V1;
using ApiConfiguration = Acme.Configuration.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiPromotions = Acme.Promotions.V1;
using ApiSurveys = Acme.Surveys.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Onboarding;

public sealed class EquipmentApprovalEndpointGetEquipmentApprovalStatus0NotificationHandler(ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient client) : INotificationHandler
{
    public string Topic => "acme.onboarding.v1.EquipmentApprovalService/GetEquipmentApprovalStatus/0";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiAudit.LintAuditEventsRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.LintAuditEventsAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
