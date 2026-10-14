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
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;

namespace Acme.Workforce.Benefits;

/// <summary>
/// The application port for CreateBenefitPlan.
/// </summary>
public interface IEnrollmentApprovalEndpointSendListPendingEnrollments0Port
{
    Task<ApiBenefits.CreateBenefitPlanResponse> SendAsync(ApiBenefits.CreateBenefitPlanRequest request, CancellationToken token);
}

public sealed class EnrollmentApprovalEndpointSendListPendingEnrollments0PortAdapter(ApiBenefits.BenefitPlanService.BenefitPlanServiceClient client) : IEnrollmentApprovalEndpointSendListPendingEnrollments0Port
{
    public async Task<ApiBenefits.CreateBenefitPlanResponse> SendAsync(ApiBenefits.CreateBenefitPlanRequest request, CancellationToken token)
    {
        using var rpc = client.CreateBenefitPlanAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
