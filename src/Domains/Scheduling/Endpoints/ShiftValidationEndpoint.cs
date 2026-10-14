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
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Scheduling;

/// <summary>
/// Checks shifts against the rules before anything acts on them.
/// </summary>
public sealed class ShiftValidationEndpoint : ApiScheduling.ShiftValidationService.ShiftValidationServiceBase
{
    private readonly ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient _authorizationAccessPolicyService;
    private readonly ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient _corporateeventsCorporateEventCalculationService;
    private readonly ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient _documentsDocumentTemplateSyncService;
    private readonly ApiDocuments.SignatureAdminService.SignatureAdminServiceClient _documentsSignatureAdminService;
    private readonly ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient _documentsSignatureHistoryService;
    private readonly ILogger<ShiftValidationEndpoint> _logger;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiScheduling.RosterCalculationService.RosterCalculationServiceClient _schedulingRosterCalculationService;
    private readonly Func<ApiScheduling.ExplainRosterCalculationRequest, CallOptions, AsyncUnaryCall<ApiScheduling.ExplainRosterCalculationResponse>> _sendPreviewShift0;
    private readonly IShiftValidationEndpointSendValidateShift2Port _shiftValidationEndpointSendValidateShift2Port;
    private readonly ApiWorkplaces.BuildingValidationService.BuildingValidationServiceClient _workplacesBuildingValidationService;

