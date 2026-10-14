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
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiMedia = Acme.Media.V1;

namespace Acme.Workforce.Corporateevents;

/// <summary>
/// Checks venues against the rules before anything acts on them.
/// </summary>
public sealed class VenueValidationEndpoint : ApiCorporateevents.VenueValidationService.VenueValidationServiceBase
{
    private readonly ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient _corporateeventsCorporateEventCalculationService;
    private readonly ApiCorporateevents.RegistrationCalculationService.RegistrationCalculationServiceClient _corporateeventsRegistrationCalculationService;
    private readonly ApiCorporateevents.RegistrationValidationService.RegistrationValidationServiceClient _corporateeventsRegistrationValidationService;
    private readonly ApiCorporateevents.VenueCalculationService.VenueCalculationServiceClient _corporateeventsVenueCalculationService;
    private readonly ApiDocuments.DocumentTemplateCalculationService.DocumentTemplateCalculationServiceClient _documentsDocumentTemplateCalculationService;
    private readonly ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient _documentsSignatureHistoryService;
    private readonly ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient _helpdeskSlaPolicyCalculationService;
    private readonly ApiIdentity.UserValidationService.UserValidationServiceClient _identityUserValidationService;
    private readonly ApiItsm.IncidentService.IncidentServiceClient _itsmIncidentService;
    private readonly ApiItsm.IncidentValidationService.IncidentValidationServiceClient _itsmIncidentValidationService;
    private readonly ILogger<VenueValidationEndpoint> _logger;
    private readonly ApiMedia.MediaAssetService.MediaAssetServiceClient _mediaMediaAssetService;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;
    private readonly IVenueValidationEndpointSendValidateVenue0Port _venueValidationEndpointSendValidateVenue0Port;

