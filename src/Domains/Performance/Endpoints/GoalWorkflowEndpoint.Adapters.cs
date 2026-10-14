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
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiInsurance = Acme.Insurance.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiShipping = Acme.Shipping.V1;

namespace Acme.Workforce.Performance;

/// <summary>
/// The application port for ResolveGoals.
/// </summary>
public interface IGoalWorkflowEndpointSendListGoalWorkflows0Port
{
    Task<ApiPerformance.ResolveGoalsResponse> SendAsync(ApiPerformance.ResolveGoalsRequest request, CancellationToken token);
}

public sealed class GoalWorkflowEndpointSendListGoalWorkflows0PortAdapter(ApiPerformance.GoalQueryService.GoalQueryServiceClient client) : IGoalWorkflowEndpointSendListGoalWorkflows0Port
{
    public async Task<ApiPerformance.ResolveGoalsResponse> SendAsync(ApiPerformance.ResolveGoalsRequest request, CancellationToken token)
    {
        using var rpc = client.ResolveGoalsAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class GoalWorkflowEndpointRequests
{
    /// <summary>
    /// Builds the RetryDocumentTemplateExport request from the current workflow state.
    /// </summary>
    public static ApiDocuments.RetryDocumentTemplateExportRequest InspectGoalWorkflowToRetryDocumentTemplateExport1(this ApiPerformance.InspectGoalWorkflowRequest request, ApiRecruiting.CalculateInterviewResponse reply0)
    {
        return new ApiDocuments.RetryDocumentTemplateExportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.RetryDocumentTemplateExportRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Interview?.Organizer?.Email ?? ""),
                },
            },
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Interview?.Audit?.CreatedBy ?? ""),
                },
                Counterparty = new ApiDocuments.DocumentFolderRef
                {
                    DisplayName = (reply0.Interview?.Organizer?.DisplayName ?? ""),
                },
            },
        };
    }
}
