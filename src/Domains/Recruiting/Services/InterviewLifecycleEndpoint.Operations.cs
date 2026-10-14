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
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Recruiting;

public sealed partial class InterviewLifecycleEndpoint
{
    /// <summary>
    /// Activates an interview.
    /// </summary>
    public override async Task<ApiRecruiting.ActivateInterviewResponse> ActivateInterview(ApiRecruiting.ActivateInterviewRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.InterviewId, "interview_id");
        _logger.LogInformation("Handling ActivateInterview for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "interview", request.InterviewId,
            "ActivateInterview", "accepted", token);
        var profile0 = new MappingProfile<ApiRecruiting.ActivateInterviewRequest, ApiRecruiting.GetInterviewRulesRequest>(value => new ApiRecruiting.GetInterviewRulesRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            InterviewId = (value.InterviewId ?? ""),
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _recruitingInterviewValidationService.GetInterviewRulesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiRecruiting.ActivateInterviewResponse
        {
            Interview = new ApiRecruiting.Interview
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Interview?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                InterviewId = (reply0.Interview?.InterviewId ?? ""),
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
            },
            Outcome = new ApiRecruiting.ActivateInterviewResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "ActivateInterview", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Expires an interview.
    /// </summary>
    public override async Task<ApiRecruiting.ExpireInterviewResponse> ExpireInterview(ApiRecruiting.ExpireInterviewRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.InterviewId, "interview_id");
        _logger.LogInformation("Handling ExpireInterview for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "interview", request.InterviewId,
            "ExpireInterview", "accepted", token);
        var input0 = new ApiLearning.ResumeCertificationWorkflowRequest
        {
            Certification = new ApiLearning.Certification
            {
                Counterparty = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLearning.ResumeCertificationWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _learningCertificationWorkflowService.ResumeCertificationWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.ExportOrgUnitsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.ExportOrgUnitsRequest.Types.Filter
            {
                Region = (reply0.Certification?.BillingAddress?.Region ?? ""),
            },
        };
        using var stream1 = _employeesOrgUnitQueryService.ExportOrgUnits(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportOrgUnits frame {Type}", change1.GetType().Name);
        }
        var response = new ApiRecruiting.ExpireInterviewResponse
        {
            Interview = new ApiRecruiting.Interview
            {
                InterviewId = (request.InterviewId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    Region = (reply0.Certification?.BillingAddress?.Region ?? ""),
                },
            },
            Outcome = new ApiRecruiting.ExpireInterviewResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "ExpireInterview", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an interview.
    /// </summary>
    public override async Task<ApiRecruiting.GetInterviewLifecycleResponse> GetInterviewLifecycle(ApiRecruiting.GetInterviewLifecycleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.InterviewId, "interview_id");
        _logger.LogInformation("Handling GetInterviewLifecycle for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "interview", request.InterviewId,
            "GetInterviewLifecycle", "accepted", token);
        var input0 = new ApiRecruiting.PreviewJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.PreviewJobOpeningRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Counterparty = new ApiRecruiting.InterviewRef
                {
                    InterviewId = (request.InterviewId ?? ""),
                },
            },
        };
        var reply0 = await SendGetInterviewLifecycle0Async(input0, token);
        var response = new ApiRecruiting.GetInterviewLifecycleResponse
        {
            Interview = new ApiRecruiting.Interview
            {
                InterviewId = (request.InterviewId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.JobOpening?.BillingAddress?.PostalCode ?? ""),
                },
            },
        };
        return response;
    }
    /// <summary>
    /// Renews an interview.
    /// </summary>
    public override async Task<ApiRecruiting.RenewInterviewResponse> RenewInterview(ApiRecruiting.RenewInterviewRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.InterviewId, "interview_id");
        _logger.LogInformation("Handling RenewInterview for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "interview", request.InterviewId,
            "RenewInterview", "accepted", token);
        var input0 = new ApiIdentity.PeekUserRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            UserId = (request.Details?.NewOwner?.UserId ?? ""),
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _identityUserQueryService.PeekUserAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiOnboarding.ShareOnboardingTaskReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.ShareOnboardingTaskReportRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                },
            },
        };
        using var rpc1 = _onboardingOnboardingTaskReportingService.ShareOnboardingTaskReportAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.RenewInterviewResponse
        {
            Interview = new ApiRecruiting.Interview
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.User?.Audit?.UpdatedBy ?? ""),
                },
                InterviewId = (request.InterviewId ?? ""),
                Organizer = new ApiIdentity.UserRef
                {
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
            },
            Outcome = new ApiRecruiting.RenewInterviewResponse.Types.Outcome
            {
                EffectiveAt = (reply1.Outcome?.EffectiveAt ?? new Timestamp()),
                NextSteps = { (reply1.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "RenewInterview", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules an interview.
    /// </summary>
    public override async Task<ApiRecruiting.ScheduleInterviewExpiryResponse> ScheduleInterviewExpiry(ApiRecruiting.ScheduleInterviewExpiryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScheduleInterviewExpiry for {Tenant}", requestContext.TenantId);
        if (request.Interview is { } record)
        {
            var snapshot = InterviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.AdvanceTicketWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.AdvanceTicketWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                },
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Interview?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (request.Interview?.Audit?.UpdatedBy ?? ""),
                },
                Comments = { new ApiHelpdesk.TicketComment
                {
                    CreatedAt = (request.Interview?.Audit?.CreatedAt ?? new Timestamp()),
                } },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiHelpdesk.AdvanceTicketWorkflowRequest>(json0);
        using var rpc0 = _helpdeskTicketWorkflowService.AdvanceTicketWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiNotifications.FindNotificationsRequest();
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        input1.Filter = new ApiNotifications.FindNotificationsRequest.Types.Filter
        {
            Region = (request.Interview?.Location?.Region ?? ""),
        };
        using var rpc1 = _notificationsNotificationQueryService.FindNotificationsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.ScheduleInterviewExpiryResponse
        {
            Interview = new ApiRecruiting.Interview
            {
                InterviewId = (request.InterviewId ?? ""),
                Participants = { new ApiRecruiting.InterviewParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply0.Ticket?.Requester?.Email ?? ""),
                    },
                } },
                Status = (request.Interview?.Status ?? (ApiRecruiting.InterviewStatus)0),
            },
            Outcome = new ApiRecruiting.ScheduleInterviewExpiryResponse.Types.Outcome
            {
                EffectiveAt = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
                Message = (reply1.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "ScheduleInterviewExpiry", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Suspends an interview.
    /// </summary>
    public override async Task<ApiRecruiting.SuspendInterviewResponse> SuspendInterview(ApiRecruiting.SuspendInterviewRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.InterviewId, "interview_id");
        _logger.LogInformation("Handling SuspendInterview for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "interview", request.InterviewId,
            "SuspendInterview", "accepted", token);
        var input0 = new ApiEmployees.ExplainPositionDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.ExplainPositionDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
            Position = new ApiEmployees.Position
            {
                Block = new ApiEmployees.PositionBlockAction
                {
                    Reason = (request.Reason ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
            },
        };
        using var rpc0 = _employeesPositionValidationService.ExplainPositionDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.SimulateOfferRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Offer = new ApiRecruiting.Offer
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Position?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _recruitingOfferCalculationService.SimulateOfferAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiOnboarding.PauseEquipmentWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Equipment = new ApiOnboarding.Equipment
            {
                Address = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply1.Offer?.BillingAddress?.CountryCode ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.Offer?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (reply0.Position?.Owner?.UserId ?? ""),
                },
            },
        };
        var reply2 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _onboardingEquipmentWorkflowService.PauseEquipmentWorkflowAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input3 = new ApiDocuments.CalculateSignatureRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.CalculateSignatureRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply2.Equipment ?? new ApiOnboarding.Equipment()).LocatorCase switch
                    {
                        ApiOnboarding.Equipment.LocatorOneofCase.Address => (reply2.Equipment?.Address?.PostalCode ?? ""),
                        _ => ""
                    },
                },
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (reply0.Position?.Owner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc3 = _documentsSignatureCalculationService.CalculateSignatureAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiRecruiting.SuspendInterviewResponse
        {
            Interview = new ApiRecruiting.Interview
            {
                InterviewId = (request.InterviewId ?? ""),
                Organizer = new ApiIdentity.UserRef
                {
                    UserId = (reply0.Position?.Owner?.UserId ?? ""),
                },
            },
            Outcome = new ApiRecruiting.SuspendInterviewResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply3.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.ReasonCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "SuspendInterview", (request.Details?.ReasonCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Terminates an interview.
    /// </summary>
    public override async Task<ApiRecruiting.TerminateInterviewResponse> TerminateInterview(ApiRecruiting.TerminateInterviewRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.InterviewId, "interview_id");
        _logger.LogInformation("Handling TerminateInterview for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "interview", request.InterviewId,
            "TerminateInterview", "accepted", token);
        var input0 = new ApiRecruiting.SyncOffersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.SyncOffersRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Offer = new ApiRecruiting.Offer
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _recruitingOfferSyncService.SyncOffersAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.ReconcileOffersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.ReconcileOffersRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Offer = new ApiRecruiting.Offer
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.Offer?.BillingAddress?.Recipient ?? ""),
                },
                Card = new ApiRecruiting.OfferCardSettlement
                {
                    HolderName = (reply0.Offer ?? new ApiRecruiting.Offer()).PaymentCase switch
                    {
                        ApiRecruiting.Offer.SettlementOneofCase.Card => (reply0.Offer?.Card?.HolderName ?? ""),
                        _ => ""
                    },
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _recruitingOfferSyncService.ReconcileOffersAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.TerminateInterviewResponse
        {
            Interview = new ApiRecruiting.Interview
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Offer?.Audit?.CreatedBy ?? ""),
                },
                InterviewId = (request.InterviewId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    Recipient = (reply1.Offer?.BillingAddress?.Recipient ?? ""),
                },
            },
            Outcome = new ApiRecruiting.TerminateInterviewResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                NextSteps = { (reply0.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "TerminateInterview", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiRecruiting.PreviewJobOpeningResponse> SendGetInterviewLifecycle0Async(ApiRecruiting.PreviewJobOpeningRequest request, CancellationToken token)
    {
        return await _interviewLifecycleEndpointSendGetInterviewLifecycle0Port.SendAsync(request, token);
    }
}
