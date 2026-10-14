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
using ApiCompliance = Acme.Compliance.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiPipelines = Acme.Pipelines.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Recruiting;

/// <summary>
/// The application port for ScheduleInterviewReport.
/// </summary>
public interface ICandidateApprovalEndpointSendApproveCandidate0Port
{
    Task<ApiRecruiting.ScheduleInterviewReportResponse> SendAsync(ApiRecruiting.ScheduleInterviewReportRequest request, CancellationToken token);
}

public sealed class CandidateApprovalEndpointSendApproveCandidate0PortAdapter(ApiRecruiting.InterviewReportingService.InterviewReportingServiceClient client) : ICandidateApprovalEndpointSendApproveCandidate0Port
{
    public async Task<ApiRecruiting.ScheduleInterviewReportResponse> SendAsync(ApiRecruiting.ScheduleInterviewReportRequest request, CancellationToken token)
    {
        using var rpc = client.ScheduleInterviewReportAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class CandidateApprovalEndpointRequests
{
    /// <summary>
    /// Builds the ScheduleProblemExpiry request from the current workflow state.
    /// </summary>
    public static ApiItsm.ScheduleProblemExpiryRequest SubmitCandidateToScheduleProblemExpiry3(this ApiRecruiting.SubmitCandidateRequest request, ApiRecruiting.SubmitJobOpeningResponse reply0)
    {
        return new ApiItsm.ScheduleProblemExpiryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Problem = new ApiItsm.Problem
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.JobOpening?.Audit?.Etag ?? ""),
                },
                Customer = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Candidate?.RelationshipManager?.Email ?? ""),
                },
            },
        };
    }
}
