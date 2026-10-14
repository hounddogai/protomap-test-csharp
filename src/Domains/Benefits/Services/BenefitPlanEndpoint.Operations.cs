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
using ApiForecasting = Acme.Forecasting.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;

namespace Acme.Workforce.Benefits;

public sealed partial class BenefitPlanEndpoint
{
    /// <summary>
    /// Takes a stream of benefit plans and answers once at the end.
    /// </summary>
    public override async Task<ApiBenefits.BatchCreateBenefitPlansResponse> BatchCreateBenefitPlans(IAsyncStreamReader<ApiBenefits.BatchCreateBenefitPlansRequest> requests, ServerCallContext context)
    {
        var token = context.CancellationToken;
        var result = new ApiBenefits.BatchCreateBenefitPlansResponse();
        await foreach (var request in requests.ReadAllAsync(token))
        {
            RequestGuard.Tenant(request.Context);
            var requestContext = request.Context;
            _logger.LogInformation("Handling BatchCreateBenefitPlans for {Tenant}", requestContext.TenantId);
            if (request.Record is { } record)
            {
                var snapshot = BenefitPlanMapper.ToRecord(requestContext.TenantId, record);
                await _repository.SaveAsync(snapshot, token);
            }
            var input0 = new ApiBenefits.ExplainDependentDecisionRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Dependent = new ApiBenefits.Dependent
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = request.ChunkCase switch
                        {
                            ApiBenefits.BatchCreateBenefitPlansRequest.ChunkOneofCase.Record => (request.Record?.Audit?.CreatedAt ?? new Timestamp()),
                            _ => new Timestamp()
                        },
                    },
                    Contact = new ApiCommon.ContactInfo
                    {
                        Name = request.ChunkCase switch
                        {
                            ApiBenefits.BatchCreateBenefitPlansRequest.ChunkOneofCase.Header => (request.Header?.SubmittedBy?.Name ?? new ApiCommon.PersonName()),
                            _ => new ApiCommon.PersonName()
                        },
                    },
                },
                Details = new ApiBenefits.ExplainDependentDecisionRequest.Types.Details
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = request.ChunkCase switch
                        {
                            ApiBenefits.BatchCreateBenefitPlansRequest.ChunkOneofCase.Header => (request.Header?.SubmittedBy?.Email ?? ""),
                            _ => ""
                        },
                    },
                },
            };
            using var rpc0 = _benefitsDependentValidationService.ExplainDependentDecisionAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            var input1 = new ApiBenefits.NormalizeDependentRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Dependent = new ApiBenefits.Dependent
                {
                    Preferences = new ApiBenefits.DependentPreferences
                    {
                        QuietHours = (reply0.Dependent?.Preferences?.QuietHours ?? new ApiCommon.TimeWindow()),
                    },
                },
            };
            var named1 = _registry.Get<ApiBenefits.DependentValidationService.DependentValidationServiceClient>("acme.benefits.v1.DependentValidationService");
            using var rpc1 = named1.NormalizeDependentAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            var response = new ApiBenefits.BatchCreateBenefitPlansResponse
            {
                Result = new ApiCommon.OperationResult
                {
                    Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    Status = (reply1.Result?.Status ?? ""),
                },
            };
            _logger.LogDebug("Local audit detail {Detail}", (request.Record?.Route?.Target ?? ""));
            await _repository.RecordAsync(requestContext.TenantId, "benefit_plan",
                "audit", "BatchCreateBenefitPlans", (request.Record?.Route?.Target ?? ""), token);
            result = response;
        }
        return result;
    }
    /// <summary>
    /// Returns several benefit plans at once.
    /// </summary>
    public override async Task<ApiBenefits.BatchGetBenefitPlansResponse> BatchGetBenefitPlans(ApiBenefits.BatchGetBenefitPlansRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling BatchGetBenefitPlans for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "benefit_plan", "batch",
            "BatchGetBenefitPlans", "accepted", token);
        var response = new ApiBenefits.BatchGetBenefitPlansResponse();
        return response;
    }
    /// <summary>
    /// Creates a benefit plan.
    /// </summary>
    public override async Task<ApiBenefits.CreateBenefitPlanResponse> CreateBenefitPlan(ApiBenefits.CreateBenefitPlanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CreateBenefitPlan for {Tenant}", requestContext.TenantId);
        if (request.BenefitPlan is { } record)
        {
            var snapshot = BenefitPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.ExplainEscalationCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Escalation = new ApiHelpdesk.Escalation
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (request.BenefitPlan?.Owner?.DisplayName ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.BenefitPlan?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (request.BenefitPlan?.Audit?.CreatedBy ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.BenefitPlan?.Owner?.Email ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiHelpdesk.ExplainEscalationCalculationRequest, ApiHelpdesk.ExplainEscalationCalculationResponse>(
            MethodType.Unary, "acme.helpdesk.v1" + ".EscalationCalculationService", "ExplainEscalationCalculation",
            Wire.Marshaller<ApiHelpdesk.ExplainEscalationCalculationRequest>(), Wire.Marshaller<ApiHelpdesk.ExplainEscalationCalculationResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiBenefits.CreateBenefitPlanResponse
        {
            BenefitPlan = new ApiBenefits.BenefitPlan
            {
                BenefitPlanId = (request.BenefitPlan?.BenefitPlanId ?? ""),
                Name = (request.BenefitPlan?.Name ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.Escalation?.Assignee?.Email ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "benefit_plan",
            "audit", "CreateBenefitPlan", (request.RequestId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a benefit plan.
    /// </summary>
    public override async Task<ApiBenefits.GetBenefitPlanResponse> GetBenefitPlan(ApiBenefits.GetBenefitPlanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.BenefitPlanId, "benefit_plan_id");
        _logger.LogInformation("Handling GetBenefitPlan for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "benefit_plan", request.BenefitPlanId,
            "GetBenefitPlan", "accepted", token);
        var input0 = new ApiBenefits.CountEnrollmentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiBenefits.CountEnrollmentsRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                OwnerId = (request.BenefitPlanId ?? ""),
            },
        };
        var command0 = new DispatchCommand<ApiBenefits.CountEnrollmentsRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _benefitsEnrollmentQueryService.CountEnrollmentsAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiBenefits.GetBenefitPlanResponse
        {
            BenefitPlan = new ApiBenefits.BenefitPlan
            {
                BenefitPlanId = (request.BenefitPlanId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Lists benefit plans a page at a time.
    /// </summary>
    public override async Task<ApiBenefits.SearchBenefitPlansResponse> SearchBenefitPlans(ApiBenefits.SearchBenefitPlansRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SearchBenefitPlans for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "benefit_plan", "batch",
            "SearchBenefitPlans", "accepted", token);
        var response = new ApiBenefits.SearchBenefitPlansResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "benefit_plan",
            "audit", "SearchBenefitPlans", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams the changes of benefit plans as they happen.
    /// </summary>
    public override async Task WatchBenefitPlans(ApiBenefits.WatchBenefitPlansRequest request, IServerStreamWriter<ApiBenefits.WatchBenefitPlansResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling WatchBenefitPlans for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "benefit_plan", "batch",
            "WatchBenefitPlans", "accepted", token);
        var profile0 = new MappingProfile<ApiBenefits.WatchBenefitPlansRequest, ApiForecasting.CountDemandForecastsRequest>(value => new ApiForecasting.CountDemandForecastsRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiForecasting.CountDemandForecastsRequest.Types.Filter
            {
                CreatedAfter = (value.Filter?.CreatedAfter ?? new Timestamp()),
                Labels = { (value.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (value.Filter?.OwnerId ?? ""),
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _forecastingDemandForecastQueryService.CountDemandForecastsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiBenefits.ExplainDependentDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Dependent = new ApiBenefits.Dependent
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Subscriber?.Email ?? ""),
                    Name = (request.Subscriber?.Name ?? new ApiCommon.PersonName()),
                },
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
            Details = new ApiBenefits.ExplainDependentDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Subscriber?.Phone ?? ""),
                },
            },
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _benefitsDependentValidationService.ExplainDependentDecisionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiBenefits.WatchBenefitPlansResponse
        {
            BenefitPlan = new ApiBenefits.BenefitPlan
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply1.Dependent?.RelationshipManager?.Email ?? ""),
                    UserId = (reply1.Dependent?.RelationshipManager?.UserId ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Subscriber?.PreferredChannel ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "benefit_plan",
            "audit", "WatchBenefitPlans", (request.Subscriber?.PreferredChannel ?? ""), token);
        await responses.WriteAsync(response, token);
    }
}
