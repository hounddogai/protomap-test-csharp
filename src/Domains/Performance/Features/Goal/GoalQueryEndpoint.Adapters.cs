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
using ApiContent = Acme.Content.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLineage = Acme.Lineage.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Performance;

/// <summary>
/// The application port for CalculateVisitorPass.
/// </summary>
public interface IGoalQueryEndpointSendLookupGoal0Port
{
    Task<ApiWorkplaces.CalculateVisitorPassResponse> SendAsync(ApiWorkplaces.CalculateVisitorPassRequest request, CancellationToken token);
}

public sealed class GoalQueryEndpointSendLookupGoal0PortAdapter(ApiWorkplaces.VisitorPassCalculationService.VisitorPassCalculationServiceClient client) : IGoalQueryEndpointSendLookupGoal0Port
{
    public async Task<ApiWorkplaces.CalculateVisitorPassResponse> SendAsync(ApiWorkplaces.CalculateVisitorPassRequest request, CancellationToken token)
    {
        using var rpc = client.CalculateVisitorPassAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
