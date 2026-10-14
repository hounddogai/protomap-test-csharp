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
using ApiAudit = Acme.Audit.V1;
using ApiCertificates = Acme.Certificates.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLearning = Acme.Learning.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiPrescriptions = Acme.Prescriptions.V1;
using ApiShows = Acme.Shows.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Onboarding;

/// <summary>
/// Checks equipments against the rules before anything acts on them.
/// </summary>
public sealed class EquipmentValidationEndpoint : ApiOnboarding.EquipmentValidationService.EquipmentValidationServiceBase
{
    private readonly ApiAudit.AuditEventService.AuditEventServiceClient _auditAuditEventService;
    private readonly ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient _auditAuditEventValidationService;
    private readonly ApiCertificates.CertificateService.CertificateServiceClient _certificatesCertificateService;
    private readonly ApiDocuments.DocumentFolderHistoryService.DocumentFolderHistoryServiceClient _documentsDocumentFolderHistoryService;
    private readonly ApiDocuments.DocumentHistoryService.DocumentHistoryServiceClient _documentsDocumentHistoryService;
    private readonly ApiIdentity.UserService.UserServiceClient _identityUserService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ChangeRequestQueryService.ChangeRequestQueryServiceClient _itsmChangeRequestQueryService;
    private readonly ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient _itsmChangeRequestValidationService;
    private readonly ApiLearning.CourseValidationService.CourseValidationServiceClient _learningCourseValidationService;
    private readonly ILogger<EquipmentValidationEndpoint> _logger;
    private readonly ApiOnboarding.OnboardingPlanValidationService.OnboardingPlanValidationServiceClient _onboardingOnboardingPlanValidationService;
    private readonly ApiOnboarding.OnboardingTaskCalculationService.OnboardingTaskCalculationServiceClient _onboardingOnboardingTaskCalculationService;
    private readonly RetryPolicy _policy;
    private readonly ApiPrescriptions.PrescriptionQueryService.PrescriptionQueryServiceClient _prescriptionsPrescriptionQueryService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiItsm.CheckChangeRequestEligibilityRequest, CallOptions, AsyncUnaryCall<ApiItsm.CheckChangeRequestEligibilityResponse>> _sendCheckEquipmentEligibility2;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiShows.ShowQueryService.ShowQueryServiceClient _showsShowQueryService;
    private readonly ApiTimekeeping.ClockEventCalculationService.ClockEventCalculationServiceClient _timekeepingClockEventCalculationService;

