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
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRatelimiting = Acme.Ratelimiting.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;
using ApiTravel = Acme.Travel.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Learning;

/// <summary>
/// Checks learning paths against the rules before anything acts on them.
/// </summary>
public sealed class LearningPathValidationEndpoint : ApiLearning.LearningPathValidationService.LearningPathValidationServiceBase
{
    private readonly ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient _authorizationAccessPolicyValidationService;
    private readonly ApiLearning.CertificationCalculationService.CertificationCalculationServiceClient _learningCertificationCalculationService;
    private readonly ApiLearning.CourseValidationService.CourseValidationServiceClient _learningCourseValidationService;
    private readonly ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient _learningLearningPathCalculationService;
    private readonly ApiLicensing.EntitlementQueryService.EntitlementQueryServiceClient _licensingEntitlementQueryService;
    private readonly ApiLicensing.LicenseAssignmentCalculationService.LicenseAssignmentCalculationServiceClient _licensingLicenseAssignmentCalculationService;
    private readonly ILogger<LearningPathValidationEndpoint> _logger;
    private readonly ApiRatelimiting.RateLimitQueryService.RateLimitQueryServiceClient _ratelimitingRateLimitQueryService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiSearch.SearchIndexService.SearchIndexServiceClient _searchSearchIndexService;
    private readonly Func<ApiAuthorization.NormalizeAccessPolicyRequest, CallOptions, AsyncUnaryCall<ApiAuthorization.NormalizeAccessPolicyResponse>> _sendScoreLearningPath0;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTravel.TravelBookingQueryService.TravelBookingQueryServiceClient _travelTravelBookingQueryService;
    private readonly ApiVenues.VenueQueryService.VenueQueryServiceClient _venuesVenueQueryService;
    private readonly ApiVenues.VenueService.VenueServiceClient _venuesVenueService;

