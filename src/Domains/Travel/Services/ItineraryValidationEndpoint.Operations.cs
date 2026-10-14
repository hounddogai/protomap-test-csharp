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
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

public sealed partial class ItineraryValidationEndpoint
{
    /// <summary>
    /// Checks an itinerary.
    /// </summary>
    public override async Task<ApiTravel.CheckItineraryEligibilityResponse> CheckItineraryEligibility(ApiTravel.CheckItineraryEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CheckItineraryEligibility for {Tenant}", requestContext.TenantId);
        if (request.Itinerary is { } record)
        {
            var snapshot = ItineraryMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.CompareDocumentVersionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.CompareDocumentVersionsRequest.Types.Details
            {
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Document = new ApiDocuments.Document
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Itinerary?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Recipient = (request.Itinerary?.BillingAddress?.Recipient ?? ""),
                },
                Card = new ApiDocuments.DocumentCardSettlement
                {
                    Token = (request.Itinerary ?? new ApiTravel.Itinerary()).PaymentCase switch
                    {
                        ApiTravel.Itinerary.SettlementOneofCase.Card => (request.Itinerary?.Card?.Token ?? ""),
                        _ => ""
                    },
                },
                Total = (request.Itinerary?.Total ?? new ApiCommon.Money()),
            },
        };
        using var rpc0 = _documentsDocumentHistoryService.CompareDocumentVersionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.ExplainServiceAssetCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.ExplainServiceAssetCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Itinerary?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiItsm.ExplainServiceAssetCalculationRequest>();
        using var rpc1 = _itsmServiceAssetCalculationService.ExplainServiceAssetCalculationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.CheckItineraryEligibilityResponse
        {
            Itinerary = new ApiTravel.Itinerary
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Document?.Audit?.Etag ?? ""),
                },
                ItineraryId = (request.ItineraryId ?? ""),
                Lines = { new ApiTravel.ItineraryLine
                {
                    Description = (reply1.ServiceAsset?.Components?.GetValueOrDefault("primary")?.Description ?? ""),
                } },
                Total = (request.Itinerary?.Total ?? new ApiCommon.Money()),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "itinerary",
            "audit", "CheckItineraryEligibility", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains an itinerary.
    /// </summary>
    public override async Task<ApiTravel.ExplainItineraryDecisionResponse> ExplainItineraryDecision(ApiTravel.ExplainItineraryDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainItineraryDecision for {Tenant}", requestContext.TenantId);
        if (request.Itinerary is { } record)
        {
            var snapshot = ItineraryMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiAuthorization.PreviewAccessPolicyResponse();
        foreach (var item in (request.Itinerary?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiTravel.ItineraryLine>()))
        {
            var input0 = new ApiAuthorization.PreviewAccessPolicyRequest
            {
                AccessPolicy = new ApiAuthorization.AccessPolicy
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        UpdatedBy = (request.Itinerary?.Audit?.UpdatedBy ?? ""),
                    },
                    Description = (item.Description ?? ""),
                    Discount = new ApiAuthorization.AccessPolicyDiscountAction
                    {
                        Amount = (item.Amount ?? new ApiCommon.Money()),
                    },
                },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
            };
            using var rpc0 = _authorizationAccessPolicyValidationService.PreviewAccessPolicyAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiTravel.RecalculateTripsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Trip = new ApiTravel.Trip
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Itinerary?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (reply0.AccessPolicy?.Audit?.Etag ?? ""),
                },
                Lines = { new ApiTravel.TripLine
                {
                    Amount = (reply0.AccessPolicy ?? new ApiAuthorization.AccessPolicy()).ActionCase switch
                    {
                        ApiAuthorization.AccessPolicy.ActionOneofCase.Discount => (reply0.AccessPolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                        _ => new ApiCommon.Money()
                    },
                } },
            },
        };
        using var rpc1 = _travelTripCalculationService.RecalculateTripsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiTravel.GetTravelBookingRatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TravelBookingId = (request.Itinerary?.Counterparty?.TravelBookingId ?? ""),
        };
        using var rpc2 = _travelTravelBookingCalculationService.GetTravelBookingRatesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiTravel.ExplainItineraryDecisionResponse
        {
            Itinerary = new ApiTravel.Itinerary
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.Trip?.Audit?.Etag ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply2.TravelBooking?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                Counterparty = new ApiTravel.TravelBookingRef
                {
                    Number = (reply1.Trip?.Number ?? ""),
                },
                ItineraryId = (request.ItineraryId ?? ""),
                Status = (request.Itinerary?.Status ?? (ApiTravel.ItineraryStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "itinerary",
            "audit", "ExplainItineraryDecision", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lints an itinerary.
    /// </summary>
    public override async Task<ApiTravel.LintItinerariesResponse> LintItineraries(ApiTravel.LintItinerariesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling LintItineraries for {Tenant}", requestContext.TenantId);
        if (request.Itinerary is { } record)
        {
            var snapshot = ItineraryMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.RecalculateTravelBookingsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.RecalculateTravelBookingsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Itinerary?.BillingAddress?.PostalCode ?? ""),
                    Recipient = (request.Itinerary?.BillingAddress?.Recipient ?? ""),
                },
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Itinerary?.BillingAddress?.Region ?? ""),
                },
                Card = new ApiTravel.TravelBookingCardSettlement
                {
                    HolderName = (request.Itinerary ?? new ApiTravel.Itinerary()).PaymentCase switch
                    {
                        ApiTravel.Itinerary.SettlementOneofCase.Card => (request.Itinerary?.Card?.HolderName ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _travelTravelBookingCalculationService.RecalculateTravelBookingsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.NormalizeTravelBookingRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TravelBooking = new ApiTravel.TravelBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.TravelBooking?.Audit?.CreatedBy ?? ""),
                },
                Bank = new ApiTravel.TravelBookingBankSettlement
                {
                    Iban = (request.Itinerary ?? new ApiTravel.Itinerary()).PaymentCase switch
                    {
                        ApiTravel.Itinerary.SettlementOneofCase.Bank => (request.Itinerary?.Bank?.Iban ?? ""),
                        _ => ""
                    },
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                    EmployeeId = (reply0.TravelBooking?.Counterparty?.EmployeeId ?? ""),
                },
            },
        };
        using var rpc1 = _travelTravelBookingValidationService.NormalizeTravelBookingAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.LintItinerariesResponse
        {
            Itinerary = new ApiTravel.Itinerary
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.TravelBooking?.Audit?.Etag ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.TravelBooking?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                ItineraryId = (request.ItineraryId ?? ""),
                Notes = (request.Itinerary?.Notes ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "itinerary",
            "audit", "LintItineraries", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Normalizes an itinerary.
    /// </summary>
    public override async Task<ApiTravel.NormalizeItineraryResponse> NormalizeItinerary(ApiTravel.NormalizeItineraryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling NormalizeItinerary for {Tenant}", requestContext.TenantId);
        if (request.Itinerary is { } record)
        {
            var snapshot = ItineraryMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiIdentity.GetUserRulesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            UserId = (request.Context?.Actor?.UserId ?? ""),
        };
        var reply0 = await Workflow.SendAsync(input0, token, _identityUserValidationService.GetUserRulesAsync);
        var reply1 = new ApiTravel.ScoreTravelPolicyResponse();
        foreach (var item in (request.Itinerary?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiTravel.ItineraryLine>()))
        {
            var input1 = new ApiTravel.ScoreTravelPolicyRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                TravelPolicy = new ApiTravel.TravelPolicy
                {
                    Description = (item.Description ?? ""),
                    Discount = new ApiTravel.TravelPolicyDiscountAction
                    {
                        Amount = (item.Amount ?? new ApiCommon.Money()),
                    },
                    Owner = new ApiIdentity.UserRef
                    {
                        Email = (reply0.User?.RelationshipManager?.Email ?? ""),
                    },
                },
            };
            using var rpc1 = _travelTravelPolicyValidationService.ScoreTravelPolicyAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var input2 = new ApiDocuments.ListDocumentFolderExportsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentFolderExportsRequest.Types.Filter
            {
                CreatedAfter = (reply0.User ?? new ApiIdentity.User()).IdentityDocumentCase switch
                {
                    ApiIdentity.User.IdentityDocumentOneofCase.Passport => (reply0.User?.Passport?.ExpiresAt ?? new Timestamp()),
                    _ => new Timestamp()
                },
                OwnerId = (reply1.TravelPolicy?.Owner?.UserId ?? ""),
                Region = (reply0.User?.PrimaryAddress?.Region ?? ""),
            },
        };
        using var rpc2 = _documentsDocumentFolderExportService.ListDocumentFolderExportsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiTravel.NormalizeItineraryResponse
        {
            Itinerary = new ApiTravel.Itinerary
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.User?.Audit?.UpdatedBy ?? ""),
                },
                ItineraryId = (request.ItineraryId ?? ""),
                Total = (request.Itinerary?.Total ?? new ApiCommon.Money()),
            },
            Outcome = new ApiTravel.NormalizeItineraryResponse.Types.Outcome
            {
                Status = (reply2.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "itinerary",
            "audit", "NormalizeItinerary", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews an itinerary.
    /// </summary>
    public override async Task<ApiTravel.PreviewItineraryResponse> PreviewItinerary(ApiTravel.PreviewItineraryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PreviewItinerary for {Tenant}", requestContext.TenantId);
        if (request.Itinerary is { } record)
        {
            var snapshot = ItineraryMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.ExplainTravelBookingDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.ExplainTravelBookingDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                Bank = new ApiTravel.TravelBookingBankSettlement
                {
                    AccountHolder = (request.Itinerary ?? new ApiTravel.Itinerary()).PaymentCase switch
                    {
                        ApiTravel.Itinerary.SettlementOneofCase.Bank => (request.Itinerary?.Bank?.AccountHolder ?? ""),
                        _ => ""
                    },
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Itinerary?.BillingAddress?.PostalCode ?? ""),
                },
            },
        };
        using var rpc0 = _travelTravelBookingValidationService.ExplainTravelBookingDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.QuoteTravelBookingRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.QuoteTravelBookingRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.TravelBooking?.BillingAddress?.CountryCode ?? ""),
                },
            },
        };
        var command1 = new DispatchCommand<ApiTravel.QuoteTravelBookingRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _travelTravelBookingCalculationService.QuoteTravelBookingAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var profile2 = new MappingProfile<ApiTravel.PreviewItineraryRequest, ApiTravel.RecalculateTravelBookingsRequest>(value => new ApiTravel.RecalculateTravelBookingsRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.RecalculateTravelBookingsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (reply1.TravelBooking?.BillingAddress?.Locality ?? ""),
                },
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (value.Itinerary?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Locality = (reply0.TravelBooking?.BillingAddress?.Locality ?? ""),
                    Recipient = (reply1.TravelBooking?.BillingAddress?.Recipient ?? ""),
                },
            },
        });
        var input2 = profile2.Map(request);
        using var rpc2 = _travelTravelBookingCalculationService.RecalculateTravelBookingsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiTravel.PreviewItineraryResponse
        {
            Itinerary = new ApiTravel.Itinerary
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply2.TravelBooking?.Audit?.Etag ?? ""),
                    UpdatedBy = (reply1.TravelBooking?.Audit?.UpdatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.TravelBooking?.BillingAddress?.CountryCode ?? ""),
                    Lines = { (reply2.TravelBooking?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                ItineraryId = (request.ItineraryId ?? ""),
                Number = (request.Itinerary?.Number ?? ""),
            },
            Outcome = new ApiTravel.PreviewItineraryResponse.Types.Outcome
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "itinerary",
            "audit", "PreviewItinerary", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores an itinerary.
    /// </summary>
    public override async Task<ApiTravel.ScoreItineraryResponse> ScoreItinerary(ApiTravel.ScoreItineraryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScoreItinerary for {Tenant}", requestContext.TenantId);
        if (request.Itinerary is { } record)
        {
            var snapshot = ItineraryMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.ListDocumentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentsRequest.Types.Filter
            {
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _documentsDocumentService.ListDocumentsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.ScoreItineraryResponse
        {
            Itinerary = new ApiTravel.Itinerary
            {
                Counterparty = (request.Itinerary?.Counterparty ?? new ApiTravel.TravelBookingRef()),
                ItineraryId = (request.ItineraryId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "itinerary",
            "audit", "ScoreItinerary", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates an itinerary.
    /// </summary>
    public override async Task<ApiTravel.ValidateItineraryResponse> ValidateItinerary(ApiTravel.ValidateItineraryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ValidateItinerary for {Tenant}", requestContext.TenantId);
        if (request.Itinerary is { } record)
        {
            var snapshot = ItineraryMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.NormalizeTravelBookingRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TravelBooking = new ApiTravel.TravelBooking
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Itinerary?.BillingAddress?.PostalCode ?? ""),
                },
                Card = new ApiTravel.TravelBookingCardSettlement
                {
                    HolderName = (request.Itinerary ?? new ApiTravel.Itinerary()).PaymentCase switch
                    {
                        ApiTravel.Itinerary.SettlementOneofCase.Card => (request.Itinerary?.Card?.HolderName ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _travelTravelBookingValidationService.NormalizeTravelBookingAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.StreamDocumentFolderExportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.StreamDocumentFolderExportRequest.Types.Filter
            {
                CreatedAfter = (request.Itinerary?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (reply0.Result?.CorrelationId ?? ""),
                Region = (request.Itinerary?.BillingAddress?.Region ?? ""),
            },
        };
        using var stream1 = _documentsDocumentFolderExportService.StreamDocumentFolderExport(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamDocumentFolderExport frame {Type}", change1.GetType().Name);
        }
        var response = new ApiTravel.ValidateItineraryResponse
        {
            Itinerary = new ApiTravel.Itinerary
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.TravelBooking?.Audit?.UpdatedBy ?? ""),
                },
                ItineraryId = (request.ItineraryId ?? ""),
                Status = (request.Itinerary?.Status ?? (ApiTravel.ItineraryStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "itinerary",
            "audit", "ValidateItinerary", (request.Reason ?? ""), token);
        return response;
    }
}
