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
using ApiBanking = Acme.Banking.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Recruiting;

public sealed partial class JobOpeningApprovalEndpoint
{
    /// <summary>
    /// Approves a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.ApproveJobOpeningResponse> ApproveJobOpening(ApiRecruiting.ApproveJobOpeningRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ApproveJobOpening for {Tenant}", requestContext.TenantId);
        if (request.JobOpening is { } record)
        {
            var snapshot = JobOpeningMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.JobOpening?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiRecruiting.JobOpeningLine>()).Select(async item =>
        {
            var input0 = new ApiEmployees.CheckPositionEligibilityRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Position = new ApiEmployees.Position
                {
                    Discount = new ApiEmployees.PositionDiscountAction
                    {
                        Amount = (item.Amount ?? new ApiCommon.Money()),
                    },
                },
            };
            using var rpc0 = _employeesPositionValidationService.CheckPositionEligibilityAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiEmployees.CheckPositionEligibilityResponse();
        var response = new ApiRecruiting.ApproveJobOpeningResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                JobOpeningId = (request.JobOpeningId ?? ""),
            },
            Outcome = new ApiRecruiting.ApproveJobOpeningResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "ApproveJobOpening", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.CommentJobOpeningApprovalResponse> CommentJobOpeningApproval(ApiRecruiting.CommentJobOpeningApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CommentJobOpeningApproval for {Tenant}", requestContext.TenantId);
        if (request.JobOpening is { } record)
        {
            var snapshot = JobOpeningMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTimekeeping.AdvanceOvertimeWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Overtime = new ApiTimekeeping.Overtime
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.JobOpening?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (request.JobOpening?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                    UserId = (request.Details?.Approver?.UserId ?? ""),
                },
            },
        };
        using var rpc0 = _timekeepingOvertimeWorkflowService.AdvanceOvertimeWorkflowAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiScheduling.RejectShiftRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiScheduling.RejectShiftRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    UserId = (request.Details?.Approver?.UserId ?? ""),
                },
            },
            Shift = new ApiScheduling.Shift
            {
                Location = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.JobOpening?.BillingAddress?.PostalCode ?? ""),
                    Recipient = (request.JobOpening?.BillingAddress?.Recipient ?? ""),
                },
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _schedulingShiftApprovalService.RejectShiftAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var input2 = new ApiRecruiting.ExplainCandidateCalculationRequest
        {
            Candidate = new ApiRecruiting.Candidate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.JobOpening?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply2 = await Workflow.SendAsync(input2, token, _recruitingCandidateCalculationService.ExplainCandidateCalculationAsync);
        var response = new ApiRecruiting.CommentJobOpeningApprovalResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (reply2.Candidate?.PrimaryAddress?.Recipient ?? ""),
                },
                JobOpeningId = (request.JobOpeningId ?? ""),
                Lines = { new ApiRecruiting.JobOpeningLine
                {
                    Amount = (reply0.Overtime?.Discount?.Amount ?? new ApiCommon.Money()),
                } },
                Notes = (request.JobOpening?.Notes ?? ""),
            },
            Outcome = new ApiRecruiting.CommentJobOpeningApprovalResponse.Types.Outcome
            {
                NextSteps = { (reply0.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "CommentJobOpeningApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.DelegateJobOpeningApprovalResponse> DelegateJobOpeningApproval(ApiRecruiting.DelegateJobOpeningApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling DelegateJobOpeningApproval for {Tenant}", requestContext.TenantId);
        if (request.JobOpening is { } record)
        {
            var snapshot = JobOpeningMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiDocuments.CreateDocumentResponse();
        foreach (var item in (request.JobOpening?.Attachments ?? new Google.Protobuf.Collections.RepeatedField<ApiRecruiting.JobOpeningAttachment>()))
        {
            var input0 = new ApiDocuments.CreateDocumentRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Document = new ApiDocuments.Document
                {
                    Attachments = { new ApiDocuments.DocumentAttachment
                    {
                        SizeBytes = item.SizeBytes,
                    } },
                },
            };
            using var rpc0 = _documentsDocumentService.CreateDocumentAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiCorporateevents.GenerateRegistrationReportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.GenerateRegistrationReportRequest.Types.Details
            {
                Filters = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Reason = (request.Reason ?? ""),
        };
        var named1 = _registry.Get<ApiCorporateevents.RegistrationReportingService.RegistrationReportingServiceClient>("acme.corporateevents.v1.RegistrationReportingService");
        using var rpc1 = named1.GenerateRegistrationReportAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.DelegateJobOpeningApprovalResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                JobOpeningId = (request.JobOpeningId ?? ""),
                Metadata = { (request.JobOpening?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Outcome = new ApiRecruiting.DelegateJobOpeningApprovalResponse.Types.Outcome
            {
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.JobOpening?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "DelegateJobOpeningApproval", (request.JobOpening?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.EscalateJobOpeningResponse> EscalateJobOpening(ApiRecruiting.EscalateJobOpeningRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EscalateJobOpening for {Tenant}", requestContext.TenantId);
        if (request.JobOpening is { } record)
        {
            var snapshot = JobOpeningMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.ListCandidateReportsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiRecruiting.ListCandidateReportsRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
                Labels = { (request.JobOpening?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Region = (request.JobOpening?.BillingAddress?.Region ?? ""),
            },
        };
        var descriptor0 = new Method<ApiRecruiting.ListCandidateReportsRequest, ApiRecruiting.ListCandidateReportsResponse>(
            MethodType.Unary, "acme.recruiting.v1" + ".CandidateReportingService", "ListCandidateReports",
            Wire.Marshaller<ApiRecruiting.ListCandidateReportsRequest>(), Wire.Marshaller<ApiRecruiting.ListCandidateReportsResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiMetrics.ExplainMetricSeriesDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            MetricSeries = new ApiMetrics.MetricSeries
            {
                Address = new ApiCommon.PostalAddress
                {
                    Locality = (request.JobOpening?.BillingAddress?.Locality ?? ""),
                },
            },
        };
        var command1 = new DispatchCommand<ApiMetrics.ExplainMetricSeriesDecisionRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _metricsMetricSeriesValidationService.ExplainMetricSeriesDecisionAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiEmployees.BatchCreateEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Record = new ApiEmployees.Employee
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.MetricSeries?.Audit?.CreatedBy ?? ""),
                },
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (request.JobOpening?.BillingAddress?.Recipient ?? ""),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    DisplayName = (request.JobOpening?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        using var stream2 = _employeesEmployeeService.BatchCreateEmployees(cancellationToken: token);
        await stream2.RequestStream.WriteAsync(input2, token);
        await stream2.RequestStream.CompleteAsync();
        var reply2 = await stream2.ResponseAsync;
        var input3 = new ApiRecruiting.TerminateJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.TerminateJobOpeningRequest.Types.Details
            {
                NewOwner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.MetricSeries?.Owner?.DisplayName ?? ""),
                    Email = (reply1.MetricSeries?.Owner?.Email ?? ""),
                    UserId = (reply1.MetricSeries?.Owner?.UserId ?? ""),
                },
                NotifyContact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
            JobOpeningId = (request.JobOpening?.JobOpeningId ?? ""),
        };
        input3 = PropertyMap.Copy(input3);
        using var rpc3 = _recruitingJobOpeningLifecycleService.TerminateJobOpeningAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiRecruiting.EscalateJobOpeningResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.MetricSeries?.Audit?.CreatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (reply3.JobOpening?.BillingAddress?.Region ?? ""),
                },
                JobOpeningId = (request.JobOpeningId ?? ""),
            },
            Outcome = new ApiRecruiting.EscalateJobOpeningResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "EscalateJobOpening", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.GetJobOpeningApprovalStatusResponse> GetJobOpeningApprovalStatus(ApiRecruiting.GetJobOpeningApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.JobOpeningId, "job_opening_id");
        _logger.LogInformation("Handling GetJobOpeningApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "job_opening", request.JobOpeningId,
            "GetJobOpeningApprovalStatus", "accepted", token);
        var input0 = request.GetJobOpeningApprovalStatusToScoreMetricSeries0();
        using var rpc0 = _metricsMetricSeriesValidationService.ScoreMetricSeriesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiRecruiting.GetJobOpeningApprovalStatusResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.MetricSeries?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiRecruiting.InterviewRef
                {
                    DisplayName = (reply0.MetricSeries?.Owner?.DisplayName ?? ""),
                },
                JobOpeningId = (request.JobOpeningId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists job openings a page at a time.
    /// </summary>
    public override async Task<ApiRecruiting.ListPendingJobOpeningsResponse> ListPendingJobOpenings(ApiRecruiting.ListPendingJobOpeningsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListPendingJobOpenings for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "job_opening", "batch",
            "ListPendingJobOpenings", "accepted", token);
        var input0 = new ApiEmployees.CheckEmploymentContractEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.CheckEmploymentContractEligibilityRequest.Types.Details
            {
                Locale = (request.Context?.Locale ?? ""),
            },
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var reply0 = await SendListPendingJobOpenings0Async(input0, token);
        var input1 = new ApiRecruiting.ResolveInterviewConflictRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.ResolveInterviewConflictRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Interview = new ApiRecruiting.Interview
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.EmploymentContract?.Audit?.UpdatedBy ?? ""),
                },
                Location = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.EmploymentContract?.BillingAddress?.Recipient ?? ""),
                },
                Organizer = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.EmploymentContract?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _recruitingInterviewSyncService.ResolveInterviewConflictAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.ListPendingJobOpeningsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "ListPendingJobOpenings", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.RecallJobOpeningResponse> RecallJobOpening(ApiRecruiting.RecallJobOpeningRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecallJobOpening for {Tenant}", requestContext.TenantId);
        if (request.JobOpening is { } record)
        {
            var snapshot = JobOpeningMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.ListDocumentTemplateConflictsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentTemplateConflictsRequest.Types.Filter
            {
                CreatedAfter = (request.JobOpening?.Audit?.UpdatedAt ?? new Timestamp()),
                Region = (request.JobOpening?.BillingAddress?.Region ?? ""),
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiDocuments.ListDocumentTemplateConflictsRequest>();
        using var rpc0 = _documentsDocumentTemplateSyncService.ListDocumentTemplateConflictsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.ValidateCandidateRequest
        {
            Candidate = new ApiRecruiting.Candidate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.JobOpening?.Audit?.UpdatedBy ?? ""),
                },
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.JobOpening?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Recipient = (request.JobOpening?.BillingAddress?.Recipient ?? ""),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    DisplayName = (request.JobOpening?.Counterparty?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiRecruiting.ValidateCandidateRequest>(json1);
        using var rpc1 = _recruitingCandidateValidationService.ValidateCandidateAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.RecallJobOpeningResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply1.Candidate?.PrimaryAddress?.Locality ?? ""),
                },
                JobOpeningId = (request.JobOpeningId ?? ""),
                Number = (request.JobOpening?.Number ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "RecallJobOpening", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.RejectJobOpeningResponse> RejectJobOpening(ApiRecruiting.RejectJobOpeningRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RejectJobOpening for {Tenant}", requestContext.TenantId);
        if (request.JobOpening is { } record)
        {
            var snapshot = JobOpeningMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiScheduling.RecallSwapRequestRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiScheduling.RecallSwapRequestRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                },
            },
            SwapRequest = new ApiScheduling.SwapRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (request.Details?.Approver?.UserId ?? ""),
                },
            },
        };
        using var rpc0 = _schedulingSwapRequestApprovalService.RecallSwapRequestAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.GetInterviewRatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            InterviewId = (request.JobOpening?.Counterparty?.InterviewId ?? ""),
        };
        using var rpc1 = _recruitingInterviewCalculationService.GetInterviewRatesAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiRecruiting.RejectJobOpeningResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.SwapRequest?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (reply1.Interview?.Audit?.Etag ?? ""),
                },
                IssuedAt = (request.JobOpening?.IssuedAt ?? new Timestamp()),
                JobOpeningId = (request.JobOpeningId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "RejectJobOpening", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.SubmitJobOpeningResponse> SubmitJobOpening(ApiRecruiting.SubmitJobOpeningRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitJobOpening for {Tenant}", requestContext.TenantId);
        if (request.JobOpening is { } record)
        {
            var snapshot = JobOpeningMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.CountEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.CountEmployeesRequest.Types.Filter
            {
                CreatedAfter = (request.JobOpening?.Audit?.CreatedAt ?? new Timestamp()),
                Region = (request.JobOpening?.BillingAddress?.Region ?? ""),
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _employeesEmployeeQueryService.CountEmployeesAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiBanking.ExportBankAccountsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiBanking.ExportBankAccountsRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
                OwnerId = (request.JobOpening?.Counterparty?.InterviewId ?? ""),
                Region = (request.JobOpening?.BillingAddress?.Region ?? ""),
            },
        };
        using var stream1 = _bankingBankAccountQueryService.ExportBankAccounts(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportBankAccounts frame {Type}", change1.GetType().Name);
        }
        var response = new ApiRecruiting.SubmitJobOpeningResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                DueAt = (request.JobOpening?.DueAt ?? new Timestamp()),
                JobOpeningId = (request.JobOpeningId ?? ""),
            },
            Outcome = new ApiRecruiting.SubmitJobOpeningResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "SubmitJobOpening", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiEmployees.CheckEmploymentContractEligibilityResponse> SendListPendingJobOpenings0Async(ApiEmployees.CheckEmploymentContractEligibilityRequest request, CancellationToken token)
    {
        return await _jobOpeningApprovalEndpointSendListPendingJobOpenings0Port.SendAsync(request, token);
    }
}
