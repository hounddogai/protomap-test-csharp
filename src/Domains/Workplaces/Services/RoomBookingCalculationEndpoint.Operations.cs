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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

public sealed partial class RoomBookingCalculationEndpoint
{
    /// <summary>
    /// Calculates a room booking.
    /// </summary>
    public override async Task<ApiWorkplaces.CalculateRoomBookingResponse> CalculateRoomBooking(ApiWorkplaces.CalculateRoomBookingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CalculateRoomBooking for {Tenant}", requestContext.TenantId);
        if (request.RoomBooking is { } record)
        {
            var snapshot = RoomBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWorkplaces.LintBuildingsRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Location = new ApiCommon.PostalAddress
                        {
                            Region = (request.Details?.Destination?.Region ?? ""),
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _workplacesBuildingValidationService.LintBuildingsAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiDevices.ValidateDevicePolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.ValidateDevicePolicyRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.RoomBooking?.Organizer?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _devicesDevicePolicyValidationService.ValidateDevicePolicyAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiWorkplaces.CalculateRoomBookingResponse
        {
            Breakdown = new ApiWorkplaces.CalculateRoomBookingResponse.Types.Breakdown
            {
                Lines = { new ApiWorkplaces.CalculateRoomBookingResponse.Types.Breakdown.Types.Line
                {
                    Amount = (reply1.DevicePolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                } },
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Building?.Audit?.CreatedBy ?? ""),
                },
                Location = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.Building?.Location?.CountryCode ?? ""),
                },
                RoomBookingId = (request.RoomBookingId ?? ""),
                Window = (request.RoomBooking?.Window ?? new ApiCommon.TimeWindow()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RoomBooking?.Title ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "room_booking",
            "audit", "CalculateRoomBooking", (request.RoomBooking?.Title ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares a room booking.
    /// </summary>
    public override async Task<ApiWorkplaces.CompareRoomBookingOptionsResponse> CompareRoomBookingOptions(ApiWorkplaces.CompareRoomBookingOptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CompareRoomBookingOptions for {Tenant}", requestContext.TenantId);
        if (request.RoomBooking is { } record)
        {
            var snapshot = RoomBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.SimulateEscalationRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Details = new ApiHelpdesk.SimulateEscalationRequest.Types.Details
        {
            Destination = new ApiCommon.PostalAddress
            {
                CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
            },
        };
        input0.Escalation = new ApiHelpdesk.Escalation
        {
            Assignee = new ApiIdentity.UserRef
            {
                DisplayName = (request.RoomBooking?.Organizer?.DisplayName ?? ""),
            },
        };
        using var rpc0 = _helpdeskEscalationCalculationService.SimulateEscalationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiWorkplaces.SimulateDeskRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Desk = new ApiWorkplaces.Desk
            {
                Address = new ApiCommon.PostalAddress
                {
                    Locality = (request.Details?.Destination?.Locality ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Escalation ?? new ApiHelpdesk.Escalation()).OriginCase switch
                    {
                        ApiHelpdesk.Escalation.OriginOneofCase.Customer => (reply0.Escalation?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                    UserId = (request.RoomBooking?.Organizer?.UserId ?? ""),
                },
            },
        };
        using var rpc1 = _workplacesDeskCalculationService.SimulateDeskAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.CompareRoomBookingOptionsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Escalation?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Metadata = { (request.RoomBooking?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Organizer = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Escalation?.Customer?.DisplayName ?? ""),
                },
                RoomBookingId = (request.RoomBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RoomBooking?.Title ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "room_booking",
            "audit", "CompareRoomBookingOptions", (request.RoomBooking?.Title ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates a room booking.
    /// </summary>
    public override async Task<ApiWorkplaces.EstimateRoomBookingResponse> EstimateRoomBooking(ApiWorkplaces.EstimateRoomBookingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EstimateRoomBooking for {Tenant}", requestContext.TenantId);
        if (request.RoomBooking is { } record)
        {
            var snapshot = RoomBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.RoomBooking?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiWorkplaces.RoomBookingParticipant>()).Select(async item =>
        {
            var input0 = new ApiWorkplaces.ValidateBuildingRequest
            {
                Building = new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Children = { new ApiWorkplaces.Building
                        {
                            Manager = new ApiIdentity.UserRef
                            {
                                Email = (item.Contact?.Email ?? ""),
                            },
                        } },
                    } },
                },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
            };
            using var rpc0 = _workplacesBuildingValidationService.ValidateBuildingAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiWorkplaces.ValidateBuildingResponse();
        var input1 = new ApiHelpdesk.QuoteEscalationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.QuoteEscalationRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (request.RoomBooking?.Location?.Locality ?? ""),
                },
            },
            Escalation = new ApiHelpdesk.Escalation
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.Building?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _helpdeskEscalationCalculationService.QuoteEscalationAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiWorkplaces.EstimateRoomBookingResponse
        {
            Breakdown = new ApiWorkplaces.EstimateRoomBookingResponse.Types.Breakdown
            {
                CurrencyCode = (request.Details?.CurrencyCode ?? ""),
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Escalation?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (reply1.Escalation?.Audit?.Etag ?? ""),
                },
                RoomBookingId = (request.RoomBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RoomBooking?.Title ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "room_booking",
            "audit", "EstimateRoomBooking", (request.RoomBooking?.Title ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a room booking.
    /// </summary>
    public override async Task<ApiWorkplaces.ExplainRoomBookingCalculationResponse> ExplainRoomBookingCalculation(ApiWorkplaces.ExplainRoomBookingCalculationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainRoomBookingCalculation for {Tenant}", requestContext.TenantId);
        if (request.RoomBooking is { } record)
        {
            var snapshot = RoomBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.FindDocumentFoldersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.FindDocumentFoldersRequest.Types.Filter
            {
                Region = (request.RoomBooking?.Location?.Region ?? ""),
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _documentsDocumentFolderQueryService.FindDocumentFoldersAsync);
        var input1 = new ApiWorkplaces.ScoreBuildingRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        UpdatedAt = (request.RoomBooking?.Audit?.UpdatedAt ?? new Timestamp()),
                    },
                    Children = { new ApiWorkplaces.Building
                    {
                        Location = new ApiCommon.PostalAddress
                        {
                            PostalCode = (request.RoomBooking?.Location?.PostalCode ?? ""),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            Email = (request.RoomBooking?.Organizer?.Email ?? ""),
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _sendExplainRoomBookingCalculation1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDevices.PreviewComplianceScanRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (reply1.Building?.Manager?.DisplayName ?? ""),
                    Email = (request.RoomBooking?.Organizer?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.PreviewComplianceScanRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.Contact?.Phone ?? ""),
                },
            },
        };
        using var rpc2 = _devicesComplianceScanValidationService.PreviewComplianceScanAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiWorkplaces.ExplainRoomBookingCalculationResponse
        {
            Outcome = new ApiWorkplaces.ExplainRoomBookingCalculationResponse.Types.Outcome
            {
                Message = (reply2.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                RoomBookingId = (request.RoomBookingId ?? ""),
                Title = (request.RoomBooking?.Title ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RoomBooking?.RoomBookingId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "room_booking",
            "audit", "ExplainRoomBookingCalculation", (request.RoomBooking?.RoomBookingId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a room booking.
    /// </summary>
    public override async Task<ApiWorkplaces.GetRoomBookingRatesResponse> GetRoomBookingRates(ApiWorkplaces.GetRoomBookingRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.RoomBookingId, "room_booking_id");
        _logger.LogInformation("Handling GetRoomBookingRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "room_booking", request.RoomBookingId,
            "GetRoomBookingRates", "accepted", token);
        var input0 = new ApiWorkplaces.CheckBuildingEligibilityRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Manager = new ApiIdentity.UserRef
                        {
                            UserId = (request.Context?.Actor?.UserId ?? ""),
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var descriptor0 = new Method<ApiWorkplaces.CheckBuildingEligibilityRequest, ApiWorkplaces.CheckBuildingEligibilityResponse>(
            MethodType.Unary, "acme.workplaces.v1" + ".BuildingValidationService", "CheckBuildingEligibility",
            Wire.Marshaller<ApiWorkplaces.CheckBuildingEligibilityRequest>(), Wire.Marshaller<ApiWorkplaces.CheckBuildingEligibilityResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiWorkplaces.GetRoomBookingRatesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Building?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                RoomBookingId = (request.RoomBookingId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Recalculates a room booking.
    /// </summary>
    public override async Task<ApiWorkplaces.RecalculateRoomBookingsResponse> RecalculateRoomBookings(ApiWorkplaces.RecalculateRoomBookingsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecalculateRoomBookings for {Tenant}", requestContext.TenantId);
        if (request.RoomBooking is { } record)
        {
            var snapshot = RoomBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.CreateDocumentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Document = new ApiDocuments.Document
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.RoomBooking?.Audit?.CreatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                    Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Locality = (request.Details?.Destination?.Locality ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiDocuments.CreateDocumentRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _documentsDocumentService.CreateDocumentAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiWorkplaces.RecalculateRoomBookingsResponse
        {
            Breakdown = new ApiWorkplaces.RecalculateRoomBookingsResponse.Types.Breakdown
            {
                CurrencyCode = (request.Details?.CurrencyCode ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                RoomBookingId = (request.RoomBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RoomBooking?.Title ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "room_booking",
            "audit", "RecalculateRoomBookings", (request.RoomBooking?.Title ?? ""), token);
        return response;
    }
}
