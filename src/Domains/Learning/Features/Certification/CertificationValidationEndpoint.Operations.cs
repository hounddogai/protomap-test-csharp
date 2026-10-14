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
using ApiCampaigns = Acme.Campaigns.V1;
using ApiCommon = Acme.Common.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Learning;

public sealed partial class CertificationValidationEndpoint
{
    /// <summary>
    /// Checks a certification.
    /// </summary>
    public override async Task<ApiLearning.CheckCertificationEligibilityResponse> CheckCertificationEligibility(ApiLearning.CheckCertificationEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CheckCertificationEligibility for {Tenant}", requestContext.TenantId);
        if (request.Certification is { } record)
        {
            var snapshot = CertificationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.QuoteSoftwareLicenseRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Details = new ApiLicensing.QuoteSoftwareLicenseRequest.Types.Details
        {
            Destination = new ApiCommon.PostalAddress
            {
                Lines = { (request.Certification?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        using var rpc0 = _licensingSoftwareLicenseCalculationService.QuoteSoftwareLicenseAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCampaigns.SearchMarketingCampaignsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiCampaigns.SearchMarketingCampaignsRequest.Types.Filter
            {
                CreatedAfter = (request.Certification?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (reply0.SoftwareLicense?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.Result?.CorrelationId ?? ""),
                Region = (request.Certification?.BillingAddress?.Region ?? ""),
            },
        };
        using var rpc1 = _campaignsMarketingCampaignService.SearchMarketingCampaignsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLearning.CheckCertificationEligibilityResponse
        {
            Certification = new ApiLearning.Certification
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.SoftwareLicense?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedBy = (reply0.SoftwareLicense?.Audit?.UpdatedBy ?? ""),
                },
                CertificationId = (request.CertificationId ?? ""),
                Number = (request.Certification?.Number ?? ""),
            },
            Outcome = new ApiLearning.CheckCertificationEligibilityResponse.Types.Outcome
            {
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "certification",
            "audit", "CheckCertificationEligibility", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a certification.
    /// </summary>
    public override async Task<ApiLearning.ExplainCertificationDecisionResponse> ExplainCertificationDecision(ApiLearning.ExplainCertificationDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainCertificationDecision for {Tenant}", requestContext.TenantId);
        if (request.Certification is { } record)
        {
            var snapshot = CertificationMapper.ToRecord(requestContext.TenantId, record);
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
                            CreatedBy = (request.Certification?.Audit?.CreatedBy ?? ""),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            Locality = (request.Certification?.BillingAddress?.Locality ?? ""),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (request.Certification?.Counterparty?.DisplayName ?? ""),
                        },
                    } },
                    Location = new ApiCommon.PostalAddress
                    {
                        Recipient = (request.Certification?.BillingAddress?.Recipient ?? ""),
                    },
                } },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _learningLearningPathCalculationService.RecalculateLearningPathsAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiTravel.ExplainTravelBookingCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TravelBooking = new ApiTravel.TravelBooking
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Certification?.BillingAddress?.PostalCode ?? ""),
                    Recipient = (reply0.LearningPath?.Location?.Recipient ?? ""),
                },
                Card = new ApiTravel.TravelBookingCardSettlement
                {
                    HolderName = (request.Certification ?? new ApiLearning.Certification()).PaymentCase switch
                    {
                        ApiLearning.Certification.SettlementOneofCase.Card => (request.Certification?.Card?.HolderName ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _travelTravelBookingCalculationService.ExplainTravelBookingCalculationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLearning.ExplainCertificationDecisionResponse
        {
            Certification = new ApiLearning.Certification
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.LearningPath?.Location?.Recipient ?? ""),
                },
                CertificationId = (request.CertificationId ?? ""),
                Notes = (request.Certification?.Notes ?? ""),
            },
            Outcome = new ApiLearning.ExplainCertificationDecisionResponse.Types.Outcome
            {
                Message = (reply1.Outcome?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "certification",
            "audit", "ExplainCertificationDecision", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a certification.
    /// </summary>
    public override async Task<ApiLearning.GetCertificationRulesResponse> GetCertificationRules(ApiLearning.GetCertificationRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CertificationId, "certification_id");
        _logger.LogInformation("Handling GetCertificationRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "certification", request.CertificationId,
            "GetCertificationRules", "accepted", token);
        var input0 = new ApiFeatureflags.ValidateFeatureFlagRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiFeatureflags.ValidateFeatureFlagRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
            FeatureFlag = new ApiFeatureflags.FeatureFlag
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _featureflagsFeatureFlagValidationService.ValidateFeatureFlagAsync);
        var response = new ApiLearning.GetCertificationRulesResponse
        {
            Certification = new ApiLearning.Certification
            {
                CertificationId = (request.CertificationId ?? ""),
                Counterparty = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (reply0.FeatureFlag?.Owner?.DisplayName ?? ""),
                },
            },
        };
        return response;
    }
    /// <summary>
    /// Lints a certification.
    /// </summary>
    public override async Task<ApiLearning.LintCertificationsResponse> LintCertifications(ApiLearning.LintCertificationsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling LintCertifications for {Tenant}", requestContext.TenantId);
        if (request.Certification is { } record)
        {
            var snapshot = CertificationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCampaigns.UpdateMarketingCampaignRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            MarketingCampaign = new ApiCampaigns.MarketingCampaign
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.Certification?.Audit?.UpdatedBy ?? ""),
                },
                Location = new ApiCommon.PostalAddress
                {
                    Region = (request.Certification?.BillingAddress?.Region ?? ""),
                },
                Participants = { new ApiCampaigns.MarketingCampaignParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                        PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                    },
                } },
            },
        };
        using var rpc0 = _sendLintCertifications0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLearning.PreviewCourseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Course = new ApiLearning.Course
            {
                Address = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.MarketingCampaign?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Region = (request.Certification?.BillingAddress?.Region ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Certification?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        var named1 = _registry.Get<ApiLearning.CourseValidationService.CourseValidationServiceClient>("acme.learning.v1.CourseValidationService");
        using var rpc1 = named1.PreviewCourseAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLearning.CheckLearningPathEligibilityRequest
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
                            Region = (reply0.MarketingCampaign?.Location?.Region ?? ""),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            UserId = (reply1.Course?.Owner?.UserId ?? ""),
                        },
                    } },
                } },
            },
        };
        var descriptor2 = new Method<ApiLearning.CheckLearningPathEligibilityRequest, ApiLearning.CheckLearningPathEligibilityResponse>(
            MethodType.Unary, "acme.learning.v1" + ".LearningPathValidationService", "CheckLearningPathEligibility",
            Wire.Marshaller<ApiLearning.CheckLearningPathEligibilityRequest>(), Wire.Marshaller<ApiLearning.CheckLearningPathEligibilityResponse>());
        using var rpc2 = _invoker.AsyncUnaryCall(descriptor2, null,
            new CallOptions(cancellationToken: token), input2);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiLearning.ValidateLearningPathRequest
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
                            CreatedAt = (reply2.LearningPath?.Audit?.CreatedAt ?? new Timestamp()),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            Region = (reply1.Course ?? new ApiLearning.Course()).LocatorCase switch
                            {
                                ApiLearning.Course.LocatorOneofCase.Address => (reply1.Course?.Address?.Region ?? ""),
                                _ => ""
                            },
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            Email = (reply1.Course?.Owner?.Email ?? ""),
                            UserId = (reply2.LearningPath?.Manager?.UserId ?? ""),
                        },
                    } },
                } },
            },
        };
        using var rpc3 = _learningLearningPathValidationService.ValidateLearningPathAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiLearning.LintCertificationsResponse
        {
            Certification = new ApiLearning.Certification
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply2.LearningPath?.Audit?.CreatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply2.LearningPath?.Location?.Locality ?? ""),
                },
                CertificationId = (request.CertificationId ?? ""),
                Notes = (request.Certification?.Notes ?? ""),
            },
            Outcome = new ApiLearning.LintCertificationsResponse.Types.Outcome
            {
                EffectiveAt = (reply1.Outcome?.EffectiveAt ?? new Timestamp()),
                Message = (reply1.Outcome?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply3.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "certification",
            "audit", "LintCertifications", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Normalizes a certification.
    /// </summary>
    public override async Task<ApiLearning.NormalizeCertificationResponse> NormalizeCertification(ApiLearning.NormalizeCertificationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling NormalizeCertification for {Tenant}", requestContext.TenantId);
        if (request.Certification is { } record)
        {
            var snapshot = CertificationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiLearning.NormalizeCertificationRequest, ApiLearning.ExplainLearningPathCalculationRequest>(value => new ApiLearning.ExplainLearningPathCalculationRequest
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
                            UpdatedBy = (value.Certification?.Audit?.UpdatedBy ?? ""),
                        },
                    } },
                } },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _learningLearningPathCalculationService.ExplainLearningPathCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLearning.NormalizeCertificationResponse
        {
            Certification = new ApiLearning.Certification
            {
                BillingAddress = (request.Certification?.BillingAddress ?? new ApiCommon.PostalAddress()),
                CertificationId = (request.CertificationId ?? ""),
            },
            Outcome = new ApiLearning.NormalizeCertificationResponse.Types.Outcome
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "certification",
            "audit", "NormalizeCertification", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews a certification.
    /// </summary>
    public override async Task<ApiLearning.PreviewCertificationResponse> PreviewCertification(ApiLearning.PreviewCertificationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PreviewCertification for {Tenant}", requestContext.TenantId);
        if (request.Certification is { } record)
        {
            var snapshot = CertificationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiMetrics.SummarizeMetricSeriesesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _metricsMetricSeriesQueryService.SummarizeMetricSeriesesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.PreviewCertificationToSearchIncidents1();
        using var rpc1 = _itsmIncidentService.SearchIncidentsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLearning.PreviewCertificationResponse
        {
            Certification = new ApiLearning.Certification
            {
                CertificationId = (request.CertificationId ?? ""),
                Number = (request.Certification?.Number ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Locale ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "certification",
            "audit", "PreviewCertification", (request.Details?.Locale ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores a certification.
    /// </summary>
    public override async Task<ApiLearning.ScoreCertificationResponse> ScoreCertification(ApiLearning.ScoreCertificationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScoreCertification for {Tenant}", requestContext.TenantId);
        if (request.Certification is { } record)
        {
            var snapshot = CertificationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.Certification?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiLearning.CertificationLine>()).Select(async item =>
        {
            var input0 = new ApiLearning.EstimateLearningPathRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                LearningPath = new ApiLearning.LearningPath
                {
                    Attributes = { (item.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    Children = { new ApiLearning.LearningPath
                    {
                        Children = { new ApiLearning.LearningPath
                        {
                            Location = new ApiCommon.PostalAddress
                            {
                                CountryCode = (request.Certification?.BillingAddress?.CountryCode ?? ""),
                                Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                                Locality = (request.Certification?.BillingAddress?.Locality ?? ""),
                            },
                        } },
                    } },
                },
            };
            using var rpc0 = _learningLearningPathCalculationService.EstimateLearningPathAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiLearning.EstimateLearningPathResponse();
        var response = new ApiLearning.ScoreCertificationResponse
        {
            Certification = new ApiLearning.Certification
            {
                CertificationId = (request.CertificationId ?? ""),
                Counterparty = (request.Certification?.Counterparty ?? new ApiRecruiting.CandidateRef()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "certification",
            "audit", "ScoreCertification", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates a certification.
    /// </summary>
    public override async Task<ApiLearning.ValidateCertificationResponse> ValidateCertification(ApiLearning.ValidateCertificationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ValidateCertification for {Tenant}", requestContext.TenantId);
        if (request.Certification is { } record)
        {
            var snapshot = CertificationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCampaigns.WatchMarketingCampaignsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Phone = (request.Details?.Contact?.Phone ?? ""),
            },
        };
        using var stream0 = _campaignsMarketingCampaignService.WatchMarketingCampaigns(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchMarketingCampaigns frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiLearning.SimulateCertificationRequest
        {
            Certification = new ApiLearning.Certification
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Certification?.Audit?.CreatedBy ?? ""),
                    Etag = (request.Certification?.Audit?.Etag ?? ""),
                },
                Bank = new ApiLearning.CertificationBankSettlement
                {
                    Iban = (request.Certification ?? new ApiLearning.Certification()).PaymentCase switch
                    {
                        ApiLearning.Certification.SettlementOneofCase.Bank => (request.Certification?.Bank?.Iban ?? ""),
                        _ => ""
                    },
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLearning.SimulateCertificationRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (request.Certification?.BillingAddress?.Locality ?? ""),
                },
            },
        };
        using var rpc1 = _learningCertificationCalculationService.SimulateCertificationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
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
                            CreatedBy = (request.Certification?.Audit?.CreatedBy ?? ""),
                        },
                    } },
                } },
            },
        };
        var json2 = JsonFormatter.Default.Format(input2);
        input2 = JsonParser.Default.Parse<ApiLearning.SimulateLearningPathRequest>(json2);
        using var rpc2 = _learningLearningPathCalculationService.SimulateLearningPathAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiLearning.CalculateLearningPathRequest();
        input3.Context = (request.Context ?? new ApiCommon.RequestContext());
        input3.LearningPath = new ApiLearning.LearningPath
        {
            Children = { new ApiLearning.LearningPath
            {
                Children = { new ApiLearning.LearningPath
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedBy = (reply1.Certification?.Audit?.CreatedBy ?? ""),
                        Etag = (reply1.Certification?.Audit?.Etag ?? ""),
                        UpdatedAt = (reply1.Certification?.Audit?.UpdatedAt ?? new Timestamp()),
                    },
                    Location = new ApiCommon.PostalAddress
                    {
                        Recipient = (reply2.LearningPath?.Location?.Recipient ?? ""),
                    },
                } },
            } },
        };
        using var rpc3 = _learningLearningPathCalculationService.CalculateLearningPathAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiLearning.ValidateCertificationResponse
        {
            Certification = new ApiLearning.Certification
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply2.LearningPath?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (reply2.LearningPath?.Audit?.CreatedBy ?? ""),
                    Etag = (reply3.LearningPath?.Audit?.Etag ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (reply1.Certification?.BillingAddress?.Region ?? ""),
                },
                CertificationId = (reply1.Certification?.CertificationId ?? ""),
                Counterparty = (request.Certification?.Counterparty ?? new ApiRecruiting.CandidateRef()),
                Status = (request.Certification?.Status ?? (ApiLearning.CertificationStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "certification",
            "audit", "ValidateCertification", (request.Reason ?? ""), token);
        return response;
    }
}
