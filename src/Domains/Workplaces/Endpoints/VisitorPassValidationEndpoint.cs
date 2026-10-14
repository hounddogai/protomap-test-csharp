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
using ApiAdmissions = Acme.Admissions.V1;
using ApiCommon = Acme.Common.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiSearch = Acme.Search.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

/// <summary>
/// Checks visitor passes against the rules before anything acts on them.
/// </summary>
public sealed class VisitorPassValidationEndpoint : ApiWorkplaces.VisitorPassValidationService.VisitorPassValidationServiceBase
{
    private readonly ApiAdmissions.AdmissionService.AdmissionServiceClient _admissionsAdmissionService;
    private readonly ApiHelpdesk.KnowledgeArticleCalculationService.KnowledgeArticleCalculationServiceClient _helpdeskKnowledgeArticleCalculationService;
    private readonly ApiItsm.ChangeRequestCalculationService.ChangeRequestCalculationServiceClient _itsmChangeRequestCalculationService;
    private readonly ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient _licensingSoftwareLicenseCalculationService;
    private readonly ILogger<VisitorPassValidationEndpoint> _logger;
    private readonly IRecordRepository _repository;
    private readonly ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient _searchSearchIndexQueryService;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiWorkplaces.BuildingCalculationService.BuildingCalculationServiceClient _workplacesBuildingCalculationService;
    private readonly ApiWorkplaces.DeskValidationService.DeskValidationServiceClient _workplacesDeskValidationService;
    private readonly ApiWorkplaces.RoomBookingCalculationService.RoomBookingCalculationServiceClient _workplacesRoomBookingCalculationService;
    private readonly ApiWorkplaces.RoomBookingValidationService.RoomBookingValidationServiceClient _workplacesRoomBookingValidationService;
    private readonly ApiWorkplaces.VisitorPassCalculationService.VisitorPassCalculationServiceClient _workplacesVisitorPassCalculationService;

