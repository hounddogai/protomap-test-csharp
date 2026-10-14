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
using ApiLearning = Acme.Learning.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Learning;

/// <summary>
/// Manages courses: creates, reads, changes, and lists them.
/// </summary>
public sealed class CourseEndpoint : ApiLearning.CourseService.CourseServiceBase
{
    private readonly NotificationBus _bus;
    private readonly ApiDevices.ComplianceScanValidationService.ComplianceScanValidationServiceClient _devicesComplianceScanValidationService;
    private readonly ApiLearning.CertificationValidationService.CertificationValidationServiceClient _learningCertificationValidationService;
    private readonly ApiLearning.CourseEnrollmentCalculationService.CourseEnrollmentCalculationServiceClient _learningCourseEnrollmentCalculationService;
    private readonly ApiLearning.CourseEnrollmentValidationService.CourseEnrollmentValidationServiceClient _learningCourseEnrollmentValidationService;
    private readonly ApiLearning.CourseQueryService.CourseQueryServiceClient _learningCourseQueryService;
    private readonly ApiLearning.CourseValidationService.CourseValidationServiceClient _learningCourseValidationService;
    private readonly ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient _learningLearningPathCalculationService;
    private readonly ApiLearning.LearningPathQueryService.LearningPathQueryServiceClient _learningLearningPathQueryService;
    private readonly ILogger<CourseEndpoint> _logger;
    private readonly ApiPerformance.FeedbackValidationService.FeedbackValidationServiceClient _performanceFeedbackValidationService;
    private readonly ApiPerformance.ReviewService.ReviewServiceClient _performanceReviewService;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiLearning.CompareLearningPathOptionsRequest, CallOptions, AsyncUnaryCall<ApiLearning.CompareLearningPathOptionsResponse>> _sendBatchCreateCourses3;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTravel.ItineraryValidationService.ItineraryValidationServiceClient _travelItineraryValidationService;

