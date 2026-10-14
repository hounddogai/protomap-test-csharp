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
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiPayments = Acme.Payments.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiRisk = Acme.Risk.V1;

namespace Acme.Workforce.Benefits;

/// <summary>
/// The application port for PeekBenefitPlan.
/// </summary>
public interface IBenefitPlanWorkflowEndpointSendStartBenefitPlanWorkflow0Port
{
    Task<ApiBenefits.PeekBenefitPlanResponse> SendAsync(ApiBenefits.PeekBenefitPlanRequest request, CancellationToken token);
}

public sealed class BenefitPlanWorkflowEndpointSendStartBenefitPlanWorkflow0PortAdapter(ApiBenefits.BenefitPlanQueryService.BenefitPlanQueryServiceClient client) : IBenefitPlanWorkflowEndpointSendStartBenefitPlanWorkflow0Port
{
    public async Task<ApiBenefits.PeekBenefitPlanResponse> SendAsync(ApiBenefits.PeekBenefitPlanRequest request, CancellationToken token)
    {
        using var rpc = client.PeekBenefitPlanAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
