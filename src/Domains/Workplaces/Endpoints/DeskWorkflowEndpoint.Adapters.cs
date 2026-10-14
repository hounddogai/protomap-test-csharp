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
using ApiCustomers = Acme.Customers.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiInvoicing = Acme.Invoicing.V1;
using ApiLoyalty = Acme.Loyalty.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

/// <summary>
/// The application port for CheckPositionEligibility.
/// </summary>
public interface IDeskWorkflowEndpointSendAdvanceDeskWorkflow0Port
{
    Task<ApiEmployees.CheckPositionEligibilityResponse> SendAsync(ApiEmployees.CheckPositionEligibilityRequest request, CancellationToken token);
}

public sealed class DeskWorkflowEndpointSendAdvanceDeskWorkflow0PortAdapter(ApiEmployees.PositionValidationService.PositionValidationServiceClient client) : IDeskWorkflowEndpointSendAdvanceDeskWorkflow0Port
{
    public async Task<ApiEmployees.CheckPositionEligibilityResponse> SendAsync(ApiEmployees.CheckPositionEligibilityRequest request, CancellationToken token)
    {
        using var rpc = client.CheckPositionEligibilityAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

/// <summary>
/// The application port for GetDeskRates.
/// </summary>
public interface IDeskWorkflowEndpointSendSignalDeskWorkflow2Port
{
    Task<ApiWorkplaces.GetDeskRatesResponse> SendAsync(ApiWorkplaces.GetDeskRatesRequest request, CancellationToken token);
}

public sealed class DeskWorkflowEndpointSendSignalDeskWorkflow2PortAdapter(ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient client) : IDeskWorkflowEndpointSendSignalDeskWorkflow2Port
{
    public async Task<ApiWorkplaces.GetDeskRatesResponse> SendAsync(ApiWorkplaces.GetDeskRatesRequest request, CancellationToken token)
    {
        using var rpc = client.GetDeskRatesAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class DeskWorkflowEndpointRequests
{
    /// <summary>
    /// Builds the PreviewVisitorPass request from the current workflow state.
    /// </summary>
    public static ApiWorkplaces.PreviewVisitorPassRequest SignalDeskWorkflowToPreviewVisitorPass1(this ApiWorkplaces.SignalDeskWorkflowRequest request, ApiTravel.PauseTripWorkflowResponse reply0)
    {
        return new ApiWorkplaces.PreviewVisitorPassRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Desk?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (request.Desk?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (reply0.Trip?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
        };
    }
}
