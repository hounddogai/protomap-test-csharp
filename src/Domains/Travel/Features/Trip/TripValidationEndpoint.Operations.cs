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
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

public sealed partial class TripValidationEndpoint
{
    /// <summary>
    /// Checks a trip.
    /// </summary>
    public override async Task<ApiTravel.CheckTripEligibilityResponse> CheckTripEligibility(ApiTravel.CheckTripEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CheckTripEligibility for {Tenant}", requestContext.TenantId);
        if (request.Trip is { } record)
        {
            var snapshot = TripMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.SimulateTripRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.SimulateTripRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Trip?.BillingAddress?.Region ?? ""),
                },
            },
            Trip = new ApiTravel.Trip
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Trip?.Audit?.Etag ?? ""),
                    UpdatedAt = (request.Trip?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _travelTripCalculationService.SimulateTripAsync);
        var input1 = new ApiTravel.QuoteTripRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Trip = new ApiTravel.Trip
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.Trip?.Audit?.UpdatedBy ?? ""),
                },
                Bank = new ApiTravel.TripBankSettlement
                {
                    Bic = (reply0.Trip ?? new ApiTravel.Trip()).PaymentCase switch
                    {
                        ApiTravel.Trip.SettlementOneofCase.Bank => (reply0.Trip?.Bank?.Bic ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _travelTripCalculationService.QuoteTripAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.CheckTripEligibilityResponse
        {
            Trip = new ApiTravel.Trip
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.Trip?.BillingAddress?.Locality ?? ""),
                },
                IssuedAt = (reply1.Trip?.IssuedAt ?? new Timestamp()),
                Number = (request.Trip?.Number ?? ""),
                TripId = (request.TripId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Trip?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "CheckTripEligibility", (request.Trip?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a trip.
    /// </summary>
    public override async Task<ApiTravel.ExplainTripDecisionResponse> ExplainTripDecision(ApiTravel.ExplainTripDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainTripDecision for {Tenant}", requestContext.TenantId);
        if (request.Trip is { } record)
        {
            var snapshot = TripMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.ExplainTripCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Trip = new ApiTravel.Trip
            {
                Card = new ApiTravel.TripCardSettlement
                {
                    Last4 = (request.Trip ?? new ApiTravel.Trip()).PaymentCase switch
                    {
                        ApiTravel.Trip.SettlementOneofCase.Card => (request.Trip?.Card?.Last4 ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _travelTripCalculationService.ExplainTripCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var reply1 = new ApiTravel.EstimateTripResponse();
        foreach (var item in (request.Trip?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiTravel.TripLine>()))
        {
            var input1 = new ApiTravel.EstimateTripRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Trip = new ApiTravel.Trip
                {
                    Lines = { new ApiTravel.TripLine
                    {
                        Amount = (item.Tax?.Amount ?? new ApiCommon.Money()),
                        Tax = new ApiTravel.TripLineTax
                        {
                            Jurisdiction = (item.Tax?.Jurisdiction ?? ""),
                        },
                    } },
                },
            };
            using var rpc1 = _travelTripCalculationService.EstimateTripAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var input2 = new ApiDevices.ExplainManagedDeviceCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Trip?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        var command2 = new DispatchCommand<ApiDevices.ExplainManagedDeviceCalculationRequest>(input2,
            "primary", requestContext.TenantId);
        var routed2 = command2 with { Region = _settings.Value.Region };
        using var rpc2 = _devicesManagedDeviceCalculationService.ExplainManagedDeviceCalculationAsync(routed2.Payload, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var profile3 = new MappingProfile<ApiTravel.ExplainTripDecisionRequest, ApiTravel.GetTripRatesRequest>(value => new ApiTravel.GetTripRatesRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            TripId = (value.Trip?.TripId ?? ""),
        });
        var input3 = profile3.Map(request);
        using var rpc3 = _travelTripCalculationService.GetTripRatesAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiTravel.ExplainTripDecisionResponse
        {
            Outcome = new ApiTravel.ExplainTripDecisionResponse.Types.Outcome
            {
                EffectiveAt = (reply2.Outcome?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Trip = new ApiTravel.Trip
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.Trip?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    PostalCode = (reply3.Trip?.BillingAddress?.PostalCode ?? ""),
                },
                IssuedAt = (request.Trip?.IssuedAt ?? new Timestamp()),
                Number = (request.Trip?.Number ?? ""),
                TripId = (reply3.Trip?.TripId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TripId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "ExplainTripDecision", (request.TripId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lints a trip.
    /// </summary>
    public override async Task<ApiTravel.LintTripsResponse> LintTrips(ApiTravel.LintTripsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling LintTrips for {Tenant}", requestContext.TenantId);
        if (request.Trip is { } record)
        {
            var snapshot = TripMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.FindCorporateEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiCorporateevents.FindCorporateEventsRequest.Types.Filter
            {
                CreatedAfter = (request.Trip?.Audit?.CreatedAt ?? new Timestamp()),
                Region = (request.Trip?.BillingAddress?.Region ?? ""),
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _corporateeventsCorporateEventQueryService.FindCorporateEventsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.LintTripsToGetTripRates1();
        using var rpc1 = _travelTripCalculationService.GetTripRatesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.LintTripsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Trip = new ApiTravel.Trip
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Trip?.Audit?.CreatedAt ?? new Timestamp()),
                },
                IssuedAt = (request.Trip?.IssuedAt ?? new Timestamp()),
                TripId = (request.TripId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Trip?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "LintTrips", (request.Trip?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Normalizes a trip.
    /// </summary>
    public override async Task<ApiTravel.NormalizeTripResponse> NormalizeTrip(ApiTravel.NormalizeTripRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling NormalizeTrip for {Tenant}", requestContext.TenantId);
        if (request.Trip is { } record)
        {
            var snapshot = TripMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.CalculateManagedDeviceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.CalculateManagedDeviceRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Trip?.BillingAddress?.PostalCode ?? ""),
                },
            },
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Address = new ApiCommon.PostalAddress
                {
                    Locality = (request.Trip?.BillingAddress?.Locality ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Trip?.Audit?.Etag ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
        };
        var reply0 = await SendNormalizeTrip0Async(input0, token);
        var input1 = new ApiTravel.RecalculateTripsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.RecalculateTripsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                    {
                        ApiDevices.ManagedDevice.LocatorOneofCase.Address => (reply0.ManagedDevice?.Address?.CountryCode ?? ""),
                        _ => ""
                    },
                },
            },
            Trip = new ApiTravel.Trip
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                    {
                        ApiDevices.ManagedDevice.LocatorOneofCase.Address => (reply0.ManagedDevice?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()),
                        _ => new Google.Protobuf.Collections.RepeatedField<string>()
                    } },
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _travelTripCalculationService.RecalculateTripsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDocuments.ReconcileDocumentFoldersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ReconcileDocumentFoldersRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            CreatedBy = (reply0.ManagedDevice?.Audit?.CreatedBy ?? ""),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            Locality = (request.Trip?.BillingAddress?.Locality ?? ""),
                        },
                    } },
                } },
            },
        };
        using var rpc2 = _documentsDocumentFolderSyncService.ReconcileDocumentFoldersAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiItsm.LintProblemsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.LintProblemsRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply2.DocumentFolder?.Manager?.Email ?? ""),
                },
            },
            Problem = new ApiItsm.Problem
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Trip?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Customer = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply1.Trip?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        var json3 = JsonFormatter.Default.Format(input3);
        input3 = JsonParser.Default.Parse<ApiItsm.LintProblemsRequest>(json3);
        using var rpc3 = _itsmProblemValidationService.LintProblemsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiTravel.NormalizeTripResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply3.Result?.CorrelationId ?? ""),
                Message = (reply2.Result?.Message ?? ""),
            },
            Trip = new ApiTravel.Trip
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.ManagedDevice?.Audit?.CreatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply1.Trip?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply1.Trip?.Counterparty?.DisplayName ?? ""),
                    Email = (reply2.DocumentFolder?.Manager?.Email ?? ""),
                },
                Metadata = { (request.Trip?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Total = (reply0.Breakdown?.Total ?? new ApiCommon.Money()),
                TripId = (request.TripId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Trip?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "NormalizeTrip", (request.Trip?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews a trip.
    /// </summary>
    public override async Task<ApiTravel.PreviewTripResponse> PreviewTrip(ApiTravel.PreviewTripRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PreviewTrip for {Tenant}", requestContext.TenantId);
        if (request.Trip is { } record)
        {
            var snapshot = TripMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.FindCorporateEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiCorporateevents.FindCorporateEventsRequest.Types.Filter
            {
                CreatedAfter = (request.Trip?.DueAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Trip?.TripId ?? ""),
                Region = (request.Trip?.BillingAddress?.Region ?? ""),
            },
        };
        using var rpc0 = _corporateeventsCorporateEventQueryService.FindCorporateEventsAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.BrowseCorporateEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiCorporateevents.BrowseCorporateEventsRequest.Types.Filter
            {
                CreatedAfter = (request.Trip?.IssuedAt ?? new Timestamp()),
                OwnerId = (request.Trip?.Counterparty?.EmployeeId ?? ""),
                Region = (request.Trip?.BillingAddress?.Region ?? ""),
            },
        };
        using var rpc1 = _corporateeventsCorporateEventQueryService.BrowseCorporateEventsAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiTravel.PreviewTripResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            Trip = new ApiTravel.Trip
            {
                Notes = (request.Trip?.Notes ?? ""),
                TripId = (request.TripId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Trip?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "PreviewTrip", (request.Trip?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores a trip.
    /// </summary>
    public override async Task<ApiTravel.ScoreTripResponse> ScoreTrip(ApiTravel.ScoreTripRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScoreTrip for {Tenant}", requestContext.TenantId);
        if (request.Trip is { } record)
        {
            var snapshot = TripMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.ListSignatureSubscriptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListSignatureSubscriptionsRequest.Types.Filter
            {
                CreatedAfter = (request.Trip?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.Trip?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Region = (request.Trip?.BillingAddress?.Region ?? ""),
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _documentsSignatureNotificationService.ListSignatureSubscriptionsAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiCorporateevents.ResolveCorporateEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _corporateeventsCorporateEventQueryService.ResolveCorporateEventsAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiItsm.ExplainProblemCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Problem = new ApiItsm.Problem
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Trip?.Counterparty?.DisplayName ?? ""),
                    Email = (request.Trip?.Counterparty?.Email ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Trip?.Audit?.Etag ?? ""),
                },
            },
        };
        var reply2 = await Workflow.SendAsync(input2, token, _itsmProblemCalculationService.ExplainProblemCalculationAsync);
        var response = new ApiTravel.ScoreTripResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
            Trip = new ApiTravel.Trip
            {
                Number = (request.Trip?.Number ?? ""),
                TripId = (request.TripId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Trip?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "ScoreTrip", (request.Trip?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates a trip.
    /// </summary>
    public override async Task<ApiTravel.ValidateTripResponse> ValidateTrip(ApiTravel.ValidateTripRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ValidateTrip for {Tenant}", requestContext.TenantId);
        if (request.Trip is { } record)
        {
            var snapshot = TripMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.RecalculateTripsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Trip = new ApiTravel.Trip
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Trip?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _sendValidateTrip0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        foreach (var item in (request.Trip?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiTravel.TripLine>()))
        {
            var input1 = new ApiDocuments.StreamSignatureNoticesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiDocuments.StreamSignatureNoticesRequest.Types.Filter
                {
                    OwnerId = (item.LineId ?? ""),
                },
                Subscriber = new ApiCommon.ContactInfo
                {
                    Email = (request.Trip?.Counterparty?.Email ?? ""),
                    Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
            };
            using var stream1 = _documentsSignatureNotificationService.StreamSignatureNotices(input1, cancellationToken: token);
            await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
            {
                _logger.LogDebug("Received StreamSignatureNotices frame {Type}", change1.GetType().Name);
            }
        }
        var response = new ApiTravel.ValidateTripResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Trip = new ApiTravel.Trip
            {
                Notes = (request.Trip?.Notes ?? ""),
                TripId = (request.TripId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Trip?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "ValidateTrip", (request.Trip?.VoucherCode ?? ""), token);
        return response;
    }
    private async Task<ApiDevices.CalculateManagedDeviceResponse> SendNormalizeTrip0Async(ApiDevices.CalculateManagedDeviceRequest request, CancellationToken token)
    {
        return await _tripValidationEndpointSendNormalizeTrip0Port.SendAsync(request, token);
    }
}
