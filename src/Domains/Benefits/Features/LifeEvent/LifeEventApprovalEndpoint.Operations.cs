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
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiMessaging = Acme.Messaging.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;
using ApiTelematics = Acme.Telematics.V1;

namespace Acme.Workforce.Benefits;

public sealed partial class LifeEventApprovalEndpoint
{
    /// <summary>
    /// Approves a life event.
    /// </summary>
    public override async Task<ApiBenefits.ApproveLifeEventResponse> ApproveLifeEvent(ApiBenefits.ApproveLifeEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ApproveLifeEvent for {Tenant}", requestContext.TenantId);
        if (request.LifeEvent is { } record)
        {
            var snapshot = LifeEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLearning.RejectLearningPathRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLearning.RejectLearningPathRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                },
            },
            LearningPath = new ApiLearning.LearningPath
            {
                Children = { new ApiLearning.LearningPath
                {
                    Children = { new ApiLearning.LearningPath
                    {
                        Manager = new ApiIdentity.UserRef
                        {
                            Email = (request.Details?.Approver?.Email ?? ""),
                            UserId = (request.Details?.Approver?.UserId ?? ""),
                        },
                    } },
                } },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiLearning.RejectLearningPathRequest>(json0);
        using var rpc0 = _learningLearningPathApprovalService.RejectLearningPathAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiBenefits.NormalizeDependentRequest();
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        input1.Dependent = new ApiBenefits.Dependent
        {
            RelationshipManager = new ApiIdentity.UserRef
            {
                DisplayName = (reply0.LearningPath?.Manager?.DisplayName ?? ""),
            },
        };
        input1.Details = new ApiBenefits.NormalizeDependentRequest.Types.Details
        {
            Contact = new ApiCommon.ContactInfo
            {
                Email = (request.LifeEvent?.Subject?.Email ?? ""),
            },
        };
        using var rpc1 = _benefitsDependentValidationService.NormalizeDependentAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiBenefits.ApproveLifeEventResponse
        {
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Attributes = { (reply1.Dependent?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                LifeEventId = (request.LifeEventId ?? ""),
                OccurredAt = (request.LifeEvent?.OccurredAt ?? new Timestamp()),
            },
            Outcome = new ApiBenefits.ApproveLifeEventResponse.Types.Outcome
            {
                NextSteps = { (reply0.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
                Status = (reply1.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "life_event",
            "audit", "ApproveLifeEvent", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates a life event.
    /// </summary>
    public override async Task<ApiBenefits.DelegateLifeEventApprovalResponse> DelegateLifeEventApproval(ApiBenefits.DelegateLifeEventApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling DelegateLifeEventApproval for {Tenant}", requestContext.TenantId);
        if (request.LifeEvent is { } record)
        {
            var snapshot = LifeEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiSearch.SuggestSearchIndexesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiSearch.SuggestSearchIndexesRequest.Types.Filter
            {
                CreatedAfter = (request.LifeEvent?.OccurredAt ?? new Timestamp()),
            },
        };
        using var rpc0 = _searchSearchIndexQueryService.SuggestSearchIndexesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiBenefits.DelegateLifeEventApprovalResponse
        {
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Attributes = { (request.LifeEvent?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                LifeEventId = (request.LifeEventId ?? ""),
            },
            Outcome = new ApiBenefits.DelegateLifeEventApprovalResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "life_event",
            "audit", "DelegateLifeEventApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates a life event.
    /// </summary>
    public override async Task<ApiBenefits.EscalateLifeEventResponse> EscalateLifeEvent(ApiBenefits.EscalateLifeEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EscalateLifeEvent for {Tenant}", requestContext.TenantId);
        if (request.LifeEvent is { } record)
        {
            var snapshot = LifeEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.CalculateJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                Counterparty = new ApiRecruiting.InterviewRef
                {
                    DisplayName = (request.LifeEvent?.Subject?.DisplayName ?? ""),
                },
                Lines = { new ApiRecruiting.JobOpeningLine
                {
                    Attributes = { (request.LifeEvent?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                } },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _recruitingJobOpeningCalculationService.CalculateJobOpeningAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiRecruiting.SimulateJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.SimulateJobOpeningRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.JobOpening?.BillingAddress?.Recipient ?? ""),
                    Region = (reply0.JobOpening?.BillingAddress?.Region ?? ""),
                },
            },
            JobOpening = new ApiRecruiting.JobOpening
            {
                Bank = new ApiRecruiting.JobOpeningBankSettlement
                {
                    Bic = (reply0.JobOpening ?? new ApiRecruiting.JobOpening()).PaymentCase switch
                    {
                        ApiRecruiting.JobOpening.SettlementOneofCase.Bank => (reply0.JobOpening?.Bank?.Bic ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _recruitingJobOpeningCalculationService.SimulateJobOpeningAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiHelpdesk.ApproveTicketRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.ApproveTicketRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.JobOpening?.Counterparty?.DisplayName ?? ""),
                },
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.JobOpening?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Customer = new ApiHelpdesk.KnowledgeArticleRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                },
            },
        };
        var reply2 = await Workflow.SendAsync(input2, token, _helpdeskTicketApprovalService.ApproveTicketAsync);
        var response = new ApiBenefits.EscalateLifeEventResponse
        {
            LifeEvent = new ApiBenefits.LifeEvent
            {
                CorrelationId = (request.LifeEvent?.CorrelationId ?? ""),
                LifeEventId = (request.LifeEventId ?? ""),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (reply0.JobOpening?.Counterparty?.DisplayName ?? ""),
                    Email = (reply2.Ticket?.Assignee?.Email ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "life_event",
            "audit", "EscalateLifeEvent", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a life event.
    /// </summary>
    public override async Task<ApiBenefits.GetLifeEventApprovalStatusResponse> GetLifeEventApprovalStatus(ApiBenefits.GetLifeEventApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.LifeEventId, "life_event_id");
        _logger.LogInformation("Handling GetLifeEventApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "life_event", request.LifeEventId,
            "GetLifeEventApprovalStatus", "accepted", token);
        var input0 = new ApiRecruiting.ExplainCandidateDecisionRequest
        {
            Candidate = new ApiRecruiting.Candidate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.ExplainCandidateDecisionRequest.Types.Details
            {
                Locale = (request.Context?.Locale ?? ""),
            },
        };
        using var rpc0 = _sendGetLifeEventApprovalStatus0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiBenefits.GetLifeEventApprovalStatusResponse
        {
            LifeEvent = new ApiBenefits.LifeEvent
            {
                LifeEventId = (request.LifeEventId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists life events a page at a time.
    /// </summary>
    public override async Task<ApiBenefits.ListPendingLifeEventsResponse> ListPendingLifeEvents(ApiBenefits.ListPendingLifeEventsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListPendingLifeEvents for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "life_event", "batch",
            "ListPendingLifeEvents", "accepted", token);
        var response = new ApiBenefits.ListPendingLifeEventsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "life_event",
            "audit", "ListPendingLifeEvents", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls a life event.
    /// </summary>
    public override async Task<ApiBenefits.RecallLifeEventResponse> RecallLifeEvent(ApiBenefits.RecallLifeEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecallLifeEvent for {Tenant}", requestContext.TenantId);
        if (request.LifeEvent is { } record)
        {
            var snapshot = LifeEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.LintOffersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Offer = new ApiRecruiting.Offer
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.LifeEvent?.Subject?.DisplayName ?? ""),
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
        };
        var named0 = _registry.Get<ApiRecruiting.OfferValidationService.OfferValidationServiceClient>("acme.recruiting.v1.OfferValidationService");
        using var rpc0 = named0.LintOffersAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.SimulateJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.SimulateJobOpeningRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.Offer?.BillingAddress?.CountryCode ?? ""),
                },
            },
            JobOpening = new ApiRecruiting.JobOpening
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.Offer?.BillingAddress?.Recipient ?? ""),
                },
            },
        };
        var descriptor1 = new Method<ApiRecruiting.SimulateJobOpeningRequest, ApiRecruiting.SimulateJobOpeningResponse>(
            MethodType.Unary, "acme.recruiting.v1" + ".JobOpeningCalculationService", "SimulateJobOpening",
            Wire.Marshaller<ApiRecruiting.SimulateJobOpeningRequest>(), Wire.Marshaller<ApiRecruiting.SimulateJobOpeningResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiRecruiting.CheckInterviewEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Interview = new ApiRecruiting.Interview
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.Offer?.Audit?.UpdatedBy ?? ""),
                },
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (request.LifeEvent?.Subject?.Email ?? ""),
                },
            },
        };
        var command2 = new DispatchCommand<ApiRecruiting.CheckInterviewEligibilityRequest>(input2,
            "primary", requestContext.TenantId);
        var routed2 = command2 with { Region = _settings.Value.Region };
        using var rpc2 = _recruitingInterviewValidationService.CheckInterviewEligibilityAsync(routed2.Payload, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiBenefits.RecallLifeEventResponse
        {
            LifeEvent = new ApiBenefits.LifeEvent
            {
                LifeEventId = (request.LifeEventId ?? ""),
                Subject = (request.LifeEvent?.Subject ?? new ApiRecruiting.CandidateRef()),
            },
            Outcome = new ApiBenefits.RecallLifeEventResponse.Types.Outcome
            {
                Message = (reply0.Outcome?.Message ?? ""),
                Status = (reply2.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "life_event",
            "audit", "RecallLifeEvent", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects a life event.
    /// </summary>
    public override async Task<ApiBenefits.RejectLifeEventResponse> RejectLifeEvent(ApiBenefits.RejectLifeEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RejectLifeEvent for {Tenant}", requestContext.TenantId);
        if (request.LifeEvent is { } record)
        {
            var snapshot = LifeEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiBenefits.RejectLifeEventRequest, ApiBenefits.CommentEnrollmentApprovalRequest>(value => new ApiBenefits.CommentEnrollmentApprovalRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.CommentEnrollmentApprovalRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (value.Details?.Approver?.DisplayName ?? ""),
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _benefitsEnrollmentApprovalService.CommentEnrollmentApprovalAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiMessaging.ListConversationsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiMessaging.ListConversationsRequest.Types.Filter
            {
                CreatedAfter = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
                Labels = { (reply0.Enrollment?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        using var rpc1 = _messagingConversationService.ListConversationsAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        var input2 = new ApiTelematics.SimulateTelemetryReadingRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TelemetryReading = new ApiTelematics.TelemetryReading
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Source = (request.LifeEvent?.Source ?? ""),
                Subject = new ApiTelematics.GeoFenceRef
                {
                    DisplayName = (reply0.Enrollment?.Holder?.DisplayName ?? ""),
                },
            },
        };
        using var rpc2 = _telematicsTelemetryReadingCalculationService.SimulateTelemetryReadingAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        await Task.WhenAll(pending1, pending2);
        var reply1 = await pending1;
        var reply2 = await pending2;
        var input3 = new ApiBenefits.NormalizeLifeEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.NormalizeLifeEventRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (reply0.Enrollment?.Contact?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (reply0.Enrollment?.Contact?.PreferredChannel ?? ""),
                },
            },
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
        };
        using var rpc3 = _benefitsLifeEventValidationService.NormalizeLifeEventAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiBenefits.RejectLifeEventResponse
        {
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Attributes = { (request.LifeEvent?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                LifeEventId = (request.LifeEventId ?? ""),
            },
            Outcome = new ApiBenefits.RejectLifeEventResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
                NextSteps = { (reply0.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.TelemetryReading?.CorrelationId ?? ""),
                Status = (reply3.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "life_event",
            "audit", "RejectLifeEvent", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits a life event.
    /// </summary>
    public override async Task<ApiBenefits.SubmitLifeEventResponse> SubmitLifeEvent(ApiBenefits.SubmitLifeEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitLifeEvent for {Tenant}", requestContext.TenantId);
        if (request.LifeEvent is { } record)
        {
            var snapshot = LifeEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.CalculateJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                Counterparty = new ApiRecruiting.InterviewRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _recruitingJobOpeningCalculationService.CalculateJobOpeningAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiRecruiting.CalculateJobOpeningRequest>("acme.benefits.v1.LifeEventApprovalService/SubmitLifeEvent/0", input0.Clone()), token);
        var input1 = new ApiRecruiting.ExplainCandidateCalculationRequest
        {
            Candidate = new ApiRecruiting.Candidate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.JobOpening?.Audit?.CreatedBy ?? ""),
                    UpdatedBy = (reply0.JobOpening?.Audit?.UpdatedBy ?? ""),
                },
                Passport = new ApiRecruiting.CandidatePassport
                {
                    Number = (reply0.JobOpening?.Number ?? ""),
                },
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.JobOpening?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiRecruiting.ExplainCandidateCalculationRequest>();
        using var rpc1 = _recruitingCandidateCalculationService.ExplainCandidateCalculationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiBenefits.SubmitLifeEventResponse
        {
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Attributes = { (request.LifeEvent?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                LifeEventId = (request.LifeEventId ?? ""),
            },
            Outcome = new ApiBenefits.SubmitLifeEventResponse.Types.Outcome
            {
                Message = (reply1.Outcome?.Message ?? ""),
                NextSteps = { (reply1.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "life_event",
            "audit", "SubmitLifeEvent", (request.Reason ?? ""), token);
        return response;
    }
}
