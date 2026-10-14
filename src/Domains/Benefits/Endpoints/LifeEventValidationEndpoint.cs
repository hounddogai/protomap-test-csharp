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
using ApiCustomers = Acme.Customers.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiForms = Acme.Forms.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;

namespace Acme.Workforce.Benefits;

/// <summary>
/// Checks life events against the rules before anything acts on them.
/// </summary>
public sealed class LifeEventValidationEndpoint : ApiBenefits.LifeEventValidationService.LifeEventValidationServiceBase
{
    private readonly ApiBenefits.DependentValidationService.DependentValidationServiceClient _benefitsDependentValidationService;
    private readonly ApiBenefits.LifeEventCalculationService.LifeEventCalculationServiceClient _benefitsLifeEventCalculationService;
    private readonly ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient _devicesComplianceScanCalculationService;
    private readonly ApiDevices.ManagedDeviceQueryService.ManagedDeviceQueryServiceClient _devicesManagedDeviceQueryService;
    private readonly ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceClient _documentsDocumentTemplateWorkflowService;
    private readonly ApiForms.FormCalculationService.FormCalculationServiceClient _formsFormCalculationService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ServiceAssetValidationService.ServiceAssetValidationServiceClient _itsmServiceAssetValidationService;
    private readonly ILogger<LifeEventValidationEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient _searchSearchIndexQueryService;
    private readonly IOptions<MeshOptions> _settings;

