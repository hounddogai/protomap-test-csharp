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
using ApiCatalog = Acme.Catalog.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiCustomers = Acme.Customers.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecommendations = Acme.Recommendations.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Performance;

/// <summary>
/// Moves a calibration between the states of its lifecycle.
/// </summary>
public sealed class CalibrationLifecycleEndpoint : ApiPerformance.CalibrationLifecycleService.CalibrationLifecycleServiceBase
{
    private readonly NotificationBus _bus;
    private readonly ICalibrationLifecycleEndpointSendActivateCalibration2Port _calibrationLifecycleEndpointSendActivateCalibration2Port;
    private readonly ApiCatalog.ProductQueryService.ProductQueryServiceClient _catalogProductQueryService;
    private readonly ApiCorporateevents.RegistrationReportingService.RegistrationReportingServiceClient _corporateeventsRegistrationReportingService;
    private readonly ApiDocuments.SignatureNotificationService.SignatureNotificationServiceClient _documentsSignatureNotificationService;
    private readonly ApiDocuments.SignatureSyncService.SignatureSyncServiceClient _documentsSignatureSyncService;
    private readonly CallInvoker _invoker;
    private readonly ApiLicensing.SoftwareLicenseWorkflowService.SoftwareLicenseWorkflowServiceClient _licensingSoftwareLicenseWorkflowService;
    private readonly ILogger<CalibrationLifecycleEndpoint> _logger;
    private readonly ApiPerformance.FeedbackCalculationService.FeedbackCalculationServiceClient _performanceFeedbackCalculationService;
    private readonly ApiPerformance.GoalQueryService.GoalQueryServiceClient _performanceGoalQueryService;
    private readonly ApiPerformance.GoalValidationService.GoalValidationServiceClient _performanceGoalValidationService;
    private readonly ApiPerformance.ReviewQueryService.ReviewQueryServiceClient _performanceReviewQueryService;
    private readonly ApiRecommendations.RecommendationSetService.RecommendationSetServiceClient _recommendationsRecommendationSetService;
    private readonly ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient _recruitingCandidateCalculationService;
    private readonly ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient _recruitingCandidateValidationService;
    private readonly ApiRecruiting.OfferCalculationService.OfferCalculationServiceClient _recruitingOfferCalculationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;

