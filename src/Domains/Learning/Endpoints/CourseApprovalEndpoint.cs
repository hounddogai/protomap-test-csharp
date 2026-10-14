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
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiMedia = Acme.Media.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Learning;

/// <summary>
/// Routes courses through approval, with escalation and delegation.
/// </summary>
public sealed class CourseApprovalEndpoint : ApiLearning.CourseApprovalService.CourseApprovalServiceBase
{
    private readonly ApiCorporateevents.CorporateEventWorkflowService.CorporateEventWorkflowServiceClient _corporateeventsCorporateEventWorkflowService;
    private readonly ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient _documentsDocumentFolderSyncService;
    private readonly ApiDocuments.DocumentTemplateAdminService.DocumentTemplateAdminServiceClient _documentsDocumentTemplateAdminService;
    private readonly ApiDocuments.DocumentTemplateExportService.DocumentTemplateExportServiceClient _documentsDocumentTemplateExportService;
    private readonly ApiHelpdesk.SlaPolicyWorkflowService.SlaPolicyWorkflowServiceClient _helpdeskSlaPolicyWorkflowService;
    private readonly ApiLearning.CourseValidationService.CourseValidationServiceClient _learningCourseValidationService;
    private readonly ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient _learningLearningPathCalculationService;
    private readonly ApiLearning.LearningPathQueryService.LearningPathQueryServiceClient _learningLearningPathQueryService;
    private readonly ILogger<CourseApprovalEndpoint> _logger;
    private readonly ApiMedia.MediaAssetService.MediaAssetServiceClient _mediaMediaAssetService;
    private readonly ApiRecruiting.OfferValidationService.OfferValidationServiceClient _recruitingOfferValidationService;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTravel.TripLifecycleService.TripLifecycleServiceClient _travelTripLifecycleService;
    private readonly ApiVenues.VenueValidationService.VenueValidationServiceClient _venuesVenueValidationService;

