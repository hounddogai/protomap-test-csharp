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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiModelregistry = Acme.Modelregistry.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiSearch = Acme.Search.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Onboarding;

/// <summary>
/// Computes the figures of an onboarding plan, such as totals, taxes, and estimates.
/// </summary>
public sealed class OnboardingPlanCalculationEndpoint : ApiOnboarding.OnboardingPlanCalculationService.OnboardingPlanCalculationServiceBase
{
    private readonly ApiCorporateevents.RegistrationCalculationService.RegistrationCalculationServiceClient _corporateeventsRegistrationCalculationService;
    private readonly ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient _documentsDocumentTemplateSyncService;
    private readonly ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient _documentsSignatureHistoryService;
    private readonly ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient _helpdeskKnowledgeArticleValidationService;
    private readonly ApiHelpdesk.TicketCalculationService.TicketCalculationServiceClient _helpdeskTicketCalculationService;
    private readonly ApiIdentity.UserQueryService.UserQueryServiceClient _identityUserQueryService;
    private readonly CallInvoker _invoker;
    private readonly ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient _licensingSoftwareLicenseQueryService;
    private readonly ILogger<OnboardingPlanCalculationEndpoint> _logger;
    private readonly ApiModelregistry.RegisteredModelCalculationService.RegisteredModelCalculationServiceClient _modelregistryRegisteredModelCalculationService;
    private readonly ApiPerformance.GoalQueryService.GoalQueryServiceClient _performanceGoalQueryService;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;
    private readonly ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient _searchSearchIndexQueryService;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTravel.TravelPolicyCalculationService.TravelPolicyCalculationServiceClient _travelTravelPolicyCalculationService;
    private readonly ApiWorkplaces.BuildingValidationService.BuildingValidationServiceClient _workplacesBuildingValidationService;

