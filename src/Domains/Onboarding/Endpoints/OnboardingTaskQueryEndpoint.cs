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
using ApiDevices = Acme.Devices.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiMedia = Acme.Media.V1;
using ApiOnboarding = Acme.Onboarding.V1;

namespace Acme.Workforce.Onboarding;

/// <summary>
/// Answers read-only questions about onboarding tasks, for screens and reports.
/// </summary>
public sealed class OnboardingTaskQueryEndpoint : ApiOnboarding.OnboardingTaskQueryService.OnboardingTaskQueryServiceBase
{
    private readonly NotificationBus _bus;
    private readonly ApiDevices.ManagedDeviceQueryService.ManagedDeviceQueryServiceClient _devicesManagedDeviceQueryService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.IncidentService.IncidentServiceClient _itsmIncidentService;
    private readonly ApiItsm.ProblemValidationService.ProblemValidationServiceClient _itsmProblemValidationService;
    private readonly ILogger<OnboardingTaskQueryEndpoint> _logger;
    private readonly ApiMedia.MediaAssetCalculationService.MediaAssetCalculationServiceClient _mediaMediaAssetCalculationService;
    private readonly ApiOnboarding.OnboardingPlanQueryService.OnboardingPlanQueryServiceClient _onboardingOnboardingPlanQueryService;
    private readonly ApiOnboarding.OnboardingTaskCalculationService.OnboardingTaskCalculationServiceClient _onboardingOnboardingTaskCalculationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiOnboarding.CountOnboardingPlansRequest, CallOptions, AsyncUnaryCall<ApiOnboarding.CountOnboardingPlansResponse>> _sendSuggestOnboardingTasks0;

