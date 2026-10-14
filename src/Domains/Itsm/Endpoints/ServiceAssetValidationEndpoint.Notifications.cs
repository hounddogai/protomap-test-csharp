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
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPerformers = Acme.Performers.V1;

namespace Acme.Workforce.Itsm;

public sealed class ServiceAssetValidationEndpointLintServiceAssets2NotificationHandler(ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient client) : INotificationHandler
{
    public string Topic => "acme.itsm.v1.ServiceAssetValidationService/LintServiceAssets/2";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiLicensing.ScoreSoftwareLicenseRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.ScoreSoftwareLicenseAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
