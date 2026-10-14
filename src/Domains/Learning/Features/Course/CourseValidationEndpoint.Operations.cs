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
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiPerformance = Acme.Performance.V1;

namespace Acme.Workforce.Learning;

public sealed partial class CourseValidationEndpoint
{
    /// <summary>
    /// Checks a course.
    /// </summary>
    public override async Task<ApiLearning.CheckCourseEligibilityResponse> CheckCourseEligibility(ApiLearning.CheckCourseEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CheckCourseEligibility for {Tenant}", requestContext.TenantId);
        if (request.Course is { } record)
        {
            var snapshot = CourseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiFeatureflags.NormalizeFeatureFlagRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.FeatureFlag = new ApiFeatureflags.FeatureFlag
        {
            Audit = new ApiCommon.AuditInfo
            {
                CreatedBy = (request.Course?.Audit?.CreatedBy ?? ""),
                UpdatedBy = (request.Course?.Audit?.UpdatedBy ?? ""),
            },
        };
        using var rpc0 = _featureflagsFeatureFlagValidationService.NormalizeFeatureFlagAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
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
                            CreatedAt = (reply0.FeatureFlag?.Audit?.CreatedAt ?? new Timestamp()),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            PostalCode = (request.Course ?? new ApiLearning.Course()).LocatorCase switch
                            {
                                ApiLearning.Course.LocatorOneofCase.Address => (request.Course?.Address?.PostalCode ?? ""),
                                _ => ""
                            },
                        },
                    } },
                } },
            },
        };
        using var rpc1 = _learningLearningPathCalculationService.SimulateLearningPathAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLearning.CheckCourseEligibilityResponse
        {
            Course = new ApiLearning.Course
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.LearningPath?.Audit?.Etag ?? ""),
                },
                Code = (request.Course?.Code ?? ""),
                CourseId = (request.CourseId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (reply0.FeatureFlag?.Owner?.UserId ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course",
            "audit", "CheckCourseEligibility", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a course.
    /// </summary>
    public override async Task<ApiLearning.ExplainCourseDecisionResponse> ExplainCourseDecision(ApiLearning.ExplainCourseDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainCourseDecision for {Tenant}", requestContext.TenantId);
        if (request.Course is { } record)
        {
            var snapshot = CourseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLearning.SimulateLearningPathRequest
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
                            CountryCode = (request.Course ?? new ApiLearning.Course()).LocatorCase switch
                            {
                                ApiLearning.Course.LocatorOneofCase.Address => (request.Course?.Address?.CountryCode ?? ""),
                                _ => ""
                            },
                            Locality = (request.Course ?? new ApiLearning.Course()).LocatorCase switch
                            {
                                ApiLearning.Course.LocatorOneofCase.Address => (request.Course?.Address?.Locality ?? ""),
                                _ => ""
                            },
                        },
                    } },
                } },
            },
        };
        using var rpc0 = _learningLearningPathCalculationService.SimulateLearningPathAsync(input0, cancellationToken: token);
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
                            CreatedAt = (reply0.LearningPath?.Audit?.CreatedAt ?? new Timestamp()),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            PostalCode = (request.Course ?? new ApiLearning.Course()).LocatorCase switch
                            {
                                ApiLearning.Course.LocatorOneofCase.Address => (request.Course?.Address?.PostalCode ?? ""),
                                _ => ""
                            },
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (request.Course?.Owner?.DisplayName ?? ""),
                        },
                    } },
                } },
            },
        };
        using var rpc1 = _learningLearningPathCalculationService.EstimateLearningPathAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiPerformance.ExplainFeedbackDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.ExplainFeedbackDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
            },
            Feedback = new ApiPerformance.Feedback
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
        };
        using var rpc2 = _performanceFeedbackValidationService.ExplainFeedbackDecisionAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiLearning.ExplainCourseDecisionResponse
        {
            Course = new ApiLearning.Course
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.LearningPath?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                CourseId = (request.CourseId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.LearningPath?.Manager?.DisplayName ?? ""),
                },
                Status = (request.Course?.Status ?? (ApiLearning.CourseStatus)0),
            },
            Outcome = new ApiLearning.ExplainCourseDecisionResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply2.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course",
            "audit", "ExplainCourseDecision", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a course.
    /// </summary>
    public override async Task<ApiLearning.GetCourseRulesResponse> GetCourseRules(ApiLearning.GetCourseRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CourseId, "course_id");
        _logger.LogInformation("Handling GetCourseRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "course", request.CourseId,
            "GetCourseRules", "accepted", token);
        var input0 = new ApiHelpdesk.ValidateKnowledgeArticleRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.ValidateKnowledgeArticleRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _sendGetCourseRules0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLearning.GetCourseRulesResponse
        {
            Course = new ApiLearning.Course
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.KnowledgeArticle?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                CourseId = (request.CourseId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lints a course.
    /// </summary>
    public override async Task<ApiLearning.LintCoursesResponse> LintCourses(ApiLearning.LintCoursesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling LintCourses for {Tenant}", requestContext.TenantId);
        if (request.Course is { } record)
        {
            var snapshot = CourseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLearning.CompareLearningPathOptionsRequest
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
                            CountryCode = (request.Course ?? new ApiLearning.Course()).LocatorCase switch
                            {
                                ApiLearning.Course.LocatorOneofCase.Address => (request.Course?.Address?.CountryCode ?? ""),
                                _ => ""
                            },
                            Region = (request.Course ?? new ApiLearning.Course()).LocatorCase switch
                            {
                                ApiLearning.Course.LocatorOneofCase.Address => (request.Course?.Address?.Region ?? ""),
                                _ => ""
                            },
                        },
                    } },
                } },
            },
        };
        using var rpc0 = _learningLearningPathCalculationService.CompareLearningPathOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiFeatureflags.ExplainFeatureFlagDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            FeatureFlag = new ApiFeatureflags.FeatureFlag
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.LearningPath?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Course?.Owner?.DisplayName ?? ""),
                },
            },
        };
        var descriptor1 = new Method<ApiFeatureflags.ExplainFeatureFlagDecisionRequest, ApiFeatureflags.ExplainFeatureFlagDecisionResponse>(
            MethodType.Unary, "acme.featureflags.v1" + ".FeatureFlagValidationService", "ExplainFeatureFlagDecision",
            Wire.Marshaller<ApiFeatureflags.ExplainFeatureFlagDecisionRequest>(), Wire.Marshaller<ApiFeatureflags.ExplainFeatureFlagDecisionResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiHelpdesk.ExplainKnowledgeArticleDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.FeatureFlag?.Audit?.CreatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (request.Course ?? new ApiLearning.Course()).LocatorCase switch
                    {
                        ApiLearning.Course.LocatorOneofCase.Address => (request.Course?.Address?.Locality ?? ""),
                        _ => ""
                    },
                    Region = (request.Course ?? new ApiLearning.Course()).LocatorCase switch
                    {
                        ApiLearning.Course.LocatorOneofCase.Address => (request.Course?.Address?.Region ?? ""),
                        _ => ""
                    },
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply1.FeatureFlag?.Owner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc2 = _helpdeskKnowledgeArticleValidationService.ExplainKnowledgeArticleDecisionAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var profile3 = new MappingProfile<ApiLearning.LintCoursesRequest, ApiLearning.ExplainLearningPathCalculationRequest>(value => new ApiLearning.ExplainLearningPathCalculationRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            LearningPath = new ApiLearning.LearningPath
            {
                Children = { new ApiLearning.LearningPath
                {
                    Children = { new ApiLearning.LearningPath
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            Etag = (reply0.LearningPath?.Audit?.Etag ?? ""),
                            UpdatedBy = (reply0.LearningPath?.Audit?.UpdatedBy ?? ""),
                        },
                    } },
                } },
            },
        });
        var input3 = profile3.Map(request);
        using var rpc3 = _learningLearningPathCalculationService.ExplainLearningPathCalculationAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiLearning.LintCoursesResponse
        {
            Course = new ApiLearning.Course
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply2.KnowledgeArticle?.Audit?.Etag ?? ""),
                },
                CourseId = (request.CourseId ?? ""),
                Owner = (reply1.FeatureFlag?.Owner ?? new ApiIdentity.UserRef()),
                Specs = { (request.Course?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Outcome = new ApiLearning.LintCoursesResponse.Types.Outcome
            {
                EffectiveAt = (reply1.Outcome?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course",
            "audit", "LintCourses", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews a course.
    /// </summary>
    public override async Task<ApiLearning.PreviewCourseResponse> PreviewCourse(ApiLearning.PreviewCourseRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PreviewCourse for {Tenant}", requestContext.TenantId);
        if (request.Course is { } record)
        {
            var snapshot = CourseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiIdentity.ResolveUsersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _identityUserQueryService.ResolveUsersAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.PreviewCourseToPeekUser1();
        using var rpc1 = _identityUserQueryService.PeekUserAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLearning.RecalculateLearningPathsRequest
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
                            Etag = (reply1.User?.Audit?.Etag ?? ""),
                            UpdatedAt = (reply1.User?.Audit?.UpdatedAt ?? new Timestamp()),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            CountryCode = (reply1.User?.PrimaryAddress?.CountryCode ?? ""),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            Email = (request.Course?.Owner?.Email ?? ""),
                        },
                    } },
                } },
            },
        };
        var reply2 = await SendPreviewCourse2Async(input2, token);
        var response = new ApiLearning.PreviewCourseResponse
        {
            Course = new ApiLearning.Course
            {
                CourseId = (request.CourseId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (reply1.User?.UserId ?? ""),
                },
                Specs = { (request.Course?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Outcome = new ApiLearning.PreviewCourseResponse.Types.Outcome
            {
                Status = (reply2.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course",
            "audit", "PreviewCourse", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates a course.
    /// </summary>
    public override async Task<ApiLearning.ValidateCourseResponse> ValidateCourse(ApiLearning.ValidateCourseRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ValidateCourse for {Tenant}", requestContext.TenantId);
        if (request.Course is { } record)
        {
            var snapshot = CourseMapper.ToRecord(requestContext.TenantId, record);
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
                        Location = new ApiCommon.PostalAddress
                        {
                            Region = (request.Course ?? new ApiLearning.Course()).LocatorCase switch
                            {
                                ApiLearning.Course.LocatorOneofCase.Address => (request.Course?.Address?.Region ?? ""),
                                _ => ""
                            },
                        },
                    } },
                } },
            },
        };
        using var rpc0 = _learningLearningPathCalculationService.RecalculateLearningPathsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLearning.ValidateCourseResponse
        {
            Course = new ApiLearning.Course
            {
                CourseId = (request.CourseId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (reply0.LearningPath?.Manager?.UserId ?? ""),
                },
                Specs = { (request.Course?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course",
            "audit", "ValidateCourse", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiLearning.RecalculateLearningPathsResponse> SendPreviewCourse2Async(ApiLearning.RecalculateLearningPathsRequest request, CancellationToken token)
    {
        return await _courseValidationEndpointSendPreviewCourse2Port.SendAsync(request, token);
    }
}
