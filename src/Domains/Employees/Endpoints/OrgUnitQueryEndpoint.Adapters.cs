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
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Employees;

/// <summary>
/// The application port for LintPositions.
/// </summary>
public interface IOrgUnitQueryEndpointSendFindOrgUnits0Port
{
    Task<ApiEmployees.LintPositionsResponse> SendAsync(ApiEmployees.LintPositionsRequest request, CancellationToken token);
}

public sealed class OrgUnitQueryEndpointSendFindOrgUnits0PortAdapter(ApiEmployees.PositionValidationService.PositionValidationServiceClient client) : IOrgUnitQueryEndpointSendFindOrgUnits0Port
{
    public async Task<ApiEmployees.LintPositionsResponse> SendAsync(ApiEmployees.LintPositionsRequest request, CancellationToken token)
    {
        using var rpc = client.LintPositionsAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class OrgUnitQueryEndpointRequests
{
    /// <summary>
    /// Builds the CheckPositionEligibility request from the current workflow state.
    /// </summary>
    public static ApiEmployees.CheckPositionEligibilityRequest ExportOrgUnitsToCheckPositionEligibility0(this ApiEmployees.ExportOrgUnitsRequest request)
    {
        return new ApiEmployees.CheckPositionEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
    }
}
