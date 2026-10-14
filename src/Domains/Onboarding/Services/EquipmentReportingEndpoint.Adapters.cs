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
using ApiDevices = Acme.Devices.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiOnboarding = Acme.Onboarding.V1;

namespace Acme.Workforce.Onboarding;

/// <summary>
/// The application port for ApproveDevicePolicy.
/// </summary>
public interface IEquipmentReportingEndpointSendGenerateEquipmentReport2Port
{
    Task<ApiDevices.ApproveDevicePolicyResponse> SendAsync(ApiDevices.ApproveDevicePolicyRequest request, CancellationToken token);
}

public sealed class EquipmentReportingEndpointSendGenerateEquipmentReport2PortAdapter(ApiDevices.DevicePolicyApprovalService.DevicePolicyApprovalServiceClient client) : IEquipmentReportingEndpointSendGenerateEquipmentReport2Port
{
    public async Task<ApiDevices.ApproveDevicePolicyResponse> SendAsync(ApiDevices.ApproveDevicePolicyRequest request, CancellationToken token)
    {
        using var rpc = client.ApproveDevicePolicyAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class EquipmentReportingEndpointRequests
{
    /// <summary>
    /// Builds the RecallRegistration request from the current workflow state.
    /// </summary>
    public static ApiCorporateevents.RecallRegistrationRequest GenerateEquipmentReportToRecallRegistration1(this ApiOnboarding.GenerateEquipmentReportRequest request, ApiOnboarding.ExplainEquipmentDecisionResponse reply0)
    {
        return new ApiCorporateevents.RecallRegistrationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Registration = new ApiCorporateevents.Registration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Equipment?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
    }
}
