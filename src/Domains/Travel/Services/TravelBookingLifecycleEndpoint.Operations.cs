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
using ApiFeaturestore = Acme.Featurestore.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

public sealed partial class TravelBookingLifecycleEndpoint
{
    /// <summary>
    /// Activates a travel booking.
    /// </summary>
    public override async Task<ApiTravel.ActivateTravelBookingResponse> ActivateTravelBooking(ApiTravel.ActivateTravelBookingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TravelBookingId, "travel_booking_id");
        _logger.LogInformation("Handling ActivateTravelBooking for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking", request.TravelBookingId,
            "ActivateTravelBooking", "accepted", token);
        var input0 = new ApiDocuments.NotifyDocumentFolderWatchersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.NotifyDocumentFolderWatchersRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc0 = _documentsDocumentFolderNotificationService.NotifyDocumentFolderWatchersAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.ActivateTravelBookingResponse
        {
            Outcome = new ApiTravel.ActivateTravelBookingResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.DocumentFolder?.Location?.CountryCode ?? ""),
                },
                TravelBookingId = (request.TravelBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "ActivateTravelBooking", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Expires a travel booking.
    /// </summary>
    public override async Task<ApiTravel.ExpireTravelBookingResponse> ExpireTravelBooking(ApiTravel.ExpireTravelBookingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TravelBookingId, "travel_booking_id");
        _logger.LogInformation("Handling ExpireTravelBooking for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking", request.TravelBookingId,
            "ExpireTravelBooking", "accepted", token);
        var input0 = new ApiEmployees.ResolveEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _employeesEmployeeQueryService.ResolveEmployeesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.ExpireTravelBookingResponse
        {
            Outcome = new ApiTravel.ExpireTravelBookingResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                TravelBookingId = (request.TravelBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "ExpireTravelBooking", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a travel booking.
    /// </summary>
    public override async Task<ApiTravel.GetTravelBookingLifecycleResponse> GetTravelBookingLifecycle(ApiTravel.GetTravelBookingLifecycleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TravelBookingId, "travel_booking_id");
        _logger.LogInformation("Handling GetTravelBookingLifecycle for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking", request.TravelBookingId,
            "GetTravelBookingLifecycle", "accepted", token);
        var input0 = request.GetTravelBookingLifecycleToScoreTrip0();
        using var rpc0 = _travelTripValidationService.ScoreTripAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.GetTravelBookingLifecycleResponse
        {
            TravelBooking = new ApiTravel.TravelBooking
            {
                IssuedAt = (reply0.Trip?.IssuedAt ?? new Timestamp()),
                Total = (reply0.Trip?.Total ?? new ApiCommon.Money()),
                TravelBookingId = (request.TravelBookingId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Reinstates a travel booking.
    /// </summary>
    public override async Task<ApiTravel.ReinstateTravelBookingResponse> ReinstateTravelBooking(ApiTravel.ReinstateTravelBookingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TravelBookingId, "travel_booking_id");
        _logger.LogInformation("Handling ReinstateTravelBooking for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking", request.TravelBookingId,
            "ReinstateTravelBooking", "accepted", token);
        var input0 = new ApiLicensing.ApproveLicenseAssignmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.ApproveLicenseAssignmentRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        var reply0 = await SendReinstateTravelBooking0Async(input0, token);
        var input1 = new ApiTravel.PushTravelPolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.PushTravelPolicyRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.LicenseAssignment?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
            },
        };
        using var rpc1 = _travelTravelPolicySyncService.PushTravelPolicyAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.ReinstateTravelBookingResponse
        {
            Outcome = new ApiTravel.ReinstateTravelBookingResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply1.Result?.Message ?? ""),
                NextSteps = { (reply1.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                BillingAddress = (reply0.LicenseAssignment?.BillingAddress ?? new ApiCommon.PostalAddress()),
                TravelBookingId = (request.TravelBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "ReinstateTravelBooking", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Renews a travel booking.
    /// </summary>
    public override async Task<ApiTravel.RenewTravelBookingResponse> RenewTravelBooking(ApiTravel.RenewTravelBookingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TravelBookingId, "travel_booking_id");
        _logger.LogInformation("Handling RenewTravelBooking for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking", request.TravelBookingId,
            "RenewTravelBooking", "accepted", token);
        var input0 = new ApiTravel.ScoreTravelBookingRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
            TravelBooking = new ApiTravel.TravelBooking
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
                TravelBookingId = (request.TravelBookingId ?? ""),
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiTravel.ScoreTravelBookingRequest>();
        using var rpc0 = _travelTravelBookingValidationService.ScoreTravelBookingAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.StreamTravelBookingMetricsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (reply0.TravelBooking?.Counterparty?.Email ?? ""),
            },
        };
        using var stream1 = _travelTravelBookingReportingService.StreamTravelBookingMetrics(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamTravelBookingMetrics frame {Type}", change1.GetType().Name);
        }
        var response = new ApiTravel.RenewTravelBookingResponse
        {
            Outcome = new ApiTravel.RenewTravelBookingResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.TravelBooking?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (reply0.TravelBooking?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                TravelBookingId = (request.TravelBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.ReasonCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "RenewTravelBooking", (request.Details?.ReasonCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules a travel booking.
    /// </summary>
    public override async Task<ApiTravel.ScheduleTravelBookingExpiryResponse> ScheduleTravelBookingExpiry(ApiTravel.ScheduleTravelBookingExpiryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScheduleTravelBookingExpiry for {Tenant}", requestContext.TenantId);
        if (request.TravelBooking is { } record)
        {
            var snapshot = TravelBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.SendDocumentTemplateTestNoticeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SendDocumentTemplateTestNoticeRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _documentsDocumentTemplateNotificationService.SendDocumentTemplateTestNoticeAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.PushDocumentTemplateRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.PushDocumentTemplateRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Bank = new ApiDocuments.DocumentTemplateBankSettlement
                {
                    Bic = (request.TravelBooking ?? new ApiTravel.TravelBooking()).PaymentCase switch
                    {
                        ApiTravel.TravelBooking.SettlementOneofCase.Bank => (request.TravelBooking?.Bank?.Bic ?? ""),
                        _ => ""
                    },
                    Iban = (reply0.DocumentTemplate ?? new ApiDocuments.DocumentTemplate()).PaymentCase switch
                    {
                        ApiDocuments.DocumentTemplate.SettlementOneofCase.Bank => (reply0.DocumentTemplate?.Bank?.Iban ?? ""),
                        _ => ""
                    },
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.TravelBooking?.BillingAddress?.PostalCode ?? ""),
                },
            },
        };
        using var rpc1 = _documentsDocumentTemplateSyncService.PushDocumentTemplateAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.ScheduleTravelBookingExpiryResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply1.DocumentTemplate?.BillingAddress?.Locality ?? ""),
                },
                Number = (reply0.DocumentTemplate?.Number ?? ""),
                Status = (request.TravelBooking?.Status ?? (ApiTravel.TravelBookingStatus)0),
                TravelBookingId = (request.TravelBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelBooking?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "ScheduleTravelBookingExpiry", (request.TravelBooking?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Suspends a travel booking.
    /// </summary>
    public override async Task<ApiTravel.SuspendTravelBookingResponse> SuspendTravelBooking(ApiTravel.SuspendTravelBookingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TravelBookingId, "travel_booking_id");
        _logger.LogInformation("Handling SuspendTravelBooking for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking", request.TravelBookingId,
            "SuspendTravelBooking", "accepted", token);
        var input0 = new ApiTravel.CountTravelPolicyTotalsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiTravel.CountTravelPolicyTotalsRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Details?.NewOwner?.UserId ?? ""),
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _travelTravelPolicyReportingService.CountTravelPolicyTotalsAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiCorporateevents.ApproveVenueRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.ApproveVenueRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
            Venue = new ApiCorporateevents.Venue
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
            },
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _corporateeventsVenueApprovalService.ApproveVenueAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiDocuments.ResolveDocumentTemplateConflictRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ResolveDocumentTemplateConflictRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.Venue?.Audit?.Etag ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply1.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                    {
                        ApiCorporateevents.Venue.LocatorOneofCase.Address => (reply1.Venue?.Address?.CountryCode ?? ""),
                        _ => ""
                    },
                },
                Lines = { new ApiDocuments.DocumentTemplateLine
                {
                    Description = (reply1.Venue?.Components?.GetValueOrDefault("primary")?.Description ?? ""),
                    Quantity = (reply1.Venue?.Components?.GetValueOrDefault("primary")?.Quantity ?? 0L),
                } },
            },
        };
        using var rpc2 = _documentsDocumentTemplateSyncService.ResolveDocumentTemplateConflictAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiTravel.SuspendTravelBookingResponse
        {
            Outcome = new ApiTravel.SuspendTravelBookingResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply2.Outcome?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.Venue?.Audit?.CreatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (reply2.DocumentTemplate?.BillingAddress?.Recipient ?? ""),
                },
                TravelBookingId = (request.TravelBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "SuspendTravelBooking", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Terminates a travel booking.
    /// </summary>
    public override async Task<ApiTravel.TerminateTravelBookingResponse> TerminateTravelBooking(ApiTravel.TerminateTravelBookingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TravelBookingId, "travel_booking_id");
        _logger.LogInformation("Handling TerminateTravelBooking for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking", request.TravelBookingId,
            "TerminateTravelBooking", "accepted", token);
        var input0 = new ApiFeaturestore.SimulateFeatureViewRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            FeatureView = new ApiFeaturestore.FeatureView
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
            },
        };
        using var rpc0 = _sendTerminateTravelBooking0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.ResumeServiceAssetWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.FeatureView?.Audit?.CreatedBy ?? ""),
                    UpdatedBy = (reply0.FeatureView?.Audit?.UpdatedBy ?? ""),
                },
                Components = { { "primary", new ApiItsm.ServiceAssetComponent
                {
                    Description = (reply0.FeatureView?.Components?.GetValueOrDefault("primary")?.Description ?? ""),
                } } },
            },
        };
        var named1 = _registry.Get<ApiItsm.ServiceAssetWorkflowService.ServiceAssetWorkflowServiceClient>("acme.itsm.v1.ServiceAssetWorkflowService");
        using var rpc1 = named1.ResumeServiceAssetWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiTravel.CompareTravelPolicyOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.CompareTravelPolicyOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.FeatureView ?? new ApiFeaturestore.FeatureView()).LocatorCase switch
                    {
                        ApiFeaturestore.FeatureView.LocatorOneofCase.Address => (reply0.FeatureView?.Address?.PostalCode ?? ""),
                        _ => ""
                    },
                    Region = (reply1.ServiceAsset ?? new ApiItsm.ServiceAsset()).LocatorCase switch
                    {
                        ApiItsm.ServiceAsset.LocatorOneofCase.Address => (reply1.ServiceAsset?.Address?.Region ?? ""),
                        _ => ""
                    },
                },
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply1.ServiceAsset?.Owner?.Email ?? ""),
                    UserId = (reply0.FeatureView?.Owner?.UserId ?? ""),
                },
            },
        };
        using var rpc2 = _travelTravelPolicyCalculationService.CompareTravelPolicyOptionsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiEmployees.EstimateEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
        };
        var command3 = new DispatchCommand<ApiEmployees.EstimateEmploymentContractRequest>(input3,
            "primary", requestContext.TenantId);
        var routed3 = command3 with { Region = _settings.Value.Region };
        using var rpc3 = _employeesEmploymentContractCalculationService.EstimateEmploymentContractAsync(routed3.Payload, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiTravel.TerminateTravelBookingResponse
        {
            Outcome = new ApiTravel.TerminateTravelBookingResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply3.EmploymentContract?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (reply2.TravelPolicy?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (reply0.FeatureView?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Lines = { new ApiTravel.TravelBookingLine
                {
                    Description = (reply1.ServiceAsset?.Components?.GetValueOrDefault("primary")?.Description ?? ""),
                } },
                Total = (reply0.Breakdown?.Total ?? new ApiCommon.Money()),
                TravelBookingId = (request.TravelBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "TerminateTravelBooking", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiLicensing.ApproveLicenseAssignmentResponse> SendReinstateTravelBooking0Async(ApiLicensing.ApproveLicenseAssignmentRequest request, CancellationToken token)
    {
        return await _travelBookingLifecycleEndpointSendReinstateTravelBooking0Port.SendAsync(request, token);
    }
}
