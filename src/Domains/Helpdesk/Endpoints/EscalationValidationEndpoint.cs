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
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Helpdesk;

/// <summary>
/// Checks escalations against the rules before anything acts on them.
/// </summary>
public sealed class EscalationValidationEndpoint : ApiHelpdesk.EscalationValidationService.EscalationValidationServiceBase
{
    private readonly ApiDevices.ComplianceScanValidationService.ComplianceScanValidationServiceClient _devicesComplianceScanValidationService;
    private readonly ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient _documentsDocumentFolderNotificationService;
    private readonly ApiHelpdesk.KnowledgeArticleCalculationService.KnowledgeArticleCalculationServiceClient _helpdeskKnowledgeArticleCalculationService;
    private readonly ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient _helpdeskKnowledgeArticleValidationService;
    private readonly ApiIdentity.UserService.UserServiceClient _identityUserService;
    private readonly ApiItsm.IncidentQueryService.IncidentQueryServiceClient _itsmIncidentQueryService;
    private readonly ApiItsm.IncidentService.IncidentServiceClient _itsmIncidentService;
    private readonly ApiLicensing.SoftwareLicenseService.SoftwareLicenseServiceClient _licensingSoftwareLicenseService;
    private readonly ILogger<EscalationValidationEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;

    public EscalationValidationEndpoint(
        ApiDevices.ComplianceScanValidationService.ComplianceScanValidationServiceClient devicesComplianceScanValidationService,
        ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient documentsDocumentFolderNotificationService,
        ApiHelpdesk.KnowledgeArticleCalculationService.KnowledgeArticleCalculationServiceClient helpdeskKnowledgeArticleCalculationService,
        ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient helpdeskKnowledgeArticleValidationService,
        ApiIdentity.UserService.UserServiceClient identityUserService,
        ApiItsm.IncidentQueryService.IncidentQueryServiceClient itsmIncidentQueryService,
        ApiItsm.IncidentService.IncidentServiceClient itsmIncidentService,
        ApiLicensing.SoftwareLicenseService.SoftwareLicenseServiceClient licensingSoftwareLicenseService,
        ILogger<EscalationValidationEndpoint> logger,
        RetryPolicy policy,
        IRecordRepository repository)
    {
        _devicesComplianceScanValidationService = devicesComplianceScanValidationService;
        _documentsDocumentFolderNotificationService = documentsDocumentFolderNotificationService;
        _helpdeskKnowledgeArticleCalculationService = helpdeskKnowledgeArticleCalculationService;
        _helpdeskKnowledgeArticleValidationService = helpdeskKnowledgeArticleValidationService;
        _identityUserService = identityUserService;
        _itsmIncidentQueryService = itsmIncidentQueryService;
        _itsmIncidentService = itsmIncidentService;
        _licensingSoftwareLicenseService = licensingSoftwareLicenseService;
        _logger = logger;
        _policy = policy;
        _repository = repository;
    }
    /// <summary>
    /// Checks an escalation.
    /// </summary>
    public override async Task<ApiHelpdesk.CheckEscalationEligibilityResponse> CheckEscalationEligibility(ApiHelpdesk.CheckEscalationEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CheckEscalationEligibility for {Tenant}", requestContext.TenantId);
        if (request.Escalation is { } record)
        {
            var snapshot = EscalationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.ExplainKnowledgeArticleDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.ExplainKnowledgeArticleDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Escalation?.Requester?.PreferredChannel ?? ""),
                },
            },
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Escalation ?? new ApiHelpdesk.Escalation()).OriginCase switch
                    {
                        ApiHelpdesk.Escalation.OriginOneofCase.Customer => (request.Escalation?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
                DueAt = (request.Escalation?.Sla?.DueAt ?? new Timestamp()),
            },
        };
        using var rpc0 = _helpdeskKnowledgeArticleValidationService.ExplainKnowledgeArticleDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDevices.LintComplianceScansRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (reply0.KnowledgeArticle?.Counterparty?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.LintComplianceScansRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Escalation?.Requester?.Email ?? ""),
                    Name = (request.Escalation?.Requester?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        using var rpc1 = _devicesComplianceScanValidationService.LintComplianceScansAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.CheckEscalationEligibilityResponse
        {
            Escalation = new ApiHelpdesk.Escalation
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply0.KnowledgeArticle?.Counterparty?.Email ?? ""),
                },
                EscalationId = (request.EscalationId ?? ""),
                Status = (request.Escalation?.Status ?? (ApiHelpdesk.EscalationStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "escalation",
            "audit", "CheckEscalationEligibility", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an escalation.
    /// </summary>
    public override async Task<ApiHelpdesk.GetEscalationRulesResponse> GetEscalationRules(ApiHelpdesk.GetEscalationRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EscalationId, "escalation_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetEscalationRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "escalation", request.EscalationId,
            "GetEscalationRules", "accepted", token);
        var response = new ApiHelpdesk.GetEscalationRulesResponse
        {
            Escalation = new ApiHelpdesk.Escalation
            {
                EscalationId = (request.EscalationId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lints an escalation.
    /// </summary>
    public override async Task<ApiHelpdesk.LintEscalationsResponse> LintEscalations(ApiHelpdesk.LintEscalationsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling LintEscalations for {Tenant}", requestContext.TenantId);
        if (request.Escalation is { } record)
        {
            var snapshot = EscalationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.ResolveIncidentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _itsmIncidentQueryService.ResolveIncidentsAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiItsm.FindIncidentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.FindIncidentsRequest.Types.Filter
            {
                CreatedAfter = (request.Escalation?.Audit?.UpdatedAt ?? new Timestamp()),
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _itsmIncidentQueryService.FindIncidentsAsync);
        var response = new ApiHelpdesk.LintEscalationsResponse
        {
            Escalation = new ApiHelpdesk.Escalation
            {
                CustomFields = { (request.Escalation?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                EscalationId = (request.EscalationId ?? ""),
            },
            Outcome = new ApiHelpdesk.LintEscalationsResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "escalation",
            "audit", "LintEscalations", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Normalizes an escalation.
    /// </summary>
    public override async Task<ApiHelpdesk.NormalizeEscalationResponse> NormalizeEscalation(ApiHelpdesk.NormalizeEscalationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling NormalizeEscalation for {Tenant}", requestContext.TenantId);
        if (request.Escalation is { } record)
        {
            var snapshot = EscalationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.ExplainKnowledgeArticleCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Escalation ?? new ApiHelpdesk.Escalation()).OriginCase switch
                    {
                        ApiHelpdesk.Escalation.OriginOneofCase.Customer => (request.Escalation?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _helpdeskKnowledgeArticleCalculationService.ExplainKnowledgeArticleCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.WatchSoftwareLicensesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLicensing.WatchSoftwareLicensesRequest.Types.Filter
            {
                Region = (reply0.KnowledgeArticle?.BillingAddress?.Region ?? ""),
            },
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (request.Escalation?.Assignee?.Email ?? ""),
                Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                PreferredChannel = (request.Escalation?.Requester?.PreferredChannel ?? ""),
            },
        };
        using var stream1 = _licensingSoftwareLicenseService.WatchSoftwareLicenses(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchSoftwareLicenses frame {Type}", change1.GetType().Name);
        }
        var response = new ApiHelpdesk.NormalizeEscalationResponse
        {
            Escalation = new ApiHelpdesk.Escalation
            {
                EscalationId = (request.EscalationId ?? ""),
                Subject = (request.Escalation?.Subject ?? ""),
            },
            Outcome = new ApiHelpdesk.NormalizeEscalationResponse.Types.Outcome
            {
                NextSteps = { (reply0.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "escalation",
            "audit", "NormalizeEscalation", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews an escalation.
    /// </summary>
    public override async Task<ApiHelpdesk.PreviewEscalationResponse> PreviewEscalation(ApiHelpdesk.PreviewEscalationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PreviewEscalation for {Tenant}", requestContext.TenantId);
        if (request.Escalation is { } record)
        {
            var snapshot = EscalationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.CreateIncidentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Incident = new ApiItsm.Incident
            {
                Channel = new ApiItsm.IncidentChannelOrigin
                {
                    Address = (request.Escalation ?? new ApiHelpdesk.Escalation()).OriginCase switch
                    {
                        ApiHelpdesk.Escalation.OriginOneofCase.Channel => (request.Escalation?.Channel?.Address ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _itsmIncidentService.CreateIncidentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiHelpdesk.PreviewEscalationResponse
        {
            Escalation = new ApiHelpdesk.Escalation
            {
                CustomFields = { (request.Escalation?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                EscalationId = (request.EscalationId ?? ""),
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Incident?.Requester?.Email ?? ""),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "escalation",
            "audit", "PreviewEscalation", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores an escalation.
    /// </summary>
    public override async Task<ApiHelpdesk.ScoreEscalationResponse> ScoreEscalation(ApiHelpdesk.ScoreEscalationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ScoreEscalation for {Tenant}", requestContext.TenantId);
        if (request.Escalation is { } record)
        {
            var snapshot = EscalationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiDevices.NormalizeComplianceScanResponse();
        foreach (var item in (request.Escalation?.History ?? new Google.Protobuf.Collections.RepeatedField<ApiHelpdesk.EscalationStatusChange>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input0 = new ApiDevices.NormalizeComplianceScanRequest
            {
                ComplianceScan = new ApiDevices.ComplianceScan
                {
                    Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                    Measurements = { new ApiDevices.ComplianceScanMeasurement
                    {
                        ObservedAt = (item.ChangedAt ?? new Timestamp()),
                    } },
                    Subject = new ApiRecruiting.CandidateRef
                    {
                        DisplayName = (request.Escalation?.Customer?.DisplayName ?? ""),
                    },
                },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiDevices.NormalizeComplianceScanRequest.Types.Details
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Escalation?.Requester?.Email ?? ""),
                        Phone = (request.Escalation?.Requester?.Phone ?? ""),
                    },
                },
            };
            using var rpc0 = _devicesComplianceScanValidationService.NormalizeComplianceScanAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var profile1 = new MappingProfile<ApiHelpdesk.ScoreEscalationRequest, ApiIdentity.CreateUserRequest>(value => new ApiIdentity.CreateUserRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            User = new ApiIdentity.User
            {
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Region = (value.Details?.Destination?.Region ?? ""),
                },
            },
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _identityUserService.CreateUserAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.ScoreEscalationResponse
        {
            Escalation = new ApiHelpdesk.Escalation
            {
                Comments = { new ApiHelpdesk.EscalationComment
                {
                    CreatedAt = (reply1.User?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                EscalationId = (request.EscalationId ?? ""),
                Subject = (request.Escalation?.Subject ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "escalation",
            "audit", "ScoreEscalation", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates an escalation.
    /// </summary>
    public override async Task<ApiHelpdesk.ValidateEscalationResponse> ValidateEscalation(ApiHelpdesk.ValidateEscalationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ValidateEscalation for {Tenant}", requestContext.TenantId);
        if (request.Escalation is { } record)
        {
            var snapshot = EscalationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.UnsubscribeDocumentFolderUpdatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.UnsubscribeDocumentFolderUpdatesRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Escalation?.Requester?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc0 = _documentsDocumentFolderNotificationService.UnsubscribeDocumentFolderUpdatesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.ValidateEscalationToCheckKnowledgeArticleEligibility1(reply0);
        using var rpc1 = _helpdeskKnowledgeArticleValidationService.CheckKnowledgeArticleEligibilityAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.ValidateEscalationResponse
        {
            Escalation = new ApiHelpdesk.Escalation
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.KnowledgeArticle?.Audit?.CreatedBy ?? ""),
                },
                CustomFields = { (request.Escalation?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                EscalationId = (request.EscalationId ?? ""),
            },
            Outcome = new ApiHelpdesk.ValidateEscalationResponse.Types.Outcome
            {
                ReferenceId = (reply0.Outcome?.ReferenceId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "escalation",
            "audit", "ValidateEscalation", (request.Reason ?? ""), token);
        return response;
    }
}