    public OnboardingTaskQueryEndpoint(
        NotificationBus bus,
        ApiDevices.ManagedDeviceQueryService.ManagedDeviceQueryServiceClient devicesManagedDeviceQueryService,
        CallInvoker invoker,
        ApiItsm.IncidentService.IncidentServiceClient itsmIncidentService,
        ApiItsm.ProblemValidationService.ProblemValidationServiceClient itsmProblemValidationService,
        ILogger<OnboardingTaskQueryEndpoint> logger,
        ApiMedia.MediaAssetCalculationService.MediaAssetCalculationServiceClient mediaMediaAssetCalculationService,
        ApiOnboarding.OnboardingPlanQueryService.OnboardingPlanQueryServiceClient onboardingOnboardingPlanQueryService,
        ApiOnboarding.OnboardingTaskCalculationService.OnboardingTaskCalculationServiceClient onboardingOnboardingTaskCalculationService,
        ClientRegistry registry,
        IRecordRepository repository)
    {
        _bus = bus;
        _devicesManagedDeviceQueryService = devicesManagedDeviceQueryService;
        _invoker = invoker;
        _itsmIncidentService = itsmIncidentService;
        _itsmProblemValidationService = itsmProblemValidationService;
        _logger = logger;
        _mediaMediaAssetCalculationService = mediaMediaAssetCalculationService;
        _onboardingOnboardingPlanQueryService = onboardingOnboardingPlanQueryService;
        _onboardingOnboardingTaskCalculationService = onboardingOnboardingTaskCalculationService;
        _registry = registry;
        _repository = repository;
        _sendSuggestOnboardingTasks0 = _onboardingOnboardingPlanQueryService.CountOnboardingPlansAsync;
    }
    /// <summary>
    /// Lists onboarding tasks a page at a time.
    /// </summary>
    public override async Task<ApiOnboarding.BrowseOnboardingTasksResponse> BrowseOnboardingTasks(ApiOnboarding.BrowseOnboardingTasksRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling BrowseOnboardingTasks for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task", "batch",
            "BrowseOnboardingTasks", "accepted", token);
        var input0 = new ApiOnboarding.QuoteOnboardingTaskRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Details = new ApiOnboarding.QuoteOnboardingTaskRequest.Types.Details
        {
            Destination = new ApiCommon.PostalAddress
            {
                Region = (request.Filter?.Region ?? ""),
            },
        };
        input0.OnboardingTask = new ApiOnboarding.OnboardingTask
        {
            Assignee = new ApiIdentity.UserRef
            {
                Email = (request.Context?.Actor?.Email ?? ""),
                UserId = (request.Context?.Actor?.UserId ?? ""),
            },
            Audit = new ApiCommon.AuditInfo
            {
                UpdatedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
            },
        };
        using var rpc0 = _onboardingOnboardingTaskCalculationService.QuoteOnboardingTaskAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiOnboarding.BrowseOnboardingTasksResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "BrowseOnboardingTasks", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Counts onboarding tasks by status.
    /// </summary>
    public override async Task<ApiOnboarding.CountOnboardingTasksResponse> CountOnboardingTasks(ApiOnboarding.CountOnboardingTasksRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CountOnboardingTasks for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task", "batch",
            "CountOnboardingTasks", "accepted", token);
        var response = new ApiOnboarding.CountOnboardingTasksResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "CountOnboardingTasks", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams every matching onboarding task.
    /// </summary>
    public override async Task ExportOnboardingTasks(ApiOnboarding.ExportOnboardingTasksRequest request, IServerStreamWriter<ApiOnboarding.ExportOnboardingTasksResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExportOnboardingTasks for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task", "batch",
            "ExportOnboardingTasks", "accepted", token);
        var input0 = new ApiItsm.ListIncidentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.ListIncidentsRequest.Types.Filter
            {
                CreatedAfter = (request.Filter?.CreatedAfter ?? new Timestamp()),
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Query = (request.Filter?.Query ?? ""),
                Region = (request.Filter?.Region ?? ""),
            },
        };
        using var rpc0 = _itsmIncidentService.ListIncidentsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiOnboarding.ExportOnboardingTasksResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "ExportOnboardingTasks", (request.Format ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    /// <summary>
    /// Lists onboarding tasks a page at a time.
    /// </summary>
    public override async Task<ApiOnboarding.FindOnboardingTasksResponse> FindOnboardingTasks(ApiOnboarding.FindOnboardingTasksRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling FindOnboardingTasks for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task", "batch",
            "FindOnboardingTasks", "accepted", token);
        var input0 = new ApiDevices.ExportManagedDevicesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDevices.ExportManagedDevicesRequest.Types.Filter
            {
                Query = (request.Filter?.Query ?? ""),
            },
        };
        using var stream0 = _devicesManagedDeviceQueryService.ExportManagedDevices(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportManagedDevices frame {Type}", change0.GetType().Name);
        }
        var response = new ApiOnboarding.FindOnboardingTasksResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "FindOnboardingTasks", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.LookupOnboardingTaskResponse> LookupOnboardingTask(ApiOnboarding.LookupOnboardingTaskRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OnboardingTaskId, "onboarding_task_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling LookupOnboardingTask for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task", request.OnboardingTaskId,
            "LookupOnboardingTask", "accepted", token);
        var input0 = new ApiOnboarding.ExportOnboardingPlansRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiOnboarding.ExportOnboardingPlansRequest.Types.Filter
            {
                OwnerId = (request.OnboardingTaskId ?? ""),
            },
        };
        using var stream0 = _onboardingOnboardingPlanQueryService.ExportOnboardingPlans(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportOnboardingPlans frame {Type}", change0.GetType().Name);
        }
        var response = new ApiOnboarding.LookupOnboardingTaskResponse
        {
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                OnboardingTaskId = (request.OnboardingTaskId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.PeekOnboardingTaskResponse> PeekOnboardingTask(ApiOnboarding.PeekOnboardingTaskRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OnboardingTaskId, "onboarding_task_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PeekOnboardingTask for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task", request.OnboardingTaskId,
            "PeekOnboardingTask", "accepted", token);
        var response = new ApiOnboarding.PeekOnboardingTaskResponse
        {
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                OnboardingTaskId = (request.OnboardingTaskId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns several onboarding tasks at once.
    /// </summary>
    public override async Task<ApiOnboarding.ResolveOnboardingTasksResponse> ResolveOnboardingTasks(ApiOnboarding.ResolveOnboardingTasksRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ResolveOnboardingTasks for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task", "batch",
            "ResolveOnboardingTasks", "accepted", token);
        var input0 = new ApiMedia.ExplainMediaAssetCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            MediaAsset = new ApiMedia.MediaAsset
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _mediaMediaAssetCalculationService.ExplainMediaAssetCalculationAsync);
        var response = new ApiOnboarding.ResolveOnboardingTasksResponse
        {
            OnboardingTasks = { new ApiOnboarding.OnboardingTask
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.MediaAsset?.Audit?.CreatedAt ?? new Timestamp()),
                },
            } },
        };
        return response;
    }
    /// <summary>
    /// Lists onboarding tasks a page at a time.
    /// </summary>
    public override async Task<ApiOnboarding.SuggestOnboardingTasksResponse> SuggestOnboardingTasks(ApiOnboarding.SuggestOnboardingTasksRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SuggestOnboardingTasks for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task", "batch",
            "SuggestOnboardingTasks", "accepted", token);
        var input0 = new ApiOnboarding.CountOnboardingPlansRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiOnboarding.CountOnboardingPlansRequest.Types.Filter
            {
                Region = (request.Filter?.Region ?? ""),
            },
            Page = new ApiCommon.PageRequest
            {
                PageSize = (request.Page?.PageSize ?? 0),
            },
        };
        using var rpc0 = _sendSuggestOnboardingTasks0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiOnboarding.SuggestOnboardingTasksResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "SuggestOnboardingTasks", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Summarizes an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.SummarizeOnboardingTasksResponse> SummarizeOnboardingTasks(ApiOnboarding.SummarizeOnboardingTasksRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SummarizeOnboardingTasks for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task", request.OnboardingTaskId,
            "SummarizeOnboardingTasks", "accepted", token);
        var input0 = new ApiItsm.ValidateProblemRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var named0 = _registry.Get<ApiItsm.ProblemValidationService.ProblemValidationServiceClient>("acme.itsm.v1.ProblemValidationService");
        using var rpc0 = named0.ValidateProblemAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiOnboarding.SummarizeOnboardingPlansRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.SummarizeOnboardingPlansRequest.Types.Details
            {
                Cron = (request.Details?.Cron ?? ""),
                Format = (request.Details?.Format ?? ""),
            },
        };
        var descriptor1 = new Method<ApiOnboarding.SummarizeOnboardingPlansRequest, ApiOnboarding.SummarizeOnboardingPlansResponse>(
            MethodType.Unary, "acme.onboarding.v1" + ".OnboardingPlanQueryService", "SummarizeOnboardingPlans",
            Wire.Marshaller<ApiOnboarding.SummarizeOnboardingPlansRequest>(), Wire.Marshaller<ApiOnboarding.SummarizeOnboardingPlansResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiOnboarding.SummarizeOnboardingPlansRequest>("acme.onboarding.v1.OnboardingTaskQueryService/SummarizeOnboardingTasks/1", input1.Clone()), token);
        var response = new ApiOnboarding.SummarizeOnboardingTasksResponse
        {
            Period = (reply1.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
            Summary = { (reply1.Summary ?? new Google.Protobuf.Collections.MapField<string, string>()) },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "SummarizeOnboardingTasks", (request.Reason ?? ""), token);
        return response;
    }
}
