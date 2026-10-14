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
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiPerformance = Acme.Performance.V1;

namespace Acme.Workforce.Onboarding;

public sealed partial class OnboardingTaskValidationEndpoint
{
    /// <summary>
    /// Checks an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.CheckOnboardingTaskEligibilityResponse> CheckOnboardingTaskEligibility(ApiOnboarding.CheckOnboardingTaskEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CheckOnboardingTaskEligibility for {Tenant}", requestContext.TenantId);
        if (request.OnboardingTask is { } record)
        {
            var snapshot = OnboardingTaskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.BrowseCorporateEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiCorporateevents.BrowseCorporateEventsRequest.Types.Filter
            {
                CreatedAfter = (request.OnboardingTask?.Sla?.DueAt ?? new Timestamp()),
            },
        };
        var descriptor0 = new Method<ApiCorporateevents.BrowseCorporateEventsRequest, ApiCorporateevents.BrowseCorporateEventsResponse>(
            MethodType.Unary, "acme.corporateevents.v1" + ".CorporateEventQueryService", "BrowseCorporateEvents",
            Wire.Marshaller<ApiCorporateevents.BrowseCorporateEventsRequest>(), Wire.Marshaller<ApiCorporateevents.BrowseCorporateEventsResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiOnboarding.ExplainEquipmentCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.ExplainEquipmentCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.OnboardingTask?.Requester?.Email ?? ""),
                    Phone = (request.Details?.Contact?.Phone ?? ""),
                },
            },
            Equipment = new ApiOnboarding.Equipment
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
        };
        var command1 = new DispatchCommand<ApiOnboarding.ExplainEquipmentCalculationRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _onboardingEquipmentCalculationService.ExplainEquipmentCalculationAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.CheckOnboardingTaskEligibilityResponse
        {
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.Equipment?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                OnboardingTaskId = (request.OnboardingTaskId ?? ""),
                Priority = (request.OnboardingTask?.Priority ?? 0),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "CheckOnboardingTaskEligibility", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.ExplainOnboardingTaskDecisionResponse> ExplainOnboardingTaskDecision(ApiOnboarding.ExplainOnboardingTaskDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainOnboardingTaskDecision for {Tenant}", requestContext.TenantId);
        if (request.OnboardingTask is { } record)
        {
            var snapshot = OnboardingTaskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.OnboardingTask?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiOnboarding.OnboardingTaskComment>()).Select(async item =>
        {
            var input0 = new ApiOnboarding.QuoteOnboardingPlanRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                OnboardingPlan = new ApiOnboarding.OnboardingPlan
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (item.CreatedAt ?? new Timestamp()),
                    },
                },
            };
            using var rpc0 = _onboardingOnboardingPlanCalculationService.QuoteOnboardingPlanAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiOnboarding.QuoteOnboardingPlanResponse();
        var input1 = new ApiOnboarding.CalculateOnboardingPlanRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.CalculateOnboardingPlanRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.OnboardingPlan?.BillingAddress?.Locality ?? ""),
                },
            },
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.OnboardingTask?.Audit?.CreatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.OnboardingPlan?.BillingAddress?.CountryCode ?? ""),
                },
                Counterparty = new ApiOnboarding.EquipmentRef
                {
                    DisplayName = (reply0.OnboardingPlan?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _onboardingOnboardingPlanCalculationService.CalculateOnboardingPlanAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        var input2 = new ApiOnboarding.ExplainOnboardingPlanCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.ExplainOnboardingPlanCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.OnboardingTask?.Requester?.Phone ?? ""),
                    PreferredChannel = (request.OnboardingTask?.Requester?.PreferredChannel ?? ""),
                },
            },
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.OnboardingPlan?.BillingAddress?.CountryCode ?? ""),
                    PostalCode = (reply0.OnboardingPlan?.BillingAddress?.PostalCode ?? ""),
                },
            },
        };
        using var rpc2 = _onboardingOnboardingPlanCalculationService.ExplainOnboardingPlanCalculationAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        await Task.WhenAll(pending1, pending2);
        var reply1 = await pending1;
        var reply2 = await pending2;
        var response = new ApiOnboarding.ExplainOnboardingTaskDecisionResponse
        {
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.OnboardingPlan?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                OnboardingTaskId = (request.OnboardingTaskId ?? ""),
                Subject = (request.OnboardingTask?.Subject ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "ExplainOnboardingTaskDecision", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.GetOnboardingTaskRulesResponse> GetOnboardingTaskRules(ApiOnboarding.GetOnboardingTaskRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OnboardingTaskId, "onboarding_task_id");
        _logger.LogInformation("Handling GetOnboardingTaskRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task", request.OnboardingTaskId,
            "GetOnboardingTaskRules", "accepted", token);
        var input0 = new ApiBenefits.FindBenefitPlansRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiBenefits.FindBenefitPlansRequest.Types.Filter
            {
                OwnerId = (request.OnboardingTaskId ?? ""),
            },
        };
        var reply0 = await SendGetOnboardingTaskRules0Async(input0, token);
        var response = new ApiOnboarding.GetOnboardingTaskRulesResponse
        {
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                OnboardingTaskId = (request.OnboardingTaskId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Lints an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.LintOnboardingTasksResponse> LintOnboardingTasks(ApiOnboarding.LintOnboardingTasksRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling LintOnboardingTasks for {Tenant}", requestContext.TenantId);
        if (request.OnboardingTask is { } record)
        {
            var snapshot = OnboardingTaskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiAuthorization.CreateAccessPolicyResponse();
        foreach (var item in (request.OnboardingTask?.History ?? new Google.Protobuf.Collections.RepeatedField<ApiOnboarding.OnboardingTaskStatusChange>()))
        {
            var input0 = new ApiAuthorization.CreateAccessPolicyRequest
            {
                AccessPolicy = new ApiAuthorization.AccessPolicy
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        UpdatedAt = (item.ChangedAt ?? new Timestamp()),
                    },
                    Owner = new ApiIdentity.UserRef
                    {
                        DisplayName = (request.OnboardingTask?.Customer?.DisplayName ?? ""),
                    },
                },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
            };
            using var rpc0 = _authorizationAccessPolicyService.CreateAccessPolicyAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiOnboarding.NormalizeOnboardingPlanRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.NormalizeOnboardingPlanRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Contact?.Phone ?? ""),
                    PreferredChannel = (request.OnboardingTask?.Requester?.PreferredChannel ?? ""),
                },
            },
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Lines = { new ApiOnboarding.OnboardingPlanLine
                {
                    Tax = new ApiOnboarding.OnboardingPlanLineTax
                    {
                        Amount = (reply0.AccessPolicy ?? new ApiAuthorization.AccessPolicy()).ActionCase switch
                        {
                            ApiAuthorization.AccessPolicy.ActionOneofCase.Discount => (reply0.AccessPolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc1 = _onboardingOnboardingPlanValidationService.NormalizeOnboardingPlanAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiOnboarding.CheckOnboardingPlanEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.CheckOnboardingPlanEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.OnboardingTask?.Requester?.Email ?? ""),
                },
            },
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Counterparty = new ApiOnboarding.EquipmentRef
                {
                    DisplayName = (request.OnboardingTask ?? new ApiOnboarding.OnboardingTask()).OriginCase switch
                    {
                        ApiOnboarding.OnboardingTask.OriginOneofCase.Customer => (request.OnboardingTask?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc2 = _onboardingOnboardingPlanValidationService.CheckOnboardingPlanEligibilityAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiOnboarding.LintOnboardingTasksResponse
        {
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.OnboardingPlan?.Audit?.Etag ?? ""),
                },
                OnboardingTaskId = (request.OnboardingTaskId ?? ""),
                Priority = (request.OnboardingTask?.Priority ?? 0),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply2.Outcome?.Message ?? ""),
                Status = (reply1.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "LintOnboardingTasks", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.PreviewOnboardingTaskResponse> PreviewOnboardingTask(ApiOnboarding.PreviewOnboardingTaskRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PreviewOnboardingTask for {Tenant}", requestContext.TenantId);
        if (request.OnboardingTask is { } record)
        {
            var snapshot = OnboardingTaskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiOnboarding.SimulateOnboardingPlanRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.OnboardingPlan = new ApiOnboarding.OnboardingPlan
        {
            Audit = new ApiCommon.AuditInfo
            {
                UpdatedAt = (request.OnboardingTask?.Audit?.UpdatedAt ?? new Timestamp()),
            },
        };
        using var rpc0 = _onboardingOnboardingPlanCalculationService.SimulateOnboardingPlanAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.ResolveIncidentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _itsmIncidentQueryService.ResolveIncidentsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.PreviewOnboardingTaskResponse
        {
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                OnboardingTaskId = (request.OnboardingTaskId ?? ""),
                Status = (request.OnboardingTask?.Status ?? (ApiOnboarding.OnboardingTaskStatus)0),
            },
            Outcome = new ApiOnboarding.PreviewOnboardingTaskResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "PreviewOnboardingTask", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.ScoreOnboardingTaskResponse> ScoreOnboardingTask(ApiOnboarding.ScoreOnboardingTaskRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScoreOnboardingTask for {Tenant}", requestContext.TenantId);
        if (request.OnboardingTask is { } record)
        {
            var snapshot = OnboardingTaskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiOnboarding.QuoteOnboardingPlanRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.OnboardingTask?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (request.OnboardingTask?.Audit?.CreatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _onboardingOnboardingPlanCalculationService.QuoteOnboardingPlanAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiCorporateevents.ExplainRegistrationCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Registration = new ApiCorporateevents.Registration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.OnboardingPlan?.Audit?.Etag ?? ""),
                },
                Card = new ApiCorporateevents.RegistrationCardFunding
                {
                    Last4 = (reply0.OnboardingPlan ?? new ApiOnboarding.OnboardingPlan()).PaymentCase switch
                    {
                        ApiOnboarding.OnboardingPlan.SettlementOneofCase.Card => (reply0.OnboardingPlan?.Card?.Last4 ?? ""),
                        _ => ""
                    },
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.OnboardingTask?.Requester?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (request.OnboardingTask?.Requester?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc1 = _corporateeventsRegistrationCalculationService.ExplainRegistrationCalculationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.ScoreOnboardingTaskResponse
        {
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Registration?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (reply0.OnboardingPlan?.Audit?.CreatedBy ?? ""),
                    UpdatedBy = (reply1.Registration?.Audit?.UpdatedBy ?? ""),
                },
                OnboardingTaskId = (request.OnboardingTaskId ?? ""),
                Requester = (request.OnboardingTask?.Requester ?? new ApiCommon.ContactInfo()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "ScoreOnboardingTask", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.ValidateOnboardingTaskResponse> ValidateOnboardingTask(ApiOnboarding.ValidateOnboardingTaskRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ValidateOnboardingTask for {Tenant}", requestContext.TenantId);
        if (request.OnboardingTask is { } record)
        {
            var snapshot = OnboardingTaskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiOnboarding.ExplainOnboardingPlanDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.ExplainOnboardingPlanDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.OnboardingTask?.Assignee?.Email ?? ""),
                },
            },
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.OnboardingTask?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _onboardingOnboardingPlanValidationService.ExplainOnboardingPlanDecisionAsync);
        var input1 = new ApiItsm.LintProblemsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Problem = new ApiItsm.Problem
            {
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.OnboardingTask?.Assignee?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _sendValidateOnboardingTask1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiOnboarding.SimulateOnboardingPlanRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.Problem?.Audit?.CreatedBy ?? ""),
                    UpdatedBy = (reply1.Problem?.Audit?.UpdatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.OnboardingPlan?.BillingAddress?.Locality ?? ""),
                },
                Card = new ApiOnboarding.OnboardingPlanCardSettlement
                {
                    Token = (reply0.OnboardingPlan ?? new ApiOnboarding.OnboardingPlan()).PaymentCase switch
                    {
                        ApiOnboarding.OnboardingPlan.SettlementOneofCase.Card => (reply0.OnboardingPlan?.Card?.Token ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc2 = _onboardingOnboardingPlanCalculationService.SimulateOnboardingPlanAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var reply3 = new ApiPerformance.PreviewGoalResponse();
        foreach (var item in (request.OnboardingTask?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiOnboarding.OnboardingTaskComment>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input3 = new ApiPerformance.PreviewGoalRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Goal = new ApiPerformance.Goal
                {
                    Comments = { new ApiPerformance.GoalComment
                    {
                        Body = (item.Body ?? ""),
                        CreatedAt = (item.CreatedAt ?? new Timestamp()),
                        Visibility = (item.Visibility ?? ""),
                    } },
                    Requester = new ApiCommon.ContactInfo
                    {
                        Email = (item.AuthorEmail ?? ""),
                    },
                },
            };
            using var rpc3 = _performanceGoalValidationService.PreviewGoalAsync(input3, cancellationToken: token);
            reply3 = await rpc3.ResponseAsync;
        }
        var response = new ApiOnboarding.ValidateOnboardingTaskResponse
        {
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply1.Problem?.Requester?.Email ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply2.OnboardingPlan?.Audit?.Etag ?? ""),
                },
                OnboardingTaskId = (request.OnboardingTaskId ?? ""),
                Subject = (request.OnboardingTask?.Subject ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "ValidateOnboardingTask", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiBenefits.FindBenefitPlansResponse> SendGetOnboardingTaskRules0Async(ApiBenefits.FindBenefitPlansRequest request, CancellationToken token)
    {
        return await _onboardingTaskValidationEndpointSendGetOnboardingTaskRules0Port.SendAsync(request, token);
    }
}
