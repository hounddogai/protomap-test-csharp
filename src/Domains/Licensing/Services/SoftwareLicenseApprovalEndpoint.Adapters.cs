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
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiTelematics = Acme.Telematics.V1;

namespace Acme.Workforce.Licensing;

/// <summary>
/// The application port for CreateUser.
/// </summary>
public interface ISoftwareLicenseApprovalEndpointSendRecallSoftwareLicense2Port
{
    Task<ApiIdentity.CreateUserResponse> SendAsync(ApiIdentity.CreateUserRequest request, CancellationToken token);
}

public sealed class SoftwareLicenseApprovalEndpointSendRecallSoftwareLicense2PortAdapter(ApiIdentity.UserService.UserServiceClient client) : ISoftwareLicenseApprovalEndpointSendRecallSoftwareLicense2Port
{
    public async Task<ApiIdentity.CreateUserResponse> SendAsync(ApiIdentity.CreateUserRequest request, CancellationToken token)
    {
        using var rpc = client.CreateUserAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class SoftwareLicenseApprovalEndpointRequests
{
    /// <summary>
    /// Builds the SuspendLicenseAssignment request from the current workflow state.
    /// </summary>
    public static ApiLicensing.SuspendLicenseAssignmentRequest RecallSoftwareLicenseToSuspendLicenseAssignment1(this ApiLicensing.RecallSoftwareLicenseRequest request, ApiLicensing.CompareEntitlementOptionsResponse reply0)
    {
        return new ApiLicensing.SuspendLicenseAssignmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.SuspendLicenseAssignmentRequest.Types.Details
            {
                NewOwner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Entitlement?.Owner?.DisplayName ?? ""),
                },
                NotifyContact = new ApiCommon.ContactInfo
                {
                    Name = (request.SoftwareLicense?.Contact?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (request.SoftwareLicense?.Contact?.PreferredChannel ?? ""),
                },
            },
            LicenseAssignmentId = (request.SoftwareLicense?.Holder?.LicenseAssignmentId ?? ""),
        };
    }
}
