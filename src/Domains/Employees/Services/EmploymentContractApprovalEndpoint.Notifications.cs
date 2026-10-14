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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMaintenance = Acme.Maintenance.V1;
using ApiPromotions = Acme.Promotions.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Employees;

public sealed class EmploymentContractApprovalEndpointSubmitEmploymentContract2NotificationHandler(ApiEmployees.EmployeeApprovalService.EmployeeApprovalServiceClient client) : INotificationHandler
{
    public string Topic => "acme.employees.v1.EmploymentContractApprovalService/SubmitEmploymentContract/2";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiEmployees.RecallEmployeeRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.RecallEmployeeAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
