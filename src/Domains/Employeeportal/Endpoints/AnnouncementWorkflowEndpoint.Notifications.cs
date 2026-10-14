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
using ApiDriverapp = Acme.Driverapp.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFanportal = Acme.Fanportal.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiSearch = Acme.Search.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Employeeportal;

public sealed class AnnouncementWorkflowEndpointCancelAnnouncementWorkflow1NotificationHandler(ApiLicensing.SoftwareLicenseWorkflowService.SoftwareLicenseWorkflowServiceClient client) : INotificationHandler
{
    public string Topic => "acme.employeeportal.v1.AnnouncementWorkflowService/CancelAnnouncementWorkflow/1";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiLicensing.PauseSoftwareLicenseWorkflowRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.PauseSoftwareLicenseWorkflowAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