    public VisitorPassValidationEndpoint(
        ApiAdmissions.AdmissionService.AdmissionServiceClient admissionsAdmissionService,
        ApiHelpdesk.KnowledgeArticleCalculationService.KnowledgeArticleCalculationServiceClient helpdeskKnowledgeArticleCalculationService,
        ApiItsm.ChangeRequestCalculationService.ChangeRequestCalculationServiceClient itsmChangeRequestCalculationService,
        ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient licensingSoftwareLicenseCalculationService,
        ILogger<VisitorPassValidationEndpoint> logger,
        IRecordRepository repository,
        ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient searchSearchIndexQueryService,
        IOptions<MeshOptions> settings,
        ApiWorkplaces.BuildingCalculationService.BuildingCalculationServiceClient workplacesBuildingCalculationService,
        ApiWorkplaces.DeskValidationService.DeskValidationServiceClient workplacesDeskValidationService,
        ApiWorkplaces.RoomBookingCalculationService.RoomBookingCalculationServiceClient workplacesRoomBookingCalculationService,
        ApiWorkplaces.RoomBookingValidationService.RoomBookingValidationServiceClient workplacesRoomBookingValidationService,
        ApiWorkplaces.VisitorPassCalculationService.VisitorPassCalculationServiceClient workplacesVisitorPassCalculationService)
    {
        _admissionsAdmissionService = admissionsAdmissionService;
        _helpdeskKnowledgeArticleCalculationService = helpdeskKnowledgeArticleCalculationService;
        _itsmChangeRequestCalculationService = itsmChangeRequestCalculationService;
        _licensingSoftwareLicenseCalculationService = licensingSoftwareLicenseCalculationService;
        _logger = logger;
        _repository = repository;
        _searchSearchIndexQueryService = searchSearchIndexQueryService;
        _settings = settings;
        _workplacesBuildingCalculationService = workplacesBuildingCalculationService;
        _workplacesDeskValidationService = workplacesDeskValidationService;
        _workplacesRoomBookingCalculationService = workplacesRoomBookingCalculationService;
        _workplacesRoomBookingValidationService = workplacesRoomBookingValidationService;
        _workplacesVisitorPassCalculationService = workplacesVisitorPassCalculationService;
    }
    /// <summary>
    /// Checks a visitor pass.
    /// </summary>
    public override async Task<ApiWorkplaces.CheckVisitorPassEligibilityResponse> CheckVisitorPassEligibility(ApiWorkplaces.CheckVisitorPassEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CheckVisitorPassEligibility for {Tenant}", requestContext.TenantId);
        if (request.VisitorPass is { } record)
        {
            var snapshot = VisitorPassMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiHelpdesk.SimulateKnowledgeArticleResponse();
        foreach (var item in (request.VisitorPass?.Attachments ?? new Google.Protobuf.Collections.RepeatedField<ApiWorkplaces.VisitorPassAttachment>()))
        {
            var input0 = new ApiHelpdesk.SimulateKnowledgeArticleRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
                {
                    Attachments = { new ApiHelpdesk.KnowledgeArticleAttachment
                    {
                        Checksum = (item.Checksum ?? ""),
                        FileName = (item.FileName ?? ""),
                        Uri = (item.Uri ?? ""),
                    } },
                },
            };
            using var rpc0 = _helpdeskKnowledgeArticleCalculationService.SimulateKnowledgeArticleAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var response = new ApiWorkplaces.CheckVisitorPassEligibilityResponse
        {
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                Notes = (request.VisitorPass?.Notes ?? ""),
                VisitorPassId = (request.VisitorPassId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.VisitorPass?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass",
            "audit", "CheckVisitorPassEligibility", (request.VisitorPass?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a visitor pass.
    /// </summary>
    public override async Task<ApiWorkplaces.ExplainVisitorPassDecisionResponse> ExplainVisitorPassDecision(ApiWorkplaces.ExplainVisitorPassDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExplainVisitorPassDecision for {Tenant}", requestContext.TenantId);
        if (request.VisitorPass is { } record)
        {
            var snapshot = VisitorPassMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWorkplaces.QuoteVisitorPassRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                Bank = new ApiWorkplaces.VisitorPassBankSettlement
                {
                    AccountHolder = (request.VisitorPass?.Bank?.AccountHolder ?? ""),
                },
            },
        };
        using var rpc0 = _workplacesVisitorPassCalculationService.QuoteVisitorPassAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiWorkplaces.NormalizeRoomBookingRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Participants = { new ApiWorkplaces.RoomBookingParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.VisitorPass?.Counterparty?.Email ?? ""),
                    },
                } },
            },
        };
        using var rpc1 = _workplacesRoomBookingValidationService.NormalizeRoomBookingAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiWorkplaces.ExplainVisitorPassDecisionResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply1.RoomBooking?.Location?.Locality ?? ""),
                    Region = (reply0.VisitorPass?.BillingAddress?.Region ?? ""),
                },
                Number = (request.VisitorPass?.Number ?? ""),
                VisitorPassId = (request.VisitorPassId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.VisitorPass?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass",
            "audit", "ExplainVisitorPassDecision", (request.VisitorPass?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a visitor pass.
    /// </summary>
    public override async Task<ApiWorkplaces.GetVisitorPassRulesResponse> GetVisitorPassRules(ApiWorkplaces.GetVisitorPassRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.VisitorPassId, "visitor_pass_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetVisitorPassRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass", request.VisitorPassId,
            "GetVisitorPassRules", "accepted", token);
        var input0 = new ApiWorkplaces.ExplainRoomBookingCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiWorkplaces.ExplainRoomBookingCalculationRequest.Types.Details
            {
                Locale = (request.Context?.Locale ?? ""),
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiWorkplaces.ExplainRoomBookingCalculationRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _workplacesRoomBookingCalculationService.ExplainRoomBookingCalculationAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiSearch.ExportSearchIndexesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiSearch.ExportSearchIndexesRequest.Types.Filter
            {
                Region = (reply0.RoomBooking?.Location?.Region ?? ""),
            },
        };
        using var stream1 = _searchSearchIndexQueryService.ExportSearchIndexes(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportSearchIndexes frame {Type}", change1.GetType().Name);
        }
        var response = new ApiWorkplaces.GetVisitorPassRulesResponse
        {
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.RoomBooking?.Audit?.UpdatedBy ?? ""),
                },
                VisitorPassId = (request.VisitorPassId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Normalizes a visitor pass.
    /// </summary>
    public override async Task<ApiWorkplaces.NormalizeVisitorPassResponse> NormalizeVisitorPass(ApiWorkplaces.NormalizeVisitorPassRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling NormalizeVisitorPass for {Tenant}", requestContext.TenantId);
        if (request.VisitorPass is { } record)
        {
            var snapshot = VisitorPassMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWorkplaces.ValidateDeskRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Desk = new ApiWorkplaces.Desk
            {
                Address = new ApiCommon.PostalAddress
                {
                    Lines = { (request.VisitorPass?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    PostalCode = (request.VisitorPass?.BillingAddress?.PostalCode ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.VisitorPass?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
            Details = new ApiWorkplaces.ValidateDeskRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.VisitorPass?.Counterparty?.Email ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _workplacesDeskValidationService.ValidateDeskAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.NormalizeVisitorPassToPreviewDesk1();
        using var rpc1 = _workplacesDeskValidationService.PreviewDeskAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiWorkplaces.ExplainRoomBookingCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Location = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                    {
                        ApiWorkplaces.Desk.LocatorOneofCase.Address => (reply0.Desk?.Address?.CountryCode ?? ""),
                        _ => ""
                    },
                    Locality = (request.VisitorPass?.BillingAddress?.Locality ?? ""),
                    Region = (reply0.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                    {
                        ApiWorkplaces.Desk.LocatorOneofCase.Address => (reply0.Desk?.Address?.Region ?? ""),
                        _ => ""
                    },
                },
                Participants = { new ApiWorkplaces.RoomBookingParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.VisitorPass?.Counterparty?.Email ?? ""),
                    },
                } },
            },
        };
        using var rpc2 = _workplacesRoomBookingCalculationService.ExplainRoomBookingCalculationAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiWorkplaces.NormalizeVisitorPassResponse
        {
            Outcome = new ApiWorkplaces.NormalizeVisitorPassResponse.Types.Outcome
            {
                NextSteps = { (reply1.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Message = (reply1.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply2.RoomBooking?.Audit?.CreatedAt ?? new Timestamp()),
                },
                IssuedAt = (request.VisitorPass?.IssuedAt ?? new Timestamp()),
                VisitorPassId = (request.VisitorPassId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.VisitorPass?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass",
            "audit", "NormalizeVisitorPass", (request.VisitorPass?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews a visitor pass.
    /// </summary>
    public override async Task<ApiWorkplaces.PreviewVisitorPassResponse> PreviewVisitorPass(ApiWorkplaces.PreviewVisitorPassRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PreviewVisitorPass for {Tenant}", requestContext.TenantId);
        if (request.VisitorPass is { } record)
        {
            var snapshot = VisitorPassMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWorkplaces.CompareRoomBookingOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiWorkplaces.CompareRoomBookingOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Recipient = (request.VisitorPass?.BillingAddress?.Recipient ?? ""),
                },
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Participants = { new ApiWorkplaces.RoomBookingParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                    },
                } },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _workplacesRoomBookingCalculationService.CompareRoomBookingOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiWorkplaces.PreviewRoomBookingRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Participants = { new ApiWorkplaces.RoomBookingParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply0.RoomBooking?.Organizer?.Email ?? ""),
                    },
                } },
            },
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiWorkplaces.PreviewRoomBookingRequest>();
        using var rpc1 = _workplacesRoomBookingValidationService.PreviewRoomBookingAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.PreviewVisitorPassResponse
        {
            Outcome = new ApiWorkplaces.PreviewVisitorPassResponse.Types.Outcome
            {
                Status = (reply1.Outcome?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Outcome?.Status ?? ""),
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                Metadata = { (reply0.RoomBooking?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (request.VisitorPass?.Status ?? (ApiWorkplaces.VisitorPassStatus)0),
                VisitorPassId = (request.VisitorPassId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.VisitorPass?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass",
            "audit", "PreviewVisitorPass", (request.VisitorPass?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores a visitor pass.
    /// </summary>
    public override async Task<ApiWorkplaces.ScoreVisitorPassResponse> ScoreVisitorPass(ApiWorkplaces.ScoreVisitorPassRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ScoreVisitorPass for {Tenant}", requestContext.TenantId);
        if (request.VisitorPass is { } record)
        {
            var snapshot = VisitorPassMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWorkplaces.ExplainRoomBookingCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.VisitorPass?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Location = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Locality = (request.Details?.Destination?.Locality ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiWorkplaces.ExplainRoomBookingCalculationRequest>(json0);
        using var rpc0 = _workplacesRoomBookingCalculationService.ExplainRoomBookingCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiWorkplaces.CalculateRoomBookingRequest();
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        input1.Details = new ApiWorkplaces.CalculateRoomBookingRequest.Types.Details
        {
            Destination = new ApiCommon.PostalAddress
            {
                Lines = { (request.VisitorPass?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Recipient = (reply0.RoomBooking?.Location?.Recipient ?? ""),
            },
        };
        input1.RoomBooking = new ApiWorkplaces.RoomBooking
        {
            Organizer = new ApiIdentity.UserRef
            {
                DisplayName = (reply0.RoomBooking?.Organizer?.DisplayName ?? ""),
            },
            Participants = { new ApiWorkplaces.RoomBookingParticipant
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.RoomBooking?.Organizer?.Email ?? ""),
                },
            } },
        };
        using var rpc1 = _workplacesRoomBookingCalculationService.CalculateRoomBookingAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.ScoreVisitorPassResponse
        {
            Breakdown = new ApiWorkplaces.ScoreVisitorPassResponse.Types.Breakdown
            {
                CurrencyCode = (request.Details?.CurrencyCode ?? ""),
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply1.RoomBooking?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (reply0.RoomBooking?.Organizer?.Email ?? ""),
                },
                VisitorPassId = (request.VisitorPassId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.VisitorPass?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass",
            "audit", "ScoreVisitorPass", (request.VisitorPass?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates a visitor pass.
    /// </summary>
    public override async Task<ApiWorkplaces.ValidateVisitorPassResponse> ValidateVisitorPass(ApiWorkplaces.ValidateVisitorPassRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ValidateVisitorPass for {Tenant}", requestContext.TenantId);
        if (request.VisitorPass is { } record)
        {
            var snapshot = VisitorPassMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.EstimateChangeRequestRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Customer = new ApiItsm.IncidentRef
                {
                    DisplayName = (request.VisitorPass?.Counterparty?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.EstimateChangeRequestRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Recipient = (request.VisitorPass?.BillingAddress?.Recipient ?? ""),
                },
            },
        };
        using var rpc0 = _itsmChangeRequestCalculationService.EstimateChangeRequestAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiAdmissions.WatchAdmissionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (request.Details?.Contact?.Email ?? ""),
                Phone = (reply0.ChangeRequest?.Requester?.Phone ?? ""),
                PreferredChannel = (reply0.ChangeRequest?.Requester?.PreferredChannel ?? ""),
            },
        };
        using var stream1 = _admissionsAdmissionService.WatchAdmissions(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchAdmissions frame {Type}", change1.GetType().Name);
        }
        var input2 = new ApiLicensing.QuoteSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.VisitorPass?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Bank = new ApiLicensing.SoftwareLicenseBankFunding
                {
                    Iban = (request.VisitorPass ?? new ApiWorkplaces.VisitorPass()).PaymentCase switch
                    {
                        ApiWorkplaces.VisitorPass.SettlementOneofCase.Bank => (request.VisitorPass?.Bank?.Iban ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc2 = _licensingSoftwareLicenseCalculationService.QuoteSoftwareLicenseAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiWorkplaces.RecalculateBuildingsRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            UpdatedBy = (reply0.ChangeRequest?.Audit?.UpdatedBy ?? ""),
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply3 = await Workflow.SendAsync(input3, token, _workplacesBuildingCalculationService.RecalculateBuildingsAsync);
        var response = new ApiWorkplaces.ValidateVisitorPassResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply2.Result?.Status ?? ""),
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply3.Building?.Audit?.Etag ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply3.Building?.Location?.PostalCode ?? ""),
                },
                Status = (request.VisitorPass?.Status ?? (ApiWorkplaces.VisitorPassStatus)0),
                VisitorPassId = (request.VisitorPassId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.VisitorPass?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass",
            "audit", "ValidateVisitorPass", (request.VisitorPass?.VoucherCode ?? ""), token);
        return response;
    }
}
