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

public sealed partial class EnrollmentApprovalEndpoint
{
    /// <summary>
    /// Approves an enrollment.
    /// </summary>
    public override async Task<ApiBenefits.ApproveEnrollmentResponse> ApproveEnrollment(ApiBenefits.ApproveEnrollmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ApproveEnrollment for {Tenant}", requestContext.TenantId);
        if (request.Enrollment is { } record)
        {
            var snapshot = EnrollmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiBenefits.CheckLifeEventEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.CheckLifeEventEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Enrollment?.Contact?.PreferredChannel ?? ""),
                },
            },
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (request.Enrollment?.Holder?.DisplayName ?? ""),
                    Email = (request.Enrollment?.Contact?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _benefitsLifeEventValidationService.CheckLifeEventEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiBenefits.SubmitBenefitPlanRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.SubmitBenefitPlanRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                    Email = (request.Enrollment?.Holder?.Email ?? ""),
                },
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _benefitsBenefitPlanApprovalService.SubmitBenefitPlanAsync);
        var input2 = new ApiBenefits.LintLifeEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    CandidateId = (reply0.LifeEvent?.Subject?.CandidateId ?? ""),
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
        };
        using var rpc2 = _benefitsLifeEventValidationService.LintLifeEventsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var batch3 = (request.Enrollment?.Limits ?? new Google.Protobuf.Collections.RepeatedField<ApiBenefits.EnrollmentLimit>()).Select(async item =>
        {
            var input3 = new ApiRecruiting.ScoreOfferRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Offer = new ApiRecruiting.Offer
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        UpdatedBy = (request.Enrollment?.Audit?.UpdatedBy ?? ""),
                    },
                    Counterparty = new ApiEmployees.EmployeeRef
                    {
                        DisplayName = (reply1.BenefitPlan?.Owner?.DisplayName ?? ""),
                        Email = (request.Enrollment?.Contact?.Email ?? ""),
                    },
                    Lines = { new ApiRecruiting.OfferLine
                    {
                        Tax = new ApiRecruiting.OfferLineTax
                        {
                            Amount = (item.Amount ?? new ApiCommon.Money()),
                        },
                    } },
                },
            };
            using var rpc3 = _recruitingOfferValidationService.ScoreOfferAsync(input3, cancellationToken: token);
            var reply3 = await rpc3.ResponseAsync;
            return reply3;
        }).ToArray();
        var replies3 = await Task.WhenAll(batch3);
        var reply3 = replies3.LastOrDefault() ?? new ApiRecruiting.ScoreOfferResponse();
        var response = new ApiBenefits.ApproveEnrollmentResponse
        {
            Enrollment = new ApiBenefits.Enrollment
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply2.LifeEvent?.Subject?.Email ?? ""),
                },
                EnrollmentId = (request.EnrollmentId ?? ""),
                Holder = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (reply0.LifeEvent?.Subject?.DisplayName ?? ""),
                },
                OpenedAt = (request.Enrollment?.OpenedAt ?? new Timestamp()),
            },
            Outcome = new ApiBenefits.ApproveEnrollmentResponse.Types.Outcome
            {
                Status = (reply1.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "enrollment",
            "audit", "ApproveEnrollment", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments an enrollment.
    /// </summary>
    public override async Task<ApiBenefits.CommentEnrollmentApprovalResponse> CommentEnrollmentApproval(ApiBenefits.CommentEnrollmentApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CommentEnrollmentApproval for {Tenant}", requestContext.TenantId);
        if (request.Enrollment is { } record)
        {
            var snapshot = EnrollmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiBenefits.CreateBenefitPlanRequest
        {
            BenefitPlan = new ApiBenefits.BenefitPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.Enrollment?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Details?.Approver?.UserId ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var descriptor0 = new Method<ApiBenefits.CreateBenefitPlanRequest, ApiBenefits.CreateBenefitPlanResponse>(
            MethodType.Unary, "acme.benefits.v1" + ".BenefitPlanService", "CreateBenefitPlan",
            Wire.Marshaller<ApiBenefits.CreateBenefitPlanRequest>(), Wire.Marshaller<ApiBenefits.CreateBenefitPlanResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiBenefits.CommentEnrollmentApprovalResponse
        {
            Enrollment = new ApiBenefits.Enrollment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.BenefitPlan?.Audit?.UpdatedBy ?? ""),
                },
                Balance = (request.Enrollment?.Balance ?? new ApiCommon.Money()),
                EnrollmentId = (request.EnrollmentId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "enrollment",
            "audit", "CommentEnrollmentApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates an enrollment.
    /// </summary>
    public override async Task<ApiBenefits.DelegateEnrollmentApprovalResponse> DelegateEnrollmentApproval(ApiBenefits.DelegateEnrollmentApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling DelegateEnrollmentApproval for {Tenant}", requestContext.TenantId);
        if (request.Enrollment is { } record)
        {
            var snapshot = EnrollmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiBenefits.BrowseBenefitPlansRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiBenefits.BrowseBenefitPlansRequest.Types.Filter
            {
                CreatedAfter = (request.Enrollment?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (request.Enrollment?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Enrollment?.Holder?.CandidateId ?? ""),
            },
        };
        var command0 = new DispatchCommand<ApiBenefits.BrowseBenefitPlansRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _benefitsBenefitPlanQueryService.BrowseBenefitPlansAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiBenefits.DelegateEnrollmentApprovalResponse
        {
            Enrollment = new ApiBenefits.Enrollment
            {
                Available = (request.Enrollment?.Available ?? new ApiCommon.Money()),
                EnrollmentId = (request.EnrollmentId ?? ""),
            },
            Outcome = new ApiBenefits.DelegateEnrollmentApprovalResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "enrollment",
            "audit", "DelegateEnrollmentApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates an enrollment.
    /// </summary>
    public override async Task<ApiBenefits.EscalateEnrollmentResponse> EscalateEnrollment(ApiBenefits.EscalateEnrollmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EscalateEnrollment for {Tenant}", requestContext.TenantId);
        if (request.Enrollment is { } record)
        {
            var snapshot = EnrollmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiBenefits.EstimateLifeEventRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.EstimateLifeEventRequest.Types.Details
            {
                CurrencyCode = (request.Enrollment?.CurrencyCode ?? ""),
            },
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                    Email = (request.Enrollment?.Holder?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _benefitsLifeEventCalculationService.EstimateLifeEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiBenefits.EscalateEnrollmentResponse
        {
            Enrollment = new ApiBenefits.Enrollment
            {
                Available = (request.Enrollment?.Available ?? new ApiCommon.Money()),
                EnrollmentId = (request.EnrollmentId ?? ""),
                Holder = new ApiRecruiting.CandidateRef
                {
                    CandidateId = (reply0.LifeEvent?.Subject?.CandidateId ?? ""),
                },
            },
            Outcome = new ApiBenefits.EscalateEnrollmentResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "enrollment",
            "audit", "EscalateEnrollment", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an enrollment.
    /// </summary>
    public override async Task<ApiBenefits.GetEnrollmentApprovalStatusResponse> GetEnrollmentApprovalStatus(ApiBenefits.GetEnrollmentApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EnrollmentId, "enrollment_id");
        _logger.LogInformation("Handling GetEnrollmentApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "enrollment", request.EnrollmentId,
            "GetEnrollmentApprovalStatus", "accepted", token);
        var input0 = new ApiBenefits.DelegateBenefitPlanApprovalRequest
        {
            BenefitPlan = new ApiBenefits.BenefitPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.DelegateBenefitPlanApprovalRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _benefitsBenefitPlanApprovalService.DelegateBenefitPlanApprovalAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.CreateJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.BenefitPlan?.Audit?.UpdatedBy ?? ""),
                },
                Counterparty = new ApiRecruiting.InterviewRef
                {
                    DisplayName = (reply0.BenefitPlan?.Owner?.DisplayName ?? ""),
                },
                Lines = { new ApiRecruiting.JobOpeningLine
                {
                    Tax = new ApiRecruiting.JobOpeningLineTax
                    {
                        Amount = (reply0.BenefitPlan ?? new ApiBenefits.BenefitPlan()).ActionCase switch
                        {
                            ApiBenefits.BenefitPlan.ActionOneofCase.Discount => (reply0.BenefitPlan?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc1 = _recruitingJobOpeningService.CreateJobOpeningAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiBenefits.GetEnrollmentApprovalStatusResponse
        {
            Enrollment = new ApiBenefits.Enrollment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.JobOpening?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedBy = (reply1.JobOpening?.Audit?.UpdatedBy ?? ""),
                },
                EnrollmentId = (request.EnrollmentId ?? ""),
                Holder = new ApiRecruiting.CandidateRef
                {
                    Email = (reply0.BenefitPlan?.Owner?.Email ?? ""),
                },
            },
        };
        return response;
    }
    /// <summary>
    /// Lists enrollments a page at a time.
    /// </summary>
    public override async Task<ApiBenefits.ListPendingEnrollmentsResponse> ListPendingEnrollments(ApiBenefits.ListPendingEnrollmentsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListPendingEnrollments for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "enrollment", "batch",
            "ListPendingEnrollments", "accepted", token);
        var input0 = new ApiBenefits.CreateBenefitPlanRequest
        {
            BenefitPlan = new ApiBenefits.BenefitPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
                Parameters = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply0 = await SendListPendingEnrollments0Async(input0, token);
        var response = new ApiBenefits.ListPendingEnrollmentsResponse
        {
            Enrollments = { new ApiBenefits.Enrollment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.BenefitPlan?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            } },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "enrollment",
            "audit", "ListPendingEnrollments", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls an enrollment.
    /// </summary>
    public override async Task<ApiBenefits.RecallEnrollmentResponse> RecallEnrollment(ApiBenefits.RecallEnrollmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecallEnrollment for {Tenant}", requestContext.TenantId);
        if (request.Enrollment is { } record)
        {
            var snapshot = EnrollmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiBenefits.BatchGetBenefitPlansRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _benefitsBenefitPlanService.BatchGetBenefitPlansAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiSearch.BatchGetSearchIndexesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiSearch.BatchGetSearchIndexesRequest>();
        using var rpc1 = _searchSearchIndexService.BatchGetSearchIndexesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiItsm.ListIncidentWorkflowsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.ListIncidentWorkflowsRequest.Types.Filter
            {
                CreatedAfter = (request.Enrollment?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.Enrollment?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Enrollment ?? new ApiBenefits.Enrollment()).PaymentCase switch
                {
                    ApiBenefits.Enrollment.FundingOneofCase.ExternalAccountId => (request.Enrollment?.ExternalAccountId ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc2 = _itsmIncidentWorkflowService.ListIncidentWorkflowsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiBenefits.RecallEnrollmentResponse
        {
            Enrollment = new ApiBenefits.Enrollment
            {
                EnrollmentId = (request.EnrollmentId ?? ""),
                Labels = { (request.Enrollment?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Outcome = new ApiBenefits.RecallEnrollmentResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "enrollment",
            "audit", "RecallEnrollment", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects an enrollment.
    /// </summary>
    public override async Task<ApiBenefits.RejectEnrollmentResponse> RejectEnrollment(ApiBenefits.RejectEnrollmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RejectEnrollment for {Tenant}", requestContext.TenantId);
        if (request.Enrollment is { } record)
        {
            var snapshot = EnrollmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiBenefits.SimulateLifeEventResponse();
        foreach (var item in (request.Enrollment?.Limits ?? new Google.Protobuf.Collections.RepeatedField<ApiBenefits.EnrollmentLimit>()))
        {
            var input0 = new ApiBenefits.SimulateLifeEventRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiBenefits.SimulateLifeEventRequest.Types.Details
                {
                    Budget = (item.Amount ?? new ApiCommon.Money()),
                },
                LifeEvent = new ApiBenefits.LifeEvent
                {
                    Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                    Subject = new ApiRecruiting.CandidateRef
                    {
                        CandidateId = (request.Enrollment?.Holder?.CandidateId ?? ""),
                        DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                    },
                },
            };
            using var rpc0 = _benefitsLifeEventCalculationService.SimulateLifeEventAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiDocuments.SubscribeSignatureUpdatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SubscribeSignatureUpdatesRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Enrollment?.Holder?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _documentsSignatureNotificationService.SubscribeSignatureUpdatesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiBenefits.RejectEnrollmentResponse
        {
            Enrollment = new ApiBenefits.Enrollment
            {
                EnrollmentId = (request.EnrollmentId ?? ""),
            },
            Outcome = new ApiBenefits.RejectEnrollmentResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                ReferenceId = (reply1.Outcome?.ReferenceId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "enrollment",
            "audit", "RejectEnrollment", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits an enrollment.
    /// </summary>
    public override async Task<ApiBenefits.SubmitEnrollmentResponse> SubmitEnrollment(ApiBenefits.SubmitEnrollmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitEnrollment for {Tenant}", requestContext.TenantId);
        if (request.Enrollment is { } record)
        {
            var snapshot = EnrollmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiBenefits.EstimateLifeEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    CandidateId = (request.Enrollment?.Holder?.CandidateId ?? ""),
                    Email = (request.Enrollment?.Contact?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _benefitsLifeEventCalculationService.EstimateLifeEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiBenefits.EstimateLifeEventRequest>("acme.benefits.v1.EnrollmentApprovalService/SubmitEnrollment/0", input0.Clone()), token);
        var input1 = new ApiRecruiting.SummarizeJobOpeningsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _recruitingJobOpeningQueryService.SummarizeJobOpeningsAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiBenefits.SubmitEnrollmentResponse
        {
            Enrollment = new ApiBenefits.Enrollment
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.LifeEvent?.Subject?.Email ?? ""),
                },
                CurrencyCode = (request.Enrollment?.CurrencyCode ?? ""),
                EnrollmentId = (request.EnrollmentId ?? ""),
            },
            Outcome = new ApiBenefits.SubmitEnrollmentResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Enrollment?.Holder?.Email ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "enrollment",
            "audit", "SubmitEnrollment", (request.Enrollment?.Holder?.Email ?? ""), token);
        return response;
    }
    private async Task<ApiBenefits.CreateBenefitPlanResponse> SendListPendingEnrollments0Async(ApiBenefits.CreateBenefitPlanRequest request, CancellationToken token)
    {
        return await _enrollmentApprovalEndpointSendListPendingEnrollments0Port.SendAsync(request, token);
    }
}
