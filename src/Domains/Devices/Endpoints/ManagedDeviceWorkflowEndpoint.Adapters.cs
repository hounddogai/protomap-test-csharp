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
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Devices;

public static class ManagedDeviceWorkflowEndpointRequests
{
    /// <summary>
    /// Builds the ScoreDevicePolicy request from the current workflow state.
    /// </summary>
    public static ApiDevices.ScoreDevicePolicyRequest ResumeManagedDeviceWorkflowToScoreDevicePolicy1(this ApiDevices.ResumeManagedDeviceWorkflowRequest request, ApiDocuments.ValidateSignatureResponse reply0)
    {
        return new ApiDevices.ScoreDevicePolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Signature?.Subject?.DisplayName ?? ""),
                },
            },
        };
    }
}
