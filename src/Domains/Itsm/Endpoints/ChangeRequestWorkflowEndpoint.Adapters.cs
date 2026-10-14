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
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPayroll = Acme.Payroll.V1;

namespace Acme.Workforce.Itsm;

public static class ChangeRequestWorkflowEndpointRequests
{
    /// <summary>
    /// Builds the EscalateServiceAsset request from the current workflow state.
    /// </summary>
    public static ApiItsm.EscalateServiceAssetRequest RetryChangeRequestWorkflowToEscalateServiceAsset0(this ApiItsm.RetryChangeRequestWorkflowRequest request)
    {
        return new ApiItsm.EscalateServiceAssetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.EscalateServiceAssetRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (request.ChangeRequest?.Assignee?.DisplayName ?? ""),
                },
            },
        };
    }
}
