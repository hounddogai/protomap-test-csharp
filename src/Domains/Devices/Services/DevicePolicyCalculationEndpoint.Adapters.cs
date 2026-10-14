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
using ApiConcessions = Acme.Concessions.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;

namespace Acme.Workforce.Devices;

public static class DevicePolicyCalculationEndpointRequests
{
    /// <summary>
    /// Builds the ExplainManagedDeviceCalculation request from the current workflow state.
    /// </summary>
    public static ApiDevices.ExplainManagedDeviceCalculationRequest ExplainDevicePolicyCalculationToExplainManagedDeviceCalculation2(this ApiDevices.ExplainDevicePolicyCalculationRequest request, ApiDevices.EstimateManagedDeviceResponse reply1)
    {
        return new ApiDevices.ExplainManagedDeviceCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.ManagedDevice?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply1.ManagedDevice?.Owner?.Email ?? ""),
                },
            },
        };
    }
}
