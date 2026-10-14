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

namespace Acme.Workforce.Helpdesk;

public static class KnowledgeArticleLifecycleEndpointRequests
{
    /// <summary>
    /// Builds the AdvanceEntitlementWorkflow request from the current workflow state.
    /// </summary>
    public static ApiLicensing.AdvanceEntitlementWorkflowRequest ActivateKnowledgeArticleToAdvanceEntitlementWorkflow0(this ApiHelpdesk.ActivateKnowledgeArticleRequest request)
    {
        return new ApiLicensing.AdvanceEntitlementWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.AdvanceEntitlementWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                },
            },
        };
    }
}
