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
using ApiDevices = Acme.Devices.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Learning;

public sealed class CourseEndpointBatchCreateCourses0NotificationHandler(ApiLearning.CourseQueryService.CourseQueryServiceClient client) : INotificationHandler
{
    public string Topic => "acme.learning.v1.CourseService/BatchCreateCourses/0";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiLearning.FindCoursesRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.FindCoursesAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
