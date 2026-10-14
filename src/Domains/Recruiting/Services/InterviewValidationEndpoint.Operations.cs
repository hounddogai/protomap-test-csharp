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
using ApiDevices = Acme.Devices.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLearning = Acme.Learning.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Recruiting;

public sealed partial class InterviewValidationEndpoint
{
    /// <summary>
    /// Checks an interview.
    /// </summary>
    public override async Task<ApiRecruiting.CheckInterviewEligibilityResponse> CheckInterviewEligibility(ApiRecruiting.CheckInterviewEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CheckInterviewEligibility for {Tenant}", requestContext.TenantId);
        if (request.Interview is { } record)
        {
            var snapshot = InterviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.QuoteCandidateRequest
        {
            Candidate = new ApiRecruiting.Candidate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Interview?.Audit?.CreatedAt ?? new Timestamp()),
                },
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Interview?.Location?.Region ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _sendCheckInterviewEligibility0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.CompareJobOpeningOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.CompareJobOpeningOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.Candidate?.PrimaryAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Interview?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.Candidate?.PrimaryAddress?.PostalCode ?? ""),
                    Region = (request.Interview?.Location?.Region ?? ""),
                },
            },
        };
        var named1 = _registry.Get<ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient>("acme.recruiting.v1.JobOpeningCalculationService");
        using var rpc1 = named1.CompareJobOpeningOptionsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.CheckInterviewEligibilityResponse
        {
            Interview = new ApiRecruiting.Interview
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Candidate?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (reply0.Candidate?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                InterviewId = (request.InterviewId ?? ""),
                Window = (request.Interview?.Window ?? new ApiCommon.TimeWindow()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "CheckInterviewEligibility", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains an interview.
    /// </summary>
    public override async Task<ApiRecruiting.ExplainInterviewDecisionResponse> ExplainInterviewDecision(ApiRecruiting.ExplainInterviewDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainInterviewDecision for {Tenant}", requestContext.TenantId);
        if (request.Interview is { } record)
        {
            var snapshot = InterviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.QuoteJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.QuoteJobOpeningRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Interview?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Interview?.Audit?.Etag ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (request.Interview?.Location?.Locality ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiRecruiting.QuoteJobOpeningRequest, ApiRecruiting.QuoteJobOpeningResponse>(
            MethodType.Unary, "acme.recruiting.v1" + ".JobOpeningCalculationService", "QuoteJobOpening",
            Wire.Marshaller<ApiRecruiting.QuoteJobOpeningRequest>(), Wire.Marshaller<ApiRecruiting.QuoteJobOpeningResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var reply1 = new ApiDevices.SimulateDevicePolicyResponse();
        foreach (var item in (request.Interview?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiRecruiting.InterviewParticipant>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input1 = new ApiDevices.SimulateDevicePolicyRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiDevices.SimulateDevicePolicyRequest.Types.Details
                {
                    Destination = new ApiCommon.PostalAddress
                    {
                        CountryCode = (request.Interview?.Location?.CountryCode ?? ""),
                        Lines = { (reply0.JobOpening?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    },
                },
                DevicePolicy = new ApiDevices.DevicePolicy
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        UpdatedAt = (reply0.JobOpening?.Audit?.UpdatedAt ?? new Timestamp()),
                    },
                    Owner = new ApiIdentity.UserRef
                    {
                        Email = (item.Contact?.Email ?? ""),
                    },
                },
            };
            using var rpc1 = _devicesDevicePolicyCalculationService.SimulateDevicePolicyAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var response = new ApiRecruiting.ExplainInterviewDecisionResponse
        {
            Interview = new ApiRecruiting.Interview
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.JobOpening?.Audit?.CreatedBy ?? ""),
                },
                InterviewId = (reply0.JobOpening?.Counterparty?.InterviewId ?? ""),
                Metadata = { (request.Interview?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Window = (request.Interview?.Window ?? new ApiCommon.TimeWindow()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "ExplainInterviewDecision", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an interview.
    /// </summary>
    public override async Task<ApiRecruiting.GetInterviewRulesResponse> GetInterviewRules(ApiRecruiting.GetInterviewRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.InterviewId, "interview_id");
        _logger.LogInformation("Handling GetInterviewRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "interview", request.InterviewId,
            "GetInterviewRules", "accepted", token);
        var profile0 = new MappingProfile<ApiRecruiting.GetInterviewRulesRequest, ApiMetrics.PreviewMetricSeriesRequest>(value => new ApiMetrics.PreviewMetricSeriesRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiMetrics.PreviewMetricSeriesRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (value.Context?.Actor?.Email ?? ""),
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _metricsMetricSeriesValidationService.PreviewMetricSeriesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiRecruiting.GetInterviewRulesResponse
        {
            Interview = new ApiRecruiting.Interview
            {
                InterviewId = (request.InterviewId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.MetricSeries?.Address?.Recipient ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lints an interview.
    /// </summary>
    public override async Task<ApiRecruiting.LintInterviewsResponse> LintInterviews(ApiRecruiting.LintInterviewsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling LintInterviews for {Tenant}", requestContext.TenantId);
        if (request.Interview is { } record)
        {
            var snapshot = InterviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.LintJobOpeningsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Interview?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _recruitingJobOpeningValidationService.LintJobOpeningsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var reply1 = new ApiItsm.RecalculateChangeRequestsResponse();
        foreach (var item in (request.Interview?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiRecruiting.InterviewParticipant>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input1 = new ApiItsm.RecalculateChangeRequestsRequest
            {
                ChangeRequest = new ApiItsm.ChangeRequest
                {
                    Assignee = new ApiIdentity.UserRef
                    {
                        Email = (item.Contact?.Email ?? ""),
                    },
                    Requester = new ApiCommon.ContactInfo
                    {
                        Name = (item.Contact?.Name ?? new ApiCommon.PersonName()),
                        Phone = (item.Contact?.Phone ?? ""),
                        PreferredChannel = (item.Contact?.PreferredChannel ?? ""),
                    },
                },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
            };
            using var rpc1 = _itsmChangeRequestCalculationService.RecalculateChangeRequestsAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var response = new ApiRecruiting.LintInterviewsResponse
        {
            Interview = new ApiRecruiting.Interview
            {
                InterviewId = (reply0.JobOpening?.Counterparty?.InterviewId ?? ""),
                Location = (request.Interview?.Location ?? new ApiCommon.PostalAddress()),
                Metadata = { (request.Interview?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "LintInterviews", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Normalizes an interview.
    /// </summary>
    public override async Task<ApiRecruiting.NormalizeInterviewResponse> NormalizeInterview(ApiRecruiting.NormalizeInterviewRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling NormalizeInterview for {Tenant}", requestContext.TenantId);
        if (request.Interview is { } record)
        {
            var snapshot = InterviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.Interview?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiRecruiting.InterviewParticipant>()).Select(async item =>
        {
            var input0 = new ApiRecruiting.CompareInterviewOptionsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Interview = new ApiRecruiting.Interview
                {
                    Participants = { new ApiRecruiting.InterviewParticipant
                    {
                        Contact = new ApiCommon.ContactInfo
                        {
                            Name = (item.Contact?.Name ?? new ApiCommon.PersonName()),
                            PreferredChannel = (item.Contact?.PreferredChannel ?? ""),
                        },
                        ParticipantId = (item.ParticipantId ?? ""),
                    } },
                },
            };
            using var rpc0 = _recruitingInterviewCalculationService.CompareInterviewOptionsAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiRecruiting.CompareInterviewOptionsResponse();
        var input1 = new ApiRecruiting.SimulateInterviewRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.SimulateInterviewRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (request.Interview?.Location?.Locality ?? ""),
                },
            },
            Interview = new ApiRecruiting.Interview
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.Interview?.Audit?.UpdatedBy ?? ""),
                },
                Participants = { new ApiRecruiting.InterviewParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply0.Interview?.Organizer?.Email ?? ""),
                        Phone = (request.Details?.Contact?.Phone ?? ""),
                    },
                } },
            },
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _recruitingInterviewCalculationService.SimulateInterviewAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.NormalizeInterviewResponse
        {
            Interview = new ApiRecruiting.Interview
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Interview?.Audit?.CreatedAt ?? new Timestamp()),
                },
                InterviewId = (request.InterviewId ?? ""),
                Title = (request.Interview?.Title ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "NormalizeInterview", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews an interview.
    /// </summary>
    public override async Task<ApiRecruiting.PreviewInterviewResponse> PreviewInterview(ApiRecruiting.PreviewInterviewRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PreviewInterview for {Tenant}", requestContext.TenantId);
        if (request.Interview is { } record)
        {
            var snapshot = InterviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLearning.CompareCourseEnrollmentOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CourseEnrollment = new ApiLearning.CourseEnrollment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.Interview?.Audit?.UpdatedBy ?? ""),
                },
                Holder = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (request.Interview?.Organizer?.DisplayName ?? ""),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiLearning.CompareCourseEnrollmentOptionsRequest>();
        using var rpc0 = _learningCourseEnrollmentCalculationService.CompareCourseEnrollmentOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiRecruiting.PreviewInterviewResponse
        {
            Interview = new ApiRecruiting.Interview
            {
                InterviewId = (request.InterviewId ?? ""),
                Organizer = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.CourseEnrollment?.Holder?.DisplayName ?? ""),
                },
                Window = (request.Interview?.Window ?? new ApiCommon.TimeWindow()),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "PreviewInterview", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores an interview.
    /// </summary>
    public override async Task<ApiRecruiting.ScoreInterviewResponse> ScoreInterview(ApiRecruiting.ScoreInterviewRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScoreInterview for {Tenant}", requestContext.TenantId);
        if (request.Interview is { } record)
        {
            var snapshot = InterviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.CompareJobOpeningOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (request.Interview?.Location?.Locality ?? ""),
                    PostalCode = (request.Interview?.Location?.PostalCode ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiRecruiting.CompareJobOpeningOptionsRequest>(json0);
        using var rpc0 = _recruitingJobOpeningCalculationService.CompareJobOpeningOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.ScoreJobOpeningRequest();
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        input1.JobOpening = new ApiRecruiting.JobOpening
        {
            Audit = new ApiCommon.AuditInfo
            {
                UpdatedBy = (request.Interview?.Audit?.UpdatedBy ?? ""),
            },
        };
        using var rpc1 = _recruitingJobOpeningValidationService.ScoreJobOpeningAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var reply2 = new ApiMetrics.CheckMetricSeriesEligibilityResponse();
        foreach (var item in (request.Interview?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiRecruiting.InterviewParticipant>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input2 = new ApiMetrics.CheckMetricSeriesEligibilityRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiMetrics.CheckMetricSeriesEligibilityRequest.Types.Details
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        PreferredChannel = (item.Contact?.PreferredChannel ?? ""),
                    },
                },
                MetricSeries = new ApiMetrics.MetricSeries
                {
                    Owner = new ApiIdentity.UserRef
                    {
                        Email = (item.Contact?.Email ?? ""),
                    },
                },
            };
            using var rpc2 = _metricsMetricSeriesValidationService.CheckMetricSeriesEligibilityAsync(input2, cancellationToken: token);
            reply2 = await rpc2.ResponseAsync;
        }
        var batch3 = (request.Interview?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiRecruiting.InterviewParticipant>()).Select(async item =>
        {
            var input3 = new ApiRecruiting.ExplainJobOpeningDecisionRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiRecruiting.ExplainJobOpeningDecisionRequest.Types.Details
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (item.Contact?.Email ?? ""),
                        Name = (item.Contact?.Name ?? new ApiCommon.PersonName()),
                        Phone = (item.Contact?.Phone ?? ""),
                        PreferredChannel = (item.Contact?.PreferredChannel ?? ""),
                    },
                },
            };
            using var rpc3 = _recruitingJobOpeningValidationService.ExplainJobOpeningDecisionAsync(input3, cancellationToken: token);
            var reply3 = await rpc3.ResponseAsync;
            return reply3;
        }).ToArray();
        var replies3 = await Task.WhenAll(batch3);
        var reply3 = replies3.LastOrDefault() ?? new ApiRecruiting.ExplainJobOpeningDecisionResponse();
        var response = new ApiRecruiting.ScoreInterviewResponse
        {
            Breakdown = new ApiRecruiting.ScoreInterviewResponse.Types.Breakdown
            {
                CurrencyCode = (request.Details?.CurrencyCode ?? ""),
            },
            Interview = new ApiRecruiting.Interview
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.JobOpening?.Audit?.Etag ?? ""),
                },
                InterviewId = (request.InterviewId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "ScoreInterview", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates an interview.
    /// </summary>
    public override async Task<ApiRecruiting.ValidateInterviewResponse> ValidateInterview(ApiRecruiting.ValidateInterviewRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ValidateInterview for {Tenant}", requestContext.TenantId);
        if (request.Interview is { } record)
        {
            var snapshot = InterviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.QuoteJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.QuoteJobOpeningRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Recipient = (request.Interview?.Location?.Recipient ?? ""),
                    Region = (request.Interview?.Location?.Region ?? ""),
                },
            },
            JobOpening = new ApiRecruiting.JobOpening
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Interview?.Location?.CountryCode ?? ""),
                    Locality = (request.Interview?.Location?.Locality ?? ""),
                },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _recruitingJobOpeningCalculationService.QuoteJobOpeningAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiCorporateevents.ExplainVenueCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Venue = new ApiCorporateevents.Venue
            {
                Address = new ApiCommon.PostalAddress
                {
                    Locality = (request.Interview?.Location?.Locality ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Interview?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Interview?.Organizer?.DisplayName ?? ""),
                },
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _corporateeventsVenueCalculationService.ExplainVenueCalculationAsync);
        var response = new ApiRecruiting.ValidateInterviewResponse
        {
            Interview = new ApiRecruiting.Interview
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.JobOpening?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedBy = (reply1.Venue?.Audit?.UpdatedBy ?? ""),
                },
                InterviewId = (request.InterviewId ?? ""),
                Location = (request.Interview?.Location ?? new ApiCommon.PostalAddress()),
            },
            Outcome = new ApiRecruiting.ValidateInterviewResponse.Types.Outcome
            {
                Status = (reply1.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "ValidateInterview", (request.Reason ?? ""), token);
        return response;
    }
}
