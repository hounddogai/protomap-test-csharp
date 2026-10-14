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
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

public sealed partial class DeskValidationEndpoint
{
    /// <summary>
    /// Checks a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.CheckDeskEligibilityResponse> CheckDeskEligibility(ApiWorkplaces.CheckDeskEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CheckDeskEligibility for {Tenant}", requestContext.TenantId);
        if (request.Desk is { } record)
        {
            var snapshot = DeskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWorkplaces.RecalculateDesksRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Desk = new ApiWorkplaces.Desk
            {
                Address = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                    {
                        ApiWorkplaces.Desk.LocatorOneofCase.Address => (request.Desk?.Address?.PostalCode ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _workplacesDeskCalculationService.RecalculateDesksAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiWorkplaces.ExplainRoomBookingCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Location = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                    {
                        ApiWorkplaces.Desk.LocatorOneofCase.Address => (request.Desk?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()),
                        _ => new Google.Protobuf.Collections.RepeatedField<string>()
                    } },
                },
                Organizer = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Desk?.Owner?.DisplayName ?? ""),
                },
                Participants = { new ApiWorkplaces.RoomBookingParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                    },
                } },
            },
        };
        using var rpc1 = _workplacesRoomBookingCalculationService.ExplainRoomBookingCalculationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiWorkplaces.CompareDeskOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Desk = new ApiWorkplaces.Desk
            {
                Address = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply1.RoomBooking?.Location?.CountryCode ?? ""),
                    PostalCode = (request.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                    {
                        ApiWorkplaces.Desk.LocatorOneofCase.Address => (request.Desk?.Address?.PostalCode ?? ""),
                        _ => ""
                    },
                },
                Dimensions = new ApiWorkplaces.DeskDimensions
                {
                    Length = (request.Desk?.Dimensions?.Length ?? 0d),
                },
            },
        };
        using var rpc2 = _workplacesDeskCalculationService.CompareDeskOptionsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiTravel.ScoreTravelBookingRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TravelBooking = new ApiTravel.TravelBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Desk?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply0.Desk?.Owner?.DisplayName ?? ""),
                },
            },
        };
        var reply3 = await SendCheckDeskEligibility3Async(input3, token);
        var response = new ApiWorkplaces.CheckDeskEligibilityResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply3.TravelBooking?.Audit?.UpdatedBy ?? ""),
                },
                Components = { { "primary", new ApiWorkplaces.DeskComponent
                {
                    Description = (reply0.Desk?.Components?.GetValueOrDefault("primary")?.Description ?? ""),
                } } },
                DeskId = (request.DeskId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (reply2.Desk?.Owner?.UserId ?? ""),
                },
                Specs = { (reply2.Desk?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (request.Desk?.Status ?? (ApiWorkplaces.DeskStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "desk",
            "audit", "CheckDeskEligibility", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.ExplainDeskDecisionResponse> ExplainDeskDecision(ApiWorkplaces.ExplainDeskDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainDeskDecision for {Tenant}", requestContext.TenantId);
        if (request.Desk is { } record)
        {
            var snapshot = DeskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.ExplainTravelBookingDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TravelBooking = new ApiTravel.TravelBooking
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                    {
                        ApiWorkplaces.Desk.LocatorOneofCase.Address => (request.Desk?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()),
                        _ => new Google.Protobuf.Collections.RepeatedField<string>()
                    } },
                },
            },
        };
        using var rpc0 = _travelTravelBookingValidationService.ExplainTravelBookingDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiWorkplaces.EstimateRoomBookingRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiWorkplaces.EstimateRoomBookingRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                    {
                        ApiWorkplaces.Desk.LocatorOneofCase.Address => (request.Desk?.Address?.CountryCode ?? ""),
                        _ => ""
                    },
                    Lines = { (reply0.TravelBooking?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Participants = { new ApiWorkplaces.RoomBookingParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply0.TravelBooking?.Counterparty?.Email ?? ""),
                        Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                    },
                } },
            },
        };
        using var rpc1 = _workplacesRoomBookingCalculationService.EstimateRoomBookingAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.ExplainDeskDecisionResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                DeskId = (request.DeskId ?? ""),
                Specs = { (request.Desk?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "desk",
            "audit", "ExplainDeskDecision", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.GetDeskRulesResponse> GetDeskRules(ApiWorkplaces.GetDeskRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DeskId, "desk_id");
        _logger.LogInformation("Handling GetDeskRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "desk", request.DeskId,
            "GetDeskRules", "accepted", token);
        var input0 = new ApiTravel.CountTripsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiTravel.CountTripsRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                Labels = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.DeskId ?? ""),
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiTravel.CountTripsRequest>(json0);
        using var rpc0 = _travelTripQueryService.CountTripsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiWorkplaces.GetDeskRulesResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                DeskId = (request.DeskId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Lints a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.LintDesksResponse> LintDesks(ApiWorkplaces.LintDesksRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling LintDesks for {Tenant}", requestContext.TenantId);
        if (request.Desk is { } record)
        {
            var snapshot = DeskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWorkplaces.RecalculateRoomBookingsRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.RoomBooking = new ApiWorkplaces.RoomBooking
        {
            Audit = new ApiCommon.AuditInfo
            {
                UpdatedAt = (request.Desk?.Audit?.UpdatedAt ?? new Timestamp()),
            },
            Participants = { new ApiWorkplaces.RoomBookingParticipant
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Desk?.Owner?.Email ?? ""),
                },
            } },
        };
        using var rpc0 = _workplacesRoomBookingCalculationService.RecalculateRoomBookingsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiWorkplaces.RecalculateRoomBookingsRequest>("acme.workplaces.v1.DeskValidationService/LintDesks/0", input0.Clone()), token);
        var input1 = new ApiTravel.ResolveTripsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _travelTripQueryService.ResolveTripsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.LintDesksResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.RoomBooking?.Audit?.CreatedBy ?? ""),
                },
                DeskId = (request.DeskId ?? ""),
                Name = (request.Desk?.Name ?? ""),
            },
            Outcome = new ApiWorkplaces.LintDesksResponse.Types.Outcome
            {
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "desk",
            "audit", "LintDesks", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Normalizes a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.NormalizeDeskResponse> NormalizeDesk(ApiWorkplaces.NormalizeDeskRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling NormalizeDesk for {Tenant}", requestContext.TenantId);
        if (request.Desk is { } record)
        {
            var snapshot = DeskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.BrowseTripsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiTravel.BrowseTripsRequest.Types.Filter
            {
                CreatedAfter = (request.Desk?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Desk?.Components?.GetValueOrDefault("primary")?.ComponentId ?? ""),
                Region = (request.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                {
                    ApiWorkplaces.Desk.LocatorOneofCase.Address => (request.Desk?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc0 = _travelTripQueryService.BrowseTripsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.SimulateVenueRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.SimulateVenueRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                    {
                        ApiWorkplaces.Desk.LocatorOneofCase.Address => (request.Desk?.Address?.Region ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _corporateeventsVenueCalculationService.SimulateVenueAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.NormalizeDeskResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                DeskId = (request.DeskId ?? ""),
                Owner = (request.Desk?.Owner ?? new ApiIdentity.UserRef()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "desk",
            "audit", "NormalizeDesk", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.PreviewDeskResponse> PreviewDesk(ApiWorkplaces.PreviewDeskRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PreviewDesk for {Tenant}", requestContext.TenantId);
        if (request.Desk is { } record)
        {
            var snapshot = DeskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWorkplaces.CompareRoomBookingOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiWorkplaces.CompareRoomBookingOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                    {
                        ApiWorkplaces.Desk.LocatorOneofCase.Address => (request.Desk?.Address?.Region ?? ""),
                        _ => ""
                    },
                },
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Location = new ApiCommon.PostalAddress
                {
                    Recipient = (request.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                    {
                        ApiWorkplaces.Desk.LocatorOneofCase.Address => (request.Desk?.Address?.Recipient ?? ""),
                        _ => ""
                    },
                },
                Participants = { new ApiWorkplaces.RoomBookingParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                    },
                } },
            },
        };
        using var rpc0 = _workplacesRoomBookingCalculationService.CompareRoomBookingOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDevices.RecalculateComplianceScansRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (request.Desk?.Owner?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.RecalculateComplianceScansRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.RoomBooking?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
        };
        using var rpc1 = _devicesComplianceScanCalculationService.RecalculateComplianceScansAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.PreviewDeskResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.RoomBooking?.Audit?.UpdatedBy ?? ""),
                },
                Code = (request.Desk?.Code ?? ""),
                DeskId = (request.DeskId ?? ""),
            },
            Outcome = new ApiWorkplaces.PreviewDeskResponse.Types.Outcome
            {
                Status = (reply1.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "desk",
            "audit", "PreviewDesk", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.ScoreDeskResponse> ScoreDesk(ApiWorkplaces.ScoreDeskRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScoreDesk for {Tenant}", requestContext.TenantId);
        if (request.Desk is { } record)
        {
            var snapshot = DeskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWorkplaces.GetDeskRatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DeskId = (request.Desk?.DeskId ?? ""),
        };
        var named0 = _registry.Get<ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient>("acme.workplaces.v1.DeskCalculationService");
        using var rpc0 = named0.GetDeskRatesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.EstimateKnowledgeArticleRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                    {
                        ApiWorkplaces.Desk.LocatorOneofCase.Address => (request.Desk?.Address?.PostalCode ?? ""),
                        _ => ""
                    },
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
            },
        };
        using var rpc1 = _helpdeskKnowledgeArticleCalculationService.EstimateKnowledgeArticleAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.ScoreDeskResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.KnowledgeArticle?.Audit?.Etag ?? ""),
                },
                Capacity = (reply0.Desk?.Capacity ?? 0L),
                DeskId = (request.DeskId ?? ""),
                Specs = { (request.Desk?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Tags = { (reply0.Desk?.Tags ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "desk",
            "audit", "ScoreDesk", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.ValidateDeskResponse> ValidateDesk(ApiWorkplaces.ValidateDeskRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ValidateDesk for {Tenant}", requestContext.TenantId);
        if (request.Desk is { } record)
        {
            var snapshot = DeskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.PushDocumentFolderRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.PushDocumentFolderRequest.Types.Details
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
                            UpdatedBy = (request.Desk?.Audit?.UpdatedBy ?? ""),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            PostalCode = (request.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                            {
                                ApiWorkplaces.Desk.LocatorOneofCase.Address => (request.Desk?.Address?.PostalCode ?? ""),
                                _ => ""
                            },
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            Email = (request.Desk?.Owner?.Email ?? ""),
                            UserId = (request.Desk?.Owner?.UserId ?? ""),
                        },
                    } },
                } },
            },
        };
        using var rpc0 = _documentsDocumentFolderSyncService.PushDocumentFolderAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.CompareVenueOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.CompareVenueOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.DocumentFolder?.Location?.PostalCode ?? ""),
                },
            },
            Venue = new ApiCorporateevents.Venue
            {
                Address = new ApiCommon.PostalAddress
                {
                    Region = (request.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                    {
                        ApiWorkplaces.Desk.LocatorOneofCase.Address => (request.Desk?.Address?.Region ?? ""),
                        _ => ""
                    },
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Desk?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Desk?.Owner?.UserId ?? ""),
                },
            },
        };
        using var rpc1 = _corporateeventsVenueCalculationService.CompareVenueOptionsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiWorkplaces.ExplainBuildingDecisionRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Location = new ApiCommon.PostalAddress
                        {
                            PostalCode = (request.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                            {
                                ApiWorkplaces.Desk.LocatorOneofCase.Address => (request.Desk?.Address?.PostalCode ?? ""),
                                _ => ""
                            },
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (reply0.DocumentFolder?.Manager?.DisplayName ?? ""),
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc2 = _workplacesBuildingValidationService.ExplainBuildingDecisionAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = request.ValidateDeskToEstimateDesk3(reply0);
        using var rpc3 = _workplacesDeskCalculationService.EstimateDeskAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiWorkplaces.ValidateDeskResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.DocumentFolder?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                DeskId = (request.DeskId ?? ""),
                Dimensions = new ApiWorkplaces.DeskDimensions
                {
                    Weight = (reply1.Venue?.Dimensions?.Weight ?? 0d),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply3.Desk?.Owner?.DisplayName ?? ""),
                },
                Specs = { (request.Desk?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Outcome = new ApiWorkplaces.ValidateDeskResponse.Types.Outcome
            {
                ReferenceId = (reply0.Outcome?.ReferenceId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "desk",
            "audit", "ValidateDesk", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiTravel.ScoreTravelBookingResponse> SendCheckDeskEligibility3Async(ApiTravel.ScoreTravelBookingRequest request, CancellationToken token)
    {
        return await _deskValidationEndpointSendCheckDeskEligibility3Port.SendAsync(request, token);
    }
}
