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
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiFeaturestore = Acme.Featurestore.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

public sealed partial class TravelPolicyCalculationEndpoint
{
    /// <summary>
    /// Calculates a travel policy.
    /// </summary>
    public override async Task<ApiTravel.CalculateTravelPolicyResponse> CalculateTravelPolicy(ApiTravel.CalculateTravelPolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CalculateTravelPolicy for {Tenant}", requestContext.TenantId);
        if (request.TravelPolicy is { } record)
        {
            var snapshot = TravelPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.SimulateTravelBookingRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TravelBooking = new ApiTravel.TravelBooking
            {
                Lines = { new ApiTravel.TravelBookingLine
                {
                    Amount = (request.TravelPolicy ?? new ApiTravel.TravelPolicy()).ActionCase switch
                    {
                        ApiTravel.TravelPolicy.ActionOneofCase.Discount => (request.TravelPolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                        _ => new ApiCommon.Money()
                    },
                } },
            },
        };
        using var rpc0 = _travelTravelBookingCalculationService.SimulateTravelBookingAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.SimulateTripRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Trip = new ApiTravel.Trip
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.TravelPolicy?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Lines = { new ApiTravel.TripLine
                {
                    Tax = new ApiTravel.TripLineTax
                    {
                        Amount = (request.TravelPolicy ?? new ApiTravel.TravelPolicy()).ActionCase switch
                        {
                            ApiTravel.TravelPolicy.ActionOneofCase.Discount => (request.TravelPolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc1 = _travelTripCalculationService.SimulateTripAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiTravel.ExplainTripCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Trip = new ApiTravel.Trip
            {
                Card = new ApiTravel.TripCardSettlement
                {
                    Token = (reply1.Trip ?? new ApiTravel.Trip()).PaymentCase switch
                    {
                        ApiTravel.Trip.SettlementOneofCase.Card => (reply1.Trip?.Card?.Token ?? ""),
                        _ => ""
                    },
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (reply0.TravelBooking?.Counterparty?.Email ?? ""),
                },
            },
        };
        using var rpc2 = _travelTripCalculationService.ExplainTripCalculationAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiTravel.CalculateTravelPolicyResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.Trip?.Audit?.CreatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.TravelBooking?.Counterparty?.Email ?? ""),
                },
                TravelPolicyId = (request.TravelPolicyId ?? ""),
                Version = (request.TravelPolicy?.Version ?? 0L),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelPolicy?.TravelPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "CalculateTravelPolicy", (request.TravelPolicy?.TravelPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares a travel policy.
    /// </summary>
    public override async Task<ApiTravel.CompareTravelPolicyOptionsResponse> CompareTravelPolicyOptions(ApiTravel.CompareTravelPolicyOptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CompareTravelPolicyOptions for {Tenant}", requestContext.TenantId);
        if (request.TravelPolicy is { } record)
        {
            var snapshot = TravelPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.CompareTravelPolicyOptionsToPreviewProblem0();
        using var rpc0 = _itsmProblemValidationService.PreviewProblemAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.SimulateTravelBookingRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TravelBooking = new ApiTravel.TravelBooking
            {
                Lines = { new ApiTravel.TravelBookingLine
                {
                    Tax = new ApiTravel.TravelBookingLineTax
                    {
                        Amount = (request.TravelPolicy ?? new ApiTravel.TravelPolicy()).ActionCase switch
                        {
                            ApiTravel.TravelPolicy.ActionOneofCase.Discount => (request.TravelPolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc1 = _travelTravelBookingCalculationService.SimulateTravelBookingAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiTravel.ExplainTravelBookingCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TravelBooking = new ApiTravel.TravelBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.TravelBooking?.Audit?.Etag ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply1.TravelBooking?.BillingAddress?.CountryCode ?? ""),
                },
                Card = new ApiTravel.TravelBookingCardSettlement
                {
                    Last4 = (reply1.TravelBooking ?? new ApiTravel.TravelBooking()).PaymentCase switch
                    {
                        ApiTravel.TravelBooking.SettlementOneofCase.Card => (reply1.TravelBooking?.Card?.Last4 ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc2 = _travelTravelBookingCalculationService.ExplainTravelBookingCalculationAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiTravel.QuoteTravelBookingRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TravelBooking = new ApiTravel.TravelBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.TravelPolicy?.Audit?.UpdatedBy ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply2.TravelBooking?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        var packed3 = Any.Pack(input3);
        input3 = packed3.Unpack<ApiTravel.QuoteTravelBookingRequest>();
        using var rpc3 = _travelTravelBookingCalculationService.QuoteTravelBookingAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiTravel.CompareTravelPolicyOptionsResponse
        {
            Breakdown = new ApiTravel.CompareTravelPolicyOptionsResponse.Types.Breakdown
            {
                Total = (reply3.TravelBooking?.Total ?? new ApiCommon.Money()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply2.Result?.Status ?? ""),
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Effective = (request.TravelPolicy?.Effective ?? new ApiCommon.TimeWindow()),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.TravelBooking?.Counterparty?.DisplayName ?? ""),
                    Email = (reply0.Problem?.Customer?.Email ?? ""),
                },
                TravelPolicyId = (request.TravelPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelPolicy?.TravelPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "CompareTravelPolicyOptions", (request.TravelPolicy?.TravelPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates a travel policy.
    /// </summary>
    public override async Task<ApiTravel.EstimateTravelPolicyResponse> EstimateTravelPolicy(ApiTravel.EstimateTravelPolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EstimateTravelPolicy for {Tenant}", requestContext.TenantId);
        if (request.TravelPolicy is { } record)
        {
            var snapshot = TravelPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.CreateIncidentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Incident = new ApiItsm.Incident
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (request.TravelPolicy?.Owner?.UserId ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.TravelPolicy?.Audit?.UpdatedBy ?? ""),
                },
                Customer = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.TravelPolicy?.Owner?.DisplayName ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiItsm.CreateIncidentRequest>(json0);
        using var rpc0 = _itsmIncidentService.CreateIncidentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.EstimateTravelPolicyResponse
        {
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Incident?.Assignee?.DisplayName ?? ""),
                    Email = (reply0.Incident?.Assignee?.Email ?? ""),
                },
                Priority = (request.TravelPolicy?.Priority ?? 0),
                TravelPolicyId = (request.TravelPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelPolicy?.TravelPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "EstimateTravelPolicy", (request.TravelPolicy?.TravelPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a travel policy.
    /// </summary>
    public override async Task<ApiTravel.ExplainTravelPolicyCalculationResponse> ExplainTravelPolicyCalculation(ApiTravel.ExplainTravelPolicyCalculationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainTravelPolicyCalculation for {Tenant}", requestContext.TenantId);
        if (request.TravelPolicy is { } record)
        {
            var snapshot = TravelPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.ExplainTravelBookingCalculationRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Details = new ApiTravel.ExplainTravelBookingCalculationRequest.Types.Details
        {
            Contact = new ApiCommon.ContactInfo
            {
                Email = (request.TravelPolicy?.Owner?.Email ?? ""),
                Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
            },
        };
        using var rpc0 = _travelTravelBookingCalculationService.ExplainTravelBookingCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.ExplainTravelPolicyCalculationResponse
        {
            Outcome = new ApiTravel.ExplainTravelPolicyCalculationResponse.Types.Outcome
            {
                EffectiveAt = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.TravelBooking?.Counterparty?.DisplayName ?? ""),
                },
                Priority = (request.TravelPolicy?.Priority ?? 0),
                TravelPolicyId = (request.TravelPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelPolicy?.TravelPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "ExplainTravelPolicyCalculation", (request.TravelPolicy?.TravelPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a travel policy.
    /// </summary>
    public override async Task<ApiTravel.GetTravelPolicyRatesResponse> GetTravelPolicyRates(ApiTravel.GetTravelPolicyRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TravelPolicyId, "travel_policy_id");
        _logger.LogInformation("Handling GetTravelPolicyRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy", request.TravelPolicyId,
            "GetTravelPolicyRates", "accepted", token);
        var input0 = new ApiTravel.CalculateTravelBookingRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TravelBooking = new ApiTravel.TravelBooking
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _travelTravelBookingCalculationService.CalculateTravelBookingAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.GetTravelPolicyRatesResponse
        {
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.TravelBooking?.Counterparty?.Email ?? ""),
                },
                TravelPolicyId = (request.TravelPolicyId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Quotes a travel policy.
    /// </summary>
    public override async Task<ApiTravel.QuoteTravelPolicyResponse> QuoteTravelPolicy(ApiTravel.QuoteTravelPolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling QuoteTravelPolicy for {Tenant}", requestContext.TenantId);
        if (request.TravelPolicy is { } record)
        {
            var snapshot = TravelPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiFeatureflags.BrowseFeatureFlagsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiFeatureflags.BrowseFeatureFlagsRequest.Types.Filter
            {
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _featureflagsFeatureFlagQueryService.BrowseFeatureFlagsAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiFeaturestore.CountFeatureViewsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiFeaturestore.CountFeatureViewsRequest.Types.Filter
            {
                CreatedAfter = (request.TravelPolicy?.Audit?.CreatedAt ?? new Timestamp()),
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _featurestoreFeatureViewQueryService.CountFeatureViewsAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiTravel.QuoteTravelPolicyResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply1.Result?.Message ?? ""),
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Parameters = { (request.TravelPolicy?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                TravelPolicyId = (request.TravelPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelPolicy?.TravelPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "QuoteTravelPolicy", (request.TravelPolicy?.TravelPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalculates a travel policy.
    /// </summary>
    public override async Task<ApiTravel.RecalculateTravelPoliciesResponse> RecalculateTravelPolicies(ApiTravel.RecalculateTravelPoliciesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecalculateTravelPolicies for {Tenant}", requestContext.TenantId);
        if (request.TravelPolicy is { } record)
        {
            var snapshot = TravelPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.EstimateTravelBookingRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TravelBooking = new ApiTravel.TravelBooking
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (request.Details?.Destination?.Locality ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _travelTravelBookingCalculationService.EstimateTravelBookingAsync);
        var response = new ApiTravel.RecalculateTravelPoliciesResponse
        {
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.TravelBooking?.Counterparty?.Email ?? ""),
                },
                Priority = (request.TravelPolicy?.Priority ?? 0),
                TravelPolicyId = (request.TravelPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelPolicy?.TravelPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "RecalculateTravelPolicies", (request.TravelPolicy?.TravelPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Simulates a travel policy.
    /// </summary>
    public override async Task<ApiTravel.SimulateTravelPolicyResponse> SimulateTravelPolicy(ApiTravel.SimulateTravelPolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SimulateTravelPolicy for {Tenant}", requestContext.TenantId);
        if (request.TravelPolicy is { } record)
        {
            var snapshot = TravelPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.BrowseRegistrationsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiCorporateevents.BrowseRegistrationsRequest.Types.Filter
            {
                CreatedAfter = (request.TravelPolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        using var rpc0 = _sendSimulateTravelPolicy0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.RevertDocumentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.RevertDocumentRequest.Types.Details
            {
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Document = new ApiDocuments.Document
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.TravelPolicy?.Audit?.Etag ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
                Counterparty = new ApiDocuments.DocumentFolderRef
                {
                    DisplayName = (request.TravelPolicy?.Owner?.DisplayName ?? ""),
                },
                Lines = { new ApiDocuments.DocumentLine
                {
                    Description = (request.TravelPolicy?.Description ?? ""),
                } },
            },
        };
        using var rpc1 = _documentsDocumentHistoryService.RevertDocumentAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiTravel.SimulateTravelBookingRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TravelBooking = new ApiTravel.TravelBooking
            {
                Card = new ApiTravel.TravelBookingCardSettlement
                {
                    HolderName = (reply1.Document ?? new ApiDocuments.Document()).PaymentCase switch
                    {
                        ApiDocuments.Document.SettlementOneofCase.Card => (reply1.Document?.Card?.HolderName ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc2 = _travelTravelBookingCalculationService.SimulateTravelBookingAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiTravel.CalculateTravelBookingRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.CalculateTravelBookingRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (reply2.TravelBooking?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply2.TravelBooking?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Bank = new ApiTravel.TravelBookingBankSettlement
                {
                    AccountHolder = (reply2.TravelBooking ?? new ApiTravel.TravelBooking()).PaymentCase switch
                    {
                        ApiTravel.TravelBooking.SettlementOneofCase.Bank => (reply2.TravelBooking?.Bank?.AccountHolder ?? ""),
                        _ => ""
                    },
                    Bic = (reply2.TravelBooking ?? new ApiTravel.TravelBooking()).PaymentCase switch
                    {
                        ApiTravel.TravelBooking.SettlementOneofCase.Bank => (reply2.TravelBooking?.Bank?.Bic ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc3 = _travelTravelBookingCalculationService.CalculateTravelBookingAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiTravel.SimulateTravelPolicyResponse
        {
            Breakdown = new ApiTravel.SimulateTravelPolicyResponse.Types.Breakdown
            {
                Total = (reply2.Breakdown?.Total ?? new ApiCommon.Money()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply2.TravelBooking?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (reply3.TravelBooking?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Parameters = { (request.TravelPolicy?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                TravelPolicyId = (request.TravelPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelPolicy?.TravelPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "SimulateTravelPolicy", (request.TravelPolicy?.TravelPolicyId ?? ""), token);
        return response;
    }
}
