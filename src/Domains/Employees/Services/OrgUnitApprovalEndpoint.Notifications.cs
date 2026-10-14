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
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiExperiments = Acme.Experiments.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLoyalty = Acme.Loyalty.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Employees;

public sealed class OrgUnitApprovalEndpointCommentOrgUnitApproval0NotificationHandler(ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient client) : INotificationHandler
{
    public string Topic => "acme.employees.v1.OrgUnitApprovalService/CommentOrgUnitApproval/0";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiEmployees.PreviewEmploymentContractRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.PreviewEmploymentContractAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