    public VenueValidationEndpoint(
        ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient corporateeventsCorporateEventCalculationService,
        ApiCorporateevents.RegistrationCalculationService.RegistrationCalculationServiceClient corporateeventsRegistrationCalculationService,
        ApiCorporateevents.RegistrationValidationService.RegistrationValidationServiceClient corporateeventsRegistrationValidationService,
        ApiCorporateevents.VenueCalculationService.VenueCalculationServiceClient corporateeventsVenueCalculationService,
        ApiDocuments.DocumentTemplateCalculationService.DocumentTemplateCalculationServiceClient documentsDocumentTemplateCalculationService,
        ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient documentsSignatureHistoryService,
        ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient helpdeskSlaPolicyCalculationService,
        ApiIdentity.UserValidationService.UserValidationServiceClient identityUserValidationService,
        ApiItsm.IncidentService.IncidentServiceClient itsmIncidentService,
        ApiItsm.IncidentValidationService.IncidentValidationServiceClient itsmIncidentValidationService,
        ILogger<VenueValidationEndpoint> logger,
        ApiMedia.MediaAssetService.MediaAssetServiceClient mediaMediaAssetService,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        IVenueValidationEndpointSendValidateVenue0Port venueValidationEndpointSendValidateVenue0PortAdapter)
    {
        _corporateeventsCorporateEventCalculationService = corporateeventsCorporateEventCalculationService;
        _corporateeventsRegistrationCalculationService = corporateeventsRegistrationCalculationService;
        _corporateeventsRegistrationValidationService = corporateeventsRegistrationValidationService;
        _corporateeventsVenueCalculationService = corporateeventsVenueCalculationService;
        _documentsDocumentTemplateCalculationService = documentsDocumentTemplateCalculationService;
        _documentsSignatureHistoryService = documentsSignatureHistoryService;
        _helpdeskSlaPolicyCalculationService = helpdeskSlaPolicyCalculationService;
        _identityUserValidationService = identityUserValidationService;
        _itsmIncidentService = itsmIncidentService;
        _itsmIncidentValidationService = itsmIncidentValidationService;
        _logger = logger;
        _mediaMediaAssetService = mediaMediaAssetService;
        _repository = repository;
        _settings = settings;
        _venueValidationEndpointSendValidateVenue0Port = venueValidationEndpointSendValidateVenue0PortAdapter;
    }
    /// <summary>
    /// Checks a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.CheckVenueEligibilityResponse> CheckVenueEligibility(ApiCorporateevents.CheckVenueEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CheckVenueEligibility for {Tenant}", requestContext.TenantId);
        if (request.Venue is { } record)
        {
            var snapshot = VenueMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.ExplainRegistrationCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.ExplainRegistrationCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
            Registration = new ApiCorporateevents.Registration
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiCorporateevents.ExplainRegistrationCalculationRequest>();
        using var rpc0 = _corporateeventsRegistrationCalculationService.ExplainRegistrationCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiIdentity.ScoreUserRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            User = new ApiIdentity.User
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Registration?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiIdentity.ScoreUserRequest>(json1);
        using var rpc1 = _identityUserValidationService.ScoreUserAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiMedia.WatchMediaAssetsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                Phone = (reply0.Registration?.Contact?.Phone ?? ""),
            },
        };
        using var stream2 = _mediaMediaAssetService.WatchMediaAssets(input2, cancellationToken: token);
        await foreach (var change2 in stream2.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchMediaAssets frame {Type}", change2.GetType().Name);
        }
        var response = new ApiCorporateevents.CheckVenueEligibilityResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            Venue = new ApiCorporateevents.Venue
            {
                Specs = { (request.Venue?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                VenueId = (request.VenueId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Venue?.VenueId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "CheckVenueEligibility", (request.Venue?.VenueId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.ExplainVenueDecisionResponse> ExplainVenueDecision(ApiCorporateevents.ExplainVenueDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExplainVenueDecision for {Tenant}", requestContext.TenantId);
        if (request.Venue is { } record)
        {
            var snapshot = VenueMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.CompareVenueOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.CompareVenueOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                    {
                        ApiCorporateevents.Venue.LocatorOneofCase.Address => (request.Venue?.Address?.Region ?? ""),
                        _ => ""
                    },
                },
            },
            Venue = new ApiCorporateevents.Venue
            {
                Address = new ApiCommon.PostalAddress
                {
                    Locality = (request.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                    {
                        ApiCorporateevents.Venue.LocatorOneofCase.Address => (request.Venue?.Address?.Locality ?? ""),
                        _ => ""
                    },
                    PostalCode = (request.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                    {
                        ApiCorporateevents.Venue.LocatorOneofCase.Address => (request.Venue?.Address?.PostalCode ?? ""),
                        _ => ""
                    },
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Venue?.Owner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _corporateeventsVenueCalculationService.CompareVenueOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiIdentity.ScoreUserRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            User = new ApiIdentity.User
            {
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _identityUserValidationService.ScoreUserAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiCorporateevents.ExplainVenueDecisionResponse
        {
            Venue = new ApiCorporateevents.Venue
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Venue?.Audit?.CreatedBy ?? ""),
                },
                Code = (request.Venue?.Code ?? ""),
                Components = { { "primary", new ApiCorporateevents.VenueComponent
                {
                    ComponentId = (reply0.Venue?.Components?.GetValueOrDefault("primary")?.ComponentId ?? ""),
                } } },
                VenueId = (request.VenueId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Venue?.VenueId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "ExplainVenueDecision", (request.Venue?.VenueId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.GetVenueRulesResponse> GetVenueRules(ApiCorporateevents.GetVenueRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.VenueId, "venue_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetVenueRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "venue", request.VenueId,
            "GetVenueRules", "accepted", token);
        var response = new ApiCorporateevents.GetVenueRulesResponse
        {
            Venue = new ApiCorporateevents.Venue
            {
                VenueId = (request.VenueId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lints a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.LintVenuesResponse> LintVenues(ApiCorporateevents.LintVenuesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling LintVenues for {Tenant}", requestContext.TenantId);
        if (request.Venue is { } record)
        {
            var snapshot = VenueMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.ExplainVenueCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.ExplainVenueCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Venue?.Owner?.Email ?? ""),
                },
            },
            Venue = new ApiCorporateevents.Venue
            {
                Address = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                    {
                        ApiCorporateevents.Venue.LocatorOneofCase.Address => (request.Venue?.Address?.CountryCode ?? ""),
                        _ => ""
                    },
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Venue?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (request.Venue?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        using var rpc0 = _corporateeventsVenueCalculationService.ExplainVenueCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.CompareVenueOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.CompareVenueOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (reply0.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                    {
                        ApiCorporateevents.Venue.LocatorOneofCase.Address => (reply0.Venue?.Address?.Region ?? ""),
                        _ => ""
                    },
                },
            },
            Venue = new ApiCorporateevents.Venue
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Venue?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Dimensions = new ApiCorporateevents.VenueDimensions
                {
                    Unit = (reply0.Venue?.Dimensions?.Unit ?? ""),
                    Width = (request.Venue?.Dimensions?.Width ?? 0d),
                },
            },
        };
        using var rpc1 = _corporateeventsVenueCalculationService.CompareVenueOptionsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDocuments.CompareDocumentTemplateOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.CompareDocumentTemplateOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                    {
                        ApiCorporateevents.Venue.LocatorOneofCase.Address => (reply0.Venue?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()),
                        _ => new Google.Protobuf.Collections.RepeatedField<string>()
                    } },
                },
            },
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.Venue?.Audit?.UpdatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (request.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                    {
                        ApiCorporateevents.Venue.LocatorOneofCase.Address => (request.Venue?.Address?.Recipient ?? ""),
                        _ => ""
                    },
                },
                Lines = { new ApiDocuments.DocumentTemplateLine
                {
                    Quantity = (request.Venue?.Components?.GetValueOrDefault("primary")?.Quantity ?? 0L),
                } },
            },
        };
        using var rpc2 = _documentsDocumentTemplateCalculationService.CompareDocumentTemplateOptionsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiCorporateevents.LintVenuesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply2.Result?.Message ?? ""),
                Status = (reply2.Result?.Status ?? ""),
            },
            Venue = new ApiCorporateevents.Venue
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.Venue?.Audit?.UpdatedBy ?? ""),
                },
                CategoryPath = { (reply0.Venue?.CategoryPath ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Name = (reply0.Venue?.Name ?? ""),
                Owner = (request.Venue?.Owner ?? new ApiIdentity.UserRef()),
                VenueId = (request.VenueId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Venue?.VenueId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "LintVenues", (request.Venue?.VenueId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Normalizes a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.NormalizeVenueResponse> NormalizeVenue(ApiCorporateevents.NormalizeVenueRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling NormalizeVenue for {Tenant}", requestContext.TenantId);
        if (request.Venue is { } record)
        {
            var snapshot = VenueMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.ListIncidentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.ListIncidentsRequest.Types.Filter
            {
                CreatedAfter = (request.Venue?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.Venue?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Venue?.Owner?.UserId ?? ""),
                Region = (request.Venue?.Address?.Region ?? ""),
            },
        };
        using var rpc0 = _itsmIncidentService.ListIncidentsAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiItsm.NormalizeIncidentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Incident = new ApiItsm.Incident
            {
                Customer = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Venue?.Owner?.DisplayName ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc1 = _itsmIncidentValidationService.NormalizeIncidentAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiCorporateevents.NormalizeVenueResponse
        {
            Outcome = new ApiCorporateevents.NormalizeVenueResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            Venue = new ApiCorporateevents.Venue
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply1.Incident?.Customer?.Email ?? ""),
                },
                Specs = { (request.Venue?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                VenueId = (request.VenueId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Venue?.VenueId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "NormalizeVenue", (request.Venue?.VenueId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.PreviewVenueResponse> PreviewVenue(ApiCorporateevents.PreviewVenueRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PreviewVenue for {Tenant}", requestContext.TenantId);
        if (request.Venue is { } record)
        {
            var snapshot = VenueMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.SimulateVenueRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Venue = new ApiCorporateevents.Venue
            {
                Components = { { "primary", new ApiCorporateevents.VenueComponent
                {
                    Quantity = (request.Venue?.Components?.GetValueOrDefault("primary")?.Quantity ?? 0L),
                } } },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Venue?.Owner?.Email ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiCorporateevents.SimulateVenueRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _corporateeventsVenueCalculationService.SimulateVenueAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiCorporateevents.PreviewVenueRequest, ApiCorporateevents.ExplainVenueCalculationRequest>(value => new ApiCorporateevents.ExplainVenueCalculationRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.ExplainVenueCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (value.Details?.Contact?.Phone ?? ""),
                },
            },
            Venue = new ApiCorporateevents.Venue
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Venue?.Audit?.Etag ?? ""),
                },
            },
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _corporateeventsVenueCalculationService.ExplainVenueCalculationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiCorporateevents.CheckRegistrationEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.CheckRegistrationEligibilityRequest.Types.Details
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
                    Etag = (reply0.Venue?.Audit?.Etag ?? ""),
                },
                Holder = new ApiCorporateevents.CorporateEventRef
                {
                    DisplayName = (request.Venue?.Owner?.DisplayName ?? ""),
                },
            },
        };
        input2 = PropertyMap.Copy(input2);
        using var rpc2 = _corporateeventsRegistrationValidationService.CheckRegistrationEligibilityAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiCorporateevents.PreviewVenueResponse
        {
            Venue = new ApiCorporateevents.Venue
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply2.Registration?.Audit?.Etag ?? ""),
                    UpdatedAt = (reply2.Registration?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (reply0.Venue?.Audit?.UpdatedBy ?? ""),
                },
                Name = (request.Venue?.Name ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (reply0.Venue?.Owner?.UserId ?? ""),
                },
                Tags = { (reply1.Venue?.Tags ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                VenueId = (request.VenueId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Venue?.VenueId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "PreviewVenue", (request.Venue?.VenueId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.ScoreVenueResponse> ScoreVenue(ApiCorporateevents.ScoreVenueRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ScoreVenue for {Tenant}", requestContext.TenantId);
        if (request.Venue is { } record)
        {
            var snapshot = VenueMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.ScoreVenueToExplainSlaPolicyCalculation0();
        using var rpc0 = _helpdeskSlaPolicyCalculationService.ExplainSlaPolicyCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiCorporateevents.ScoreVenueResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Venue = new ApiCorporateevents.Venue
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.SlaPolicy?.Audit?.UpdatedBy ?? ""),
                },
                Code = (request.Venue?.Code ?? ""),
                VenueId = (request.VenueId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Venue?.VenueId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "ScoreVenue", (request.Venue?.VenueId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.ValidateVenueResponse> ValidateVenue(ApiCorporateevents.ValidateVenueRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ValidateVenue for {Tenant}", requestContext.TenantId);
        if (request.Venue is { } record)
        {
            var snapshot = VenueMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.PreviewRegistrationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Registration = new ApiCorporateevents.Registration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Venue?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedBy = (request.Venue?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        var reply0 = await SendValidateVenue0Async(input0, token);
        var input1 = new ApiCorporateevents.CalculateCorporateEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Registration?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Name = (reply0.Registration?.Contact?.Name ?? new ApiCommon.PersonName()),
                        PreferredChannel = (reply0.Registration?.Contact?.PreferredChannel ?? ""),
                    },
                } },
            },
            Details = new ApiCorporateevents.CalculateCorporateEventRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Venue?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
        };
        using var rpc1 = _corporateeventsCorporateEventCalculationService.CalculateCorporateEventAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        var input2 = new ApiDocuments.CompareSignatureVersionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.CompareSignatureVersionsRequest.Types.Details
            {
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Signature = new ApiDocuments.Signature
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (request.Venue?.Owner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc2 = _documentsSignatureHistoryService.CompareSignatureVersionsAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        await Task.WhenAll(pending1, pending2);
        var reply1 = await pending1;
        var reply2 = await pending2;
        var response = new ApiCorporateevents.ValidateVenueResponse
        {
            Outcome = new ApiCorporateevents.ValidateVenueResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Venue = new ApiCorporateevents.Venue
            {
                Name = (request.Venue?.Name ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply2.Signature?.Subject?.DisplayName ?? ""),
                    UserId = (reply1.CorporateEvent?.Organizer?.UserId ?? ""),
                },
                VenueId = (request.VenueId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Venue?.VenueId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "ValidateVenue", (request.Venue?.VenueId ?? ""), token);
        return response;
    }
    private async Task<ApiCorporateevents.PreviewRegistrationResponse> SendValidateVenue0Async(ApiCorporateevents.PreviewRegistrationRequest request, CancellationToken token)
    {
        return await _venueValidationEndpointSendValidateVenue0Port.SendAsync(request, token);
    }
}
