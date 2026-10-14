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
using ApiFuel = Acme.Fuel.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSurveys = Acme.Surveys.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Learning;

public sealed partial class CourseEnrollmentValidationEndpoint
{
    /// <summary>
    /// Checks a course enrollment.
    /// </summary>
    public override async Task<ApiLearning.CheckCourseEnrollmentEligibilityResponse> CheckCourseEnrollmentEligibility(ApiLearning.CheckCourseEnrollmentEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CheckCourseEnrollmentEligibility for {Tenant}", requestContext.TenantId);
        if (request.CourseEnrollment is { } record)
        {
            var snapshot = CourseEnrollmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.NormalizeKnowledgeArticleRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.NormalizeKnowledgeArticleRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.CourseEnrollment?.Audit?.Etag ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiHelpdesk.NormalizeKnowledgeArticleRequest, ApiHelpdesk.NormalizeKnowledgeArticleResponse>(
            MethodType.Unary, "acme.helpdesk.v1" + ".KnowledgeArticleValidationService", "NormalizeKnowledgeArticle",
            Wire.Marshaller<ApiHelpdesk.NormalizeKnowledgeArticleRequest>(), Wire.Marshaller<ApiHelpdesk.NormalizeKnowledgeArticleResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLearning.CheckCourseEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Course = new ApiLearning.Course
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
        };
        var command1 = new DispatchCommand<ApiLearning.CheckCourseEligibilityRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _learningCourseValidationService.CheckCourseEligibilityAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var reply2 = new ApiWorkplaces.CloneBuildingResponse();
        foreach (var item in (request.CourseEnrollment?.Limits ?? new Google.Protobuf.Collections.RepeatedField<ApiLearning.CourseEnrollmentLimit>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input2 = new ApiWorkplaces.CloneBuildingRequest
            {
                Building = new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Children = { new ApiWorkplaces.Building
                        {
                            Name = (item.Name ?? ""),
                        } },
                    } },
                },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
            };
            using var rpc2 = _workplacesBuildingService.CloneBuildingAsync(input2, cancellationToken: token);
            reply2 = await rpc2.ResponseAsync;
        }
        var input3 = new ApiSurveys.ResolveSurveysRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        input3 = PropertyMap.Copy(input3);
        using var rpc3 = _surveysSurveyQueryService.ResolveSurveysAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiLearning.CheckCourseEnrollmentEligibilityResponse
        {
            CourseEnrollment = new ApiLearning.CourseEnrollment
            {
                CourseEnrollmentId = (request.CourseEnrollmentId ?? ""),
                CurrencyCode = (request.CourseEnrollment?.CurrencyCode ?? ""),
                Holder = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (reply0.KnowledgeArticle?.Counterparty?.DisplayName ?? ""),
                },
            },
            Outcome = new ApiLearning.CheckCourseEnrollmentEligibilityResponse.Types.Outcome
            {
                Message = (reply1.Outcome?.Message ?? ""),
                NextSteps = { (reply0.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply3.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply3.Result?.Message ?? ""),
                Status = (reply1.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course_enrollment",
            "audit", "CheckCourseEnrollmentEligibility", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a course enrollment.
    /// </summary>
    public override async Task<ApiLearning.ExplainCourseEnrollmentDecisionResponse> ExplainCourseEnrollmentDecision(ApiLearning.ExplainCourseEnrollmentDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainCourseEnrollmentDecision for {Tenant}", requestContext.TenantId);
        if (request.CourseEnrollment is { } record)
        {
            var snapshot = CourseEnrollmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.ExplainCourseEnrollmentDecisionToValidateLearningPath0();
        using var rpc0 = _learningLearningPathValidationService.ValidateLearningPathAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLearning.ExplainCourseEnrollmentDecisionResponse
        {
            CourseEnrollment = new ApiLearning.CourseEnrollment
            {
                CourseEnrollmentId = (request.CourseEnrollmentId ?? ""),
                Status = (request.CourseEnrollment?.Status ?? (ApiLearning.CourseEnrollmentStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course_enrollment",
            "audit", "ExplainCourseEnrollmentDecision", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a course enrollment.
    /// </summary>
    public override async Task<ApiLearning.GetCourseEnrollmentRulesResponse> GetCourseEnrollmentRules(ApiLearning.GetCourseEnrollmentRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CourseEnrollmentId, "course_enrollment_id");
        _logger.LogInformation("Handling GetCourseEnrollmentRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "course_enrollment", request.CourseEnrollmentId,
            "GetCourseEnrollmentRules", "accepted", token);
        var input0 = new ApiTravel.FindTravelBookingsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiTravel.FindTravelBookingsRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                OwnerId = (request.CourseEnrollmentId ?? ""),
            },
        };
        var reply0 = await SendGetCourseEnrollmentRules0Async(input0, token);
        var response = new ApiLearning.GetCourseEnrollmentRulesResponse
        {
            CourseEnrollment = new ApiLearning.CourseEnrollment
            {
                CourseEnrollmentId = (request.CourseEnrollmentId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Lints a course enrollment.
    /// </summary>
    public override async Task<ApiLearning.LintCourseEnrollmentsResponse> LintCourseEnrollments(ApiLearning.LintCourseEnrollmentsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling LintCourseEnrollments for {Tenant}", requestContext.TenantId);
        if (request.CourseEnrollment is { } record)
        {
            var snapshot = CourseEnrollmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLearning.RecalculateLearningPathsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LearningPath = new ApiLearning.LearningPath
            {
                Children = { new ApiLearning.LearningPath
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedBy = (request.CourseEnrollment?.Audit?.CreatedBy ?? ""),
                    },
                    Children = { new ApiLearning.LearningPath
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            CreatedAt = (request.CourseEnrollment?.Audit?.CreatedAt ?? new Timestamp()),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            Email = (request.Details?.Contact?.Email ?? ""),
                        },
                    } },
                    Manager = new ApiIdentity.UserRef
                    {
                        Email = (request.CourseEnrollment?.Contact?.Email ?? ""),
                    },
                } },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _learningLearningPathCalculationService.RecalculateLearningPathsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.ExplainTravelBookingCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.ExplainTravelBookingCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                Bank = new ApiTravel.TravelBookingBankSettlement
                {
                    Bic = (request.CourseEnrollment ?? new ApiLearning.CourseEnrollment()).PaymentCase switch
                    {
                        ApiLearning.CourseEnrollment.FundingOneofCase.Bank => (request.CourseEnrollment?.Bank?.Bic ?? ""),
                        _ => ""
                    },
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.LearningPath?.Location?.CountryCode ?? ""),
                    PostalCode = (reply0.LearningPath?.Location?.PostalCode ?? ""),
                },
            },
        };
        using var rpc1 = _travelTravelBookingCalculationService.ExplainTravelBookingCalculationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiSurveys.BrowseSurveysRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiSurveys.BrowseSurveysRequest.Types.Filter
            {
                CreatedAfter = (request.CourseEnrollment?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (request.CourseEnrollment?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.LearningPath?.ParentId ?? ""),
                Region = (reply0.LearningPath?.Location?.Region ?? ""),
            },
        };
        var json2 = JsonFormatter.Default.Format(input2);
        input2 = JsonParser.Default.Parse<ApiSurveys.BrowseSurveysRequest>(json2);
        using var rpc2 = _surveysSurveyQueryService.BrowseSurveysAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiLearning.LintCourseEnrollmentsResponse
        {
            CourseEnrollment = new ApiLearning.CourseEnrollment
            {
                Contact = (request.CourseEnrollment?.Contact ?? new ApiCommon.ContactInfo()),
                CourseEnrollmentId = (request.CourseEnrollmentId ?? ""),
                Holder = new ApiRecruiting.CandidateRef
                {
                    Email = (reply0.LearningPath?.Manager?.Email ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply1.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course_enrollment",
            "audit", "LintCourseEnrollments", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Normalizes a course enrollment.
    /// </summary>
    public override async Task<ApiLearning.NormalizeCourseEnrollmentResponse> NormalizeCourseEnrollment(ApiLearning.NormalizeCourseEnrollmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling NormalizeCourseEnrollment for {Tenant}", requestContext.TenantId);
        if (request.CourseEnrollment is { } record)
        {
            var snapshot = CourseEnrollmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLearning.CompareLearningPathOptionsRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.LearningPath = new ApiLearning.LearningPath
        {
            Children = { new ApiLearning.LearningPath
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.CourseEnrollment?.Audit?.UpdatedBy ?? ""),
                },
                Children = { new ApiLearning.LearningPath
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (request.CourseEnrollment?.Audit?.CreatedAt ?? new Timestamp()),
                        Etag = (request.CourseEnrollment?.Audit?.Etag ?? ""),
                    },
                    Manager = new ApiIdentity.UserRef
                    {
                        Email = (request.CourseEnrollment?.Holder?.Email ?? ""),
                    },
                } },
            } },
        };
        using var rpc0 = _learningLearningPathCalculationService.CompareLearningPathOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var reply1 = new ApiLearning.CalculateCourseEnrollmentResponse();
        foreach (var item in (request.CourseEnrollment?.Limits ?? new Google.Protobuf.Collections.RepeatedField<ApiLearning.CourseEnrollmentLimit>()))
        {
            var input1 = new ApiLearning.CalculateCourseEnrollmentRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                CourseEnrollment = new ApiLearning.CourseEnrollment
                {
                    Limits = { new ApiLearning.CourseEnrollmentLimit
                    {
                        Amount = (item.Amount ?? new ApiCommon.Money()),
                        Name = (item.Name ?? ""),
                        Period = (item.Period ?? new Duration()),
                    } },
                },
            };
            using var rpc1 = _learningCourseEnrollmentCalculationService.CalculateCourseEnrollmentAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var response = new ApiLearning.NormalizeCourseEnrollmentResponse
        {
            CourseEnrollment = new ApiLearning.CourseEnrollment
            {
                Contact = (request.Details?.Contact ?? new ApiCommon.ContactInfo()),
                CourseEnrollmentId = (request.CourseEnrollmentId ?? ""),
                CurrencyCode = (reply0.Breakdown?.CurrencyCode ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course_enrollment",
            "audit", "NormalizeCourseEnrollment", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews a course enrollment.
    /// </summary>
    public override async Task<ApiLearning.PreviewCourseEnrollmentResponse> PreviewCourseEnrollment(ApiLearning.PreviewCourseEnrollmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PreviewCourseEnrollment for {Tenant}", requestContext.TenantId);
        if (request.CourseEnrollment is { } record)
        {
            var snapshot = CourseEnrollmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLearning.RecalculateLearningPathsRequest
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
                            CreatedAt = (request.CourseEnrollment?.Audit?.CreatedAt ?? new Timestamp()),
                            Etag = (request.CourseEnrollment?.Audit?.Etag ?? ""),
                        },
                    } },
                    Manager = new ApiIdentity.UserRef
                    {
                        Email = (request.CourseEnrollment?.Contact?.Email ?? ""),
                    },
                } },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _learningLearningPathCalculationService.RecalculateLearningPathsAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiTravel.BrowseTripsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiTravel.BrowseTripsRequest.Types.Filter
            {
                Labels = { (request.CourseEnrollment?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Region = (reply0.LearningPath?.Location?.Region ?? ""),
            },
        };
        using var rpc1 = _travelTripQueryService.BrowseTripsAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        var input2 = new ApiLearning.NormalizeCertificationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLearning.NormalizeCertificationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        using var rpc2 = _learningCertificationValidationService.NormalizeCertificationAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        await Task.WhenAll(pending1, pending2);
        var reply1 = await pending1;
        var reply2 = await pending2;
        var input3 = new ApiLearning.SimulateLearningPathRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LearningPath = new ApiLearning.LearningPath
            {
                Children = { new ApiLearning.LearningPath
                {
                    Children = { new ApiLearning.LearningPath
                    {
                        Location = new ApiCommon.PostalAddress
                        {
                            CountryCode = (reply2.Certification?.BillingAddress?.CountryCode ?? ""),
                            Region = (reply2.Certification?.BillingAddress?.Region ?? ""),
                        },
                    } },
                } },
            },
        };
        using var rpc3 = _sendPreviewCourseEnrollment3(input3, new CallOptions(cancellationToken: token));
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiLearning.PreviewCourseEnrollmentResponse
        {
            CourseEnrollment = new ApiLearning.CourseEnrollment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply2.Certification?.Audit?.CreatedAt ?? new Timestamp()),
                },
                CourseEnrollmentId = (request.CourseEnrollmentId ?? ""),
                CurrencyCode = (request.CourseEnrollment?.CurrencyCode ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course_enrollment",
            "audit", "PreviewCourseEnrollment", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores a course enrollment.
    /// </summary>
    public override async Task<ApiLearning.ScoreCourseEnrollmentResponse> ScoreCourseEnrollment(ApiLearning.ScoreCourseEnrollmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScoreCourseEnrollment for {Tenant}", requestContext.TenantId);
        if (request.CourseEnrollment is { } record)
        {
            var snapshot = CourseEnrollmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLearning.CheckCertificationEligibilityRequest
        {
            Certification = new ApiLearning.Certification
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.CourseEnrollment?.Audit?.CreatedBy ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var named0 = _registry.Get<ApiLearning.CertificationValidationService.CertificationValidationServiceClient>("acme.learning.v1.CertificationValidationService");
        using var rpc0 = named0.CheckCertificationEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLearning.ValidateLearningPathRequest
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
                            UpdatedBy = (reply0.Certification?.Audit?.UpdatedBy ?? ""),
                        },
                    } },
                } },
            },
        };
        var descriptor1 = new Method<ApiLearning.ValidateLearningPathRequest, ApiLearning.ValidateLearningPathResponse>(
            MethodType.Unary, "acme.learning.v1" + ".LearningPathValidationService", "ValidateLearningPath",
            Wire.Marshaller<ApiLearning.ValidateLearningPathRequest>(), Wire.Marshaller<ApiLearning.ValidateLearningPathResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLearning.ScoreCourseEnrollmentResponse
        {
            Breakdown = new ApiLearning.ScoreCourseEnrollmentResponse.Types.Breakdown
            {
                CurrencyCode = (request.CourseEnrollment?.CurrencyCode ?? ""),
            },
            CourseEnrollment = new ApiLearning.CourseEnrollment
            {
                CourseEnrollmentId = (request.CourseEnrollmentId ?? ""),
                Holder = new ApiRecruiting.CandidateRef
                {
                    CandidateId = (reply0.Certification?.Counterparty?.CandidateId ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
                Status = (reply1.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course_enrollment",
            "audit", "ScoreCourseEnrollment", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates a course enrollment.
    /// </summary>
    public override async Task<ApiLearning.ValidateCourseEnrollmentResponse> ValidateCourseEnrollment(ApiLearning.ValidateCourseEnrollmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ValidateCourseEnrollment for {Tenant}", requestContext.TenantId);
        if (request.CourseEnrollment is { } record)
        {
            var snapshot = CourseEnrollmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.SummarizeTripsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        var command0 = new DispatchCommand<ApiTravel.SummarizeTripsRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _travelTripQueryService.SummarizeTripsAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiLearning.ValidateCourseEnrollmentRequest, ApiFuel.CreateFuelCardRequest>(value => new ApiFuel.CreateFuelCardRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            FuelCard = new ApiFuel.FuelCard
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (value.CourseEnrollment?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (value.CourseEnrollment?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (value.CourseEnrollment?.Holder?.Email ?? ""),
                },
            },
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _fuelFuelCardService.CreateFuelCardAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLearning.QuoteCourseEnrollmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CourseEnrollment = new ApiLearning.CourseEnrollment
            {
                Card = new ApiLearning.CourseEnrollmentCardFunding
                {
                    Last4 = (request.CourseEnrollment ?? new ApiLearning.CourseEnrollment()).PaymentCase switch
                    {
                        ApiLearning.CourseEnrollment.FundingOneofCase.Card => (request.CourseEnrollment?.Card?.Last4 ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc2 = _learningCourseEnrollmentCalculationService.QuoteCourseEnrollmentAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var reply3 = new ApiLearning.EstimateCourseEnrollmentResponse();
        foreach (var item in (request.CourseEnrollment?.Limits ?? new Google.Protobuf.Collections.RepeatedField<ApiLearning.CourseEnrollmentLimit>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input3 = new ApiLearning.EstimateCourseEnrollmentRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                CourseEnrollment = new ApiLearning.CourseEnrollment
                {
                    Limits = { new ApiLearning.CourseEnrollmentLimit
                    {
                        Amount = (item.Amount ?? new ApiCommon.Money()),
                        Name = (item.Name ?? ""),
                        Period = (item.Period ?? new Duration()),
                    } },
                },
            };
            using var rpc3 = _learningCourseEnrollmentCalculationService.EstimateCourseEnrollmentAsync(input3, cancellationToken: token);
            reply3 = await rpc3.ResponseAsync;
        }
        var response = new ApiLearning.ValidateCourseEnrollmentResponse
        {
            CourseEnrollment = new ApiLearning.CourseEnrollment
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply2.CourseEnrollment?.Holder?.Email ?? ""),
                },
                CourseEnrollmentId = (request.CourseEnrollmentId ?? ""),
                Holder = new ApiRecruiting.CandidateRef
                {
                    Email = (reply1.FuelCard?.Contact?.Email ?? ""),
                },
                Labels = { (request.CourseEnrollment?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Outcome = new ApiLearning.ValidateCourseEnrollmentResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Locale ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course_enrollment",
            "audit", "ValidateCourseEnrollment", (request.Details?.Locale ?? ""), token);
        return response;
    }
    private async Task<ApiTravel.FindTravelBookingsResponse> SendGetCourseEnrollmentRules0Async(ApiTravel.FindTravelBookingsRequest request, CancellationToken token)
    {
        return await _courseEnrollmentValidationEndpointSendGetCourseEnrollmentRules0Port.SendAsync(request, token);
    }
}
