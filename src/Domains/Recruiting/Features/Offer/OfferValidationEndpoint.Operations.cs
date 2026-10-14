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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiMedia = Acme.Media.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Recruiting;

public sealed partial class OfferValidationEndpoint
{
    /// <summary>
    /// Checks an offer.
    /// </summary>
    public override async Task<ApiRecruiting.CheckOfferEligibilityResponse> CheckOfferEligibility(ApiRecruiting.CheckOfferEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CheckOfferEligibility for {Tenant}", requestContext.TenantId);
        if (request.Offer is { } record)
        {
            var snapshot = OfferMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.PreviewInterviewRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.PreviewInterviewRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Offer?.Counterparty?.Email ?? ""),
                },
            },
            Interview = new ApiRecruiting.Interview
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.Offer?.Audit?.UpdatedBy ?? ""),
                },
                Location = new ApiCommon.PostalAddress
                {
                    Locality = (request.Offer?.BillingAddress?.Locality ?? ""),
                },
                Participants = { new ApiRecruiting.InterviewParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Phone = (request.Details?.Contact?.Phone ?? ""),
                    },
                } },
            },
        };
        var command0 = new DispatchCommand<ApiRecruiting.PreviewInterviewRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _recruitingInterviewValidationService.PreviewInterviewAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiRecruiting.CheckOfferEligibilityResponse
        {
            Offer = new ApiRecruiting.Offer
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.Interview?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                IssuedAt = (request.Offer?.IssuedAt ?? new Timestamp()),
                OfferId = (request.OfferId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "offer",
            "audit", "CheckOfferEligibility", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lints an offer.
    /// </summary>
    public override async Task<ApiRecruiting.LintOffersResponse> LintOffers(ApiRecruiting.LintOffersRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling LintOffers for {Tenant}", requestContext.TenantId);
        if (request.Offer is { } record)
        {
            var snapshot = OfferMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.SubscribeDocumentUpdatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SubscribeDocumentUpdatesRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Offer?.Counterparty?.Email ?? ""),
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc0 = _documentsDocumentNotificationService.SubscribeDocumentUpdatesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.QuoteCandidateRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.QuoteCandidateRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.Document?.BillingAddress?.PostalCode ?? ""),
                },
            },
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _recruitingCandidateCalculationService.QuoteCandidateAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.LintOffersResponse
        {
            Offer = new ApiRecruiting.Offer
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply1.Candidate?.PrimaryAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Locality = (reply0.Document?.BillingAddress?.Locality ?? ""),
                },
                Number = (reply1.Candidate?.NationalId?.Number ?? ""),
                OfferId = (request.OfferId ?? ""),
                Status = (request.Offer?.Status ?? (ApiRecruiting.OfferStatus)0),
            },
            Outcome = new ApiRecruiting.LintOffersResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "offer",
            "audit", "LintOffers", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Normalizes an offer.
    /// </summary>
    public override async Task<ApiRecruiting.NormalizeOfferResponse> NormalizeOffer(ApiRecruiting.NormalizeOfferRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling NormalizeOffer for {Tenant}", requestContext.TenantId);
        if (request.Offer is { } record)
        {
            var snapshot = OfferMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.NormalizeOfferToSimulateOffer0();
        using var rpc0 = _recruitingOfferCalculationService.SimulateOfferAsync(input0, cancellationToken: token);
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
                            CreatedBy = (reply0.Offer?.Audit?.CreatedBy ?? ""),
                            UpdatedAt = (request.Offer?.Audit?.UpdatedAt ?? new Timestamp()),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (reply0.Offer?.Counterparty?.DisplayName ?? ""),
                        },
                    } },
                } },
            },
        };
        var reply1 = await SendNormalizeOffer1Async(input1, token);
        var input2 = new ApiDocuments.UnsubscribeDocumentUpdatesRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.UnsubscribeDocumentUpdatesRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (reply1.LearningPath?.Manager?.Email ?? ""),
                    Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        using var rpc2 = _documentsDocumentNotificationService.UnsubscribeDocumentUpdatesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var batch3 = (request.Offer?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiRecruiting.OfferLine>()).Select(async item =>
        {
            var input3 = new ApiFeatureflags.CreateFeatureFlagRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                FeatureFlag = new ApiFeatureflags.FeatureFlag
                {
                    Description = (item.Description ?? ""),
                    Discount = new ApiFeatureflags.FeatureFlagDiscountAction
                    {
                        Amount = (item.Tax?.Amount ?? new ApiCommon.Money()),
                    },
                },
            };
            using var rpc3 = _featureflagsFeatureFlagService.CreateFeatureFlagAsync(input3, cancellationToken: token);
            var reply3 = await rpc3.ResponseAsync;
            return reply3;
        }).ToArray();
        var replies3 = await Task.WhenAll(batch3);
        var reply3 = replies3.LastOrDefault() ?? new ApiFeatureflags.CreateFeatureFlagResponse();
        var response = new ApiRecruiting.NormalizeOfferResponse
        {
            Offer = new ApiRecruiting.Offer
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Offer?.Audit?.CreatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply1.LearningPath?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                IssuedAt = (request.Offer?.IssuedAt ?? new Timestamp()),
                OfferId = (request.OfferId ?? ""),
            },
            Outcome = new ApiRecruiting.NormalizeOfferResponse.Types.Outcome
            {
                Message = (reply2.Result?.Message ?? ""),
                Status = (reply2.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "offer",
            "audit", "NormalizeOffer", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews an offer.
    /// </summary>
    public override async Task<ApiRecruiting.PreviewOfferResponse> PreviewOffer(ApiRecruiting.PreviewOfferRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PreviewOffer for {Tenant}", requestContext.TenantId);
        if (request.Offer is { } record)
        {
            var snapshot = OfferMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.SendDocumentTestNoticeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SendDocumentTestNoticeRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                    Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.Contact?.Phone ?? ""),
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc0 = _documentsDocumentNotificationService.SendDocumentTestNoticeAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiAuthorization.CountAccessPoliciesRequest();
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        input1.Filter = new ApiAuthorization.CountAccessPoliciesRequest.Types.Filter
        {
            CreatedAfter = (reply0.Document?.Audit?.CreatedAt ?? new Timestamp()),
            Labels = { (reply0.Document?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            OwnerId = (reply0.Result?.CorrelationId ?? ""),
            Region = (request.Offer?.BillingAddress?.Region ?? ""),
        };
        using var rpc1 = _authorizationAccessPolicyQueryService.CountAccessPoliciesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.PreviewOfferResponse
        {
            Offer = new ApiRecruiting.Offer
            {
                BillingAddress = (reply0.Document?.BillingAddress ?? new ApiCommon.PostalAddress()),
                IssuedAt = (request.Offer?.IssuedAt ?? new Timestamp()),
                OfferId = (request.OfferId ?? ""),
            },
            Outcome = new ApiRecruiting.PreviewOfferResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "offer",
            "audit", "PreviewOffer", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores an offer.
    /// </summary>
    public override async Task<ApiRecruiting.ScoreOfferResponse> ScoreOffer(ApiRecruiting.ScoreOfferRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScoreOffer for {Tenant}", requestContext.TenantId);
        if (request.Offer is { } record)
        {
            var snapshot = OfferMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiMedia.BrowseMediaAssetsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiMedia.BrowseMediaAssetsRequest.Types.Filter
            {
                CreatedAfter = (request.Offer?.Audit?.CreatedAt ?? new Timestamp()),
                Region = (request.Offer?.BillingAddress?.Region ?? ""),
            },
        };
        using var rpc0 = _mediaMediaAssetQueryService.BrowseMediaAssetsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.LintDocumentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Document = new ApiDocuments.Document
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Offer?.Audit?.CreatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Region = (request.Offer?.BillingAddress?.Region ?? ""),
                },
                Card = new ApiDocuments.DocumentCardSettlement
                {
                    Token = (request.Offer ?? new ApiRecruiting.Offer()).PaymentCase switch
                    {
                        ApiRecruiting.Offer.SettlementOneofCase.Card => (request.Offer?.Card?.Token ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _documentsDocumentValidationService.LintDocumentsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.ScoreOfferResponse
        {
            Offer = new ApiRecruiting.Offer
            {
                DueAt = (request.Offer?.DueAt ?? new Timestamp()),
                Metadata = { (reply1.Document?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OfferId = (request.OfferId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "offer",
            "audit", "ScoreOffer", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates an offer.
    /// </summary>
    public override async Task<ApiRecruiting.ValidateOfferResponse> ValidateOffer(ApiRecruiting.ValidateOfferRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ValidateOffer for {Tenant}", requestContext.TenantId);
        if (request.Offer is { } record)
        {
            var snapshot = OfferMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.ValidateRegistrationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.ValidateRegistrationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
            },
            Registration = new ApiCorporateevents.Registration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.Offer?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _corporateeventsRegistrationValidationService.ValidateRegistrationAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiRecruiting.ValidateOfferResponse
        {
            Offer = new ApiRecruiting.Offer
            {
                IssuedAt = (request.Offer?.IssuedAt ?? new Timestamp()),
                OfferId = (request.OfferId ?? ""),
            },
            Outcome = new ApiRecruiting.ValidateOfferResponse.Types.Outcome
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "offer",
            "audit", "ValidateOffer", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiLearning.SimulateLearningPathResponse> SendNormalizeOffer1Async(ApiLearning.SimulateLearningPathRequest request, CancellationToken token)
    {
        return await _offerValidationEndpointSendNormalizeOffer1Port.SendAsync(request, token);
    }
}
