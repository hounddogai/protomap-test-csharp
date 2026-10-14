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
using ApiDocuments = Acme.Documents.V1;
using ApiDrivers = Acme.Drivers.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiVenues = Acme.Venues.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

/// <summary>
/// Answers read-only questions about buildings, for screens and reports.
/// </summary>
public sealed class BuildingQueryEndpoint : ApiWorkplaces.BuildingQueryService.BuildingQueryServiceBase
{
    private readonly ApiDocuments.DocumentFolderExportService.DocumentFolderExportServiceClient _documentsDocumentFolderExportService;
    private readonly ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient _documentsDocumentNotificationService;
    private readonly ApiDocuments.DocumentQueryService.DocumentQueryServiceClient _documentsDocumentQueryService;
    private readonly ApiDrivers.DriverCalculationService.DriverCalculationServiceClient _driversDriverCalculationService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.IncidentValidationService.IncidentValidationServiceClient _itsmIncidentValidationService;
    private readonly ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient _licensingEntitlementCalculationService;
    private readonly ILogger<BuildingQueryEndpoint> _logger;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiItsm.CheckIncidentEligibilityRequest, CallOptions, AsyncUnaryCall<ApiItsm.CheckIncidentEligibilityResponse>> _sendFindBuildings0;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiVenues.VenueService.VenueServiceClient _venuesVenueService;
    private readonly ApiWorkplaces.BuildingCalculationService.BuildingCalculationServiceClient _workplacesBuildingCalculationService;
    private readonly ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient _workplacesDeskCalculationService;
    private readonly ApiWorkplaces.DeskValidationService.DeskValidationServiceClient _workplacesDeskValidationService;
    private readonly ApiWorkplaces.RoomBookingValidationService.RoomBookingValidationServiceClient _workplacesRoomBookingValidationService;
    private readonly ApiWorkplaces.VisitorPassValidationService.VisitorPassValidationServiceClient _workplacesVisitorPassValidationService;

