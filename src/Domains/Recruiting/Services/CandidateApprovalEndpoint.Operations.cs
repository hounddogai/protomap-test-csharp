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

public sealed partial class CandidateApprovalEndpoint
{
    /// <summary>
    /// Approves a candidate.
    /// </summary>
    public override async Task<ApiRecruiting.ApproveCandidateResponse> ApproveCandidate(ApiRecruiting.ApproveCandidateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ApproveCandidate for {Tenant}", requestContext.TenantId);
        if (request.Candidate is { } record)
        {
            var snapshot = CandidateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.ScheduleInterviewReportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.ScheduleInterviewReportRequest.Types.Details
            {
                Filters = { (request.Candidate?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Period = (request.Candidate?.Preferences?.QuietHours ?? new ApiCommon.TimeWindow()),
            },
            Reason = (request.Reason ?? ""),
        };
        var reply0 = await SendApproveCandidate0Async(input0, token);
        var input1 = new ApiDocuments.RecalculateDocumentTemplatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.RecalculateDocumentTemplatesRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Recipient = (request.Candidate?.PrimaryAddress?.Recipient ?? ""),
                },
            },
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Candidate?.Audit?.CreatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Candidate?.PrimaryAddress?.PostalCode ?? ""),
                },
            },
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _documentsDocumentTemplateCalculationService.RecalculateDocumentTemplatesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.ApproveCandidateResponse
        {
            Candidate = new ApiRecruiting.Candidate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.DocumentTemplate?.Audit?.Etag ?? ""),
                },
                CandidateId = (request.CandidateId ?? ""),
                PrimaryAddress = (request.Candidate?.PrimaryAddress ?? new ApiCommon.PostalAddress()),
            },
            Outcome = new ApiRecruiting.ApproveCandidateResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Comment ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "ApproveCandidate", (request.Details?.Comment ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates a candidate.
    /// </summary>
    public override async Task<ApiRecruiting.EscalateCandidateResponse> EscalateCandidate(ApiRecruiting.EscalateCandidateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EscalateCandidate for {Tenant}", requestContext.TenantId);
        if (request.Candidate is { } record)
        {
            var snapshot = CandidateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.ScheduleDocumentFolderExportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ScheduleDocumentFolderExportRequest.Types.Details
            {
                Filters = { (request.Candidate?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Period = (request.Candidate?.Preferences?.QuietHours ?? new ApiCommon.TimeWindow()),
            },
            Reason = (request.Reason ?? ""),
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiDocuments.ScheduleDocumentFolderExportRequest>();
        using var rpc0 = _documentsDocumentFolderExportService.ScheduleDocumentFolderExportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiPipelines.CreatePipelineRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Pipeline = new ApiPipelines.Pipeline
            {
                Location = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Candidate?.PrimaryAddress?.PostalCode ?? ""),
                },
                Participants = { new ApiPipelines.PipelineParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Candidate?.RelationshipManager?.Email ?? ""),
                        Name = (request.Candidate?.Contact?.Name ?? new ApiCommon.PersonName()),
                    },
                } },
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiPipelines.CreatePipelineRequest>(json1);
        using var rpc1 = _pipelinesPipelineService.CreatePipelineAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.EscalateCandidateResponse
        {
            Candidate = new ApiRecruiting.Candidate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.Pipeline?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                CandidateId = (request.CandidateId ?? ""),
                RelationshipManager = (request.Candidate?.RelationshipManager ?? new ApiIdentity.UserRef()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Comment ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "EscalateCandidate", (request.Details?.Comment ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a candidate.
    /// </summary>
    public override async Task<ApiRecruiting.GetCandidateApprovalStatusResponse> GetCandidateApprovalStatus(ApiRecruiting.GetCandidateApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CandidateId, "candidate_id");
        _logger.LogInformation("Handling GetCandidateApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "candidate", request.CandidateId,
            "GetCandidateApprovalStatus", "accepted", token);
        var response = new ApiRecruiting.GetCandidateApprovalStatusResponse
        {
            Candidate = new ApiRecruiting.Candidate
            {
                CandidateId = (request.CandidateId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists candidates a page at a time.
    /// </summary>
    public override async Task<ApiRecruiting.ListPendingCandidatesResponse> ListPendingCandidates(ApiRecruiting.ListPendingCandidatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListPendingCandidates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "candidate", "batch",
            "ListPendingCandidates", "accepted", token);
        var input0 = new ApiRecruiting.ScheduleOfferExpiryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Offer = new ApiRecruiting.Offer
            {
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
        using var rpc0 = _recruitingOfferLifecycleService.ScheduleOfferExpiryAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiEmployees.ListEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.ListEmployeesRequest.Types.Filter
            {
                Region = (request.Filter?.Region ?? ""),
            },
            Page = new ApiCommon.PageRequest
            {
                PageSize = (request.Page?.PageSize ?? 0),
            },
        };
        using var rpc1 = _employeesEmployeeService.ListEmployeesAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiRecruiting.ListPendingCandidatesResponse
        {
            Candidates = { new ApiRecruiting.Candidate
            {
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.Offer?.BillingAddress?.Recipient ?? ""),
                },
            } },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "ListPendingCandidates", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls a candidate.
    /// </summary>
    public override async Task<ApiRecruiting.RecallCandidateResponse> RecallCandidate(ApiRecruiting.RecallCandidateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecallCandidate for {Tenant}", requestContext.TenantId);
        if (request.Candidate is { } record)
        {
            var snapshot = CandidateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.SyncProblemsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.SyncProblemsRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Problem = new ApiItsm.Problem
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (request.Details?.Approver?.UserId ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Candidate?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Customer = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Candidate?.RelationshipManager?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _itsmProblemSyncService.SyncProblemsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiIdentity.PreviewUserRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiIdentity.PreviewUserRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (reply0.Problem?.Requester?.PreferredChannel ?? ""),
                },
            },
            User = new ApiIdentity.User
            {
                Passport = new ApiIdentity.UserPassport
                {
                    ExpiresAt = (request.Candidate ?? new ApiRecruiting.Candidate()).IdentityDocumentCase switch
                    {
                        ApiRecruiting.Candidate.IdentityDocumentOneofCase.Passport => (request.Candidate?.Passport?.ExpiresAt ?? new Timestamp()),
                        _ => new Timestamp()
                    },
                    IssuingCountry = (request.Candidate ?? new ApiRecruiting.Candidate()).IdentityDocumentCase switch
                    {
                        ApiRecruiting.Candidate.IdentityDocumentOneofCase.NationalId => (request.Candidate?.NationalId?.IssuingCountry ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _identityUserValidationService.PreviewUserAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiEmployees.LintPositionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.LintPositionsRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply1.User?.Contact?.Email ?? ""),
                },
            },
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.User?.Audit?.CreatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.User?.RelationshipManager?.DisplayName ?? ""),
                },
            },
        };
        var reply2 = await Workflow.SendAsync(input2, token, _employeesPositionValidationService.LintPositionsAsync);
        var response = new ApiRecruiting.RecallCandidateResponse
        {
            Candidate = new ApiRecruiting.Candidate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.Problem?.Audit?.UpdatedBy ?? ""),
                },
                CandidateId = (request.CandidateId ?? ""),
                Status = (request.Candidate?.Status ?? (ApiRecruiting.CandidateStatus)0),
            },
            Outcome = new ApiRecruiting.RecallCandidateResponse.Types.Outcome
            {
                ReferenceId = (reply1.Outcome?.ReferenceId ?? ""),
                Status = (reply2.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "RecallCandidate", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects a candidate.
    /// </summary>
    public override async Task<ApiRecruiting.RejectCandidateResponse> RejectCandidate(ApiRecruiting.RejectCandidateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RejectCandidate for {Tenant}", requestContext.TenantId);
        if (request.Candidate is { } record)
        {
            var snapshot = CandidateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.ScheduleCandidateReportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            CandidateId = (request.Candidate?.CandidateId ?? ""),
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        using var rpc0 = _sendRejectCandidate0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.ValidatePositionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Candidate?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Candidate?.RelationshipManager?.DisplayName ?? ""),
                    Email = (request.Candidate?.Contact?.Email ?? ""),
                },
            },
        };
        var named1 = _registry.Get<ApiEmployees.PositionValidationService.PositionValidationServiceClient>("acme.employees.v1.PositionValidationService");
        using var rpc1 = named1.ValidatePositionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiCompliance.CloneComplianceCaseRequest
        {
            ComplianceCase = new ApiCompliance.ComplianceCase
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.Position?.Owner?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var descriptor2 = new Method<ApiCompliance.CloneComplianceCaseRequest, ApiCompliance.CloneComplianceCaseResponse>(
            MethodType.Unary, "acme.compliance.v1" + ".ComplianceCaseService", "CloneComplianceCase",
            Wire.Marshaller<ApiCompliance.CloneComplianceCaseRequest>(), Wire.Marshaller<ApiCompliance.CloneComplianceCaseResponse>());
        using var rpc2 = _invoker.AsyncUnaryCall(descriptor2, null,
            new CallOptions(cancellationToken: token), input2);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiRecruiting.RejectCandidateResponse
        {
            Candidate = new ApiRecruiting.Candidate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.Position?.Audit?.Etag ?? ""),
                },
                CandidateId = (request.CandidateId ?? ""),
                RelationshipManager = (request.Candidate?.RelationshipManager ?? new ApiIdentity.UserRef()),
            },
            Outcome = new ApiRecruiting.RejectCandidateResponse.Types.Outcome
            {
                Message = (reply1.Outcome?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply2.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Comment ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "RejectCandidate", (request.Details?.Comment ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits a candidate.
    /// </summary>
    public override async Task<ApiRecruiting.SubmitCandidateResponse> SubmitCandidate(ApiRecruiting.SubmitCandidateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitCandidate for {Tenant}", requestContext.TenantId);
        if (request.Candidate is { } record)
        {
            var snapshot = CandidateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.SubmitJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.SubmitJobOpeningRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Candidate?.RelationshipManager?.DisplayName ?? ""),
                },
            },
            JobOpening = new ApiRecruiting.JobOpening
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Candidate?.PrimaryAddress?.CountryCode ?? ""),
                    Locality = (request.Candidate?.PrimaryAddress?.Locality ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiRecruiting.SubmitJobOpeningRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _recruitingJobOpeningApprovalService.SubmitJobOpeningAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiRecruiting.SubmitCandidateRequest, ApiAudit.ResolveAuditEventsRequest>(value => new ApiAudit.ResolveAuditEventsRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _auditAuditEventQueryService.ResolveAuditEventsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLedger.SearchLedgerAccountsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLedger.SearchLedgerAccountsRequest.Types.Filter
            {
                CreatedAfter = (reply0.JobOpening?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.Candidate?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.JobOpening?.Counterparty?.InterviewId ?? ""),
                Region = (request.Candidate?.PrimaryAddress?.Region ?? ""),
            },
        };
        input2 = PropertyMap.Copy(input2);
        using var rpc2 = _ledgerLedgerAccountService.SearchLedgerAccountsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = request.SubmitCandidateToScheduleProblemExpiry3(reply0);
        using var rpc3 = _itsmProblemLifecycleService.ScheduleProblemExpiryAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiRecruiting.SubmitCandidateResponse
        {
            Candidate = new ApiRecruiting.Candidate
            {
                CandidateId = (request.CandidateId ?? ""),
            },
            Outcome = new ApiRecruiting.SubmitCandidateResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply1.Result?.Message ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply3.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply2.Result?.Message ?? ""),
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "SubmitCandidate", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiRecruiting.ScheduleInterviewReportResponse> SendApproveCandidate0Async(ApiRecruiting.ScheduleInterviewReportRequest request, CancellationToken token)
    {
        return await _candidateApprovalEndpointSendApproveCandidate0Port.SendAsync(request, token);
    }
}