    public EquipmentValidationEndpoint(
        ApiAudit.AuditEventService.AuditEventServiceClient auditAuditEventService,
        ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient auditAuditEventValidationService,
        ApiCertificates.CertificateService.CertificateServiceClient certificatesCertificateService,
        ApiDocuments.DocumentFolderHistoryService.DocumentFolderHistoryServiceClient documentsDocumentFolderHistoryService,
        ApiDocuments.DocumentHistoryService.DocumentHistoryServiceClient documentsDocumentHistoryService,
        ApiIdentity.UserService.UserServiceClient identityUserService,
        CallInvoker invoker,
        ApiItsm.ChangeRequestQueryService.ChangeRequestQueryServiceClient itsmChangeRequestQueryService,
        ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient itsmChangeRequestValidationService,
        ApiLearning.CourseValidationService.CourseValidationServiceClient learningCourseValidationService,
        ILogger<EquipmentValidationEndpoint> logger,
        ApiOnboarding.OnboardingPlanValidationService.OnboardingPlanValidationServiceClient onboardingOnboardingPlanValidationService,
        ApiOnboarding.OnboardingTaskCalculationService.OnboardingTaskCalculationServiceClient onboardingOnboardingTaskCalculationService,
        RetryPolicy policy,
        ApiPrescriptions.PrescriptionQueryService.PrescriptionQueryServiceClient prescriptionsPrescriptionQueryService,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiShows.ShowQueryService.ShowQueryServiceClient showsShowQueryService,
        ApiTimekeeping.ClockEventCalculationService.ClockEventCalculationServiceClient timekeepingClockEventCalculationService)
    {
        _auditAuditEventService = auditAuditEventService;
        _auditAuditEventValidationService = auditAuditEventValidationService;
        _certificatesCertificateService = certificatesCertificateService;
        _documentsDocumentFolderHistoryService = documentsDocumentFolderHistoryService;
        _documentsDocumentHistoryService = documentsDocumentHistoryService;
        _identityUserService = identityUserService;
        _invoker = invoker;
        _itsmChangeRequestQueryService = itsmChangeRequestQueryService;
        _itsmChangeRequestValidationService = itsmChangeRequestValidationService;
        _learningCourseValidationService = learningCourseValidationService;
        _logger = logger;
        _onboardingOnboardingPlanValidationService = onboardingOnboardingPlanValidationService;
        _onboardingOnboardingTaskCalculationService = onboardingOnboardingTaskCalculationService;
        _policy = policy;
        _prescriptionsPrescriptionQueryService = prescriptionsPrescriptionQueryService;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _showsShowQueryService = showsShowQueryService;
        _timekeepingClockEventCalculationService = timekeepingClockEventCalculationService;
        _sendCheckEquipmentEligibility2 = _itsmChangeRequestValidationService.CheckChangeRequestEligibilityAsync;
    }
    /// <summary>
    /// Checks an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.CheckEquipmentEligibilityResponse> CheckEquipmentEligibility(ApiOnboarding.CheckEquipmentEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CheckEquipmentEligibility for {Tenant}", requestContext.TenantId);
        if (request.Equipment is { } record)
        {
            var snapshot = EquipmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAudit.CheckAuditEventEligibilityRequest
        {
            AuditEvent = new ApiAudit.AuditEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiAudit.CheckAuditEventEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        using var rpc0 = _auditAuditEventValidationService.CheckAuditEventEligibilityAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiLearning.PreviewCourseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Course = new ApiLearning.Course
            {
                Address = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Equipment?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Equipment?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Dimensions = new ApiLearning.CourseDimensions
                {
                    Height = (request.Equipment?.Dimensions?.Height ?? 0d),
                },
            },
        };
        using var rpc1 = _learningCourseValidationService.PreviewCourseAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var input2 = new ApiItsm.CheckChangeRequestEligibilityRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Equipment?.Audit?.Etag ?? ""),
                    UpdatedBy = (reply1.Course?.Audit?.UpdatedBy ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.CheckChangeRequestEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Equipment?.Owner?.Email ?? ""),
                },
            },
        };
        using var rpc2 = _sendCheckEquipmentEligibility2(input2, new CallOptions(cancellationToken: token));
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiLearning.ValidateCourseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Course = new ApiLearning.Course
            {
                Components = { { "primary", new ApiLearning.CourseComponent
                {
                    Description = (request.Equipment?.Components?.GetValueOrDefault("primary")?.Description ?? ""),
                } } },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply2.ChangeRequest ?? new ApiItsm.ChangeRequest()).OriginCase switch
                    {
                        ApiItsm.ChangeRequest.OriginOneofCase.Customer => (reply2.ChangeRequest?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                    UserId = (reply2.ChangeRequest?.Assignee?.UserId ?? ""),
                },
            },
        };
        using var rpc3 = _learningCourseValidationService.ValidateCourseAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiOnboarding.CheckEquipmentEligibilityResponse
        {
            Equipment = new ApiOnboarding.Equipment
            {
                CategoryPath = { (reply1.Course?.CategoryPath ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Code = (request.Equipment?.Code ?? ""),
                EquipmentId = (request.EquipmentId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.AuditEvent?.Subject?.DisplayName ?? ""),
                },
            },
            Outcome = new ApiOnboarding.CheckEquipmentEligibilityResponse.Types.Outcome
            {
                Message = (reply2.Outcome?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply3.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "equipment",
            "audit", "CheckEquipmentEligibility", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.ExplainEquipmentDecisionResponse> ExplainEquipmentDecision(ApiOnboarding.ExplainEquipmentDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExplainEquipmentDecision for {Tenant}", requestContext.TenantId);
        if (request.Equipment is { } record)
        {
            var snapshot = EquipmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.FindChangeRequestsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.FindChangeRequestsRequest.Types.Filter
            {
                CreatedAfter = (request.Equipment?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (request.Equipment?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Equipment?.Owner?.UserId ?? ""),
                Region = (request.Equipment ?? new ApiOnboarding.Equipment()).LocatorCase switch
                {
                    ApiOnboarding.Equipment.LocatorOneofCase.Address => (request.Equipment?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc0 = _itsmChangeRequestQueryService.FindChangeRequestsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLearning.LintCoursesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLearning.LintCoursesRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
            },
        };
        var command1 = new DispatchCommand<ApiLearning.LintCoursesRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _learningCourseValidationService.LintCoursesAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.ExplainEquipmentDecisionResponse
        {
            Equipment = new ApiOnboarding.Equipment
            {
                EquipmentId = (request.EquipmentId ?? ""),
                Owner = (request.Equipment?.Owner ?? new ApiIdentity.UserRef()),
            },
            Outcome = new ApiOnboarding.ExplainEquipmentDecisionResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "equipment",
            "audit", "ExplainEquipmentDecision", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.GetEquipmentRulesResponse> GetEquipmentRules(ApiOnboarding.GetEquipmentRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EquipmentId, "equipment_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetEquipmentRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "equipment", request.EquipmentId,
            "GetEquipmentRules", "accepted", token);
        var profile0 = new MappingProfile<ApiOnboarding.GetEquipmentRulesRequest, ApiAudit.SearchAuditEventsRequest>(value => new ApiAudit.SearchAuditEventsRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiAudit.SearchAuditEventsRequest.Types.Filter
            {
                CreatedAfter = (value.Context?.Deadline ?? new Timestamp()),
                Labels = { (value.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (value.EquipmentId ?? ""),
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _auditAuditEventService.SearchAuditEventsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiOnboarding.GetEquipmentRulesResponse
        {
            Equipment = new ApiOnboarding.Equipment
            {
                EquipmentId = (request.EquipmentId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lints an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.LintEquipmentsResponse> LintEquipments(ApiOnboarding.LintEquipmentsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling LintEquipments for {Tenant}", requestContext.TenantId);
        if (request.Equipment is { } record)
        {
            var snapshot = EquipmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiOnboarding.NormalizeOnboardingPlanRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.NormalizeOnboardingPlanRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Contact?.Phone ?? ""),
                },
            },
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Equipment?.Audit?.CreatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Equipment?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Locality = (request.Equipment?.Address?.Locality ?? ""),
                },
            },
        };
        using var rpc0 = _onboardingOnboardingPlanValidationService.NormalizeOnboardingPlanAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiItsm.LintChangeRequestsRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Requester = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Contact?.Phone ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _itsmChangeRequestValidationService.LintChangeRequestsAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var input2 = new ApiDocuments.CompareDocumentFolderVersionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.CompareDocumentFolderVersionsRequest.Types.Details
            {
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Location = new ApiCommon.PostalAddress
                        {
                            CountryCode = (reply0.OnboardingPlan?.BillingAddress?.CountryCode ?? ""),
                        },
                    } },
                } },
            },
        };
        using var rpc2 = _documentsDocumentFolderHistoryService.CompareDocumentFolderVersionsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiOnboarding.ValidateOnboardingPlanRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply2.DocumentFolder?.Audit?.CreatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.OnboardingPlan?.BillingAddress?.Locality ?? ""),
                },
            },
        };
        var labels3 = input3.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input3.Context.Baggage.Clear();
        input3.Context.Baggage.Add(labels3);
        using var rpc3 = _onboardingOnboardingPlanValidationService.ValidateOnboardingPlanAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiOnboarding.LintEquipmentsResponse
        {
            Equipment = new ApiOnboarding.Equipment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply2.DocumentFolder?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (reply1.ChangeRequest?.Audit?.UpdatedBy ?? ""),
                },
                Code = (reply3.OnboardingPlan?.Counterparty?.Code ?? ""),
                EquipmentId = (request.EquipmentId ?? ""),
                Owner = (request.Equipment?.Owner ?? new ApiIdentity.UserRef()),
            },
            Outcome = new ApiOnboarding.LintEquipmentsResponse.Types.Outcome
            {
                EffectiveAt = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "equipment",
            "audit", "LintEquipments", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Normalizes an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.NormalizeEquipmentResponse> NormalizeEquipment(ApiOnboarding.NormalizeEquipmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling NormalizeEquipment for {Tenant}", requestContext.TenantId);
        if (request.Equipment is { } record)
        {
            var snapshot = EquipmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCertificates.WatchCertificatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
            },
        };
        using var stream0 = _certificatesCertificateService.WatchCertificates(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchCertificates frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiTimekeeping.CalculateClockEventRequest
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiTimekeeping.TimesheetRef
                {
                    DisplayName = (request.Equipment?.Owner?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.CalculateClockEventRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Equipment ?? new ApiOnboarding.Equipment()).LocatorCase switch
                    {
                        ApiOnboarding.Equipment.LocatorOneofCase.Address => (request.Equipment?.Address?.PostalCode ?? ""),
                        _ => ""
                    },
                    Region = (request.Equipment ?? new ApiOnboarding.Equipment()).LocatorCase switch
                    {
                        ApiOnboarding.Equipment.LocatorOneofCase.Address => (request.Equipment?.Address?.Region ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _timekeepingClockEventCalculationService.CalculateClockEventAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiOnboarding.ScoreOnboardingPlanRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.ScoreOnboardingPlanRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Equipment ?? new ApiOnboarding.Equipment()).LocatorCase switch
                    {
                        ApiOnboarding.Equipment.LocatorOneofCase.Address => (request.Equipment?.Address?.CountryCode ?? ""),
                        _ => ""
                    },
                },
            },
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Equipment ?? new ApiOnboarding.Equipment()).LocatorCase switch
                    {
                        ApiOnboarding.Equipment.LocatorOneofCase.Address => (request.Equipment?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()),
                        _ => new Google.Protobuf.Collections.RepeatedField<string>()
                    } },
                },
                Counterparty = new ApiOnboarding.EquipmentRef
                {
                    DisplayName = (request.Equipment?.Owner?.DisplayName ?? ""),
                },
                Lines = { new ApiOnboarding.OnboardingPlanLine
                {
                    Quantity = (request.Equipment?.Components?.GetValueOrDefault("primary")?.Quantity ?? 0L),
                } },
            },
        };
        using var rpc2 = _onboardingOnboardingPlanValidationService.ScoreOnboardingPlanAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiShows.ResolveShowsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc3 = _showsShowQueryService.ResolveShowsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiOnboarding.NormalizeEquipmentResponse
        {
            Equipment = new ApiOnboarding.Equipment
            {
                EquipmentId = (request.EquipmentId ?? ""),
                Owner = (request.Equipment?.Owner ?? new ApiIdentity.UserRef()),
            },
            Outcome = new ApiOnboarding.NormalizeEquipmentResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "equipment",
            "audit", "NormalizeEquipment", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.PreviewEquipmentResponse> PreviewEquipment(ApiOnboarding.PreviewEquipmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PreviewEquipment for {Tenant}", requestContext.TenantId);
        if (request.Equipment is { } record)
        {
            var snapshot = EquipmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiOnboarding.ScoreOnboardingPlanRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Equipment?.Audit?.CreatedBy ?? ""),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _onboardingOnboardingPlanValidationService.ScoreOnboardingPlanAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiOnboarding.RecalculateOnboardingTasksRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Equipment?.Owner?.DisplayName ?? ""),
                    UserId = (request.Equipment?.Owner?.UserId ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Equipment?.Audit?.Etag ?? ""),
                },
                Comments = { new ApiOnboarding.OnboardingTaskComment
                {
                    CreatedAt = (reply0.OnboardingPlan?.Audit?.CreatedAt ?? new Timestamp()),
                } },
            },
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _onboardingOnboardingTaskCalculationService.RecalculateOnboardingTasksAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiShows.BrowseShowsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiShows.BrowseShowsRequest.Types.Filter
            {
                Region = (request.Equipment ?? new ApiOnboarding.Equipment()).LocatorCase switch
                {
                    ApiOnboarding.Equipment.LocatorOneofCase.Address => (request.Equipment?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc2 = _showsShowQueryService.BrowseShowsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiOnboarding.PreviewEquipmentResponse
        {
            Equipment = new ApiOnboarding.Equipment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.OnboardingPlan?.Audit?.Etag ?? ""),
                },
                EquipmentId = (request.EquipmentId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.OnboardingTask?.Assignee?.DisplayName ?? ""),
                },
                Specs = { (request.Equipment?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply2.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "equipment",
            "audit", "PreviewEquipment", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.ScoreEquipmentResponse> ScoreEquipment(ApiOnboarding.ScoreEquipmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ScoreEquipment for {Tenant}", requestContext.TenantId);
        if (request.Equipment is { } record)
        {
            var snapshot = EquipmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiOnboarding.ScoreOnboardingPlanRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.ScoreOnboardingPlanRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Equipment ?? new ApiOnboarding.Equipment()).LocatorCase switch
                    {
                        ApiOnboarding.Equipment.LocatorOneofCase.Address => (request.Equipment?.Address?.CountryCode ?? ""),
                        _ => ""
                    },
                    Locality = (request.Equipment ?? new ApiOnboarding.Equipment()).LocatorCase switch
                    {
                        ApiOnboarding.Equipment.LocatorOneofCase.Address => (request.Equipment?.Address?.Locality ?? ""),
                        _ => ""
                    },
                },
            },
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
            },
        };
        using var rpc0 = _onboardingOnboardingPlanValidationService.ScoreOnboardingPlanAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiPrescriptions.SuggestPrescriptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiPrescriptions.SuggestPrescriptionsRequest.Types.Filter
            {
                CreatedAfter = (request.Equipment?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (reply0.OnboardingPlan?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.OnboardingPlan?.Counterparty?.EquipmentId ?? ""),
                Region = (reply0.OnboardingPlan?.BillingAddress?.Region ?? ""),
            },
        };
        var named1 = _registry.Get<ApiPrescriptions.PrescriptionQueryService.PrescriptionQueryServiceClient>("acme.prescriptions.v1.PrescriptionQueryService");
        using var rpc1 = named1.SuggestPrescriptionsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.ScoreEquipmentResponse
        {
            Breakdown = new ApiOnboarding.ScoreEquipmentResponse.Types.Breakdown
            {
                CurrencyCode = (reply0.Breakdown?.CurrencyCode ?? ""),
            },
            Equipment = new ApiOnboarding.Equipment
            {
                Capacity = (request.Equipment?.Capacity ?? 0L),
                EquipmentId = (request.EquipmentId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "equipment",
            "audit", "ScoreEquipment", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.ValidateEquipmentResponse> ValidateEquipment(ApiOnboarding.ValidateEquipmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ValidateEquipment for {Tenant}", requestContext.TenantId);
        if (request.Equipment is { } record)
        {
            var snapshot = EquipmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiIdentity.BatchGetUsersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var descriptor0 = new Method<ApiIdentity.BatchGetUsersRequest, ApiIdentity.BatchGetUsersResponse>(
            MethodType.Unary, "acme.identity.v1" + ".UserService", "BatchGetUsers",
            Wire.Marshaller<ApiIdentity.BatchGetUsersRequest>(), Wire.Marshaller<ApiIdentity.BatchGetUsersResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.ListDocumentVersionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentVersionsRequest.Types.Filter
            {
                Region = (request.Equipment ?? new ApiOnboarding.Equipment()).LocatorCase switch
                {
                    ApiOnboarding.Equipment.LocatorOneofCase.Address => (request.Equipment?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc1 = _documentsDocumentHistoryService.ListDocumentVersionsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.ValidateEquipmentResponse
        {
            Equipment = new ApiOnboarding.Equipment
            {
                Capacity = (request.Equipment?.Capacity ?? 0L),
                EquipmentId = (request.EquipmentId ?? ""),
            },
            Outcome = new ApiOnboarding.ValidateEquipmentResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "equipment",
            "audit", "ValidateEquipment", (request.Reason ?? ""), token);
        return response;
    }
}