    public ShiftValidationEndpoint(
        ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient authorizationAccessPolicyService,
        ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient corporateeventsCorporateEventCalculationService,
        ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient documentsDocumentTemplateSyncService,
        ApiDocuments.SignatureAdminService.SignatureAdminServiceClient documentsSignatureAdminService,
        ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient documentsSignatureHistoryService,
        ILogger<ShiftValidationEndpoint> logger,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiScheduling.RosterCalculationService.RosterCalculationServiceClient schedulingRosterCalculationService,
        IShiftValidationEndpointSendValidateShift2Port shiftValidationEndpointSendValidateShift2PortAdapter,
        ApiWorkplaces.BuildingValidationService.BuildingValidationServiceClient workplacesBuildingValidationService)
    {
        _authorizationAccessPolicyService = authorizationAccessPolicyService;
        _corporateeventsCorporateEventCalculationService = corporateeventsCorporateEventCalculationService;
        _documentsDocumentTemplateSyncService = documentsDocumentTemplateSyncService;
        _documentsSignatureAdminService = documentsSignatureAdminService;
        _documentsSignatureHistoryService = documentsSignatureHistoryService;
        _logger = logger;
        _registry = registry;
        _repository = repository;
        _schedulingRosterCalculationService = schedulingRosterCalculationService;
        _shiftValidationEndpointSendValidateShift2Port = shiftValidationEndpointSendValidateShift2PortAdapter;
        _workplacesBuildingValidationService = workplacesBuildingValidationService;
        _sendPreviewShift0 = _schedulingRosterCalculationService.ExplainRosterCalculationAsync;
    }
    /// <summary>
    /// Checks a shift.
    /// </summary>
    public override async Task<ApiScheduling.CheckShiftEligibilityResponse> CheckShiftEligibility(ApiScheduling.CheckShiftEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CheckShiftEligibility for {Tenant}", requestContext.TenantId);
        if (request.Shift is { } record)
        {
            var snapshot = ShiftMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiScheduling.CalculateRosterRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Roster = new ApiScheduling.Roster
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Shift?.Audit?.CreatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Shift?.Location?.CountryCode ?? ""),
                    Region = (request.Shift?.Location?.Region ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Shift?.Organizer?.DisplayName ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _schedulingRosterCalculationService.CalculateRosterAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiScheduling.EstimateRosterRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Roster = new ApiScheduling.Roster
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.Roster?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Recipient = (request.Shift?.Location?.Recipient ?? ""),
                },
                Card = new ApiScheduling.RosterCardSettlement
                {
                    Token = (reply0.Roster ?? new ApiScheduling.Roster()).PaymentCase switch
                    {
                        ApiScheduling.Roster.SettlementOneofCase.Card => (reply0.Roster?.Card?.Token ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _schedulingRosterCalculationService.EstimateRosterAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var batch2 = (request.Shift?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.ShiftParticipant>()).Select(async item =>
        {
            var input2 = new ApiAuthorization.UpdateAccessPolicyRequest
            {
                AccessPolicy = new ApiAuthorization.AccessPolicy
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        Etag = (request.Shift?.Audit?.Etag ?? ""),
                        UpdatedBy = (reply0.Roster?.Audit?.UpdatedBy ?? ""),
                    },
                    Owner = new ApiIdentity.UserRef
                    {
                        Email = (item.Contact?.Email ?? ""),
                    },
                },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
            };
            using var rpc2 = _authorizationAccessPolicyService.UpdateAccessPolicyAsync(input2, cancellationToken: token);
            var reply2 = await rpc2.ResponseAsync;
            return reply2;
        }).ToArray();
        var replies2 = await Task.WhenAll(batch2);
        var reply2 = replies2.LastOrDefault() ?? new ApiAuthorization.UpdateAccessPolicyResponse();
        var input3 = new ApiScheduling.ExplainRosterCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Roster = new ApiScheduling.Roster
            {
                Bank = new ApiScheduling.RosterBankSettlement
                {
                    AccountHolder = (reply0.Roster ?? new ApiScheduling.Roster()).PaymentCase switch
                    {
                        ApiScheduling.Roster.SettlementOneofCase.Bank => (reply0.Roster?.Bank?.AccountHolder ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc3 = _schedulingRosterCalculationService.ExplainRosterCalculationAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiScheduling.CheckShiftEligibilityResponse
        {
            Shift = new ApiScheduling.Shift
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Roster?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Location = new ApiCommon.PostalAddress
                {
                    Region = (reply0.Roster?.BillingAddress?.Region ?? ""),
                },
                Metadata = { (reply3.Roster?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (reply1.Roster?.Counterparty?.Email ?? ""),
                },
                ShiftId = (request.ShiftId ?? ""),
                Title = (request.Shift?.Title ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Shift?.ShiftId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "shift",
            "audit", "CheckShiftEligibility", (request.Shift?.ShiftId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a shift.
    /// </summary>
    public override async Task<ApiScheduling.ExplainShiftDecisionResponse> ExplainShiftDecision(ApiScheduling.ExplainShiftDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExplainShiftDecision for {Tenant}", requestContext.TenantId);
        if (request.Shift is { } record)
        {
            var snapshot = ShiftMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.RepairSignatureRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.RepairSignatureRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Reason = (request.Reason ?? ""),
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                OccurredAt = (request.Shift?.Audit?.CreatedAt ?? new Timestamp()),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (request.Shift?.Organizer?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _documentsSignatureAdminService.RepairSignatureAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.CompareSignatureVersionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.CompareSignatureVersionsRequest.Types.Details
            {
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Signature = new ApiDocuments.Signature
            {
                CorrelationId = (reply0.Signature?.CorrelationId ?? ""),
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Sequence = (reply0.Signature?.Sequence ?? 0L),
            },
        };
        using var rpc1 = _documentsSignatureHistoryService.CompareSignatureVersionsAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        var input2 = new ApiDocuments.ListDocumentTemplateConflictsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentTemplateConflictsRequest.Types.Filter
            {
                CreatedAfter = (reply0.Signature?.OccurredAt ?? new Timestamp()),
                OwnerId = (request.Shift?.Organizer?.UserId ?? ""),
                Region = (request.Shift?.Location?.Region ?? ""),
            },
        };
        using var rpc2 = _documentsDocumentTemplateSyncService.ListDocumentTemplateConflictsAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        await Task.WhenAll(pending1, pending2);
        var reply1 = await pending1;
        var reply2 = await pending2;
        var input3 = new ApiScheduling.SimulateRosterRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Roster = new ApiScheduling.Roster
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (request.Shift?.Location?.Locality ?? ""),
                },
            },
        };
        using var rpc3 = _schedulingRosterCalculationService.SimulateRosterAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiScheduling.ExplainShiftDecisionResponse
        {
            Outcome = new ApiScheduling.ExplainShiftDecisionResponse.Types.Outcome
            {
                Message = (reply2.Result?.Message ?? ""),
                NextSteps = { (reply0.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                ReferenceId = (reply1.Outcome?.ReferenceId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Shift = new ApiScheduling.Shift
            {
                Location = new ApiCommon.PostalAddress
                {
                    Region = (reply3.Roster?.BillingAddress?.Region ?? ""),
                },
                ShiftId = (request.ShiftId ?? ""),
                Window = (request.Shift?.Window ?? new ApiCommon.TimeWindow()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Shift?.Title ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "shift",
            "audit", "ExplainShiftDecision", (request.Shift?.Title ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lints a shift.
    /// </summary>
    public override async Task<ApiScheduling.LintShiftsResponse> LintShifts(ApiScheduling.LintShiftsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling LintShifts for {Tenant}", requestContext.TenantId);
        if (request.Shift is { } record)
        {
            var snapshot = ShiftMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiScheduling.EstimateRosterRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiScheduling.EstimateRosterRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Shift?.Location?.PostalCode ?? ""),
                },
            },
            Roster = new ApiScheduling.Roster
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Shift?.Audit?.Etag ?? ""),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _schedulingRosterCalculationService.EstimateRosterAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiScheduling.LintShiftsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Shift = new ApiScheduling.Shift
            {
                Location = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.Roster?.BillingAddress?.Locality ?? ""),
                },
                Organizer = (request.Shift?.Organizer ?? new ApiIdentity.UserRef()),
                ShiftId = (request.ShiftId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Shift?.Title ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "shift",
            "audit", "LintShifts", (request.Shift?.Title ?? ""), token);
        return response;
    }
    /// <summary>
    /// Normalizes a shift.
    /// </summary>
    public override async Task<ApiScheduling.NormalizeShiftResponse> NormalizeShift(ApiScheduling.NormalizeShiftRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling NormalizeShift for {Tenant}", requestContext.TenantId);
        if (request.Shift is { } record)
        {
            var snapshot = ShiftMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiWorkplaces.ExplainBuildingDecisionResponse();
        foreach (var item in (request.Shift?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.ShiftParticipant>()))
        {
            var input0 = new ApiWorkplaces.ExplainBuildingDecisionRequest
            {
                Building = new ApiWorkplaces.Building
                {
                    Manager = new ApiIdentity.UserRef
                    {
                        Email = (item.Contact?.Email ?? ""),
                    },
                },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiWorkplaces.ExplainBuildingDecisionRequest.Types.Details
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Name = (item.Contact?.Name ?? new ApiCommon.PersonName()),
                    },
                },
            };
            using var rpc0 = _workplacesBuildingValidationService.ExplainBuildingDecisionAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var reply1 = new ApiCorporateevents.ExplainCorporateEventCalculationResponse();
        foreach (var item in (request.Shift?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.ShiftParticipant>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input1 = new ApiCorporateevents.ExplainCorporateEventCalculationRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                CorporateEvent = new ApiCorporateevents.CorporateEvent
                {
                    Participants = { new ApiCorporateevents.CorporateEventParticipant
                    {
                        Contact = new ApiCommon.ContactInfo
                        {
                            Phone = (item.Contact?.Phone ?? ""),
                            PreferredChannel = (item.Contact?.PreferredChannel ?? ""),
                        },
                    } },
                },
                Details = new ApiCorporateevents.ExplainCorporateEventCalculationRequest.Types.Details
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Name = (item.Contact?.Name ?? new ApiCommon.PersonName()),
                    },
                },
            };
            using var rpc1 = _corporateeventsCorporateEventCalculationService.ExplainCorporateEventCalculationAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var response = new ApiScheduling.NormalizeShiftResponse
        {
            Shift = new ApiScheduling.Shift
            {
                Organizer = (request.Shift?.Organizer ?? new ApiIdentity.UserRef()),
                ShiftId = (request.ShiftId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Shift?.Title ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "shift",
            "audit", "NormalizeShift", (request.Shift?.Title ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews a shift.
    /// </summary>
    public override async Task<ApiScheduling.PreviewShiftResponse> PreviewShift(ApiScheduling.PreviewShiftRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PreviewShift for {Tenant}", requestContext.TenantId);
        if (request.Shift is { } record)
        {
            var snapshot = ShiftMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiScheduling.ExplainRosterCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiScheduling.ExplainRosterCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Contact?.Phone ?? ""),
                },
            },
            Roster = new ApiScheduling.Roster
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Shift?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Shift?.Location?.Region ?? ""),
                },
            },
        };
        using var rpc0 = _sendPreviewShift0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiScheduling.PreviewShiftResponse
        {
            Outcome = new ApiScheduling.PreviewShiftResponse.Types.Outcome
            {
                EffectiveAt = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
            },
            Shift = new ApiScheduling.Shift
            {
                Location = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.Roster?.BillingAddress?.Recipient ?? ""),
                },
                ShiftId = (request.ShiftId ?? ""),
                Window = (request.Shift?.Window ?? new ApiCommon.TimeWindow()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Shift?.Title ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "shift",
            "audit", "PreviewShift", (request.Shift?.Title ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores a shift.
    /// </summary>
    public override async Task<ApiScheduling.ScoreShiftResponse> ScoreShift(ApiScheduling.ScoreShiftRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ScoreShift for {Tenant}", requestContext.TenantId);
        if (request.Shift is { } record)
        {
            var snapshot = ShiftMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiScheduling.EstimateRosterRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiScheduling.EstimateRosterRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    PostalCode = (request.Shift?.Location?.PostalCode ?? ""),
                    Recipient = (request.Details?.Destination?.Recipient ?? ""),
                },
            },
            Roster = new ApiScheduling.Roster
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Shift?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        var named0 = _registry.Get<ApiScheduling.RosterCalculationService.RosterCalculationServiceClient>("acme.scheduling.v1.RosterCalculationService");
        using var rpc0 = named0.EstimateRosterAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiScheduling.SimulateRosterRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiScheduling.SimulateRosterRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.Roster?.BillingAddress?.Locality ?? ""),
                },
            },
            Roster = new ApiScheduling.Roster
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                },
                Card = new ApiScheduling.RosterCardSettlement
                {
                    Last4 = (reply0.Roster ?? new ApiScheduling.Roster()).PaymentCase switch
                    {
                        ApiScheduling.Roster.SettlementOneofCase.Card => (reply0.Roster?.Card?.Last4 ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _schedulingRosterCalculationService.SimulateRosterAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDocuments.PullDocumentTemplatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.PullDocumentTemplatesRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.Roster?.BillingAddress?.Locality ?? ""),
                },
            },
        };
        using var rpc2 = _documentsDocumentTemplateSyncService.PullDocumentTemplatesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var reply3 = new ApiScheduling.ExplainRosterCalculationResponse();
        foreach (var item in (request.Shift?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.ShiftParticipant>()))
        {
            var input3 = new ApiScheduling.ExplainRosterCalculationRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiScheduling.ExplainRosterCalculationRequest.Types.Details
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Name = (item.Contact?.Name ?? new ApiCommon.PersonName()),
                        PreferredChannel = (item.Contact?.PreferredChannel ?? ""),
                    },
                },
                Roster = new ApiScheduling.Roster
                {
                    Counterparty = new ApiEmployees.EmployeeRef
                    {
                        Email = (item.Contact?.Email ?? ""),
                    },
                },
            };
            using var rpc3 = _schedulingRosterCalculationService.ExplainRosterCalculationAsync(input3, cancellationToken: token);
            reply3 = await rpc3.ResponseAsync;
        }
        var response = new ApiScheduling.ScoreShiftResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Shift = new ApiScheduling.Shift
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Roster?.Audit?.Etag ?? ""),
                },
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (reply1.Roster?.Counterparty?.Email ?? ""),
                },
                ShiftId = (request.ShiftId ?? ""),
                Window = (request.Shift?.Window ?? new ApiCommon.TimeWindow()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Shift?.Title ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "shift",
            "audit", "ScoreShift", (request.Shift?.Title ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates a shift.
    /// </summary>
    public override async Task<ApiScheduling.ValidateShiftResponse> ValidateShift(ApiScheduling.ValidateShiftRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ValidateShift for {Tenant}", requestContext.TenantId);
        if (request.Shift is { } record)
        {
            var snapshot = ShiftMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiScheduling.RecalculateRostersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiScheduling.RecalculateRostersRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Shift?.Location?.PostalCode ?? ""),
                    Recipient = (request.Shift?.Location?.Recipient ?? ""),
                },
            },
            Roster = new ApiScheduling.Roster
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Shift?.Audit?.Etag ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Shift?.Location?.Region ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _schedulingRosterCalculationService.RecalculateRostersAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiScheduling.QuoteRosterRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Roster = new ApiScheduling.Roster
            {
                Bank = new ApiScheduling.RosterBankSettlement
                {
                    Iban = (reply0.Roster ?? new ApiScheduling.Roster()).PaymentCase switch
                    {
                        ApiScheduling.Roster.SettlementOneofCase.Bank => (reply0.Roster?.Bank?.Iban ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _schedulingRosterCalculationService.QuoteRosterAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiScheduling.CalculateRosterRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Roster = new ApiScheduling.Roster
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.Shift?.Audit?.UpdatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply1.Roster?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
        };
        var reply2 = await SendValidateShift2Async(input2, token);
        var response = new ApiScheduling.ValidateShiftResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Shift = new ApiScheduling.Shift
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Roster?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (reply1.Roster?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Location = new ApiCommon.PostalAddress
                {
                    Recipient = (reply2.Roster?.BillingAddress?.Recipient ?? ""),
                },
                Organizer = new ApiIdentity.UserRef
                {
                    DisplayName = (reply2.Roster?.Counterparty?.DisplayName ?? ""),
                },
                ShiftId = (request.ShiftId ?? ""),
                Window = (request.Shift?.Window ?? new ApiCommon.TimeWindow()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Shift?.Title ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "shift",
            "audit", "ValidateShift", (request.Shift?.Title ?? ""), token);
        return response;
    }
    private async Task<ApiScheduling.CalculateRosterResponse> SendValidateShift2Async(ApiScheduling.CalculateRosterRequest request, CancellationToken token)
    {
        return await _shiftValidationEndpointSendValidateShift2Port.SendAsync(request, token);
    }
}
