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
using ApiDatasets = Acme.Datasets.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiLineage = Acme.Lineage.V1;
using ApiPersonalization = Acme.Personalization.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;

namespace Acme.Workforce.Benefits;

public sealed partial class BenefitPlanQueryEndpoint
{
    /// <summary>
    /// Lists benefit plans a page at a time.
    /// </summary>
    public override async Task<ApiBenefits.BrowseBenefitPlansResponse> BrowseBenefitPlans(ApiBenefits.BrowseBenefitPlansRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling BrowseBenefitPlans for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "benefit_plan", "batch",
            "BrowseBenefitPlans", "accepted", token);
        var input0 = new ApiBenefits.ExplainDependentDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Dependent = new ApiBenefits.Dependent
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiBenefits.ExplainDependentDecisionRequest, ApiBenefits.ExplainDependentDecisionResponse>(
            MethodType.Unary, "acme.benefits.v1" + ".DependentValidationService", "ExplainDependentDecision",
            Wire.Marshaller<ApiBenefits.ExplainDependentDecisionRequest>(), Wire.Marshaller<ApiBenefits.ExplainDependentDecisionResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiBenefits.BrowseBenefitPlansResponse
        {
            BenefitPlans = { new ApiBenefits.BenefitPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.Dependent?.Audit?.UpdatedBy ?? ""),
                },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "benefit_plan",
            "audit", "BrowseBenefitPlans", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Counts benefit plans by status.
    /// </summary>
    public override async Task<ApiBenefits.CountBenefitPlansResponse> CountBenefitPlans(ApiBenefits.CountBenefitPlansRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CountBenefitPlans for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "benefit_plan", "batch",
            "CountBenefitPlans", "accepted", token);
        var input0 = new ApiLicensing.SimulateLicenseAssignmentRequest
        {
            Annotations = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.SimulateLicenseAssignmentRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                IssuedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
            },
        };
        var command0 = new DispatchCommand<ApiLicensing.SimulateLicenseAssignmentRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _licensingLicenseAssignmentCalculationService.SimulateLicenseAssignmentAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiBenefits.CountBenefitPlansResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "benefit_plan",
            "audit", "CountBenefitPlans", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams every matching benefit plan.
    /// </summary>
    public override async Task ExportBenefitPlans(ApiBenefits.ExportBenefitPlansRequest request, IServerStreamWriter<ApiBenefits.ExportBenefitPlansResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExportBenefitPlans for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "benefit_plan", "batch",
            "ExportBenefitPlans", "accepted", token);
        var profile0 = new MappingProfile<ApiBenefits.ExportBenefitPlansRequest, ApiHelpdesk.NormalizeKnowledgeArticleRequest>(value => new ApiHelpdesk.NormalizeKnowledgeArticleRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (value.Filter?.Region ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (value.Context?.Actor?.Email ?? ""),
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _helpdeskKnowledgeArticleValidationService.NormalizeKnowledgeArticleAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiBenefits.ExportBenefitPlansResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "benefit_plan",
            "audit", "ExportBenefitPlans", (request.Format ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    /// <summary>
    /// Lists benefit plans a page at a time.
    /// </summary>
    public override async Task<ApiBenefits.FindBenefitPlansResponse> FindBenefitPlans(ApiBenefits.FindBenefitPlansRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling FindBenefitPlans for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "benefit_plan", "batch",
            "FindBenefitPlans", "accepted", token);
        var response = new ApiBenefits.FindBenefitPlansResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "benefit_plan",
            "audit", "FindBenefitPlans", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a benefit plan.
    /// </summary>
    public override async Task<ApiBenefits.LookupBenefitPlanResponse> LookupBenefitPlan(ApiBenefits.LookupBenefitPlanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.BenefitPlanId, "benefit_plan_id");
        _logger.LogInformation("Handling LookupBenefitPlan for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "benefit_plan", request.BenefitPlanId,
            "LookupBenefitPlan", "accepted", token);
        var input0 = new ApiLineage.CalculateLineageNodeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LineageNode = new ApiLineage.LineageNode
            {
                Children = { new ApiLineage.LineageNode
                {
                    Children = { new ApiLineage.LineageNode
                    {
                        Manager = new ApiIdentity.UserRef
                        {
                            Email = (request.Context?.Actor?.Email ?? ""),
                        },
                    } },
                    Manager = new ApiIdentity.UserRef
                    {
                        UserId = (request.Context?.Actor?.UserId ?? ""),
                    },
                } },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _lineageLineageNodeCalculationService.CalculateLineageNodeAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDatasets.WatchDatasetsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (reply0.LineageNode?.Manager?.Email ?? ""),
            },
        };
        using var stream1 = _datasetsDatasetService.WatchDatasets(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchDatasets frame {Type}", change1.GetType().Name);
        }
        var response = new ApiBenefits.LookupBenefitPlanResponse
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
    /// Returns a benefit plan.
    /// </summary>
    public override async Task<ApiBenefits.PeekBenefitPlanResponse> PeekBenefitPlan(ApiBenefits.PeekBenefitPlanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.BenefitPlanId, "benefit_plan_id");
        _logger.LogInformation("Handling PeekBenefitPlan for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "benefit_plan", request.BenefitPlanId,
            "PeekBenefitPlan", "accepted", token);
        var input0 = new ApiBenefits.SimulateLifeEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Source = (request.BenefitPlanId ?? ""),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _benefitsLifeEventCalculationService.SimulateLifeEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.ValidateKnowledgeArticleRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply0.LifeEvent?.Subject?.DisplayName ?? ""),
                    Email = (reply0.LifeEvent?.Subject?.Email ?? ""),
                },
                Lines = { new ApiHelpdesk.KnowledgeArticleLine
                {
                    Attributes = { (reply0.LifeEvent?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                } },
                Total = (reply0.Breakdown?.Total ?? new ApiCommon.Money()),
            },
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _helpdeskKnowledgeArticleValidationService.ValidateKnowledgeArticleAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiBenefits.PeekBenefitPlanResponse
        {
            BenefitPlan = new ApiBenefits.BenefitPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.KnowledgeArticle?.Audit?.UpdatedBy ?? ""),
                },
                BenefitPlanId = (request.BenefitPlanId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.LifeEvent?.Subject?.DisplayName ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns several benefit plans at once.
    /// </summary>
    public override async Task<ApiBenefits.ResolveBenefitPlansResponse> ResolveBenefitPlans(ApiBenefits.ResolveBenefitPlansRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResolveBenefitPlans for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "benefit_plan", "batch",
            "ResolveBenefitPlans", "accepted", token);
        var input0 = new ApiBenefits.LintDependentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Dependent = new ApiBenefits.Dependent
            {
                RelationshipManager = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiBenefits.LintDependentsRequest>();
        using var rpc0 = _benefitsDependentValidationService.LintDependentsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiBenefits.SimulateLifeEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.SimulateLifeEventRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.Dependent?.PrimaryAddress?.PostalCode ?? ""),
                    Recipient = (reply0.Dependent?.PrimaryAddress?.Recipient ?? ""),
                },
            },
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
        };
        using var rpc1 = _benefitsLifeEventCalculationService.SimulateLifeEventAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiBenefits.ResolveBenefitPlansResponse
        {
            BenefitPlans = { new ApiBenefits.BenefitPlan
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.LifeEvent?.Subject?.DisplayName ?? ""),
                    Email = (reply0.Dependent?.RelationshipManager?.Email ?? ""),
                },
            } },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists benefit plans a page at a time.
    /// </summary>
    public override async Task<ApiBenefits.SuggestBenefitPlansResponse> SuggestBenefitPlans(ApiBenefits.SuggestBenefitPlansRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SuggestBenefitPlans for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "benefit_plan", "batch",
            "SuggestBenefitPlans", "accepted", token);
        var input0 = new ApiPersonalization.ExplainProfileCalculationRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Profile = new ApiPersonalization.Profile
        {
            PrimaryAddress = new ApiCommon.PostalAddress
            {
                Region = (request.Filter?.Region ?? ""),
            },
            RelationshipManager = new ApiIdentity.UserRef
            {
                UserId = (request.Context?.Actor?.UserId ?? ""),
            },
        };
        using var rpc0 = _personalizationProfileCalculationService.ExplainProfileCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiBenefits.SuggestBenefitPlansResponse
        {
            BenefitPlans = { new ApiBenefits.BenefitPlan
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Profile?.RelationshipManager?.DisplayName ?? ""),
                },
            } },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "benefit_plan",
            "audit", "SuggestBenefitPlans", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Summarizes a benefit plan.
    /// </summary>
    public override async Task<ApiBenefits.SummarizeBenefitPlansResponse> SummarizeBenefitPlans(ApiBenefits.SummarizeBenefitPlansRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SummarizeBenefitPlans for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "benefit_plan", request.BenefitPlanId,
            "SummarizeBenefitPlans", "accepted", token);
        var input0 = new ApiSearch.CreateSearchIndexRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SearchIndex = new ApiSearch.SearchIndex
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
                Specs = { (request.Details?.Filters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        using var rpc0 = _searchSearchIndexService.CreateSearchIndexAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.SuggestDocumentFoldersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.SuggestDocumentFoldersRequest.Types.Filter
            {
                OwnerId = (reply0.SearchIndex?.Components?.GetValueOrDefault("primary")?.ComponentId ?? ""),
                Region = (reply0.SearchIndex ?? new ApiSearch.SearchIndex()).LocatorCase switch
                {
                    ApiSearch.SearchIndex.LocatorOneofCase.Address => (reply0.SearchIndex?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc1 = _documentsDocumentFolderQueryService.SuggestDocumentFoldersAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiBenefits.ScoreDependentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Dependent = new ApiBenefits.Dependent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.SearchIndex?.Audit?.CreatedBy ?? ""),
                },
            },
            Details = new ApiBenefits.ScoreDependentRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.SearchIndex ?? new ApiSearch.SearchIndex()).LocatorCase switch
                    {
                        ApiSearch.SearchIndex.LocatorOneofCase.Address => (reply0.SearchIndex?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()),
                        _ => new Google.Protobuf.Collections.RepeatedField<string>()
                    } },
                },
            },
        };
        using var rpc2 = _benefitsDependentValidationService.ScoreDependentAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiDocuments.ListDocumentTemplateVersionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentTemplateVersionsRequest.Types.Filter
            {
                Region = (reply0.SearchIndex ?? new ApiSearch.SearchIndex()).LocatorCase switch
                {
                    ApiSearch.SearchIndex.LocatorOneofCase.Address => (reply0.SearchIndex?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc3 = _documentsDocumentTemplateHistoryService.ListDocumentTemplateVersionsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiBenefits.SummarizeBenefitPlansResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "benefit_plan",
            "audit", "SummarizeBenefitPlans", (request.Reason ?? ""), token);
        return response;
    }
}