    public LifeEventValidationEndpoint(
        ApiBenefits.DependentValidationService.DependentValidationServiceClient benefitsDependentValidationService,
        ApiBenefits.LifeEventCalculationService.LifeEventCalculationServiceClient benefitsLifeEventCalculationService,
        ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient devicesComplianceScanCalculationService,
        ApiDevices.ManagedDeviceQueryService.ManagedDeviceQueryServiceClient devicesManagedDeviceQueryService,
        ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceClient documentsDocumentTemplateWorkflowService,
        ApiForms.FormCalculationService.FormCalculationServiceClient formsFormCalculationService,
        CallInvoker invoker,
        ApiItsm.ServiceAssetValidationService.ServiceAssetValidationServiceClient itsmServiceAssetValidationService,
        ILogger<LifeEventValidationEndpoint> logger,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient searchSearchIndexQueryService,
        IOptions<MeshOptions> settings)
    {
        _benefitsDependentValidationService = benefitsDependentValidationService;
        _benefitsLifeEventCalculationService = benefitsLifeEventCalculationService;
        _devicesComplianceScanCalculationService = devicesComplianceScanCalculationService;
        _devicesManagedDeviceQueryService = devicesManagedDeviceQueryService;
        _documentsDocumentTemplateWorkflowService = documentsDocumentTemplateWorkflowService;
        _formsFormCalculationService = formsFormCalculationService;
        _invoker = invoker;
        _itsmServiceAssetValidationService = itsmServiceAssetValidationService;
        _logger = logger;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _searchSearchIndexQueryService = searchSearchIndexQueryService;
        _settings = settings;
    }
    /// <summary>
    /// Checks a life event.
    /// </summary>
    public override async Task<ApiBenefits.CheckLifeEventEligibilityResponse> CheckLifeEventEligibility(ApiBenefits.CheckLifeEventEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CheckLifeEventEligibility for {Tenant}", requestContext.TenantId);
        if (request.LifeEvent is { } record)
        {
            var snapshot = LifeEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiBenefits.ValidateDependentRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Dependent = new ApiBenefits.Dependent
        {
            Contact = new ApiCommon.ContactInfo
            {
                Phone = (request.Details?.Contact?.Phone ?? ""),
            },
        };
        using var rpc0 = _benefitsDependentValidationService.ValidateDependentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.CheckServiceAssetEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Dependent?.Audit?.CreatedBy ?? ""),
                },
            },
        };
        using var rpc1 = _itsmServiceAssetValidationService.CheckServiceAssetEligibilityAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiBenefits.CheckLifeEventEligibilityResponse
        {
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Attributes = { (request.LifeEvent?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                LifeEventId = (request.LifeEventId ?? ""),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (reply0.Dependent?.RelationshipManager?.Email ?? ""),
                },
            },
            Outcome = new ApiBenefits.CheckLifeEventEligibilityResponse.Types.Outcome
            {
                Message = (reply1.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "life_event",
            "audit", "CheckLifeEventEligibility", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a life event.
    /// </summary>
    public override async Task<ApiBenefits.ExplainLifeEventDecisionResponse> ExplainLifeEventDecision(ApiBenefits.ExplainLifeEventDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExplainLifeEventDecision for {Tenant}", requestContext.TenantId);
        if (request.LifeEvent is { } record)
        {
            var snapshot = LifeEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.RecalculateComplianceScansRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    CandidateId = (request.LifeEvent?.Subject?.CandidateId ?? ""),
                    DisplayName = (request.LifeEvent?.Subject?.DisplayName ?? ""),
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _devicesComplianceScanCalculationService.RecalculateComplianceScansAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiBenefits.ExplainLifeEventDecisionResponse
        {
            LifeEvent = new ApiBenefits.LifeEvent
            {
                CorrelationId = (request.LifeEvent?.CorrelationId ?? ""),
                LifeEventId = (request.LifeEventId ?? ""),
                OccurredAt = (reply0.ComplianceScan?.OccurredAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "life_event",
            "audit", "ExplainLifeEventDecision", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lints a life event.
    /// </summary>
    public override async Task<ApiBenefits.LintLifeEventsResponse> LintLifeEvents(ApiBenefits.LintLifeEventsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling LintLifeEvents for {Tenant}", requestContext.TenantId);
        if (request.LifeEvent is { } record)
        {
            var snapshot = LifeEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiSearch.BrowseSearchIndexesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiSearch.BrowseSearchIndexesRequest.Types.Filter
            {
                CreatedAfter = (request.LifeEvent?.OccurredAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _searchSearchIndexQueryService.BrowseSearchIndexesAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiBenefits.CheckDependentEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Dependent = new ApiBenefits.Dependent
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    Email = (request.LifeEvent?.Subject?.Email ?? ""),
                },
            },
            Details = new ApiBenefits.CheckDependentEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Contact?.Phone ?? ""),
                },
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _benefitsDependentValidationService.CheckDependentEligibilityAsync);
        var input2 = new ApiBenefits.SimulateLifeEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.SimulateLifeEventRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply1.Dependent?.PrimaryAddress?.CountryCode ?? ""),
                },
            },
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (reply1.Dependent?.RelationshipManager?.DisplayName ?? ""),
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
        };
        using var rpc2 = _benefitsLifeEventCalculationService.SimulateLifeEventAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiBenefits.ExplainDependentDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Dependent = new ApiBenefits.Dependent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.Dependent?.Audit?.Etag ?? ""),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    Email = (reply1.Dependent?.Contact?.Email ?? ""),
                },
            },
        };
        var named3 = _registry.Get<ApiBenefits.DependentValidationService.DependentValidationServiceClient>("acme.benefits.v1.DependentValidationService");
        using var rpc3 = named3.ExplainDependentDecisionAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiBenefits.LintLifeEventsResponse
        {
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Attributes = { (request.LifeEvent?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                LifeEventId = (request.LifeEventId ?? ""),
            },
            Outcome = new ApiBenefits.LintLifeEventsResponse.Types.Outcome
            {
                NextSteps = { (reply1.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                ReferenceId = (reply1.Outcome?.ReferenceId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.LifeEvent?.CorrelationId ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "life_event",
            "audit", "LintLifeEvents", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Normalizes a life event.
    /// </summary>
    public override async Task<ApiBenefits.NormalizeLifeEventResponse> NormalizeLifeEvent(ApiBenefits.NormalizeLifeEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling NormalizeLifeEvent for {Tenant}", requestContext.TenantId);
        if (request.LifeEvent is { } record)
        {
            var snapshot = LifeEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiBenefits.CheckDependentEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Dependent = new ApiBenefits.Dependent
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.LifeEvent?.Subject?.Email ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiBenefits.CheckDependentEligibilityRequest, ApiBenefits.CheckDependentEligibilityResponse>(
            MethodType.Unary, "acme.benefits.v1" + ".DependentValidationService", "CheckDependentEligibility",
            Wire.Marshaller<ApiBenefits.CheckDependentEligibilityRequest>(), Wire.Marshaller<ApiBenefits.CheckDependentEligibilityResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiBenefits.NormalizeLifeEventResponse
        {
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Attributes = { (request.LifeEvent?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                LifeEventId = (request.LifeEventId ?? ""),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (reply0.Dependent?.Contact?.Email ?? ""),
                },
            },
            Outcome = new ApiBenefits.NormalizeLifeEventResponse.Types.Outcome
            {
                NextSteps = { (reply0.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "life_event",
            "audit", "NormalizeLifeEvent", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews a life event.
    /// </summary>
    public override async Task<ApiBenefits.PreviewLifeEventResponse> PreviewLifeEvent(ApiBenefits.PreviewLifeEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PreviewLifeEvent for {Tenant}", requestContext.TenantId);
        if (request.LifeEvent is { } record)
        {
            var snapshot = LifeEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.StartDocumentTemplateWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.StartDocumentTemplateWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.LifeEvent?.Subject?.Email ?? ""),
                    Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.Contact?.Phone ?? ""),
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiDocuments.StartDocumentTemplateWorkflowRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _documentsDocumentTemplateWorkflowService.StartDocumentTemplateWorkflowAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiBenefits.PreviewLifeEventRequest, ApiBenefits.LintDependentsRequest>(value => new ApiBenefits.LintDependentsRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Dependent = new ApiBenefits.Dependent
            {
                RelationshipManager = new ApiIdentity.UserRef
                {
                    DisplayName = (value.LifeEvent?.Subject?.DisplayName ?? ""),
                    Email = (value.LifeEvent?.Subject?.Email ?? ""),
                },
            },
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _benefitsDependentValidationService.LintDependentsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var batch2 = (request.LifeEvent?.Measurements ?? new Google.Protobuf.Collections.RepeatedField<ApiBenefits.LifeEventMeasurement>()).Select(async item =>
        {
            var input2 = new ApiBenefits.ExplainLifeEventCalculationRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                LifeEvent = new ApiBenefits.LifeEvent
                {
                    Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                    Measurements = { new ApiBenefits.LifeEventMeasurement
                    {
                        Value = item.Value,
                    } },
                },
            };
            using var rpc2 = _benefitsLifeEventCalculationService.ExplainLifeEventCalculationAsync(input2, cancellationToken: token);
            var reply2 = await rpc2.ResponseAsync;
            return reply2;
        }).ToArray();
        var replies2 = await Task.WhenAll(batch2);
        var reply2 = replies2.LastOrDefault() ?? new ApiBenefits.ExplainLifeEventCalculationResponse();
        var response = new ApiBenefits.PreviewLifeEventResponse
        {
            LifeEvent = new ApiBenefits.LifeEvent
            {
                LifeEventId = (request.LifeEventId ?? ""),
                Subject = (request.LifeEvent?.Subject ?? new ApiRecruiting.CandidateRef()),
            },
            Outcome = new ApiBenefits.PreviewLifeEventResponse.Types.Outcome
            {
                EffectiveAt = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
                Message = (reply1.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "life_event",
            "audit", "PreviewLifeEvent", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores a life event.
    /// </summary>
    public override async Task<ApiBenefits.ScoreLifeEventResponse> ScoreLifeEvent(ApiBenefits.ScoreLifeEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ScoreLifeEvent for {Tenant}", requestContext.TenantId);
        if (request.LifeEvent is { } record)
        {
            var snapshot = LifeEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.ScoreLifeEventToPreviewDependent0();
        using var rpc0 = _benefitsDependentValidationService.PreviewDependentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDevices.FindManagedDevicesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDevices.FindManagedDevicesRequest.Types.Filter
            {
                CreatedAfter = (reply0.Dependent ?? new ApiBenefits.Dependent()).IdentityDocumentCase switch
                {
                    ApiBenefits.Dependent.IdentityDocumentOneofCase.Passport => (reply0.Dependent?.Passport?.ExpiresAt ?? new Timestamp()),
                    _ => new Timestamp()
                },
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.Outcome?.ReferenceId ?? ""),
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        using var rpc1 = _devicesManagedDeviceQueryService.FindManagedDevicesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDevices.SuggestManagedDevicesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDevices.SuggestManagedDevicesRequest.Types.Filter
            {
                CreatedAfter = (reply0.Dependent?.Passport?.ExpiresAt ?? new Timestamp()),
                Region = (reply0.Dependent?.PrimaryAddress?.Region ?? ""),
            },
        };
        using var rpc2 = _devicesManagedDeviceQueryService.SuggestManagedDevicesAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        var input3 = new ApiBenefits.GetLifeEventRatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LifeEventId = (request.LifeEventId ?? ""),
        };
        using var rpc3 = _benefitsLifeEventCalculationService.GetLifeEventRatesAsync(input3, cancellationToken: token);
        var pending3 = rpc3.ResponseAsync;
        await Task.WhenAll(pending2, pending3);
        var reply2 = await pending2;
        var reply3 = await pending3;
        var response = new ApiBenefits.ScoreLifeEventResponse
        {
            LifeEvent = new ApiBenefits.LifeEvent
            {
                LifeEventId = (request.LifeEventId ?? ""),
                Sequence = (request.LifeEvent?.Sequence ?? 0L),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "life_event",
            "audit", "ScoreLifeEvent", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates a life event.
    /// </summary>
    public override async Task<ApiBenefits.ValidateLifeEventResponse> ValidateLifeEvent(ApiBenefits.ValidateLifeEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ValidateLifeEvent for {Tenant}", requestContext.TenantId);
        if (request.LifeEvent is { } record)
        {
            var snapshot = LifeEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiBenefits.ScoreDependentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Dependent = new ApiBenefits.Dependent
            {
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
                Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                RelationshipManager = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiBenefits.ScoreDependentRequest>(json0);
        using var rpc0 = _benefitsDependentValidationService.ScoreDependentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiForms.QuoteFormRequest();
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        input1.Details = new ApiForms.QuoteFormRequest.Types.Details
        {
            Destination = new ApiCommon.PostalAddress
            {
                PostalCode = (reply0.Dependent?.PrimaryAddress?.PostalCode ?? ""),
            },
        };
        input1.Form = new ApiForms.Form
        {
            BillingAddress = new ApiCommon.PostalAddress
            {
                Locality = (reply0.Dependent?.PrimaryAddress?.Locality ?? ""),
            },
            Counterparty = new ApiCustomers.CustomerRef
            {
                DisplayName = (reply0.Dependent?.RelationshipManager?.DisplayName ?? ""),
                Email = (request.Details?.Contact?.Email ?? ""),
            },
        };
        using var rpc1 = _formsFormCalculationService.QuoteFormAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiBenefits.ValidateLifeEventResponse
        {
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Attributes = { (request.LifeEvent?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                LifeEventId = (request.LifeEventId ?? ""),
            },
            Outcome = new ApiBenefits.ValidateLifeEventResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "life_event",
            "audit", "ValidateLifeEvent", (request.Reason ?? ""), token);
        return response;
    }
}
