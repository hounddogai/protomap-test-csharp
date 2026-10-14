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
using ApiAppointments = Acme.Appointments.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiSearch = Acme.Search.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

public sealed partial class TravelBookingValidationEndpoint
{
    /// <summary>
    /// Checks a travel booking.
    /// </summary>
    public override async Task<ApiTravel.CheckTravelBookingEligibilityResponse> CheckTravelBookingEligibility(ApiTravel.CheckTravelBookingEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CheckTravelBookingEligibility for {Tenant}", requestContext.TenantId);
        if (request.TravelBooking is { } record)
        {
            var snapshot = TravelBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.ExportKnowledgeArticlesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiHelpdesk.ExportKnowledgeArticlesRequest.Types.Filter
            {
                CreatedAfter = (request.TravelBooking?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.TravelBooking?.Counterparty?.EmployeeId ?? ""),
                Region = (request.TravelBooking?.BillingAddress?.Region ?? ""),
            },
        };
        using var stream0 = _helpdeskKnowledgeArticleQueryService.ExportKnowledgeArticles(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportKnowledgeArticles frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiTravel.EstimateTravelPolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _travelTravelPolicyCalculationService.EstimateTravelPolicyAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiTravel.CheckTravelBookingEligibilityResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.TravelPolicy?.Audit?.Etag ?? ""),
                },
                Notes = (request.TravelBooking?.Notes ?? ""),
                TravelBookingId = (request.TravelBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelBooking?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "CheckTravelBookingEligibility", (request.TravelBooking?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a travel booking.
    /// </summary>
    public override async Task<ApiTravel.ExplainTravelBookingDecisionResponse> ExplainTravelBookingDecision(ApiTravel.ExplainTravelBookingDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainTravelBookingDecision for {Tenant}", requestContext.TenantId);
        if (request.TravelBooking is { } record)
        {
            var snapshot = TravelBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.CalculateTravelPolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.TravelBooking?.Audit?.CreatedBy ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _travelTravelPolicyCalculationService.CalculateTravelPolicyAsync);
        var response = new ApiTravel.ExplainTravelBookingDecisionResponse
        {
            Outcome = new ApiTravel.ExplainTravelBookingDecisionResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                DueAt = (request.TravelBooking?.DueAt ?? new Timestamp()),
                TravelBookingId = (request.TravelBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelBooking?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "ExplainTravelBookingDecision", (request.TravelBooking?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a travel booking.
    /// </summary>
    public override async Task<ApiTravel.GetTravelBookingRulesResponse> GetTravelBookingRules(ApiTravel.GetTravelBookingRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TravelBookingId, "travel_booking_id");
        _logger.LogInformation("Handling GetTravelBookingRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking", request.TravelBookingId,
            "GetTravelBookingRules", "accepted", token);
        var input0 = new ApiItsm.SuggestIncidentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.SuggestIncidentsRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                Labels = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.TravelBookingId ?? ""),
            },
        };
        using var rpc0 = _sendGetTravelBookingRules0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.GetTravelBookingRulesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                TravelBookingId = (request.TravelBookingId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lints a travel booking.
    /// </summary>
    public override async Task<ApiTravel.LintTravelBookingsResponse> LintTravelBookings(ApiTravel.LintTravelBookingsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling LintTravelBookings for {Tenant}", requestContext.TenantId);
        if (request.TravelBooking is { } record)
        {
            var snapshot = TravelBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.ExplainTravelPolicyCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.ExplainTravelPolicyCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.TravelBooking?.Counterparty?.Email ?? ""),
                },
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.TravelBooking?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        var named0 = _registry.Get<ApiTravel.TravelPolicyCalculationService.TravelPolicyCalculationServiceClient>("acme.travel.v1.TravelPolicyCalculationService");
        using var rpc0 = named0.ExplainTravelPolicyCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiNotifications.LintNotificationsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiNotifications.LintNotificationsRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
            Notification = new ApiNotifications.Notification
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.TravelBooking?.Counterparty?.Email ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.TravelBooking?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Customer = new ApiNotifications.NotificationTemplateRef
                {
                    DisplayName = (request.TravelBooking?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        var descriptor1 = new Method<ApiNotifications.LintNotificationsRequest, ApiNotifications.LintNotificationsResponse>(
            MethodType.Unary, "acme.notifications.v1" + ".NotificationValidationService", "LintNotifications",
            Wire.Marshaller<ApiNotifications.LintNotificationsRequest>(), Wire.Marshaller<ApiNotifications.LintNotificationsResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.LintTravelBookingsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.Notification?.Audit?.Etag ?? ""),
                    UpdatedBy = (reply0.TravelPolicy?.Audit?.UpdatedBy ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply1.Notification?.Customer?.DisplayName ?? ""),
                },
                Metadata = { (request.TravelBooking?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                TravelBookingId = (request.TravelBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelBooking?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "LintTravelBookings", (request.TravelBooking?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Normalizes a travel booking.
    /// </summary>
    public override async Task<ApiTravel.NormalizeTravelBookingResponse> NormalizeTravelBooking(ApiTravel.NormalizeTravelBookingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling NormalizeTravelBooking for {Tenant}", requestContext.TenantId);
        if (request.TravelBooking is { } record)
        {
            var snapshot = TravelBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.EstimateRegistrationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.EstimateRegistrationRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Recipient = (request.TravelBooking?.BillingAddress?.Recipient ?? ""),
                },
            },
            Registration = new ApiCorporateevents.Registration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.TravelBooking?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var command0 = new DispatchCommand<ApiCorporateevents.EstimateRegistrationRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _corporateeventsRegistrationCalculationService.EstimateRegistrationAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiTravel.NormalizeTravelBookingRequest, ApiLicensing.ScoreSoftwareLicenseRequest>(value => new ApiLicensing.ScoreSoftwareLicenseRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.Registration?.Audit?.UpdatedBy ?? ""),
                },
            },
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _licensingSoftwareLicenseValidationService.ScoreSoftwareLicenseAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.NormalizeTravelBookingResponse
        {
            TravelBooking = new ApiTravel.TravelBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.SoftwareLicense?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (reply0.Registration?.Contact?.Email ?? ""),
                },
                Number = (request.TravelBooking?.Number ?? ""),
                TravelBookingId = (request.TravelBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelBooking?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "NormalizeTravelBooking", (request.TravelBooking?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews a travel booking.
    /// </summary>
    public override async Task<ApiTravel.PreviewTravelBookingResponse> PreviewTravelBooking(ApiTravel.PreviewTravelBookingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PreviewTravelBooking for {Tenant}", requestContext.TenantId);
        if (request.TravelBooking is { } record)
        {
            var snapshot = TravelBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.CalculateTravelPolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.CalculateTravelPolicyRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (request.TravelBooking?.BillingAddress?.Locality ?? ""),
                },
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.TravelBooking?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.TravelBooking?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _travelTravelPolicyCalculationService.CalculateTravelPolicyAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var batch1 = (request.TravelBooking?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiTravel.TravelBookingLine>()).Select(async item =>
        {
            var input1 = new ApiLedger.CloneLedgerAccountRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                LedgerAccount = new ApiLedger.LedgerAccount
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (request.TravelBooking?.Audit?.CreatedAt ?? new Timestamp()),
                        UpdatedBy = (reply0.TravelPolicy?.Audit?.UpdatedBy ?? ""),
                    },
                    Limits = { new ApiLedger.LedgerAccountLimit
                    {
                        Amount = (item.Amount ?? new ApiCommon.Money()),
                    } },
                },
            };
            using var rpc1 = _ledgerLedgerAccountService.CloneLedgerAccountAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            return reply1;
        }).ToArray();
        var replies1 = await Task.WhenAll(batch1);
        var reply1 = replies1.LastOrDefault() ?? new ApiLedger.CloneLedgerAccountResponse();
        var response = new ApiTravel.PreviewTravelBookingResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                DueAt = (request.TravelBooking?.DueAt ?? new Timestamp()),
                TravelBookingId = (request.TravelBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelBooking?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "PreviewTravelBooking", (request.TravelBooking?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores a travel booking.
    /// </summary>
    public override async Task<ApiTravel.ScoreTravelBookingResponse> ScoreTravelBooking(ApiTravel.ScoreTravelBookingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScoreTravelBooking for {Tenant}", requestContext.TenantId);
        if (request.TravelBooking is { } record)
        {
            var snapshot = TravelBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.CompareTripOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Trip = new ApiTravel.Trip
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.TravelBooking?.Audit?.CreatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.TravelBooking?.BillingAddress?.PostalCode ?? ""),
                },
            },
        };
        var reply0 = await SendScoreTravelBooking0Async(input0, token);
        var input1 = new ApiSearch.ResolveSearchIndexesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _searchSearchIndexQueryService.ResolveSearchIndexesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.ScoreTravelBookingResponse
        {
            Breakdown = new ApiTravel.ScoreTravelBookingResponse.Types.Breakdown
            {
                Total = (reply0.Breakdown?.Total ?? new ApiCommon.Money()),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Result?.Status ?? ""),
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                Number = (request.TravelBooking?.Number ?? ""),
                TravelBookingId = (request.TravelBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelBooking?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "ScoreTravelBooking", (request.TravelBooking?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates a travel booking.
    /// </summary>
    public override async Task<ApiTravel.ValidateTravelBookingResponse> ValidateTravelBooking(ApiTravel.ValidateTravelBookingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ValidateTravelBooking for {Tenant}", requestContext.TenantId);
        if (request.TravelBooking is { } record)
        {
            var snapshot = TravelBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.CalculateTravelPolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.CalculateTravelPolicyRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.TravelBooking?.BillingAddress?.Region ?? ""),
                },
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.TravelBooking?.Counterparty?.DisplayName ?? ""),
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiTravel.CalculateTravelPolicyRequest>();
        using var rpc0 = _travelTravelPolicyCalculationService.CalculateTravelPolicyAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLedger.ExportLedgerAccountsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLedger.ExportLedgerAccountsRequest.Types.Filter
            {
                Region = (request.TravelBooking?.BillingAddress?.Region ?? ""),
            },
        };
        using var stream1 = _ledgerLedgerAccountQueryService.ExportLedgerAccounts(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportLedgerAccounts frame {Type}", change1.GetType().Name);
        }
        var input2 = new ApiAppointments.BatchGetAppointmentsRequest();
        input2.Context = (request.Context ?? new ApiCommon.RequestContext());
        using var rpc2 = _appointmentsAppointmentService.BatchGetAppointmentsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiSearch.SummarizeSearchIndexesRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiSearch.SummarizeSearchIndexesRequest.Types.Details
            {
                Filters = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Period = (reply0.TravelPolicy?.Effective ?? new ApiCommon.TimeWindow()),
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc3 = _searchSearchIndexQueryService.SummarizeSearchIndexesAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiTravel.ValidateTravelBookingResponse
        {
            Outcome = new ApiTravel.ValidateTravelBookingResponse.Types.Outcome
            {
                Status = (reply2.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply2.Result?.Status ?? ""),
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.TravelPolicy?.Audit?.UpdatedBy ?? ""),
                },
                Number = (request.TravelBooking?.Number ?? ""),
                TravelBookingId = (request.TravelBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelBooking?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "ValidateTravelBooking", (request.TravelBooking?.VoucherCode ?? ""), token);
        return response;
    }
    private async Task<ApiTravel.CompareTripOptionsResponse> SendScoreTravelBooking0Async(ApiTravel.CompareTripOptionsRequest request, CancellationToken token)
    {
        return await _travelBookingValidationEndpointSendScoreTravelBooking0Port.SendAsync(request, token);
    }
}
