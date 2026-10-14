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
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Recruiting;

public sealed partial class CandidateValidationEndpoint
{
    /// <summary>
    /// Checks a candidate.
    /// </summary>
    public override async Task<ApiRecruiting.CheckCandidateEligibilityResponse> CheckCandidateEligibility(ApiRecruiting.CheckCandidateEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CheckCandidateEligibility for {Tenant}", requestContext.TenantId);
        if (request.Candidate is { } record)
        {
            var snapshot = CandidateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAuthorization.CountAccessPoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiAuthorization.CountAccessPoliciesRequest.Types.Filter
            {
                CreatedAfter = (request.Candidate?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (request.Candidate?.RelationshipManager?.UserId ?? ""),
                Region = (request.Candidate?.PrimaryAddress?.Region ?? ""),
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _authorizationAccessPolicyQueryService.CountAccessPoliciesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiAuthorization.CountAccessPoliciesRequest>("acme.recruiting.v1.CandidateValidationService/CheckCandidateEligibility/0", input0.Clone()), token);
        var input1 = request.CheckCandidateEligibilityToExplainJobOpeningDecision1();
        using var rpc1 = _recruitingJobOpeningValidationService.ExplainJobOpeningDecisionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDocuments.ExportDocumentFoldersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ExportDocumentFoldersRequest.Types.Filter
            {
                CreatedAfter = (reply1.JobOpening?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.Candidate?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Candidate?.RelationshipManager?.UserId ?? ""),
                Region = (reply1.JobOpening?.BillingAddress?.Region ?? ""),
            },
        };
        using var stream2 = _documentsDocumentFolderQueryService.ExportDocumentFolders(input2, cancellationToken: token);
        await foreach (var change2 in stream2.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportDocumentFolders frame {Type}", change2.GetType().Name);
        }
        var response = new ApiRecruiting.CheckCandidateEligibilityResponse
        {
            Candidate = new ApiRecruiting.Candidate
            {
                CandidateId = (request.CandidateId ?? ""),
                RelationshipManager = (request.Candidate?.RelationshipManager ?? new ApiIdentity.UserRef()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "CheckCandidateEligibility", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a candidate.
    /// </summary>
    public override async Task<ApiRecruiting.ExplainCandidateDecisionResponse> ExplainCandidateDecision(ApiRecruiting.ExplainCandidateDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainCandidateDecision for {Tenant}", requestContext.TenantId);
        if (request.Candidate is { } record)
        {
            var snapshot = CandidateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.ExplainJobOpeningDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Candidate?.PrimaryAddress?.PostalCode ?? ""),
                },
            },
        };
        using var rpc0 = _recruitingJobOpeningValidationService.ExplainJobOpeningDecisionAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.LintJobOpeningsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Candidate?.Audit?.CreatedBy ?? ""),
                },
            },
        };
        using var rpc1 = _recruitingJobOpeningValidationService.LintJobOpeningsAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiRecruiting.ExplainCandidateDecisionResponse
        {
            Candidate = new ApiRecruiting.Candidate
            {
                CandidateId = (request.CandidateId ?? ""),
                Contact = (request.Candidate?.Contact ?? new ApiCommon.ContactInfo()),
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply1.JobOpening?.BillingAddress?.CountryCode ?? ""),
                },
            },
            Outcome = new ApiRecruiting.ExplainCandidateDecisionResponse.Types.Outcome
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "ExplainCandidateDecision", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a candidate.
    /// </summary>
    public override async Task<ApiRecruiting.GetCandidateRulesResponse> GetCandidateRules(ApiRecruiting.GetCandidateRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CandidateId, "candidate_id");
        _logger.LogInformation("Handling GetCandidateRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "candidate", request.CandidateId,
            "GetCandidateRules", "accepted", token);
        var input0 = new ApiDocuments.ExplainSignatureDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ExplainSignatureDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
        };
        using var rpc0 = _documentsSignatureValidationService.ExplainSignatureDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.PreviewJobOpeningRequest();
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        input1.Details = new ApiRecruiting.PreviewJobOpeningRequest.Types.Details
        {
            Contact = new ApiCommon.ContactInfo
            {
                Email = (request.Context?.Actor?.Email ?? ""),
            },
        };
        input1.JobOpening = new ApiRecruiting.JobOpening
        {
            Counterparty = new ApiRecruiting.InterviewRef
            {
                DisplayName = (reply0.Signature?.Subject?.DisplayName ?? ""),
            },
            Lines = { new ApiRecruiting.JobOpeningLine
            {
                Attributes = { (reply0.Signature?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            } },
            Number = (reply0.Signature?.Subject?.Number ?? ""),
        };
        using var rpc1 = _recruitingJobOpeningValidationService.PreviewJobOpeningAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.GetCandidateRulesResponse
        {
            Candidate = new ApiRecruiting.Candidate
            {
                CandidateId = (request.CandidateId ?? ""),
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply1.JobOpening?.BillingAddress?.CountryCode ?? ""),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Signature?.Subject?.DisplayName ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lints a candidate.
    /// </summary>
    public override async Task<ApiRecruiting.LintCandidatesResponse> LintCandidates(ApiRecruiting.LintCandidatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling LintCandidates for {Tenant}", requestContext.TenantId);
        if (request.Candidate is { } record)
        {
            var snapshot = CandidateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPerformance.NormalizeCalibrationRequest
        {
            Calibration = new ApiPerformance.Calibration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Candidate?.Audit?.CreatedBy ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Candidate?.RelationshipManager?.Email ?? ""),
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.NormalizeCalibrationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Candidate?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        using var rpc0 = _performanceCalibrationValidationService.NormalizeCalibrationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiRecruiting.LintCandidatesResponse
        {
            Candidate = new ApiRecruiting.Candidate
            {
                CandidateId = (request.CandidateId ?? ""),
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Candidate?.Name ?? new ApiCommon.PersonName()),
                },
            },
            Outcome = new ApiRecruiting.LintCandidatesResponse.Types.Outcome
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "LintCandidates", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews a candidate.
    /// </summary>
    public override async Task<ApiRecruiting.PreviewCandidateResponse> PreviewCandidate(ApiRecruiting.PreviewCandidateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PreviewCandidate for {Tenant}", requestContext.TenantId);
        if (request.Candidate is { } record)
        {
            var snapshot = CandidateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTimekeeping.CloneTimesheetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Timesheet = new ApiTimekeeping.Timesheet
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Candidate?.PrimaryAddress?.PostalCode ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Candidate?.RelationshipManager?.DisplayName ?? ""),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _timekeepingTimesheetService.CloneTimesheetAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiRecruiting.QuoteJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.QuoteJobOpeningRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Candidate?.PrimaryAddress?.PostalCode ?? ""),
                },
            },
            JobOpening = new ApiRecruiting.JobOpening
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (request.Candidate?.PrimaryAddress?.Recipient ?? ""),
                },
                Counterparty = new ApiRecruiting.InterviewRef
                {
                    DisplayName = (reply0.Timesheet?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _recruitingJobOpeningCalculationService.QuoteJobOpeningAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiRecruiting.CalculateInterviewRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Interview = new ApiRecruiting.Interview
            {
                Location = new ApiCommon.PostalAddress
                {
                    Region = (reply1.JobOpening?.BillingAddress?.Region ?? ""),
                },
                Participants = { new ApiRecruiting.InterviewParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Candidate?.RelationshipManager?.Email ?? ""),
                        Phone = (request.Details?.Contact?.Phone ?? ""),
                    },
                } },
            },
        };
        var reply2 = await Workflow.SendAsync(input2, token, _recruitingInterviewCalculationService.CalculateInterviewAsync);
        var response = new ApiRecruiting.PreviewCandidateResponse
        {
            Candidate = new ApiRecruiting.Candidate
            {
                CandidateId = (request.CandidateId ?? ""),
                Contact = (request.Candidate?.Contact ?? new ApiCommon.ContactInfo()),
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Region = (reply1.JobOpening?.BillingAddress?.Region ?? ""),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    Email = (reply0.Timesheet?.Counterparty?.Email ?? ""),
                    UserId = (reply2.Interview?.Organizer?.UserId ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "PreviewCandidate", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates a candidate.
    /// </summary>
    public override async Task<ApiRecruiting.ValidateCandidateResponse> ValidateCandidate(ApiRecruiting.ValidateCandidateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ValidateCandidate for {Tenant}", requestContext.TenantId);
        if (request.Candidate is { } record)
        {
            var snapshot = CandidateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.ExplainJobOpeningDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.ExplainJobOpeningDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Contact?.Phone ?? ""),
                },
            },
            JobOpening = new ApiRecruiting.JobOpening
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Candidate?.PrimaryAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    PostalCode = (request.Candidate?.PrimaryAddress?.PostalCode ?? ""),
                },
            },
        };
        using var rpc0 = _sendValidateCandidate0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.CompareCandidateOptionsRequest
        {
            Candidate = new ApiRecruiting.Candidate
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Candidate?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var named1 = _registry.Get<ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient>("acme.recruiting.v1.CandidateCalculationService");
        using var rpc1 = named1.CompareCandidateOptionsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.ValidateCandidateResponse
        {
            Candidate = new ApiRecruiting.Candidate
            {
                CandidateId = (request.CandidateId ?? ""),
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Candidate?.Name ?? new ApiCommon.PersonName()),
                },
                Preferences = new ApiRecruiting.CandidatePreferences
                {
                    QuietHours = (reply1.Candidate?.Preferences?.QuietHours ?? new ApiCommon.TimeWindow()),
                },
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply1.Candidate?.PrimaryAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
            Outcome = new ApiRecruiting.ValidateCandidateResponse.Types.Outcome
            {
                EffectiveAt = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "ValidateCandidate", (request.Reason ?? ""), token);
        return response;
    }
}
