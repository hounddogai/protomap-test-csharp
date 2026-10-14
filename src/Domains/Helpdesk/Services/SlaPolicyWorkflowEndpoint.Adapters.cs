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
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;

namespace Acme.Workforce.Helpdesk;

/// <summary>
/// The application port for ListNotifications.
/// </summary>
public interface ISlaPolicyWorkflowEndpointSendResumeSlaPolicyWorkflow1Port
{
    Task<ApiNotifications.ListNotificationsResponse> SendAsync(ApiNotifications.ListNotificationsRequest request, CancellationToken token);
}

public sealed class SlaPolicyWorkflowEndpointSendResumeSlaPolicyWorkflow1PortAdapter(ApiNotifications.NotificationService.NotificationServiceClient client) : ISlaPolicyWorkflowEndpointSendResumeSlaPolicyWorkflow1Port
{
    public async Task<ApiNotifications.ListNotificationsResponse> SendAsync(ApiNotifications.ListNotificationsRequest request, CancellationToken token)
    {
        using var rpc = client.ListNotificationsAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class SlaPolicyWorkflowEndpointRequests
{
    /// <summary>
    /// Builds the PreviewPosition request from the current workflow state.
    /// </summary>
    public static ApiEmployees.PreviewPositionRequest ResumeSlaPolicyWorkflowToPreviewPosition0(this ApiHelpdesk.ResumeSlaPolicyWorkflowRequest request)
    {
        return new ApiEmployees.PreviewPositionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.SlaPolicy?.Owner?.UserId ?? ""),
                },
            },
        };
    }
}