    public LearningPathValidationEndpoint(
        ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient authorizationAccessPolicyValidationService,
        ApiLearning.CertificationCalculationService.CertificationCalculationServiceClient learningCertificationCalculationService,
        ApiLearning.CourseValidationService.CourseValidationServiceClient learningCourseValidationService,
        ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient learningLearningPathCalculationService,
        ApiLicensing.EntitlementQueryService.EntitlementQueryServiceClient licensingEntitlementQueryService,
        ApiLicensing.LicenseAssignmentCalculationService.LicenseAssignmentCalculationServiceClient licensingLicenseAssignmentCalculationService,
        ILogger<LearningPathValidationEndpoint> logger,
        ApiRatelimiting.RateLimitQueryService.RateLimitQueryServiceClient ratelimitingRateLimitQueryService,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiSearch.SearchIndexService.SearchIndexServiceClient searchSearchIndexService,
        IOptions<MeshOptions> settings,
        ApiTravel.TravelBookingQueryService.TravelBookingQueryServiceClient travelTravelBookingQueryService,
        ApiVenues.VenueQueryService.VenueQueryServiceClient venuesVenueQueryService,
        ApiVenues.VenueService.VenueServiceClient venuesVenueService)
    {
        _authorizationAccessPolicyValidationService = authorizationAccessPolicyValidationService;
        _learningCertificationCalculationService = learningCertificationCalculationService;
        _learningCourseValidationService = learningCourseValidationService;
        _learningLearningPathCalculationService = learningLearningPathCalculationService;
        _licensingEntitlementQueryService = licensingEntitlementQueryService;
        _licensingLicenseAssignmentCalculationService = licensingLicenseAssignmentCalculationService;
        _logger = logger;
        _ratelimitingRateLimitQueryService = ratelimitingRateLimitQueryService;
        _registry = registry;
        _repository = repository;
        _searchSearchIndexService = searchSearchIndexService;
        _settings = settings;
        _travelTravelBookingQueryService = travelTravelBookingQueryService;
        _venuesVenueQueryService = venuesVenueQueryService;
        _venuesVenueService = venuesVenueService;
        _sendScoreLearningPath0 = _authorizationAccessPolicyValidationService.NormalizeAccessPolicyAsync;
    }
    /// <summary>
    /// Checks a learning path.
    /// </summary>
    public override async Task<ApiLearning.CheckLearningPathEligibilityResponse> CheckLearningPathEligibility(ApiLearning.CheckLearningPathEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CheckLearningPathEligibility for {Tenant}", requestContext.TenantId);
        if (request.LearningPath is { } record)
        {
            var snapshot = LearningPathMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRatelimiting.BrowseRateLimitsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiRatelimiting.BrowseRateLimitsRequest.Types.Filter
            {
                CreatedAfter = (request.LearningPath?.Audit?.UpdatedAt ?? new Timestamp()),
                OwnerId = (request.LearningPath?.ParentId ?? ""),
                Region = (request.LearningPath?.Location?.Region ?? ""),
            },
        };
        using var rpc0 = _ratelimitingRateLimitQueryService.BrowseRateLimitsAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiLearning.RecalculateLearningPathsRequest
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
                            UpdatedAt = (request.LearningPath?.Audit?.UpdatedAt ?? new Timestamp()),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            Lines = { (request.LearningPath?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                            Locality = (request.LearningPath?.Location?.Locality ?? ""),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            Email = (request.Details?.Contact?.Email ?? ""),
                        },
                    } },
                } },
            },
        };
        using var rpc1 = _learningLearningPathCalculationService.RecalculateLearningPathsAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var input2 = new ApiTravel.FindTravelBookingsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiTravel.FindTravelBookingsRequest.Types.Filter
            {
                CreatedAfter = (request.LearningPath?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.LearningPath?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply1.Result?.CorrelationId ?? ""),
                Region = (request.LearningPath?.Location?.Region ?? ""),
            },
        };
        var packed2 = Any.Pack(input2);
        input2 = packed2.Unpack<ApiTravel.FindTravelBookingsRequest>();
        using var rpc2 = _travelTravelBookingQueryService.FindTravelBookingsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiLicensing.EstimateLicenseAssignmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.EstimateLicenseAssignmentRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (reply1.LearningPath?.Location?.Locality ?? ""),
                    PostalCode = (request.LearningPath?.Location?.PostalCode ?? ""),
                },
            },
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.LearningPath?.Audit?.Etag ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (reply1.LearningPath?.Location?.Recipient ?? ""),
                },
            },
        };
        var json3 = JsonFormatter.Default.Format(input3);
        input3 = JsonParser.Default.Parse<ApiLicensing.EstimateLicenseAssignmentRequest>(json3);
        using var rpc3 = _licensingLicenseAssignmentCalculationService.EstimateLicenseAssignmentAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiLearning.CheckLearningPathEligibilityResponse
        {
            LearningPath = new ApiLearning.LearningPath
            {
                Children = { new ApiLearning.LearningPath
                {
                    Children = { new ApiLearning.LearningPath
                    {
                        Location = (reply1.LearningPath?.Location ?? new ApiCommon.PostalAddress()),
                    } },
                    Location = new ApiCommon.PostalAddress
                    {
                        Locality = (reply3.LicenseAssignment?.BillingAddress?.Locality ?? ""),
                        Recipient = (reply1.LearningPath?.Location?.Recipient ?? ""),
                    },
                } },
                Depth = (request.LearningPath?.Depth ?? 0),
                LearningPathId = (request.LearningPathId ?? ""),
            },
            Outcome = new ApiLearning.CheckLearningPathEligibilityResponse.Types.Outcome
            {
                Status = (reply2.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "learning_path",
            "audit", "CheckLearningPathEligibility", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a learning path.
    /// </summary>
    public override async Task<ApiLearning.GetLearningPathRulesResponse> GetLearningPathRules(ApiLearning.GetLearningPathRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.LearningPathId, "learning_path_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetLearningPathRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "learning_path", request.LearningPathId,
            "GetLearningPathRules", "accepted", token);
        var response = new ApiLearning.GetLearningPathRulesResponse
        {
            LearningPath = new ApiLearning.LearningPath
            {
                LearningPathId = (request.LearningPathId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Normalizes a learning path.
    /// </summary>
    public override async Task<ApiLearning.NormalizeLearningPathResponse> NormalizeLearningPath(ApiLearning.NormalizeLearningPathRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling NormalizeLearningPath for {Tenant}", requestContext.TenantId);
        if (request.LearningPath is { } record)
        {
            var snapshot = LearningPathMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.FindEntitlementsRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Filter = new ApiLicensing.FindEntitlementsRequest.Types.Filter
        {
            CreatedAfter = (request.LearningPath?.Audit?.CreatedAt ?? new Timestamp()),
            Labels = { (request.LearningPath?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Region = (request.LearningPath?.Location?.Region ?? ""),
        };
        using var rpc0 = _licensingEntitlementQueryService.FindEntitlementsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLearning.LintCoursesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Course = new ApiLearning.Course
            {
                Address = new ApiCommon.PostalAddress
                {
                    Recipient = (request.LearningPath?.Location?.Recipient ?? ""),
                },
            },
        };
        using var rpc1 = _learningCourseValidationService.LintCoursesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiSearch.SearchSearchIndexesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiSearch.SearchSearchIndexesRequest.Types.Filter
            {
                CreatedAfter = (reply1.Course?.Audit?.UpdatedAt ?? new Timestamp()),
                Region = (reply1.Course ?? new ApiLearning.Course()).LocatorCase switch
                {
                    ApiLearning.Course.LocatorOneofCase.Address => (reply1.Course?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc2 = _searchSearchIndexService.SearchSearchIndexesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiLearning.NormalizeLearningPathResponse
        {
            LearningPath = new ApiLearning.LearningPath
            {
                LearningPathId = (request.LearningPathId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    Lines = { (reply1.Course?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                ParentId = (request.LearningPath?.ParentId ?? ""),
            },
            Outcome = new ApiLearning.NormalizeLearningPathResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply2.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "learning_path",
            "audit", "NormalizeLearningPath", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews a learning path.
    /// </summary>
    public override async Task<ApiLearning.PreviewLearningPathResponse> PreviewLearningPath(ApiLearning.PreviewLearningPathRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PreviewLearningPath for {Tenant}", requestContext.TenantId);
        if (request.LearningPath is { } record)
        {
            var snapshot = LearningPathMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiVenues.BrowseVenuesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiVenues.BrowseVenuesRequest.Types.Filter
            {
                Region = (request.LearningPath?.Location?.Region ?? ""),
            },
        };
        using var rpc0 = _venuesVenueQueryService.BrowseVenuesAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiLearning.SimulateLearningPathRequest
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
                            CreatedBy = (request.LearningPath?.Audit?.CreatedBy ?? ""),
                            Etag = (request.LearningPath?.Audit?.Etag ?? ""),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            UserId = (request.LearningPath?.Manager?.UserId ?? ""),
                        },
                    } },
                } },
            },
        };
        using var rpc1 = _learningLearningPathCalculationService.SimulateLearningPathAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiLearning.PreviewLearningPathResponse
        {
            LearningPath = new ApiLearning.LearningPath
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.LearningPath?.Audit?.Etag ?? ""),
                },
                LearningPathId = (request.LearningPathId ?? ""),
                Name = (request.LearningPath?.Name ?? ""),
            },
            Outcome = new ApiLearning.PreviewLearningPathResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "learning_path",
            "audit", "PreviewLearningPath", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores a learning path.
    /// </summary>
    public override async Task<ApiLearning.ScoreLearningPathResponse> ScoreLearningPath(ApiLearning.ScoreLearningPathRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ScoreLearningPath for {Tenant}", requestContext.TenantId);
        if (request.LearningPath is { } record)
        {
            var snapshot = LearningPathMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAuthorization.NormalizeAccessPolicyRequest
        {
            AccessPolicy = new ApiAuthorization.AccessPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.LearningPath?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedBy = (request.LearningPath?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.LearningPath?.Manager?.UserId ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiAuthorization.NormalizeAccessPolicyRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.LearningPath?.Manager?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _sendScoreLearningPath0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLearning.RecalculateLearningPathsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LearningPath = new ApiLearning.LearningPath
            {
                Children = { new ApiLearning.LearningPath
                {
                    Children = { new ApiLearning.LearningPath
                    {
                        Manager = new ApiIdentity.UserRef
                        {
                            Email = (reply0.AccessPolicy?.Owner?.Email ?? ""),
                        },
                    } },
                } },
            },
        };
        var named1 = _registry.Get<ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient>("acme.learning.v1.LearningPathCalculationService");
        using var rpc1 = named1.RecalculateLearningPathsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLearning.QuoteCertificationRequest
        {
            Certification = new ApiLearning.Certification
            {
                Counterparty = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (request.LearningPath?.Manager?.DisplayName ?? ""),
                },
                Lines = { new ApiLearning.CertificationLine
                {
                    Tax = new ApiLearning.CertificationLineTax
                    {
                        Amount = (reply0.AccessPolicy ?? new ApiAuthorization.AccessPolicy()).ActionCase switch
                        {
                            ApiAuthorization.AccessPolicy.ActionOneofCase.Discount => (reply0.AccessPolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc2 = _learningCertificationCalculationService.QuoteCertificationAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiLearning.ScoreLearningPathResponse
        {
            LearningPath = new ApiLearning.LearningPath
            {
                Children = { new ApiLearning.LearningPath
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (reply2.Certification?.Audit?.CreatedAt ?? new Timestamp()),
                    },
                    Children = { new ApiLearning.LearningPath
                    {
                        Location = new ApiCommon.PostalAddress
                        {
                            Lines = { (reply1.LearningPath?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                        },
                    } },
                } },
                LearningPathId = (request.LearningPathId ?? ""),
                Status = (request.LearningPath?.Status ?? (ApiLearning.LearningPathStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "learning_path",
            "audit", "ScoreLearningPath", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates a learning path.
    /// </summary>
    public override async Task<ApiLearning.ValidateLearningPathResponse> ValidateLearningPath(ApiLearning.ValidateLearningPathRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ValidateLearningPath for {Tenant}", requestContext.TenantId);
        if (request.LearningPath is { } record)
        {
            var snapshot = LearningPathMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiVenues.UpdateVenueRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Venue = new ApiVenues.Venue
            {
                Address = new ApiCommon.PostalAddress
                {
                    Locality = (request.LearningPath?.Location?.Locality ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiVenues.UpdateVenueRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _venuesVenueService.UpdateVenueAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLearning.EstimateLearningPathRequest
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
                            UpdatedAt = (request.LearningPath?.Audit?.UpdatedAt ?? new Timestamp()),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            Locality = (reply0.Venue ?? new ApiVenues.Venue()).LocatorCase switch
                            {
                                ApiVenues.Venue.LocatorOneofCase.Address => (reply0.Venue?.Address?.Locality ?? ""),
                                _ => ""
                            },
                            Recipient = (request.LearningPath?.Location?.Recipient ?? ""),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            Email = (reply0.Venue?.Owner?.Email ?? ""),
                        },
                    } },
                } },
            },
        };
        using var rpc1 = _learningLearningPathCalculationService.EstimateLearningPathAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLearning.ValidateLearningPathResponse
        {
            LearningPath = new ApiLearning.LearningPath
            {
                Children = { new ApiLearning.LearningPath
                {
                    Children = { new ApiLearning.LearningPath
                    {
                        Status = (reply1.LearningPath?.Status ?? (ApiLearning.LearningPathStatus)0),
                    } },
                    Manager = (reply1.LearningPath?.Manager ?? new ApiIdentity.UserRef()),
                } },
                LearningPathId = (request.LearningPathId ?? ""),
                ParentId = (request.LearningPath?.ParentId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "learning_path",
            "audit", "ValidateLearningPath", (request.Reason ?? ""), token);
        return response;
    }
}