    public CalibrationLifecycleEndpoint(
        NotificationBus bus,
        ICalibrationLifecycleEndpointSendActivateCalibration2Port calibrationLifecycleEndpointSendActivateCalibration2PortAdapter,
        ApiCatalog.ProductQueryService.ProductQueryServiceClient catalogProductQueryService,
        ApiCorporateevents.RegistrationReportingService.RegistrationReportingServiceClient corporateeventsRegistrationReportingService,
        ApiDocuments.SignatureNotificationService.SignatureNotificationServiceClient documentsSignatureNotificationService,
        ApiDocuments.SignatureSyncService.SignatureSyncServiceClient documentsSignatureSyncService,
        CallInvoker invoker,
        ApiLicensing.SoftwareLicenseWorkflowService.SoftwareLicenseWorkflowServiceClient licensingSoftwareLicenseWorkflowService,
        ILogger<CalibrationLifecycleEndpoint> logger,
        ApiPerformance.FeedbackCalculationService.FeedbackCalculationServiceClient performanceFeedbackCalculationService,
        ApiPerformance.GoalQueryService.GoalQueryServiceClient performanceGoalQueryService,
        ApiPerformance.GoalValidationService.GoalValidationServiceClient performanceGoalValidationService,
        ApiPerformance.ReviewQueryService.ReviewQueryServiceClient performanceReviewQueryService,
        ApiRecommendations.RecommendationSetService.RecommendationSetServiceClient recommendationsRecommendationSetService,
        ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient recruitingCandidateCalculationService,
        ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient recruitingCandidateValidationService,
        ApiRecruiting.OfferCalculationService.OfferCalculationServiceClient recruitingOfferCalculationService,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings)
    {
        _bus = bus;
        _calibrationLifecycleEndpointSendActivateCalibration2Port = calibrationLifecycleEndpointSendActivateCalibration2PortAdapter;
        _catalogProductQueryService = catalogProductQueryService;
        _corporateeventsRegistrationReportingService = corporateeventsRegistrationReportingService;
        _documentsSignatureNotificationService = documentsSignatureNotificationService;
        _documentsSignatureSyncService = documentsSignatureSyncService;
        _invoker = invoker;
        _licensingSoftwareLicenseWorkflowService = licensingSoftwareLicenseWorkflowService;
        _logger = logger;
        _performanceFeedbackCalculationService = performanceFeedbackCalculationService;
        _performanceGoalQueryService = performanceGoalQueryService;
        _performanceGoalValidationService = performanceGoalValidationService;
        _performanceReviewQueryService = performanceReviewQueryService;
        _recommendationsRecommendationSetService = recommendationsRecommendationSetService;
        _recruitingCandidateCalculationService = recruitingCandidateCalculationService;
        _recruitingCandidateValidationService = recruitingCandidateValidationService;
        _recruitingOfferCalculationService = recruitingOfferCalculationService;
        _registry = registry;
        _repository = repository;
        _settings = settings;
    }
    /// <summary>
    /// Activates a calibration.
    /// </summary>
    public override async Task<ApiPerformance.ActivateCalibrationResponse> ActivateCalibration(ApiPerformance.ActivateCalibrationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CalibrationId, "calibration_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ActivateCalibration for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "calibration", request.CalibrationId,
            "ActivateCalibration", "accepted", token);
        var input0 = new ApiPerformance.FindReviewsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiPerformance.FindReviewsRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _performanceReviewQueryService.FindReviewsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.ActivateCalibrationToCountProducts1();
        using var rpc1 = _catalogProductQueryService.CountProductsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiPerformance.PreviewGoalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.PreviewGoalRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
            },
            Goal = new ApiPerformance.Goal
            {
                Customer = new ApiPerformance.ReviewRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
        };
        var reply2 = await SendActivateCalibration2Async(input2, token);
        var response = new ApiPerformance.ActivateCalibrationResponse
        {
            Calibration = new ApiPerformance.Calibration
            {
                CalibrationId = (request.CalibrationId ?? ""),
            },
            Outcome = new ApiPerformance.ActivateCalibrationResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply1.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "calibration",
            "audit", "ActivateCalibration", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Expires a calibration.
    /// </summary>
    public override async Task<ApiPerformance.ExpireCalibrationResponse> ExpireCalibration(ApiPerformance.ExpireCalibrationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CalibrationId, "calibration_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExpireCalibration for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "calibration", request.CalibrationId,
            "ExpireCalibration", "accepted", token);
        var input0 = new ApiRecruiting.CalculateCandidateRequest
        {
            Candidate = new ApiRecruiting.Candidate
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _recruitingCandidateCalculationService.CalculateCandidateAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiRecruiting.CalculateCandidateRequest>("acme.performance.v1.CalibrationLifecycleService/ExpireCalibration/0", input0.Clone()), token);
        var input1 = new ApiLicensing.CancelSoftwareLicenseWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.CancelSoftwareLicenseWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Candidate?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Holder = new ApiLicensing.LicenseAssignmentRef
                {
                    DisplayName = (reply0.Candidate?.RelationshipManager?.DisplayName ?? ""),
                },
            },
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiLicensing.CancelSoftwareLicenseWorkflowRequest>();
        using var rpc1 = _licensingSoftwareLicenseWorkflowService.CancelSoftwareLicenseWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiPerformance.ExpireCalibrationResponse
        {
            Calibration = new ApiPerformance.Calibration
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                    UserId = (reply0.Candidate?.RelationshipManager?.UserId ?? ""),
                },
                CalibrationId = (request.CalibrationId ?? ""),
            },
            Outcome = new ApiPerformance.ExpireCalibrationResponse.Types.Outcome
            {
                EffectiveAt = (reply1.Outcome?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "calibration",
            "audit", "ExpireCalibration", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a calibration.
    /// </summary>
    public override async Task<ApiPerformance.GetCalibrationLifecycleResponse> GetCalibrationLifecycle(ApiPerformance.GetCalibrationLifecycleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CalibrationId, "calibration_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetCalibrationLifecycle for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "calibration", request.CalibrationId,
            "GetCalibrationLifecycle", "accepted", token);
        var input0 = new ApiDocuments.ResolveSignatureConflictRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ResolveSignatureConflictRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Measurements = { new ApiDocuments.SignatureMeasurement
                {
                    ObservedAt = (request.Context?.Deadline ?? new Timestamp()),
                } },
            },
        };
        using var rpc0 = _documentsSignatureSyncService.ResolveSignatureConflictAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiPerformance.ValidateGoalRequest();
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        input1.Goal = new ApiPerformance.Goal
        {
            Assignee = new ApiIdentity.UserRef
            {
                UserId = (request.Context?.Actor?.UserId ?? ""),
            },
            Customer = new ApiPerformance.ReviewRef
            {
                DisplayName = (reply0.Signature?.Subject?.DisplayName ?? ""),
                Number = (reply0.Signature?.Subject?.Number ?? ""),
            },
        };
        using var rpc1 = _performanceGoalValidationService.ValidateGoalAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiPerformance.GetCalibrationLifecycleResponse
        {
            Calibration = new ApiPerformance.Calibration
            {
                CalibrationId = (request.CalibrationId ?? ""),
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply1.Goal?.Requester?.Email ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply1.Outcome?.Message ?? ""),
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Reinstates a calibration.
    /// </summary>
    public override async Task<ApiPerformance.ReinstateCalibrationResponse> ReinstateCalibration(ApiPerformance.ReinstateCalibrationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CalibrationId, "calibration_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ReinstateCalibration for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "calibration", request.CalibrationId,
            "ReinstateCalibration", "accepted", token);
        var input0 = new ApiDocuments.SyncSignaturesRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SyncSignaturesRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _documentsSignatureSyncService.SyncSignaturesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecommendations.CreateRecommendationSetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            RecommendationSet = new ApiRecommendations.RecommendationSet
            {
                Counterparty = new ApiCustomers.CustomerRef
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _recommendationsRecommendationSetService.CreateRecommendationSetAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiPerformance.ExportGoalsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiPerformance.ExportGoalsRequest.Types.Filter
            {
                CreatedAfter = (reply1.RecommendationSet?.Audit?.UpdatedAt ?? new Timestamp()),
                Region = (reply1.RecommendationSet?.BillingAddress?.Region ?? ""),
            },
        };
        using var stream2 = _performanceGoalQueryService.ExportGoals(input2, cancellationToken: token);
        await foreach (var change2 in stream2.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportGoals frame {Type}", change2.GetType().Name);
        }
        var response = new ApiPerformance.ReinstateCalibrationResponse
        {
            Calibration = new ApiPerformance.Calibration
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply1.RecommendationSet?.Counterparty?.Email ?? ""),
                },
                CalibrationId = (request.CalibrationId ?? ""),
            },
            Outcome = new ApiPerformance.ReinstateCalibrationResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "calibration",
            "audit", "ReinstateCalibration", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules a calibration.
    /// </summary>
    public override async Task<ApiPerformance.ScheduleCalibrationExpiryResponse> ScheduleCalibrationExpiry(ApiPerformance.ScheduleCalibrationExpiryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ScheduleCalibrationExpiry for {Tenant}", requestContext.TenantId);
        if (request.Calibration is { } record)
        {
            var snapshot = CalibrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPerformance.SummarizeReviewsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ReviewId = (request.Calibration ?? new ApiPerformance.Calibration()).OriginCase switch
            {
                ApiPerformance.Calibration.OriginOneofCase.Customer => (request.Calibration?.Customer?.ReviewId ?? ""),
                _ => ""
            },
        };
        using var rpc0 = _performanceReviewQueryService.SummarizeReviewsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiPerformance.RecalculateFeedbacksRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Feedback = new ApiPerformance.Feedback
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (request.Calibration ?? new ApiPerformance.Calibration()).OriginCase switch
                    {
                        ApiPerformance.Calibration.OriginOneofCase.Customer => (request.Calibration?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _performanceFeedbackCalculationService.RecalculateFeedbacksAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiPerformance.ScheduleCalibrationExpiryResponse
        {
            Calibration = new ApiPerformance.Calibration
            {
                CalibrationId = (request.CalibrationId ?? ""),
                CustomFields = { (request.Calibration?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "calibration",
            "audit", "ScheduleCalibrationExpiry", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Suspends a calibration.
    /// </summary>
    public override async Task<ApiPerformance.SuspendCalibrationResponse> SuspendCalibration(ApiPerformance.SuspendCalibrationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CalibrationId, "calibration_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SuspendCalibration for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "calibration", request.CalibrationId,
            "SuspendCalibration", "accepted", token);
        var input0 = new ApiCorporateevents.GenerateRegistrationReportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.GenerateRegistrationReportRequest.Types.Details
            {
                Filters = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Reason = (request.Reason ?? ""),
        };
        var named0 = _registry.Get<ApiCorporateevents.RegistrationReportingService.RegistrationReportingServiceClient>("acme.corporateevents.v1.RegistrationReportingService");
        using var rpc0 = named0.GenerateRegistrationReportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.ExplainCandidateDecisionRequest
        {
            Candidate = new ApiRecruiting.Candidate
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.ExplainCandidateDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
        };
        var descriptor1 = new Method<ApiRecruiting.ExplainCandidateDecisionRequest, ApiRecruiting.ExplainCandidateDecisionResponse>(
            MethodType.Unary, "acme.recruiting.v1" + ".CandidateValidationService", "ExplainCandidateDecision",
            Wire.Marshaller<ApiRecruiting.ExplainCandidateDecisionRequest>(), Wire.Marshaller<ApiRecruiting.ExplainCandidateDecisionResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiPerformance.SuspendCalibrationResponse
        {
            Calibration = new ApiPerformance.Calibration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Candidate?.Audit?.CreatedAt ?? new Timestamp()),
                },
                CalibrationId = (request.CalibrationId ?? ""),
            },
            Outcome = new ApiPerformance.SuspendCalibrationResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply1.Outcome?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.ReasonCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "calibration",
            "audit", "SuspendCalibration", (request.Details?.ReasonCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Terminates a calibration.
    /// </summary>
    public override async Task<ApiPerformance.TerminateCalibrationResponse> TerminateCalibration(ApiPerformance.TerminateCalibrationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CalibrationId, "calibration_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling TerminateCalibration for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "calibration", request.CalibrationId,
            "TerminateCalibration", "accepted", token);
        var input0 = new ApiRecruiting.EstimateOfferRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Offer = new ApiRecruiting.Offer
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiRecruiting.EstimateOfferRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _recruitingOfferCalculationService.EstimateOfferAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiPerformance.TerminateCalibrationRequest, ApiRecruiting.QuoteCandidateRequest>(value => new ApiRecruiting.QuoteCandidateRequest
        {
            Candidate = new ApiRecruiting.Candidate
            {
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.Offer?.BillingAddress?.PostalCode ?? ""),
                },
            },
            Context = (value.Context ?? new ApiCommon.RequestContext()),
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _recruitingCandidateCalculationService.QuoteCandidateAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDocuments.UnsubscribeSignatureUpdatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.UnsubscribeSignatureUpdatesRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        using var rpc2 = _documentsSignatureNotificationService.UnsubscribeSignatureUpdatesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiPerformance.TerminateCalibrationResponse
        {
            Calibration = new ApiPerformance.Calibration
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply2.Signature?.Subject?.DisplayName ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.Candidate?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                CalibrationId = (request.CalibrationId ?? ""),
            },
            Outcome = new ApiPerformance.TerminateCalibrationResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "calibration",
            "audit", "TerminateCalibration", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiPerformance.PreviewGoalResponse> SendActivateCalibration2Async(ApiPerformance.PreviewGoalRequest request, CancellationToken token)
    {
        return await _calibrationLifecycleEndpointSendActivateCalibration2Port.SendAsync(request, token);
    }
}
