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
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Benefits;

public static class DependentApprovalEndpointRequests
{
    /// <summary>
    /// Builds the ResumeBenefitPlanWorkflow request from the current workflow state.
    /// </summary>
    public static ApiBenefits.ResumeBenefitPlanWorkflowRequest SubmitDependentToResumeBenefitPlanWorkflow1(this ApiBenefits.SubmitDependentRequest request)
    {
        return new ApiBenefits.ResumeBenefitPlanWorkflowRequest
        {
            BenefitPlan = new ApiBenefits.BenefitPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Dependent?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Dependent?.Contact?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.ResumeBenefitPlanWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Dependent?.RelationshipManager?.Email ?? ""),
                },
            },
        };
    }
}
