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
using ApiAudiences = Acme.Audiences.V1;
using ApiBenefits = Acme.Benefits.V1;
using ApiChat = Acme.Chat.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiDrivers = Acme.Drivers.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Onboarding;

public sealed partial class OnboardingPlanValidationEndpoint
{
    /// <summary>
    /// Checks an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.CheckOnboardingPlanEligibilityResponse> CheckOnboardingPlanEligibility(ApiOnboarding.CheckOnboardingPlanEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CheckOnboardingPlanEligibility for {Tenant}", requestContext.TenantId);
        if (request.OnboardingPlan is { } record)
        {
            var snapshot = OnboardingPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiOnboarding.EstimateOnboardingTaskRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.EstimateOnboardingTaskRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.OnboardingPlan?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _onboardingOnboardingTaskCalculationService.EstimateOnboardingTaskAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiOnboarding.CompareOnboardingTaskOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Customer = new ApiOnboarding.EquipmentRef
                {
                    EquipmentId = (request.OnboardingPlan?.Counterparty?.EquipmentId ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply0.OnboardingTask?.Requester?.Email ?? ""),
                    Name = (reply0.OnboardingTask?.Requester?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (reply0.OnboardingTask?.Requester?.PreferredChannel ?? ""),
                },
            },
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiOnboarding.CompareOnboardingTaskOptionsRequest>();
        using var rpc1 = _onboardingOnboardingTaskCalculationService.CompareOnboardingTaskOptionsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiChat.CloneChatRoomRequest
        {
            ChatRoom = new ApiChat.ChatRoom
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.OnboardingPlan?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply1.OnboardingTask?.Assignee?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var json2 = JsonFormatter.Default.Format(input2);
        input2 = JsonParser.Default.Parse<ApiChat.CloneChatRoomRequest>(json2);
        using var rpc2 = _chatChatRoomService.CloneChatRoomAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiOnboarding.CheckOnboardingPlanEligibilityResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Counterparty = new ApiOnboarding.EquipmentRef
                {
                    Code = (reply0.OnboardingTask?.Customer?.Code ?? ""),
                    EquipmentId = (reply1.OnboardingTask?.Customer?.EquipmentId ?? ""),
                },
                Lines = { new ApiOnboarding.OnboardingPlanLine
                {
                    Description = (reply2.ChatRoom?.Components?.GetValueOrDefault("primary")?.Description ?? ""),
                } },
                OnboardingPlanId = (request.OnboardingPlanId ?? ""),
                Status = (request.OnboardingPlan?.Status ?? (ApiOnboarding.OnboardingPlanStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "CheckOnboardingPlanEligibility", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.ExplainOnboardingPlanDecisionResponse> ExplainOnboardingPlanDecision(ApiOnboarding.ExplainOnboardingPlanDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainOnboardingPlanDecision for {Tenant}", requestContext.TenantId);
        if (request.OnboardingPlan is { } record)
        {
            var snapshot = OnboardingPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTimekeeping.PreviewOvertimeRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Details = new ApiTimekeeping.PreviewOvertimeRequest.Types.Details
        {
            Contact = new ApiCommon.ContactInfo
            {
                PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
            },
        };
        input0.Overtime = new ApiTimekeeping.Overtime
        {
            Audit = new ApiCommon.AuditInfo
            {
                CreatedAt = (request.OnboardingPlan?.Audit?.CreatedAt ?? new Timestamp()),
            },
        };
        using var rpc0 = _timekeepingOvertimeValidationService.PreviewOvertimeAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDrivers.CloneDriverRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Driver = new ApiDrivers.Driver
            {
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.OnboardingPlan?.BillingAddress?.PostalCode ?? ""),
                },
            },
        };
        using var rpc1 = _driversDriverService.CloneDriverAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiDrivers.CloneDriverRequest>("acme.onboarding.v1.OnboardingPlanValidationService/ExplainOnboardingPlanDecision/1", input1.Clone()), token);
        var response = new ApiOnboarding.ExplainOnboardingPlanDecisionResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                DueAt = (request.OnboardingPlan?.DueAt ?? new Timestamp()),
                Lines = { new ApiOnboarding.OnboardingPlanLine
                {
                    Description = (reply0.Overtime?.Description ?? ""),
                } },
                OnboardingPlanId = (request.OnboardingPlanId ?? ""),
            },
            Outcome = new ApiOnboarding.ExplainOnboardingPlanDecisionResponse.Types.Outcome
            {
                NextSteps = { (reply0.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "ExplainOnboardingPlanDecision", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.GetOnboardingPlanRulesResponse> GetOnboardingPlanRules(ApiOnboarding.GetOnboardingPlanRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OnboardingPlanId, "onboarding_plan_id");
        _logger.LogInformation("Handling GetOnboardingPlanRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan", request.OnboardingPlanId,
            "GetOnboardingPlanRules", "accepted", token);
        var input0 = new ApiOnboarding.EstimateOnboardingPlanRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Lines = { new ApiOnboarding.OnboardingPlanLine
                {
                    Attributes = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                } },
                OnboardingPlanId = (request.OnboardingPlanId ?? ""),
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _onboardingOnboardingPlanCalculationService.EstimateOnboardingPlanAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiBenefits.BatchGetBenefitPlansRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _benefitsBenefitPlanService.BatchGetBenefitPlansAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiOnboarding.GetOnboardingPlanRulesResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                DueAt = (reply0.OnboardingPlan?.DueAt ?? new Timestamp()),
                OnboardingPlanId = (reply0.OnboardingPlan?.OnboardingPlanId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Normalizes an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.NormalizeOnboardingPlanResponse> NormalizeOnboardingPlan(ApiOnboarding.NormalizeOnboardingPlanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling NormalizeOnboardingPlan for {Tenant}", requestContext.TenantId);
        if (request.OnboardingPlan is { } record)
        {
            var snapshot = OnboardingPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.CompareIncidentOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.CompareIncidentOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Recipient = (request.OnboardingPlan?.BillingAddress?.Recipient ?? ""),
                },
            },
            Incident = new ApiItsm.Incident
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.OnboardingPlan?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _itsmIncidentCalculationService.CompareIncidentOptionsAsync);
        var input1 = new ApiAudiences.ResolveAudiencesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _sendNormalizeOnboardingPlan1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDocuments.RepairDocumentTemplateRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.RepairDocumentTemplateRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.Incident?.Audit?.UpdatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.OnboardingPlan?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                Card = new ApiDocuments.DocumentTemplateCardSettlement
                {
                    HolderName = (request.OnboardingPlan ?? new ApiOnboarding.OnboardingPlan()).PaymentCase switch
                    {
                        ApiOnboarding.OnboardingPlan.SettlementOneofCase.Card => (request.OnboardingPlan?.Card?.HolderName ?? ""),
                        _ => ""
                    },
                },
                Counterparty = new ApiDocuments.DocumentFolderRef
                {
                    DisplayName = (reply0.Incident?.Assignee?.DisplayName ?? ""),
                },
            },
        };
        using var rpc2 = _documentsDocumentTemplateAdminService.RepairDocumentTemplateAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiOnboarding.NormalizeOnboardingPlanResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Incident?.Audit?.CreatedBy ?? ""),
                },
                BillingAddress = (request.OnboardingPlan?.BillingAddress ?? new ApiCommon.PostalAddress()),
                OnboardingPlanId = (request.OnboardingPlanId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "NormalizeOnboardingPlan", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.PreviewOnboardingPlanResponse> PreviewOnboardingPlan(ApiOnboarding.PreviewOnboardingPlanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PreviewOnboardingPlan for {Tenant}", requestContext.TenantId);
        if (request.OnboardingPlan is { } record)
        {
            var snapshot = OnboardingPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiOnboarding.CompareOnboardingPlanOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.CompareOnboardingPlanOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.OnboardingPlan?.BillingAddress?.CountryCode ?? ""),
                },
            },
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (request.OnboardingPlan?.BillingAddress?.Locality ?? ""),
                },
                Card = new ApiOnboarding.OnboardingPlanCardSettlement
                {
                    Token = (request.OnboardingPlan ?? new ApiOnboarding.OnboardingPlan()).PaymentCase switch
                    {
                        ApiOnboarding.OnboardingPlan.SettlementOneofCase.Card => (request.OnboardingPlan?.Card?.Token ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _onboardingOnboardingPlanCalculationService.CompareOnboardingPlanOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiOnboarding.EstimateOnboardingTaskRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.EstimateOnboardingTaskRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.OnboardingPlan?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.OnboardingPlan?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        var command1 = new DispatchCommand<ApiOnboarding.EstimateOnboardingTaskRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _onboardingOnboardingTaskCalculationService.EstimateOnboardingTaskAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var batch2 = (request.OnboardingPlan?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiOnboarding.OnboardingPlanLine>()).Select(async item =>
        {
            var input2 = new ApiHelpdesk.QuoteSlaPolicyRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                SlaPolicy = new ApiHelpdesk.SlaPolicy
                {
                    Description = (item.Description ?? ""),
                    Discount = new ApiHelpdesk.SlaPolicyDiscountAction
                    {
                        Amount = (item.Tax?.Amount ?? new ApiCommon.Money()),
                    },
                },
            };
            using var rpc2 = _helpdeskSlaPolicyCalculationService.QuoteSlaPolicyAsync(input2, cancellationToken: token);
            var reply2 = await rpc2.ResponseAsync;
            return reply2;
        }).ToArray();
        var replies2 = await Task.WhenAll(batch2);
        var reply2 = replies2.LastOrDefault() ?? new ApiHelpdesk.QuoteSlaPolicyResponse();
        var reply3 = new ApiHelpdesk.ExplainSlaPolicyCalculationResponse();
        foreach (var item in (request.OnboardingPlan?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiOnboarding.OnboardingPlanLine>()))
        {
            var input3 = new ApiHelpdesk.ExplainSlaPolicyCalculationRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                SlaPolicy = new ApiHelpdesk.SlaPolicy
                {
                    Description = (item.Description ?? ""),
                    Discount = new ApiHelpdesk.SlaPolicyDiscountAction
                    {
                        Amount = (item.Tax?.Amount ?? new ApiCommon.Money()),
                    },
                    Owner = new ApiIdentity.UserRef
                    {
                        DisplayName = (reply1.OnboardingTask?.Assignee?.DisplayName ?? ""),
                    },
                },
            };
            using var rpc3 = _helpdeskSlaPolicyCalculationService.ExplainSlaPolicyCalculationAsync(input3, cancellationToken: token);
            reply3 = await rpc3.ResponseAsync;
        }
        var response = new ApiOnboarding.PreviewOnboardingPlanResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                DueAt = (request.OnboardingPlan?.DueAt ?? new Timestamp()),
                Number = (reply0.OnboardingPlan?.Number ?? ""),
                OnboardingPlanId = (request.OnboardingPlanId ?? ""),
                Total = (reply1.Breakdown?.Total ?? new ApiCommon.Money()),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "PreviewOnboardingPlan", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.ScoreOnboardingPlanResponse> ScoreOnboardingPlan(ApiOnboarding.ScoreOnboardingPlanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScoreOnboardingPlan for {Tenant}", requestContext.TenantId);
        if (request.OnboardingPlan is { } record)
        {
            var snapshot = OnboardingPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        foreach (var item in (request.OnboardingPlan?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiOnboarding.OnboardingPlanLine>()))
        {
            var input0 = new ApiAudiences.ExportAudiencesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiAudiences.ExportAudiencesRequest.Types.Filter
                {
                    Labels = { (item.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    OwnerId = (item.LineId ?? ""),
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
            };
            using var stream0 = _audiencesAudienceQueryService.ExportAudiences(input0, cancellationToken: token);
            await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
            {
                _logger.LogDebug("Received ExportAudiences frame {Type}", change0.GetType().Name);
            }
        }
        var response = new ApiOnboarding.ScoreOnboardingPlanResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                OnboardingPlanId = (request.OnboardingPlanId ?? ""),
                Status = (request.OnboardingPlan?.Status ?? (ApiOnboarding.OnboardingPlanStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "ScoreOnboardingPlan", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.ValidateOnboardingPlanResponse> ValidateOnboardingPlan(ApiOnboarding.ValidateOnboardingPlanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ValidateOnboardingPlan for {Tenant}", requestContext.TenantId);
        if (request.OnboardingPlan is { } record)
        {
            var snapshot = OnboardingPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiOnboarding.RecalculateOnboardingTasksRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.OnboardingPlan?.Audit?.CreatedBy ?? ""),
                },
                Customer = new ApiOnboarding.EquipmentRef
                {
                    DisplayName = (request.OnboardingPlan?.Counterparty?.DisplayName ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
        };
        var reply0 = await SendValidateOnboardingPlan0Async(input0, token);
        var response = new ApiOnboarding.ValidateOnboardingPlanResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Counterparty = new ApiOnboarding.EquipmentRef
                {
                    Code = (reply0.OnboardingTask?.Customer?.Code ?? ""),
                },
                Notes = (request.OnboardingPlan?.Notes ?? ""),
                OnboardingPlanId = (request.OnboardingPlanId ?? ""),
            },
            Outcome = new ApiOnboarding.ValidateOnboardingPlanResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "ValidateOnboardingPlan", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiOnboarding.RecalculateOnboardingTasksResponse> SendValidateOnboardingPlan0Async(ApiOnboarding.RecalculateOnboardingTasksRequest request, CancellationToken token)
    {
        return await _onboardingPlanValidationEndpointSendValidateOnboardingPlan0Port.SendAsync(request, token);
    }
}
