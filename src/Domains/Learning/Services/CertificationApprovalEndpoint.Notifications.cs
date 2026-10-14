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
using ApiDeliveries = Acme.Deliveries.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLabeling = Acme.Labeling.V1;
using ApiLearning = Acme.Learning.V1;
using ApiMedia = Acme.Media.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Learning;

public sealed class CertificationApprovalEndpointSubmitCertification3NotificationHandler(ApiLearning.CourseApprovalService.CourseApprovalServiceClient client) : INotificationHandler
{
    public string Topic => "acme.learning.v1.CertificationApprovalService/SubmitCertification/3";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiLearning.ApproveCourseRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.ApproveCourseAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
