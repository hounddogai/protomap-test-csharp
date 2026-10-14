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
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

public sealed partial class VisitorPassCalculationEndpoint
{
    /// <summary>
    /// Calculates a visitor pass.
    /// </summary>
    public override async Task<ApiWorkplaces.CalculateVisitorPassResponse> CalculateVisitorPass(ApiWorkplaces.CalculateVisitorPassRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CalculateVisitorPass for {Tenant}", requestContext.TenantId);
        if (request.VisitorPass is { } record)
        {
            var snapshot = VisitorPassMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.FindTripsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiTravel.FindTripsRequest.Types.Filter
            {
                Region = (request.VisitorPass?.BillingAddress?.Region ?? ""),
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _travelTripQueryService.FindTripsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.CalculateVisitorPassToListFeatureFlags1();
        using var rpc1 = _featureflagsFeatureFlagService.ListFeatureFlagsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiWorkplaces.QuoteDeskRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Desk = new ApiWorkplaces.Desk
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.VisitorPass?.Audit?.Etag ?? ""),
                    UpdatedBy = (request.VisitorPass?.Audit?.UpdatedBy ?? ""),
                },
            },
            Details = new ApiWorkplaces.QuoteDeskRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                },
            },
        };
        var reply2 = await SendCalculateVisitorPass2Async(input2, token);
        var input3 = new ApiWorkplaces.EstimateDeskRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Desk = new ApiWorkplaces.Desk
            {
                Address = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                    Recipient = (request.Details?.Destination?.Recipient ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply2.Desk?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply2.Desk?.Owner?.DisplayName ?? ""),
                },
            },
        };
        var labels3 = input3.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input3.Context.Baggage.Clear();
        input3.Context.Baggage.Add(labels3);
        using var rpc3 = _workplacesDeskCalculationService.EstimateDeskAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiWorkplaces.CalculateVisitorPassResponse
        {
            Breakdown = new ApiWorkplaces.CalculateVisitorPassResponse.Types.Breakdown
            {
                CurrencyCode = (reply2.Breakdown?.CurrencyCode ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply2.Desk?.Audit?.Etag ?? ""),
                },
                BillingAddress = (request.VisitorPass?.BillingAddress ?? new ApiCommon.PostalAddress()),
                VisitorPassId = (request.VisitorPassId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.VisitorPass?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass",
            "audit", "CalculateVisitorPass", (request.VisitorPass?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares a visitor pass.
    /// </summary>
    public override async Task<ApiWorkplaces.CompareVisitorPassOptionsResponse> CompareVisitorPassOptions(ApiWorkplaces.CompareVisitorPassOptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CompareVisitorPassOptions for {Tenant}", requestContext.TenantId);
        if (request.VisitorPass is { } record)
        {
            var snapshot = VisitorPassMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWorkplaces.SimulateDeskRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiWorkplaces.SimulateDeskRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiWorkplaces.SimulateDeskRequest>();
        using var rpc0 = _workplacesDeskCalculationService.SimulateDeskAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var batch1 = (request.VisitorPass?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiWorkplaces.VisitorPassLine>()).Select(async item =>
        {
            var input1 = new ApiWorkplaces.CalculateDeskRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Desk = new ApiWorkplaces.Desk
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedBy = (request.VisitorPass?.Audit?.CreatedBy ?? ""),
                    },
                    Components = { { "primary", new ApiWorkplaces.DeskComponent
                    {
                        Description = (item.Description ?? ""),
                        Quantity = item.Quantity,
                    } } },
                },
                Details = new ApiWorkplaces.CalculateDeskRequest.Types.Details
                {
                    Destination = new ApiCommon.PostalAddress
                    {
                        Recipient = (request.VisitorPass?.BillingAddress?.Recipient ?? ""),
                    },
                },
            };
            using var rpc1 = _workplacesDeskCalculationService.CalculateDeskAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            return reply1;
        }).ToArray();
        var replies1 = await Task.WhenAll(batch1);
        var reply1 = replies1.LastOrDefault() ?? new ApiWorkplaces.CalculateDeskResponse();
        var input2 = new ApiWorkplaces.CompareBuildingOptionsRequest();
        input2.Building = new ApiWorkplaces.Building
        {
            Children = { new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (reply1.Desk?.Audit?.CreatedAt ?? new Timestamp()),
                        CreatedBy = (reply0.Desk?.Audit?.CreatedBy ?? ""),
                    },
                    Manager = new ApiIdentity.UserRef
                    {
                        Email = (request.VisitorPass?.Counterparty?.Email ?? ""),
                    },
                } },
            } },
        };
        input2.Context = (request.Context ?? new ApiCommon.RequestContext());
        using var rpc2 = _workplacesBuildingCalculationService.CompareBuildingOptionsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiWorkplaces.CompareVisitorPassOptionsResponse
        {
            Breakdown = new ApiWorkplaces.CompareVisitorPassOptionsResponse.Types.Breakdown
            {
                Total = (request.VisitorPass?.Total ?? new ApiCommon.Money()),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply2.Building?.Location?.CountryCode ?? ""),
                    Lines = { (reply0.Desk?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                VisitorPassId = (request.VisitorPassId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.VisitorPass?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass",
            "audit", "CompareVisitorPassOptions", (request.VisitorPass?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates a visitor pass.
    /// </summary>
    public override async Task<ApiWorkplaces.EstimateVisitorPassResponse> EstimateVisitorPass(ApiWorkplaces.EstimateVisitorPassRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EstimateVisitorPass for {Tenant}", requestContext.TenantId);
        if (request.VisitorPass is { } record)
        {
            var snapshot = VisitorPassMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWorkplaces.EstimateDeskRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Desk = new ApiWorkplaces.Desk
            {
                Address = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
            Details = new ApiWorkplaces.EstimateDeskRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                },
            },
        };
        using var rpc0 = _workplacesDeskCalculationService.EstimateDeskAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiAppointments.ListAppointmentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiAppointments.ListAppointmentsRequest.Types.Filter
            {
                CreatedAfter = (reply0.Desk?.Audit?.UpdatedAt ?? new Timestamp()),
                Region = (reply0.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                {
                    ApiWorkplaces.Desk.LocatorOneofCase.Address => (reply0.Desk?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc1 = _appointmentsAppointmentService.ListAppointmentsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDocuments.CancelDocumentTemplateWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.VisitorPass?.Audit?.CreatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.VisitorPass?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                    Region = (reply0.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                    {
                        ApiWorkplaces.Desk.LocatorOneofCase.Address => (reply0.Desk?.Address?.Region ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc2 = _documentsDocumentTemplateWorkflowService.CancelDocumentTemplateWorkflowAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiWorkplaces.EstimateVisitorPassResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (reply0.Desk?.Address?.Region ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply0.Desk?.Owner?.DisplayName ?? ""),
                },
                Notes = (request.VisitorPass?.Notes ?? ""),
                VisitorPassId = (request.VisitorPassId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.VisitorPass?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass",
            "audit", "EstimateVisitorPass", (request.VisitorPass?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a visitor pass.
    /// </summary>
    public override async Task<ApiWorkplaces.ExplainVisitorPassCalculationResponse> ExplainVisitorPassCalculation(ApiWorkplaces.ExplainVisitorPassCalculationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainVisitorPassCalculation for {Tenant}", requestContext.TenantId);
        if (request.VisitorPass is { } record)
        {
            var snapshot = VisitorPassMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLedger.WatchLedgerAccountsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLedger.WatchLedgerAccountsRequest.Types.Filter
            {
                Region = (request.VisitorPass?.BillingAddress?.Region ?? ""),
            },
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (request.VisitorPass?.Counterparty?.Email ?? ""),
                Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
            },
        };
        using var stream0 = _ledgerLedgerAccountService.WatchLedgerAccounts(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchLedgerAccounts frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiWorkplaces.ExplainDeskDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Desk = new ApiWorkplaces.Desk
            {
                Address = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.VisitorPass?.BillingAddress?.CountryCode ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.VisitorPass?.Audit?.UpdatedBy ?? ""),
                },
            },
            Details = new ApiWorkplaces.ExplainDeskDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        using var rpc1 = _sendExplainVisitorPassCalculation1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiWorkplaces.LintDesksRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Desk = new ApiWorkplaces.Desk
            {
                Address = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply1.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                    {
                        ApiWorkplaces.Desk.LocatorOneofCase.Address => (reply1.Desk?.Address?.CountryCode ?? ""),
                        _ => ""
                    },
                    PostalCode = (reply1.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                    {
                        ApiWorkplaces.Desk.LocatorOneofCase.Address => (reply1.Desk?.Address?.PostalCode ?? ""),
                        _ => ""
                    },
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.Desk?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        using var rpc2 = _workplacesDeskValidationService.LintDesksAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiWorkplaces.ExplainDeskCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Desk = new ApiWorkplaces.Desk
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply2.Desk?.Audit?.Etag ?? ""),
                },
                Dimensions = new ApiWorkplaces.DeskDimensions
                {
                    Unit = (reply1.Desk?.Dimensions?.Unit ?? ""),
                },
            },
        };
        var descriptor3 = new Method<ApiWorkplaces.ExplainDeskCalculationRequest, ApiWorkplaces.ExplainDeskCalculationResponse>(
            MethodType.Unary, "acme.workplaces.v1" + ".DeskCalculationService", "ExplainDeskCalculation",
            Wire.Marshaller<ApiWorkplaces.ExplainDeskCalculationRequest>(), Wire.Marshaller<ApiWorkplaces.ExplainDeskCalculationResponse>());
        using var rpc3 = _invoker.AsyncUnaryCall(descriptor3, null,
            new CallOptions(cancellationToken: token), input3);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiWorkplaces.ExplainVisitorPassCalculationResponse
        {
            Outcome = new ApiWorkplaces.ExplainVisitorPassCalculationResponse.Types.Outcome
            {
                EffectiveAt = (reply1.Outcome?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply2.Outcome?.Message ?? ""),
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (reply2.Desk?.Address?.Recipient ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply3.Desk?.Owner?.DisplayName ?? ""),
                },
                Metadata = { (request.VisitorPass?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                VisitorPassId = (request.VisitorPassId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.VisitorPass?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass",
            "audit", "ExplainVisitorPassCalculation", (request.VisitorPass?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a visitor pass.
    /// </summary>
    public override async Task<ApiWorkplaces.GetVisitorPassRatesResponse> GetVisitorPassRates(ApiWorkplaces.GetVisitorPassRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.VisitorPassId, "visitor_pass_id");
        _logger.LogInformation("Handling GetVisitorPassRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass", request.VisitorPassId,
            "GetVisitorPassRates", "accepted", token);
        var input0 = new ApiTravel.ResolveTripsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var command0 = new DispatchCommand<ApiTravel.ResolveTripsRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _travelTripQueryService.ResolveTripsAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiWorkplaces.GetVisitorPassRatesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                VisitorPassId = (request.VisitorPassId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Quotes a visitor pass.
    /// </summary>
    public override async Task<ApiWorkplaces.QuoteVisitorPassResponse> QuoteVisitorPass(ApiWorkplaces.QuoteVisitorPassRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling QuoteVisitorPass for {Tenant}", requestContext.TenantId);
        if (request.VisitorPass is { } record)
        {
            var snapshot = VisitorPassMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiWorkplaces.QuoteVisitorPassRequest, ApiWorkplaces.EstimateRoomBookingRequest>(value => new ApiWorkplaces.EstimateRoomBookingRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Participants = { new ApiWorkplaces.RoomBookingParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (value.VisitorPass?.Counterparty?.Email ?? ""),
                    },
                } },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _workplacesRoomBookingCalculationService.EstimateRoomBookingAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiWorkplaces.QuoteVisitorPassResponse
        {
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.RoomBooking?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                Metadata = { (request.VisitorPass?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                VisitorPassId = (request.VisitorPassId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.VisitorPass?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass",
            "audit", "QuoteVisitorPass", (request.VisitorPass?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalculates a visitor pass.
    /// </summary>
    public override async Task<ApiWorkplaces.RecalculateVisitorPassesResponse> RecalculateVisitorPasses(ApiWorkplaces.RecalculateVisitorPassesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecalculateVisitorPasses for {Tenant}", requestContext.TenantId);
        if (request.VisitorPass is { } record)
        {
            var snapshot = VisitorPassMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.EstimateVenueRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.EstimateVenueRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.VisitorPass?.BillingAddress?.CountryCode ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _corporateeventsVenueCalculationService.EstimateVenueAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.RecalculateVisitorPassesToPreviewBuilding1();
        using var rpc1 = _workplacesBuildingValidationService.PreviewBuildingAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiWorkplaces.CheckBuildingEligibilityRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            CreatedAt = (reply0.Venue?.Audit?.CreatedAt ?? new Timestamp()),
                            UpdatedAt = (reply1.Building?.Audit?.UpdatedAt ?? new Timestamp()),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            Recipient = (request.VisitorPass?.BillingAddress?.Recipient ?? ""),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (request.VisitorPass?.Counterparty?.DisplayName ?? ""),
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply2 = await SendRecalculateVisitorPasses2Async(input2, token);
        var response = new ApiWorkplaces.RecalculateVisitorPassesResponse
        {
            Breakdown = new ApiWorkplaces.RecalculateVisitorPassesResponse.Types.Breakdown
            {
                CurrencyCode = (request.Details?.CurrencyCode ?? ""),
                Total = (reply0.Breakdown?.Total ?? new ApiCommon.Money()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply2.Building?.Audit?.Etag ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply2.Building?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Region = (reply1.Building?.Location?.Region ?? ""),
                },
                VisitorPassId = (request.VisitorPassId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.VisitorPass?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass",
            "audit", "RecalculateVisitorPasses", (request.VisitorPass?.VoucherCode ?? ""), token);
        return response;
    }
    private async Task<ApiWorkplaces.QuoteDeskResponse> SendCalculateVisitorPass2Async(ApiWorkplaces.QuoteDeskRequest request, CancellationToken token)
    {
        return await _visitorPassCalculationEndpointSendCalculateVisitorPass2Port.SendAsync(request, token);
    }
    private async Task<ApiWorkplaces.CheckBuildingEligibilityResponse> SendRecalculateVisitorPasses2Async(ApiWorkplaces.CheckBuildingEligibilityRequest request, CancellationToken token)
    {
        return await _visitorPassCalculationEndpointSendRecalculateVisitorPasses2Port.SendAsync(request, token);
    }
}
