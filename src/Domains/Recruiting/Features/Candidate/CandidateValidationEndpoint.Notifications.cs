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
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Recruiting;

public sealed class CandidateValidationEndpointCheckCandidateEligibility0NotificationHandler(ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient client) : INotificationHandler
{
    public string Topic => "acme.recruiting.v1.CandidateValidationService/CheckCandidateEligibility/0";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiAuthorization.CountAccessPoliciesRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.CountAccessPoliciesAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