    public CourseEndpoint(
        NotificationBus bus,
        ApiDevices.ComplianceScanValidationService.ComplianceScanValidationServiceClient devicesComplianceScanValidationService,
        ApiLearning.CertificationValidationService.CertificationValidationServiceClient learningCertificationValidationService,
        ApiLearning.CourseEnrollmentCalculationService.CourseEnrollmentCalculationServiceClient learningCourseEnrollmentCalculationService,
        ApiLearning.CourseEnrollmentValidationService.CourseEnrollmentValidationServiceClient learningCourseEnrollmentValidationService,
        ApiLearning.CourseQueryService.CourseQueryServiceClient learningCourseQueryService,
        ApiLearning.CourseValidationService.CourseValidationServiceClient learningCourseValidationService,
        ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient learningLearningPathCalculationService,
        ApiLearning.LearningPathQueryService.LearningPathQueryServiceClient learningLearningPathQueryService,
        ILogger<CourseEndpoint> logger,
        ApiPerformance.FeedbackValidationService.FeedbackValidationServiceClient performanceFeedbackValidationService,
        ApiPerformance.ReviewService.ReviewServiceClient performanceReviewService,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiTravel.ItineraryValidationService.ItineraryValidationServiceClient travelItineraryValidationService)
    {
        _bus = bus;
        _devicesComplianceScanValidationService = devicesComplianceScanValidationService;
        _learningCertificationValidationService = learningCertificationValidationService;
        _learningCourseEnrollmentCalculationService = learningCourseEnrollmentCalculationService;
        _learningCourseEnrollmentValidationService = learningCourseEnrollmentValidationService;
        _learningCourseQueryService = learningCourseQueryService;
        _learningCourseValidationService = learningCourseValidationService;
        _learningLearningPathCalculationService = learningLearningPathCalculationService;
        _learningLearningPathQueryService = learningLearningPathQueryService;
        _logger = logger;
        _performanceFeedbackValidationService = performanceFeedbackValidationService;
        _performanceReviewService = performanceReviewService;
        _repository = repository;
        _settings = settings;
        _travelItineraryValidationService = travelItineraryValidationService;
        _sendBatchCreateCourses3 = _learningLearningPathCalculationService.CompareLearningPathOptionsAsync;
    }
    /// <summary>
    /// Takes a stream of courses and answers once at the end.
    /// </summary>
    public override async Task<ApiLearning.BatchCreateCoursesResponse> BatchCreateCourses(IAsyncStreamReader<ApiLearning.BatchCreateCoursesRequest> requests, ServerCallContext context)
    {
        var token = context.CancellationToken;
        var result = new ApiLearning.BatchCreateCoursesResponse();
        await foreach (var request in requests.ReadAllAsync(token))
        {
            RequestGuard.Tenant(request.Context);
            var requestContext = request.Context;
            using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
                requestContext.TenantId, requestContext.TraceId);
            _logger.LogInformation("Handling BatchCreateCourses for {Tenant}", requestContext.TenantId);
            if (request.Record is { } record)
            {
                var snapshot = CourseMapper.ToRecord(requestContext.TenantId, record);
                await _repository.SaveAsync(snapshot, token);
            }
            var input0 = new ApiLearning.FindCoursesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiLearning.FindCoursesRequest.Types.Filter
                {
                    CreatedAfter = request.ChunkCase switch
                    {
                        ApiLearning.BatchCreateCoursesRequest.ChunkOneofCase.Record => (request.Record?.Audit?.UpdatedAt ?? new Timestamp()),
                        _ => new Timestamp()
                    },
                    OwnerId = request.ChunkCase switch
                    {
                        ApiLearning.BatchCreateCoursesRequest.ChunkOneofCase.Record => (request.Record?.Owner?.UserId ?? ""),
                        _ => ""
                    },
                    Region = request.ChunkCase switch
                    {
                        ApiLearning.BatchCreateCoursesRequest.ChunkOneofCase.Record => (request.Record?.Address?.Region ?? ""),
                        _ => ""
                    },
                },
            };
            using var rpc0 = _learningCourseQueryService.FindCoursesAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            await _bus.PublishAsync(new TransportNotification<ApiLearning.FindCoursesRequest>("acme.learning.v1.CourseService/BatchCreateCourses/0", input0.Clone()), token);
            var input1 = new ApiTravel.LintItinerariesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Itinerary = new ApiTravel.Itinerary
                {
                    BillingAddress = new ApiCommon.PostalAddress
                    {
                        Lines = { request.ChunkCase switch
                        {
                            ApiLearning.BatchCreateCoursesRequest.ChunkOneofCase.Record => (request.Record?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()),
                            _ => new Google.Protobuf.Collections.RepeatedField<string>()
                        } },
                        PostalCode = request.ChunkCase switch
                        {
                            ApiLearning.BatchCreateCoursesRequest.ChunkOneofCase.Record => (request.Record?.Address?.PostalCode ?? ""),
                            _ => ""
                        },
                        Recipient = request.ChunkCase switch
                        {
                            ApiLearning.BatchCreateCoursesRequest.ChunkOneofCase.Record => (request.Record?.Address?.Recipient ?? ""),
                            _ => ""
                        },
                    },
                    Counterparty = new ApiTravel.TravelBookingRef
                    {
                        DisplayName = request.ChunkCase switch
                        {
                            ApiLearning.BatchCreateCoursesRequest.ChunkOneofCase.Record => (request.Record?.Owner?.DisplayName ?? ""),
                            _ => ""
                        },
                    },
                },
            };
            using var rpc1 = _travelItineraryValidationService.LintItinerariesAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            var input2 = new ApiPerformance.ExplainFeedbackDecisionRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiPerformance.ExplainFeedbackDecisionRequest.Types.Details
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = request.ChunkCase switch
                        {
                            ApiLearning.BatchCreateCoursesRequest.ChunkOneofCase.Record => (request.Record?.Owner?.Email ?? ""),
                            _ => ""
                        },
                        Name = request.ChunkCase switch
                        {
                            ApiLearning.BatchCreateCoursesRequest.ChunkOneofCase.Header => (request.Header?.SubmittedBy?.Name ?? new ApiCommon.PersonName()),
                            _ => new ApiCommon.PersonName()
                        },
                        Phone = request.ChunkCase switch
                        {
                            ApiLearning.BatchCreateCoursesRequest.ChunkOneofCase.Header => (request.Header?.SubmittedBy?.Phone ?? ""),
                            _ => ""
                        },
                    },
                },
                Feedback = new ApiPerformance.Feedback
                {
                    Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                    Subject = new ApiRecruiting.CandidateRef
                    {
                        Email = request.ChunkCase switch
                        {
                            ApiLearning.BatchCreateCoursesRequest.ChunkOneofCase.Header => (request.Header?.SubmittedBy?.Email ?? ""),
                            _ => ""
                        },
                    },
                },
            };
            using var rpc2 = _performanceFeedbackValidationService.ExplainFeedbackDecisionAsync(input2, cancellationToken: token);
            var reply2 = await rpc2.ResponseAsync;
            var input3 = new ApiLearning.CompareLearningPathOptionsRequest
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
                                CreatedBy = (reply1.Itinerary?.Audit?.CreatedBy ?? ""),
                            },
                            Location = new ApiCommon.PostalAddress
                            {
                                Region = (reply1.Itinerary?.BillingAddress?.Region ?? ""),
                            },
                        } },
                    } },
                },
            };
            using var rpc3 = _sendBatchCreateCourses3(input3, new CallOptions(cancellationToken: token));
            var reply3 = await rpc3.ResponseAsync;
            var response = new ApiLearning.BatchCreateCoursesResponse
            {
                Result = new ApiCommon.OperationResult
                {
                    Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    Message = (reply0.Result?.Message ?? ""),
                },
            };
            _logger.LogDebug("Local audit detail {Detail}", (request.Record?.Name ?? ""));
            await _repository.RecordAsync(requestContext.TenantId, "course",
                "audit", "BatchCreateCourses", (request.Record?.Name ?? ""), token);
            result = response;
        }
        return result;
    }
    /// <summary>
    /// Creates a course.
    /// </summary>
    public override async Task<ApiLearning.CloneCourseResponse> CloneCourse(ApiLearning.CloneCourseRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CloneCourse for {Tenant}", requestContext.TenantId);
        if (request.Course is { } record)
        {
            var snapshot = CourseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLearning.RecalculateCourseEnrollmentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CourseEnrollment = new ApiLearning.CourseEnrollment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Course?.Audit?.CreatedBy ?? ""),
                    UpdatedBy = (request.Course?.Audit?.UpdatedBy ?? ""),
                },
            },
            Details = new ApiLearning.RecalculateCourseEnrollmentsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Course ?? new ApiLearning.Course()).LocatorCase switch
                    {
                        ApiLearning.Course.LocatorOneofCase.Address => (request.Course?.Address?.PostalCode ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _learningCourseEnrollmentCalculationService.RecalculateCourseEnrollmentsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiPerformance.CreateReviewRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Review = new ApiPerformance.Review
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.Course?.Audit?.UpdatedBy ?? ""),
                },
                Card = new ApiPerformance.ReviewCardSettlement
                {
                    Network = (reply0.CourseEnrollment ?? new ApiLearning.CourseEnrollment()).PaymentCase switch
                    {
                        ApiLearning.CourseEnrollment.FundingOneofCase.Card => (reply0.CourseEnrollment?.Card?.Network ?? ""),
                        _ => ""
                    },
                },
                Lines = { new ApiPerformance.ReviewLine
                {
                    Quantity = (request.Course?.Components?.GetValueOrDefault("primary")?.Quantity ?? 0L),
                } },
            },
        };
        using var rpc1 = _performanceReviewService.CreateReviewAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLearning.ValidateCourseEnrollmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CourseEnrollment = new ApiLearning.CourseEnrollment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Review?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (reply0.CourseEnrollment?.Audit?.Etag ?? ""),
                },
            },
        };
        var command2 = new DispatchCommand<ApiLearning.ValidateCourseEnrollmentRequest>(input2,
            "primary", requestContext.TenantId);
        var routed2 = command2 with { Region = _settings.Value.Region };
        using var rpc2 = _learningCourseEnrollmentValidationService.ValidateCourseEnrollmentAsync(routed2.Payload, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiLearning.SuggestLearningPathsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLearning.SuggestLearningPathsRequest.Types.Filter
            {
                CreatedAfter = (reply2.CourseEnrollment?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (reply0.CourseEnrollment?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Course?.Owner?.UserId ?? ""),
                Region = (request.Course ?? new ApiLearning.Course()).LocatorCase switch
                {
                    ApiLearning.Course.LocatorOneofCase.Address => (request.Course?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc3 = _learningLearningPathQueryService.SuggestLearningPathsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiLearning.CloneCourseResponse
        {
            Course = new ApiLearning.Course
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply2.CourseEnrollment?.Audit?.Etag ?? ""),
                },
                CourseId = (request.Course?.CourseId ?? ""),
                Name = (request.Course?.Name ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.Review?.Counterparty?.DisplayName ?? ""),
                    Email = (reply2.CourseEnrollment?.Holder?.Email ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply3.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course",
            "audit", "CloneCourse", (request.RequestId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Creates a course.
    /// </summary>
    public override async Task<ApiLearning.CreateCourseResponse> CreateCourse(ApiLearning.CreateCourseRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CreateCourse for {Tenant}", requestContext.TenantId);
        if (request.Course is { } record)
        {
            var snapshot = CourseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLearning.PreviewCertificationRequest
        {
            Certification = new ApiLearning.Certification
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Course ?? new ApiLearning.Course()).LocatorCase switch
                    {
                        ApiLearning.Course.LocatorOneofCase.Address => (request.Course?.Address?.PostalCode ?? ""),
                        _ => ""
                    },
                    Recipient = (request.Course ?? new ApiLearning.Course()).LocatorCase switch
                    {
                        ApiLearning.Course.LocatorOneofCase.Address => (request.Course?.Address?.Recipient ?? ""),
                        _ => ""
                    },
                },
                Lines = { new ApiLearning.CertificationLine
                {
                    Description = (request.Course?.Components?.GetValueOrDefault("primary")?.Description ?? ""),
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _learningCertificationValidationService.PreviewCertificationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLearning.ExplainLearningPathCalculationRequest
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
                            CreatedBy = (request.Course?.Audit?.CreatedBy ?? ""),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            CountryCode = (reply0.Certification?.BillingAddress?.CountryCode ?? ""),
                            Locality = (request.Course ?? new ApiLearning.Course()).LocatorCase switch
                            {
                                ApiLearning.Course.LocatorOneofCase.Address => (request.Course?.Address?.Locality ?? ""),
                                _ => ""
                            },
                            PostalCode = (reply0.Certification?.BillingAddress?.PostalCode ?? ""),
                        },
                    } },
                } },
            },
        };
        using var rpc1 = _learningLearningPathCalculationService.ExplainLearningPathCalculationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiPerformance.ExplainFeedbackDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.ExplainFeedbackDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply1.LearningPath?.Manager?.Email ?? ""),
                },
            },
            Feedback = new ApiPerformance.Feedback
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (reply0.Certification?.Counterparty?.DisplayName ?? ""),
                    Email = (request.Course?.Owner?.Email ?? ""),
                },
            },
        };
        using var rpc2 = _performanceFeedbackValidationService.ExplainFeedbackDecisionAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiLearning.CreateCourseResponse
        {
            Course = new ApiLearning.Course
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.LearningPath?.Audit?.Etag ?? ""),
                },
                Capacity = (request.Course?.Capacity ?? 0L),
                CourseId = (request.Course?.CourseId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Certification?.Counterparty?.DisplayName ?? ""),
                    Email = (reply1.LearningPath?.Manager?.Email ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course",
            "audit", "CreateCourse", (request.RequestId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Deletes a course.
    /// </summary>
    public override async Task<ApiLearning.DeleteCourseResponse> DeleteCourse(ApiLearning.DeleteCourseRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CourseId, "course_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling DeleteCourse for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "course", request.CourseId,
            "DeleteCourse", "accepted", token);
        var input0 = new ApiLearning.CheckCourseEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Course = new ApiLearning.Course
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Etag ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _learningCourseValidationService.CheckCourseEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.NormalizeItineraryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Itinerary = new ApiTravel.Itinerary
            {
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
                },
                Counterparty = new ApiTravel.TravelBookingRef
                {
                    DisplayName = (reply0.Course?.Owner?.DisplayName ?? ""),
                },
                Lines = { new ApiTravel.ItineraryLine
                {
                    Quantity = (reply0.Course?.Components?.GetValueOrDefault("primary")?.Quantity ?? 0L),
                } },
            },
        };
        using var rpc1 = _travelItineraryValidationService.NormalizeItineraryAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLearning.DeleteCourseResponse
        {
            EffectiveAt = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course",
            "audit", "DeleteCourse", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a course.
    /// </summary>
    public override async Task<ApiLearning.GetCourseResponse> GetCourse(ApiLearning.GetCourseRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CourseId, "course_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetCourse for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "course", request.CourseId,
            "GetCourse", "accepted", token);
        var response = new ApiLearning.GetCourseResponse
        {
            Course = new ApiLearning.Course
            {
                CourseId = (request.CourseId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Changes a course.
    /// </summary>
    public override async Task<ApiLearning.UpdateCourseResponse> UpdateCourse(ApiLearning.UpdateCourseRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling UpdateCourse for {Tenant}", requestContext.TenantId);
        if (request.Course is { } record)
        {
            var snapshot = CourseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.NormalizeComplianceScanRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Measurements = { new ApiDevices.ComplianceScanMeasurement
                {
                    Unit = (request.Course?.Dimensions?.Unit ?? ""),
                } },
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (request.Course?.Owner?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.NormalizeComplianceScanRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Course?.Owner?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _devicesComplianceScanValidationService.NormalizeComplianceScanAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLearning.UpdateCourseResponse
        {
            Course = new ApiLearning.Course
            {
                Capacity = (request.Course?.Capacity ?? 0L),
                Status = (request.Course?.Status ?? (ApiLearning.CourseStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Course?.Owner?.UserId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course",
            "audit", "UpdateCourse", (request.Course?.Owner?.UserId ?? ""), token);
        return response;
    }
}
