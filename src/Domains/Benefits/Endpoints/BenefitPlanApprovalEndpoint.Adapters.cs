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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFuel = Acme.Fuel.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Benefits;

/// <summary>
/// The application port for LintDependents.
/// </summary>
public interface IBenefitPlanApprovalEndpointSendEscalateBenefitPlan3Port
{
    Task<ApiBenefits.LintDependentsResponse> SendAsync(ApiBenefits.LintDependentsRequest request, CancellationToken token);
}

public sealed class BenefitPlanApprovalEndpointSendEscalateBenefitPlan3PortAdapter(ApiBenefits.DependentValidationService.DependentValidationServiceClient client) : IBenefitPlanApprovalEndpointSendEscalateBenefitPlan3Port
{
    public async Task<ApiBenefits.LintDependentsResponse> SendAsync(ApiBenefits.LintDependentsRequest request, CancellationToken token)
    {
        using var rpc = client.LintDependentsAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