    public OnboardingPlanCalculationEndpoint(
        ApiCorporateevents.RegistrationCalculationService.RegistrationCalculationServiceClient corporateeventsRegistrationCalculationService,
        ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient documentsDocumentTemplateSyncService,
        ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient documentsSignatureHistoryService,
        ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient helpdeskKnowledgeArticleValidationService,
        ApiHelpdesk.TicketCalculationService.TicketCalculationServiceClient helpdeskTicketCalculationService,
        ApiIdentity.UserQueryService.UserQueryServiceClient identityUserQueryService,
        CallInvoker invoker,
        ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient licensingSoftwareLicenseQueryService,
        ILogger<OnboardingPlanCalculationEndpoint> logger,
        ApiModelregistry.RegisteredModelCalculationService.RegisteredModelCalculationServiceClient modelregistryRegisteredModelCalculationService,
        ApiPerformance.GoalQueryService.GoalQueryServiceClient performanceGoalQueryService,
        RetryPolicy policy,
        IRecordRepository repository,
        ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient searchSearchIndexQueryService,
        IOptions<MeshOptions> settings,
        ApiTravel.TravelPolicyCalculationService.TravelPolicyCalculationServiceClient travelTravelPolicyCalculationService,
        ApiWorkplaces.BuildingValidationService.BuildingValidationServiceClient workplacesBuildingValidationService)
    {
        _corporateeventsRegistrationCalculationService = corporateeventsRegistrationCalculationService;
        _documentsDocumentTemplateSyncService = documentsDocumentTemplateSyncService;
        _documentsSignatureHistoryService = documentsSignatureHistoryService;
        _helpdeskKnowledgeArticleValidationService = helpdeskKnowledgeArticleValidationService;
        _helpdeskTicketCalculationService = helpdeskTicketCalculationService;
        _identityUserQueryService = identityUserQueryService;
        _invoker = invoker;
        _licensingSoftwareLicenseQueryService = licensingSoftwareLicenseQueryService;
        _logger = logger;
        _modelregistryRegisteredModelCalculationService = modelregistryRegisteredModelCalculationService;
        _performanceGoalQueryService = performanceGoalQueryService;
        _policy = policy;
        _repository = repository;
        _searchSearchIndexQueryService = searchSearchIndexQueryService;
        _settings = settings;
        _travelTravelPolicyCalculationService = travelTravelPolicyCalculationService;
        _workplacesBuildingValidationService = workplacesBuildingValidationService;
    }
    /// <summary>
    /// Calculates an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.CalculateOnboardingPlanResponse> CalculateOnboardingPlan(ApiOnboarding.CalculateOnboardingPlanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CalculateOnboardingPlan for {Tenant}", requestContext.TenantId);
        if (request.OnboardingPlan is { } record)
        {
            var snapshot = OnboardingPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.ExplainKnowledgeArticleDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiHelpdesk.ExplainKnowledgeArticleDecisionRequest, ApiHelpdesk.ExplainKnowledgeArticleDecisionResponse>(
            MethodType.Unary, "acme.helpdesk.v1" + ".KnowledgeArticleValidationService", "ExplainKnowledgeArticleDecision",
            Wire.Marshaller<ApiHelpdesk.ExplainKnowledgeArticleDecisionRequest>(), Wire.Marshaller<ApiHelpdesk.ExplainKnowledgeArticleDecisionResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiWorkplaces.ScoreBuildingRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Location = new ApiCommon.PostalAddress
                        {
                            CountryCode = (request.OnboardingPlan?.BillingAddress?.CountryCode ?? ""),
                            PostalCode = (reply0.KnowledgeArticle?.BillingAddress?.PostalCode ?? ""),
                            Recipient = (request.OnboardingPlan?.BillingAddress?.Recipient ?? ""),
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var command1 = new DispatchCommand<ApiWorkplaces.ScoreBuildingRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _workplacesBuildingValidationService.ScoreBuildingAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var profile2 = new MappingProfile<ApiOnboarding.CalculateOnboardingPlanRequest, ApiCorporateevents.CompareRegistrationOptionsRequest>(value => new ApiCorporateevents.CompareRegistrationOptionsRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.CompareRegistrationOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (value.Details?.Destination?.PostalCode ?? ""),
                    Recipient = (value.OnboardingPlan?.BillingAddress?.Recipient ?? ""),
                },
            },
            Registration = new ApiCorporateevents.Registration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (value.OnboardingPlan?.Audit?.UpdatedBy ?? ""),
                },
            },
        });
        var input2 = profile2.Map(request);
        using var rpc2 = _corporateeventsRegistrationCalculationService.CompareRegistrationOptionsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiOnboarding.CalculateOnboardingPlanResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply1.Building?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    PostalCode = (reply0.KnowledgeArticle?.BillingAddress?.PostalCode ?? ""),
                },
                OnboardingPlanId = (request.OnboardingPlanId ?? ""),
                Status = (request.OnboardingPlan?.Status ?? (ApiOnboarding.OnboardingPlanStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "CalculateOnboardingPlan", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.CompareOnboardingPlanOptionsResponse> CompareOnboardingPlanOptions(ApiOnboarding.CompareOnboardingPlanOptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CompareOnboardingPlanOptions for {Tenant}", requestContext.TenantId);
        if (request.OnboardingPlan is { } record)
        {
            var snapshot = OnboardingPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.ExportSoftwareLicensesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLicensing.ExportSoftwareLicensesRequest.Types.Filter
            {
                Region = (request.OnboardingPlan?.BillingAddress?.Region ?? ""),
            },
        };
        using var stream0 = _licensingSoftwareLicenseQueryService.ExportSoftwareLicenses(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportSoftwareLicenses frame {Type}", change0.GetType().Name);
        }
        var input1 = request.CompareOnboardingPlanOptionsToCompareTravelPolicyOptions1();
        using var rpc1 = _travelTravelPolicyCalculationService.CompareTravelPolicyOptionsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.CompareOnboardingPlanOptionsResponse
        {
            Breakdown = new ApiOnboarding.CompareOnboardingPlanOptionsResponse.Types.Breakdown
            {
                CurrencyCode = (reply1.Breakdown?.CurrencyCode ?? ""),
            },
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Counterparty = (request.OnboardingPlan?.Counterparty ?? new ApiOnboarding.EquipmentRef()),
                OnboardingPlanId = (request.OnboardingPlanId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "CompareOnboardingPlanOptions", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.EstimateOnboardingPlanResponse> EstimateOnboardingPlan(ApiOnboarding.EstimateOnboardingPlanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling EstimateOnboardingPlan for {Tenant}", requestContext.TenantId);
        if (request.OnboardingPlan is { } record)
        {
            var snapshot = OnboardingPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.AnnotateSignatureVersionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.AnnotateSignatureVersionRequest.Types.Details
            {
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (request.OnboardingPlan?.Counterparty?.DisplayName ?? ""),
                    Number = (request.OnboardingPlan?.Number ?? ""),
                },
            },
        };
        using var rpc0 = _documentsSignatureHistoryService.AnnotateSignatureVersionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.ValidateKnowledgeArticleRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.OnboardingPlan?.Audit?.CreatedBy ?? ""),
                },
                Bank = new ApiHelpdesk.KnowledgeArticleBankSettlement
                {
                    Bic = (request.OnboardingPlan ?? new ApiOnboarding.OnboardingPlan()).PaymentCase switch
                    {
                        ApiOnboarding.OnboardingPlan.SettlementOneofCase.Bank => (request.OnboardingPlan?.Bank?.Bic ?? ""),
                        _ => ""
                    },
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.OnboardingPlan?.BillingAddress?.CountryCode ?? ""),
                },
            },
        };
        using var rpc1 = _helpdeskKnowledgeArticleValidationService.ValidateKnowledgeArticleAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var reply2 = new ApiModelregistry.EstimateRegisteredModelResponse();
        foreach (var item in (request.OnboardingPlan?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiOnboarding.OnboardingPlanLine>()))
        {
            var input2 = new ApiModelregistry.EstimateRegisteredModelRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                RegisteredModel = new ApiModelregistry.RegisteredModel
                {
                    Components = { { "primary", new ApiModelregistry.RegisteredModelComponent
                    {
                        Description = (item.Description ?? ""),
                        Quantity = item.Quantity,
                    } } },
                },
            };
            using var rpc2 = _modelregistryRegisteredModelCalculationService.EstimateRegisteredModelAsync(input2, cancellationToken: token);
            reply2 = await rpc2.ResponseAsync;
        }
        var reply3 = new ApiHelpdesk.ScoreKnowledgeArticleResponse();
        foreach (var item in (request.OnboardingPlan?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiOnboarding.OnboardingPlanLine>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input3 = new ApiHelpdesk.ScoreKnowledgeArticleRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
                {
                    Lines = { new ApiHelpdesk.KnowledgeArticleLine
                    {
                        Amount = (item.Tax?.Amount ?? new ApiCommon.Money()),
                        Tax = new ApiHelpdesk.KnowledgeArticleLineTax
                        {
                            Amount = (item.Amount ?? new ApiCommon.Money()),
                            Jurisdiction = (item.Tax?.Jurisdiction ?? ""),
                        },
                    } },
                },
            };
            using var rpc3 = _helpdeskKnowledgeArticleValidationService.ScoreKnowledgeArticleAsync(input3, cancellationToken: token);
            reply3 = await rpc3.ResponseAsync;
        }
        var response = new ApiOnboarding.EstimateOnboardingPlanResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Counterparty = (request.OnboardingPlan?.Counterparty ?? new ApiOnboarding.EquipmentRef()),
                OnboardingPlanId = (request.OnboardingPlanId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "EstimateOnboardingPlan", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.ExplainOnboardingPlanCalculationResponse> ExplainOnboardingPlanCalculation(ApiOnboarding.ExplainOnboardingPlanCalculationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExplainOnboardingPlanCalculation for {Tenant}", requestContext.TenantId);
        if (request.OnboardingPlan is { } record)
        {
            var snapshot = OnboardingPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.CompareTicketOptionsRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Details = new ApiHelpdesk.CompareTicketOptionsRequest.Types.Details
        {
            Destination = new ApiCommon.PostalAddress
            {
                PostalCode = (request.OnboardingPlan?.BillingAddress?.PostalCode ?? ""),
            },
        };
        using var rpc0 = _helpdeskTicketCalculationService.CompareTicketOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiIdentity.ExportUsersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiIdentity.ExportUsersRequest.Types.Filter
            {
                CreatedAfter = (reply0.Ticket?.Sla?.DueAt ?? new Timestamp()),
                Region = (request.OnboardingPlan?.BillingAddress?.Region ?? ""),
            },
        };
        using var stream1 = _identityUserQueryService.ExportUsers(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportUsers frame {Type}", change1.GetType().Name);
        }
        var response = new ApiOnboarding.ExplainOnboardingPlanCalculationResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Counterparty = (request.OnboardingPlan?.Counterparty ?? new ApiOnboarding.EquipmentRef()),
                DueAt = (reply0.Ticket?.Sla?.DueAt ?? new Timestamp()),
                OnboardingPlanId = (request.OnboardingPlanId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "ExplainOnboardingPlanCalculation", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.GetOnboardingPlanRatesResponse> GetOnboardingPlanRates(ApiOnboarding.GetOnboardingPlanRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OnboardingPlanId, "onboarding_plan_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetOnboardingPlanRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan", request.OnboardingPlanId,
            "GetOnboardingPlanRates", "accepted", token);
        var input0 = new ApiIdentity.FindUsersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiIdentity.FindUsersRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                Labels = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.OnboardingPlanId ?? ""),
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _identityUserQueryService.FindUsersAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiOnboarding.GetOnboardingPlanRatesResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                OnboardingPlanId = (request.OnboardingPlanId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Quotes an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.QuoteOnboardingPlanResponse> QuoteOnboardingPlan(ApiOnboarding.QuoteOnboardingPlanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling QuoteOnboardingPlan for {Tenant}", requestContext.TenantId);
        if (request.OnboardingPlan is { } record)
        {
            var snapshot = OnboardingPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPerformance.BrowseGoalsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiPerformance.BrowseGoalsRequest.Types.Filter
            {
                Region = (request.OnboardingPlan?.BillingAddress?.Region ?? ""),
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _performanceGoalQueryService.BrowseGoalsAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiOnboarding.QuoteOnboardingPlanResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Counterparty = (request.OnboardingPlan?.Counterparty ?? new ApiOnboarding.EquipmentRef()),
                OnboardingPlanId = (request.OnboardingPlanId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "QuoteOnboardingPlan", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Simulates an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.SimulateOnboardingPlanResponse> SimulateOnboardingPlan(ApiOnboarding.SimulateOnboardingPlanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SimulateOnboardingPlan for {Tenant}", requestContext.TenantId);
        if (request.OnboardingPlan is { } record)
        {
            var snapshot = OnboardingPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiSearch.SuggestSearchIndexesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiSearch.SuggestSearchIndexesRequest.Types.Filter
            {
                CreatedAfter = (request.OnboardingPlan?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (request.OnboardingPlan?.Counterparty?.EquipmentId ?? ""),
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _searchSearchIndexQueryService.SuggestSearchIndexesAsync);
        var input1 = new ApiDocuments.ResolveDocumentTemplateConflictRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ResolveDocumentTemplateConflictRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.OnboardingPlan?.Audit?.CreatedBy ?? ""),
                },
                Bank = new ApiDocuments.DocumentTemplateBankSettlement
                {
                    Iban = (request.OnboardingPlan ?? new ApiOnboarding.OnboardingPlan()).PaymentCase switch
                    {
                        ApiOnboarding.OnboardingPlan.SettlementOneofCase.Bank => (request.OnboardingPlan?.Bank?.Iban ?? ""),
                        _ => ""
                    },
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (request.Details?.Destination?.Recipient ?? ""),
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
            },
        };
        using var rpc1 = _documentsDocumentTemplateSyncService.ResolveDocumentTemplateConflictAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.SimulateOnboardingPlanResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.DocumentTemplate?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Metadata = { (request.OnboardingPlan?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Notes = (reply1.DocumentTemplate?.Notes ?? ""),
                OnboardingPlanId = (request.OnboardingPlanId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "SimulateOnboardingPlan", (request.Reason ?? ""), token);
        return response;
    }
}