    public CourseApprovalEndpoint(
        ApiCorporateevents.CorporateEventWorkflowService.CorporateEventWorkflowServiceClient corporateeventsCorporateEventWorkflowService,
        ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient documentsDocumentFolderSyncService,
        ApiDocuments.DocumentTemplateAdminService.DocumentTemplateAdminServiceClient documentsDocumentTemplateAdminService,
        ApiDocuments.DocumentTemplateExportService.DocumentTemplateExportServiceClient documentsDocumentTemplateExportService,
        ApiHelpdesk.SlaPolicyWorkflowService.SlaPolicyWorkflowServiceClient helpdeskSlaPolicyWorkflowService,
        ApiLearning.CourseValidationService.CourseValidationServiceClient learningCourseValidationService,
        ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient learningLearningPathCalculationService,
        ApiLearning.LearningPathQueryService.LearningPathQueryServiceClient learningLearningPathQueryService,
        ILogger<CourseApprovalEndpoint> logger,
        ApiMedia.MediaAssetService.MediaAssetServiceClient mediaMediaAssetService,
        ApiRecruiting.OfferValidationService.OfferValidationServiceClient recruitingOfferValidationService,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiTravel.TripLifecycleService.TripLifecycleServiceClient travelTripLifecycleService,
        ApiVenues.VenueValidationService.VenueValidationServiceClient venuesVenueValidationService)
    {
        _corporateeventsCorporateEventWorkflowService = corporateeventsCorporateEventWorkflowService;
        _documentsDocumentFolderSyncService = documentsDocumentFolderSyncService;
        _documentsDocumentTemplateAdminService = documentsDocumentTemplateAdminService;
        _documentsDocumentTemplateExportService = documentsDocumentTemplateExportService;
        _helpdeskSlaPolicyWorkflowService = helpdeskSlaPolicyWorkflowService;
        _learningCourseValidationService = learningCourseValidationService;
        _learningLearningPathCalculationService = learningLearningPathCalculationService;
        _learningLearningPathQueryService = learningLearningPathQueryService;
        _logger = logger;
        _mediaMediaAssetService = mediaMediaAssetService;
        _recruitingOfferValidationService = recruitingOfferValidationService;
        _repository = repository;
        _settings = settings;
        _travelTripLifecycleService = travelTripLifecycleService;
        _venuesVenueValidationService = venuesVenueValidationService;
    }
    /// <summary>
    /// Approves a course.
    /// </summary>
    public override async Task<ApiLearning.ApproveCourseResponse> ApproveCourse(ApiLearning.ApproveCourseRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ApproveCourse for {Tenant}", requestContext.TenantId);
        if (request.Course is { } record)
        {
            var snapshot = CourseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiVenues.LintVenuesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Venue = new ApiVenues.Venue
            {
                Address = new ApiCommon.PostalAddress
                {
                    Locality = (request.Course ?? new ApiLearning.Course()).LocatorCase switch
                    {
                        ApiLearning.Course.LocatorOneofCase.Address => (request.Course?.Address?.Locality ?? ""),
                        _ => ""
                    },
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Course?.Audit?.CreatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _venuesVenueValidationService.LintVenuesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLearning.ExportLearningPathsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLearning.ExportLearningPathsRequest.Types.Filter
            {
                CreatedAfter = (reply0.Venue?.Audit?.UpdatedAt ?? new Timestamp()),
                Region = (request.Course?.Address?.Region ?? ""),
            },
        };
        using var stream1 = _learningLearningPathQueryService.ExportLearningPaths(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportLearningPaths frame {Type}", change1.GetType().Name);
        }
        var response = new ApiLearning.ApproveCourseResponse
        {
            Course = new ApiLearning.Course
            {
                CourseId = (request.CourseId ?? ""),
                Status = (request.Course?.Status ?? (ApiLearning.CourseStatus)0),
            },
            Outcome = new ApiLearning.ApproveCourseResponse.Types.Outcome
            {
                EffectiveAt = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
                NextSteps = { (reply0.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course",
            "audit", "ApproveCourse", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments a course.
    /// </summary>
    public override async Task<ApiLearning.CommentCourseApprovalResponse> CommentCourseApproval(ApiLearning.CommentCourseApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CommentCourseApproval for {Tenant}", requestContext.TenantId);
        if (request.Course is { } record)
        {
            var snapshot = CourseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.ScheduleTripExpiryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.ScheduleTripExpiryRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Course?.Owner?.Email ?? ""),
                },
            },
            Trip = new ApiTravel.Trip
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Course?.Audit?.Etag ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Course ?? new ApiLearning.Course()).LocatorCase switch
                    {
                        ApiLearning.Course.LocatorOneofCase.Address => (request.Course?.Address?.PostalCode ?? ""),
                        _ => ""
                    },
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _travelTripLifecycleService.ScheduleTripExpiryAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLearning.CommentCourseApprovalResponse
        {
            Course = new ApiLearning.Course
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.Trip?.Audit?.UpdatedBy ?? ""),
                },
                CourseId = (request.CourseId ?? ""),
            },
            Outcome = new ApiLearning.CommentCourseApprovalResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course",
            "audit", "CommentCourseApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates a course.
    /// </summary>
    public override async Task<ApiLearning.DelegateCourseApprovalResponse> DelegateCourseApproval(ApiLearning.DelegateCourseApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling DelegateCourseApproval for {Tenant}", requestContext.TenantId);
        if (request.Course is { } record)
        {
            var snapshot = CourseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.LintOffersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Offer = new ApiRecruiting.Offer
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (request.Course ?? new ApiLearning.Course()).LocatorCase switch
                    {
                        ApiLearning.Course.LocatorOneofCase.Address => (request.Course?.Address?.Locality ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _recruitingOfferValidationService.LintOffersAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLearning.DelegateCourseApprovalResponse
        {
            Course = new ApiLearning.Course
            {
                Code = (request.Course?.Code ?? ""),
                CourseId = (request.CourseId ?? ""),
            },
            Outcome = new ApiLearning.DelegateCourseApprovalResponse.Types.Outcome
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course",
            "audit", "DelegateCourseApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates a course.
    /// </summary>
    public override async Task<ApiLearning.EscalateCourseResponse> EscalateCourse(ApiLearning.EscalateCourseRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling EscalateCourse for {Tenant}", requestContext.TenantId);
        if (request.Course is { } record)
        {
            var snapshot = CourseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLearning.PreviewCourseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Course = new ApiLearning.Course
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.Course?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                    UserId = (request.Details?.Approver?.UserId ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _learningCourseValidationService.PreviewCourseAsync);
        var input1 = new ApiDocuments.RepairDocumentTemplateRequest
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
                    CreatedAt = (reply0.Course?.Audit?.CreatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.Course ?? new ApiLearning.Course()).LocatorCase switch
                    {
                        ApiLearning.Course.LocatorOneofCase.Address => (reply0.Course?.Address?.CountryCode ?? ""),
                        _ => ""
                    },
                    Lines = { (reply0.Course ?? new ApiLearning.Course()).LocatorCase switch
                    {
                        ApiLearning.Course.LocatorOneofCase.Address => (reply0.Course?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()),
                        _ => new Google.Protobuf.Collections.RepeatedField<string>()
                    } },
                    PostalCode = (reply0.Course ?? new ApiLearning.Course()).LocatorCase switch
                    {
                        ApiLearning.Course.LocatorOneofCase.Address => (reply0.Course?.Address?.PostalCode ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _documentsDocumentTemplateAdminService.RepairDocumentTemplateAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDocuments.SyncDocumentFoldersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SyncDocumentFoldersRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            CreatedAt = (request.Course?.Audit?.CreatedAt ?? new Timestamp()),
                        },
                    } },
                } },
            },
        };
        using var rpc2 = _documentsDocumentFolderSyncService.SyncDocumentFoldersAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiLearning.EscalateCourseResponse
        {
            Course = new ApiLearning.Course
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.Course?.Audit?.UpdatedBy ?? ""),
                },
                CourseId = (request.CourseId ?? ""),
                Owner = (reply0.Course?.Owner ?? new ApiIdentity.UserRef()),
            },
            Outcome = new ApiLearning.EscalateCourseResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply2.Result?.Message ?? ""),
                ReferenceId = (reply1.Outcome?.ReferenceId ?? ""),
                Status = (reply2.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course",
            "audit", "EscalateCourse", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls a course.
    /// </summary>
    public override async Task<ApiLearning.RecallCourseResponse> RecallCourse(ApiLearning.RecallCourseRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RecallCourse for {Tenant}", requestContext.TenantId);
        if (request.Course is { } record)
        {
            var snapshot = CourseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.AdvanceCorporateEventWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Location = new ApiCommon.PostalAddress
                {
                    Region = (request.Course ?? new ApiLearning.Course()).LocatorCase switch
                    {
                        ApiLearning.Course.LocatorOneofCase.Address => (request.Course?.Address?.Region ?? ""),
                        _ => ""
                    },
                },
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _corporateeventsCorporateEventWorkflowService.AdvanceCorporateEventWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLearning.ValidateCourseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Course = new ApiLearning.Course
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.Course?.Audit?.UpdatedBy ?? ""),
                },
                Dimensions = new ApiLearning.CourseDimensions
                {
                    Unit = (request.Course?.Dimensions?.Unit ?? ""),
                },
            },
            Details = new ApiLearning.ValidateCourseRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
        };
        var command1 = new DispatchCommand<ApiLearning.ValidateCourseRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _learningCourseValidationService.ValidateCourseAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiHelpdesk.ListSlaPolicyWorkflowsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiHelpdesk.ListSlaPolicyWorkflowsRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
                OwnerId = (reply1.Course?.Components?.GetValueOrDefault("primary")?.ComponentId ?? ""),
                Region = (reply1.Course ?? new ApiLearning.Course()).LocatorCase switch
                {
                    ApiLearning.Course.LocatorOneofCase.Address => (reply1.Course?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc2 = _helpdeskSlaPolicyWorkflowService.ListSlaPolicyWorkflowsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiLearning.RecallCourseResponse
        {
            Course = new ApiLearning.Course
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.Course?.Audit?.CreatedBy ?? ""),
                },
                Code = (request.Course?.Code ?? ""),
                CourseId = (request.CourseId ?? ""),
            },
            Outcome = new ApiLearning.RecallCourseResponse.Types.Outcome
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course",
            "audit", "RecallCourse", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects a course.
    /// </summary>
    public override async Task<ApiLearning.RejectCourseResponse> RejectCourse(ApiLearning.RejectCourseRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RejectCourse for {Tenant}", requestContext.TenantId);
        if (request.Course is { } record)
        {
            var snapshot = CourseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLearning.ExportLearningPathsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLearning.ExportLearningPathsRequest.Types.Filter
            {
                CreatedAfter = (request.Course?.Audit?.UpdatedAt ?? new Timestamp()),
                OwnerId = (request.Course?.Owner?.UserId ?? ""),
                Region = (request.Course?.Address?.Region ?? ""),
            },
        };
        using var stream0 = _learningLearningPathQueryService.ExportLearningPaths(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportLearningPaths frame {Type}", change0.GetType().Name);
        }
        var response = new ApiLearning.RejectCourseResponse
        {
            Course = new ApiLearning.Course
            {
                CourseId = (request.CourseId ?? ""),
            },
            Outcome = new ApiLearning.RejectCourseResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course",
            "audit", "RejectCourse", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits a course.
    /// </summary>
    public override async Task<ApiLearning.SubmitCourseResponse> SubmitCourse(ApiLearning.SubmitCourseRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SubmitCourse for {Tenant}", requestContext.TenantId);
        if (request.Course is { } record)
        {
            var snapshot = CourseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiMedia.WatchMediaAssetsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiMedia.WatchMediaAssetsRequest.Types.Filter
            {
                Region = (request.Course?.Address?.Region ?? ""),
            },
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (request.Course?.Owner?.Email ?? ""),
            },
        };
        using var stream0 = _mediaMediaAssetService.WatchMediaAssets(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchMediaAssets frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiDocuments.StreamDocumentTemplateExportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.StreamDocumentTemplateExportRequest.Types.Filter
            {
                Region = (request.Course?.Address?.Region ?? ""),
            },
        };
        using var stream1 = _documentsDocumentTemplateExportService.StreamDocumentTemplateExport(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamDocumentTemplateExport frame {Type}", change1.GetType().Name);
        }
        var input2 = new ApiLearning.SimulateLearningPathRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LearningPath = new ApiLearning.LearningPath
            {
                Children = { new ApiLearning.LearningPath
                {
                    Children = { new ApiLearning.LearningPath
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            Etag = (request.Course?.Audit?.Etag ?? ""),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            CountryCode = (request.Course ?? new ApiLearning.Course()).LocatorCase switch
                            {
                                ApiLearning.Course.LocatorOneofCase.Address => (request.Course?.Address?.CountryCode ?? ""),
                                _ => ""
                            },
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (request.Course?.Owner?.DisplayName ?? ""),
                            Email = (request.Course?.Owner?.Email ?? ""),
                        },
                    } },
                } },
            },
        };
        using var rpc2 = _learningLearningPathCalculationService.SimulateLearningPathAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiLearning.GetCourseRulesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CourseId = (request.Course?.CourseId ?? ""),
        };
        var packed3 = Any.Pack(input3);
        input3 = packed3.Unpack<ApiLearning.GetCourseRulesRequest>();
        using var rpc3 = _learningCourseValidationService.GetCourseRulesAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiLearning.SubmitCourseResponse
        {
            Course = new ApiLearning.Course
            {
                Capacity = (reply3.Course?.Capacity ?? 0L),
                CourseId = (request.CourseId ?? ""),
                Specs = { (request.Course?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Outcome = new ApiLearning.SubmitCourseResponse.Types.Outcome
            {
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course",
            "audit", "SubmitCourse", (request.Reason ?? ""), token);
        return response;
    }
}