    public BuildingQueryEndpoint(
        ApiDocuments.DocumentFolderExportService.DocumentFolderExportServiceClient documentsDocumentFolderExportService,
        ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient documentsDocumentNotificationService,
        ApiDocuments.DocumentQueryService.DocumentQueryServiceClient documentsDocumentQueryService,
        ApiDrivers.DriverCalculationService.DriverCalculationServiceClient driversDriverCalculationService,
        CallInvoker invoker,
        ApiItsm.IncidentValidationService.IncidentValidationServiceClient itsmIncidentValidationService,
        ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient licensingEntitlementCalculationService,
        ILogger<BuildingQueryEndpoint> logger,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiVenues.VenueService.VenueServiceClient venuesVenueService,
        ApiWorkplaces.BuildingCalculationService.BuildingCalculationServiceClient workplacesBuildingCalculationService,
        ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient workplacesDeskCalculationService,
        ApiWorkplaces.DeskValidationService.DeskValidationServiceClient workplacesDeskValidationService,
        ApiWorkplaces.RoomBookingValidationService.RoomBookingValidationServiceClient workplacesRoomBookingValidationService,
        ApiWorkplaces.VisitorPassValidationService.VisitorPassValidationServiceClient workplacesVisitorPassValidationService)
    {
        _documentsDocumentFolderExportService = documentsDocumentFolderExportService;
        _documentsDocumentNotificationService = documentsDocumentNotificationService;
        _documentsDocumentQueryService = documentsDocumentQueryService;
        _driversDriverCalculationService = driversDriverCalculationService;
        _invoker = invoker;
        _itsmIncidentValidationService = itsmIncidentValidationService;
        _licensingEntitlementCalculationService = licensingEntitlementCalculationService;
        _logger = logger;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _venuesVenueService = venuesVenueService;
        _workplacesBuildingCalculationService = workplacesBuildingCalculationService;
        _workplacesDeskCalculationService = workplacesDeskCalculationService;
        _workplacesDeskValidationService = workplacesDeskValidationService;
        _workplacesRoomBookingValidationService = workplacesRoomBookingValidationService;
        _workplacesVisitorPassValidationService = workplacesVisitorPassValidationService;
        _sendFindBuildings0 = _itsmIncidentValidationService.CheckIncidentEligibilityAsync;
    }
    /// <summary>
    /// Lists buildings a page at a time.
    /// </summary>
    public override async Task<ApiWorkplaces.BrowseBuildingsResponse> BrowseBuildings(ApiWorkplaces.BrowseBuildingsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling BrowseBuildings for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "building", "batch",
            "BrowseBuildings", "accepted", token);
        var response = new ApiWorkplaces.BrowseBuildingsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "BrowseBuildings", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Counts buildings by status.
    /// </summary>
    public override async Task<ApiWorkplaces.CountBuildingsResponse> CountBuildings(ApiWorkplaces.CountBuildingsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CountBuildings for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "building", "batch",
            "CountBuildings", "accepted", token);
        var input0 = new ApiDocuments.ScheduleDocumentFolderExportRequest
        {
            Annotations = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ScheduleDocumentFolderExportRequest.Types.Details
            {
                Filters = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        using var rpc0 = _documentsDocumentFolderExportService.ScheduleDocumentFolderExportAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiDrivers.ExplainDriverCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDrivers.ExplainDriverCalculationRequest.Types.Details
            {
                Locale = (request.Context?.Locale ?? ""),
            },
            Driver = new ApiDrivers.Driver
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Passport = new ApiDrivers.DriverPassport
                {
                    ExpiresAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                },
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        using var rpc1 = _driversDriverCalculationService.ExplainDriverCalculationAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiWorkplaces.CountBuildingsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "CountBuildings", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams every matching building.
    /// </summary>
    public override async Task ExportBuildings(ApiWorkplaces.ExportBuildingsRequest request, IServerStreamWriter<ApiWorkplaces.ExportBuildingsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExportBuildings for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "building", "batch",
            "ExportBuildings", "accepted", token);
        var input0 = new ApiLicensing.QuoteEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.QuoteEntitlementRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
            Entitlement = new ApiLicensing.Entitlement
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        using var rpc0 = _licensingEntitlementCalculationService.QuoteEntitlementAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiWorkplaces.EstimateDeskRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Desk = new ApiWorkplaces.Desk
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Entitlement?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedBy = (reply0.Entitlement?.Audit?.UpdatedBy ?? ""),
                },
                Components = { { "primary", new ApiWorkplaces.DeskComponent
                {
                    Description = (reply0.Entitlement?.Description ?? ""),
                } } },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Entitlement?.Owner?.DisplayName ?? ""),
                },
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _workplacesDeskCalculationService.EstimateDeskAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiWorkplaces.QuoteDeskRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Desk = new ApiWorkplaces.Desk
            {
                Dimensions = new ApiWorkplaces.DeskDimensions
                {
                    Height = (reply1.Desk?.Dimensions?.Height ?? 0d),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.Entitlement?.Owner?.Email ?? ""),
                },
            },
            Details = new ApiWorkplaces.QuoteDeskRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply1.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                    {
                        ApiWorkplaces.Desk.LocatorOneofCase.Address => (reply1.Desk?.Address?.CountryCode ?? ""),
                        _ => ""
                    },
                    Region = (reply1.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                    {
                        ApiWorkplaces.Desk.LocatorOneofCase.Address => (reply1.Desk?.Address?.Region ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc2 = _workplacesDeskCalculationService.QuoteDeskAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiWorkplaces.ExplainVisitorPassDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.Desk?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply2.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                    {
                        ApiWorkplaces.Desk.LocatorOneofCase.Address => (reply2.Desk?.Address?.PostalCode ?? ""),
                        _ => ""
                    },
                    Recipient = (reply2.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                    {
                        ApiWorkplaces.Desk.LocatorOneofCase.Address => (reply2.Desk?.Address?.Recipient ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc3 = _workplacesVisitorPassValidationService.ExplainVisitorPassDecisionAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiWorkplaces.ExportBuildingsResponse
        {
            Building = new ApiWorkplaces.Building
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Entitlement?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (reply2.Desk?.Audit?.Etag ?? ""),
                },
                Children = { new ApiWorkplaces.Building
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        Etag = (reply2.Desk?.Audit?.Etag ?? ""),
                        UpdatedAt = (reply3.VisitorPass?.Audit?.UpdatedAt ?? new Timestamp()),
                    },
                    Children = { new ApiWorkplaces.Building
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            CreatedBy = (reply0.Entitlement?.Audit?.CreatedBy ?? ""),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            Locality = (reply3.VisitorPass?.BillingAddress?.Locality ?? ""),
                            Region = (reply1.Desk?.Address?.Region ?? ""),
                        },
                    } },
                } },
                Location = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "ExportBuildings", (request.Format ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    /// <summary>
    /// Lists buildings a page at a time.
    /// </summary>
    public override async Task<ApiWorkplaces.FindBuildingsResponse> FindBuildings(ApiWorkplaces.FindBuildingsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling FindBuildings for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "building", "batch",
            "FindBuildings", "accepted", token);
        var input0 = new ApiItsm.CheckIncidentEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.CheckIncidentEligibilityRequest.Types.Details
            {
                Locale = (request.Context?.Locale ?? ""),
            },
            Incident = new ApiItsm.Incident
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        using var rpc0 = _sendFindBuildings0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiWorkplaces.FindBuildingsResponse
        {
            Buildings = { new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        UpdatedBy = (reply0.Incident?.Audit?.UpdatedBy ?? ""),
                    },
                } },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "FindBuildings", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a building.
    /// </summary>
    public override async Task<ApiWorkplaces.LookupBuildingResponse> LookupBuilding(ApiWorkplaces.LookupBuildingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.BuildingId, "building_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling LookupBuilding for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "building", request.BuildingId,
            "LookupBuilding", "accepted", token);
        var input0 = new ApiWorkplaces.NormalizeDeskRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Desk = new ApiWorkplaces.Desk
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            Details = new ApiWorkplaces.NormalizeDeskRequest.Types.Details
            {
                Locale = (request.Context?.Locale ?? ""),
            },
        };
        var named0 = _registry.Get<ApiWorkplaces.DeskValidationService.DeskValidationServiceClient>("acme.workplaces.v1.DeskValidationService");
        using var rpc0 = named0.NormalizeDeskAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiWorkplaces.LookupBuildingResponse
        {
            Building = new ApiWorkplaces.Building
            {
                BuildingId = (request.BuildingId ?? ""),
                Children = { new ApiWorkplaces.Building
                {
                    Name = (reply0.Desk?.Name ?? ""),
                } },
            },
        };
        return response;
    }
    /// <summary>
    /// Returns a building.
    /// </summary>
    public override async Task<ApiWorkplaces.PeekBuildingResponse> PeekBuilding(ApiWorkplaces.PeekBuildingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.BuildingId, "building_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PeekBuilding for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "building", request.BuildingId,
            "PeekBuilding", "accepted", token);
        var input0 = new ApiVenues.UpdateVenueRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Venue = new ApiVenues.Venue
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Components = { { "primary", new ApiVenues.VenueComponent
                {
                    ComponentId = (request.BuildingId ?? ""),
                } } },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiVenues.UpdateVenueRequest, ApiVenues.UpdateVenueResponse>(
            MethodType.Unary, "acme.venues.v1" + ".VenueService", "UpdateVenue",
            Wire.Marshaller<ApiVenues.UpdateVenueRequest>(), Wire.Marshaller<ApiVenues.UpdateVenueResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiWorkplaces.PeekBuildingResponse
        {
            Building = new ApiWorkplaces.Building
            {
                BuildingId = (request.BuildingId ?? ""),
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            Etag = (reply0.Venue?.Audit?.Etag ?? ""),
                        },
                    } },
                } },
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns several buildings at once.
    /// </summary>
    public override async Task<ApiWorkplaces.ResolveBuildingsResponse> ResolveBuildings(ApiWorkplaces.ResolveBuildingsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ResolveBuildings for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "building", "batch",
            "ResolveBuildings", "accepted", token);
        var input0 = new ApiDocuments.ListDocumentSubscriptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentSubscriptionsRequest.Types.Filter
            {
                OwnerId = (request.Context?.TraceId ?? ""),
            },
        };
        var command0 = new DispatchCommand<ApiDocuments.ListDocumentSubscriptionsRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _documentsDocumentNotificationService.ListDocumentSubscriptionsAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiWorkplaces.ResolveBuildingsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists buildings a page at a time.
    /// </summary>
    public override async Task<ApiWorkplaces.SuggestBuildingsResponse> SuggestBuildings(ApiWorkplaces.SuggestBuildingsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SuggestBuildings for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "building", "batch",
            "SuggestBuildings", "accepted", token);
        var response = new ApiWorkplaces.SuggestBuildingsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "SuggestBuildings", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Summarizes a building.
    /// </summary>
    public override async Task<ApiWorkplaces.SummarizeBuildingsResponse> SummarizeBuildings(ApiWorkplaces.SummarizeBuildingsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SummarizeBuildings for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "building", request.BuildingId,
            "SummarizeBuildings", "accepted", token);
        var profile0 = new MappingProfile<ApiWorkplaces.SummarizeBuildingsRequest, ApiWorkplaces.ScoreRoomBookingRequest>(value => new ApiWorkplaces.ScoreRoomBookingRequest
        {
            Annotations = { (value.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (value.Context?.Actor?.Email ?? ""),
                    UserId = (value.Context?.Actor?.UserId ?? ""),
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _workplacesRoomBookingValidationService.ScoreRoomBookingAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiWorkplaces.CompareBuildingOptionsRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedBy = (reply0.RoomBooking?.Audit?.CreatedBy ?? ""),
                    },
                    Children = { new ApiWorkplaces.Building
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            Etag = (reply0.RoomBooking?.Audit?.Etag ?? ""),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (reply0.RoomBooking?.Organizer?.DisplayName ?? ""),
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _workplacesBuildingCalculationService.CompareBuildingOptionsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = request.SummarizeBuildingsToSummarizeDocuments2();
        using var rpc2 = _documentsDocumentQueryService.SummarizeDocumentsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiWorkplaces.SummarizeBuildingsResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
            },
            Summary = { (reply2.Summary ?? new Google.Protobuf.Collections.MapField<string, string>()) },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "SummarizeBuildings", (request.Reason ?? ""), token);
        return response;
    }
}
