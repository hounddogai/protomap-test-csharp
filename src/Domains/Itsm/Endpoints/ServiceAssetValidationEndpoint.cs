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
using ApiAudit = Acme.Audit.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPerformers = Acme.Performers.V1;

namespace Acme.Workforce.Itsm;

/// <summary>
/// Checks service assets against the rules before anything acts on them.
/// </summary>
public sealed class ServiceAssetValidationEndpoint : ApiItsm.ServiceAssetValidationService.ServiceAssetValidationServiceBase
{
    private readonly ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient _auditAuditEventValidationService;
    private readonly NotificationBus _bus;
    private readonly ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient _documentsDocumentFolderQueryService;
    private readonly ApiDocuments.SignatureExportService.SignatureExportServiceClient _documentsSignatureExportService;
    private readonly ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient _featureflagsFeatureFlagValidationService;
    private readonly ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient _itsmChangeRequestValidationService;
    private readonly ApiItsm.IncidentCalculationService.IncidentCalculationServiceClient _itsmIncidentCalculationService;
    private readonly ApiItsm.IncidentValidationService.IncidentValidationServiceClient _itsmIncidentValidationService;
    private readonly ApiItsm.ServiceAssetCalculationService.ServiceAssetCalculationServiceClient _itsmServiceAssetCalculationService;
    private readonly ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient _licensingSoftwareLicenseValidationService;
    private readonly ILogger<ServiceAssetValidationEndpoint> _logger;
    private readonly ApiPerformers.PerformerService.PerformerServiceClient _performersPerformerService;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly IServiceAssetValidationEndpointSendScoreServiceAsset0Port _serviceAssetValidationEndpointSendScoreServiceAsset0Port;
    private readonly IOptions<MeshOptions> _settings;

    public ServiceAssetValidationEndpoint(
        ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient auditAuditEventValidationService,
        NotificationBus bus,
        ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient documentsDocumentFolderQueryService,
        ApiDocuments.SignatureExportService.SignatureExportServiceClient documentsSignatureExportService,
        ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient featureflagsFeatureFlagValidationService,
        ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient itsmChangeRequestValidationService,
        ApiItsm.IncidentCalculationService.IncidentCalculationServiceClient itsmIncidentCalculationService,
        ApiItsm.IncidentValidationService.IncidentValidationServiceClient itsmIncidentValidationService,
        ApiItsm.ServiceAssetCalculationService.ServiceAssetCalculationServiceClient itsmServiceAssetCalculationService,
        ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient licensingSoftwareLicenseValidationService,
        ILogger<ServiceAssetValidationEndpoint> logger,
        ApiPerformers.PerformerService.PerformerServiceClient performersPerformerService,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        IServiceAssetValidationEndpointSendScoreServiceAsset0Port serviceAssetValidationEndpointSendScoreServiceAsset0PortAdapter,
        IOptions<MeshOptions> settings)
    {
        _auditAuditEventValidationService = auditAuditEventValidationService;
        _bus = bus;
        _documentsDocumentFolderQueryService = documentsDocumentFolderQueryService;
        _documentsSignatureExportService = documentsSignatureExportService;
        _featureflagsFeatureFlagValidationService = featureflagsFeatureFlagValidationService;
        _itsmChangeRequestValidationService = itsmChangeRequestValidationService;
        _itsmIncidentCalculationService = itsmIncidentCalculationService;
        _itsmIncidentValidationService = itsmIncidentValidationService;
        _itsmServiceAssetCalculationService = itsmServiceAssetCalculationService;
        _licensingSoftwareLicenseValidationService = licensingSoftwareLicenseValidationService;
        _logger = logger;
        _performersPerformerService = performersPerformerService;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _serviceAssetValidationEndpointSendScoreServiceAsset0Port = serviceAssetValidationEndpointSendScoreServiceAsset0PortAdapter;
        _settings = settings;
    }
    /// <summary>
    /// Checks a service asset.
    /// </summary>
    public override async Task<ApiItsm.CheckServiceAssetEligibilityResponse> CheckServiceAssetEligibility(ApiItsm.CheckServiceAssetEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CheckServiceAssetEligibility for {Tenant}", requestContext.TenantId);
        if (request.ServiceAsset is { } record)
        {
            var snapshot = ServiceAssetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.ListSignatureExportsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListSignatureExportsRequest.Types.Filter
            {
                CreatedAfter = (request.ServiceAsset?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.ServiceAsset?.ServiceAssetId ?? ""),
                Region = (request.ServiceAsset ?? new ApiItsm.ServiceAsset()).LocatorCase switch
                {
                    ApiItsm.ServiceAsset.LocatorOneofCase.Address => (request.ServiceAsset?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc0 = _documentsSignatureExportService.ListSignatureExportsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.ScoreChangeRequestRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.ScoreChangeRequestRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.ServiceAsset ?? new ApiItsm.ServiceAsset()).LocatorCase switch
                    {
                        ApiItsm.ServiceAsset.LocatorOneofCase.Address => (request.ServiceAsset?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()),
                        _ => new Google.Protobuf.Collections.RepeatedField<string>()
                    } },
                    Recipient = (request.ServiceAsset ?? new ApiItsm.ServiceAsset()).LocatorCase switch
                    {
                        ApiItsm.ServiceAsset.LocatorOneofCase.Address => (request.ServiceAsset?.Address?.Recipient ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _itsmChangeRequestValidationService.ScoreChangeRequestAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.CheckServiceAssetEligibilityResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                ServiceAssetId = (request.ServiceAssetId ?? ""),
                Specs = { (request.ServiceAsset?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.ServiceAsset?.Owner?.UserId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "CheckServiceAssetEligibility", (request.ServiceAsset?.Owner?.UserId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a service asset.
    /// </summary>
    public override async Task<ApiItsm.ExplainServiceAssetDecisionResponse> ExplainServiceAssetDecision(ApiItsm.ExplainServiceAssetDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExplainServiceAssetDecision for {Tenant}", requestContext.TenantId);
        if (request.ServiceAsset is { } record)
        {
            var snapshot = ServiceAssetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.SimulateServiceAssetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Address = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.ServiceAsset ?? new ApiItsm.ServiceAsset()).LocatorCase switch
                    {
                        ApiItsm.ServiceAsset.LocatorOneofCase.Address => (request.ServiceAsset?.Address?.PostalCode ?? ""),
                        _ => ""
                    },
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.ServiceAsset?.Audit?.CreatedBy ?? ""),
                },
                Components = { { "primary", new ApiItsm.ServiceAssetComponent
                {
                    Quantity = (request.ServiceAsset?.Components?.GetValueOrDefault("primary")?.Quantity ?? 0L),
                } } },
                Dimensions = new ApiItsm.ServiceAssetDimensions
                {
                    Weight = (request.ServiceAsset?.Dimensions?.Weight ?? 0d),
                },
            },
        };
        using var rpc0 = _itsmServiceAssetCalculationService.SimulateServiceAssetAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.PreviewChangeRequestRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.ServiceAsset?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _itsmChangeRequestValidationService.PreviewChangeRequestAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiItsm.CheckIncidentEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.CheckIncidentEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply1.ChangeRequest?.Assignee?.Email ?? ""),
                },
            },
            Incident = new ApiItsm.Incident
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.ServiceAsset?.Owner?.Email ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.ServiceAsset?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        var reply2 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _itsmIncidentValidationService.CheckIncidentEligibilityAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiItsm.ExplainServiceAssetDecisionResponse
        {
            Outcome = new ApiItsm.ExplainServiceAssetDecisionResponse.Types.Outcome
            {
                Status = (reply1.Outcome?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply2.Incident?.Audit?.CreatedBy ?? ""),
                },
                Capacity = (reply0.ServiceAsset?.Capacity ?? 0L),
                Name = (request.ServiceAsset?.Name ?? ""),
                ServiceAssetId = (request.ServiceAssetId ?? ""),
                Specs = { (reply0.ServiceAsset?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.ServiceAsset?.ServiceAssetId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "ExplainServiceAssetDecision", (request.ServiceAsset?.ServiceAssetId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lints a service asset.
    /// </summary>
    public override async Task<ApiItsm.LintServiceAssetsResponse> LintServiceAssets(ApiItsm.LintServiceAssetsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling LintServiceAssets for {Tenant}", requestContext.TenantId);
        if (request.ServiceAsset is { } record)
        {
            var snapshot = ServiceAssetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.CheckChangeRequestEligibilityRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.ServiceAsset?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (request.ServiceAsset?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.CheckChangeRequestEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _itsmChangeRequestValidationService.CheckChangeRequestEligibilityAsync);
        var input1 = new ApiItsm.ValidateIncidentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.ValidateIncidentRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (reply0.ChangeRequest?.Requester?.PreferredChannel ?? ""),
                },
            },
            Incident = new ApiItsm.Incident
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.ChangeRequest ?? new ApiItsm.ChangeRequest()).OriginCase switch
                    {
                        ApiItsm.ChangeRequest.OriginOneofCase.Customer => (reply0.ChangeRequest?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                    UserId = (reply0.ChangeRequest?.Assignee?.UserId ?? ""),
                },
            },
        };
        using var rpc1 = _itsmIncidentValidationService.ValidateIncidentAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLicensing.ScoreSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (reply1.Incident?.Requester?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        var named2 = _registry.Get<ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient>("acme.licensing.v1.SoftwareLicenseValidationService");
        using var rpc2 = named2.ScoreSoftwareLicenseAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiLicensing.ScoreSoftwareLicenseRequest>("acme.itsm.v1.ServiceAssetValidationService/LintServiceAssets/2", input2.Clone()), token);
        var input3 = new ApiAudit.ValidateAuditEventRequest
        {
            AuditEvent = new ApiAudit.AuditEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiAudit.AuditTrailRef
                {
                    DisplayName = (reply0.ChangeRequest ?? new ApiItsm.ChangeRequest()).OriginCase switch
                    {
                        ApiItsm.ChangeRequest.OriginOneofCase.Customer => (reply0.ChangeRequest?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiAudit.ValidateAuditEventRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.ServiceAsset?.Owner?.Email ?? ""),
                    Name = (reply2.SoftwareLicense?.Contact?.Name ?? new ApiCommon.PersonName()),
                    Phone = (reply2.SoftwareLicense?.Contact?.Phone ?? ""),
                },
            },
        };
        using var rpc3 = _auditAuditEventValidationService.ValidateAuditEventAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiItsm.LintServiceAssetsResponse
        {
            Outcome = new ApiItsm.LintServiceAssetsResponse.Types.Outcome
            {
                NextSteps = { (reply3.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply2.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Outcome?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply2.SoftwareLicense?.Audit?.UpdatedBy ?? ""),
                },
                Owner = (request.ServiceAsset?.Owner ?? new ApiIdentity.UserRef()),
                ServiceAssetId = (request.ServiceAssetId ?? ""),
                Tags = { (reply0.ChangeRequest?.Tags ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.ServiceAsset?.ServiceAssetId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "LintServiceAssets", (request.ServiceAsset?.ServiceAssetId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Normalizes a service asset.
    /// </summary>
    public override async Task<ApiItsm.NormalizeServiceAssetResponse> NormalizeServiceAsset(ApiItsm.NormalizeServiceAssetRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling NormalizeServiceAsset for {Tenant}", requestContext.TenantId);
        if (request.ServiceAsset is { } record)
        {
            var snapshot = ServiceAssetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.NormalizeIncidentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Incident = new ApiItsm.Incident
            {
                Comments = { new ApiItsm.IncidentComment
                {
                    CreatedAt = (request.ServiceAsset?.Audit?.CreatedAt ?? new Timestamp()),
                } },
            },
        };
        var command0 = new DispatchCommand<ApiItsm.NormalizeIncidentRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _itsmIncidentValidationService.NormalizeIncidentAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiItsm.NormalizeServiceAssetRequest, ApiItsm.ScoreChangeRequestRequest>(value => new ApiItsm.ScoreChangeRequestRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (value.ServiceAsset?.Owner?.Email ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (value.Details?.Contact?.Email ?? ""),
                },
                Sla = new ApiItsm.ChangeRequestSla
                {
                    Breached = (reply0.Incident?.Sla?.Breached ?? false),
                },
            },
            Context = (value.Context ?? new ApiCommon.RequestContext()),
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _itsmChangeRequestValidationService.ScoreChangeRequestAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.NormalizeServiceAssetResponse
        {
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.ChangeRequest?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Incident?.Assignee?.DisplayName ?? ""),
                },
                ServiceAssetId = (request.ServiceAssetId ?? ""),
                Status = (request.ServiceAsset?.Status ?? (ApiItsm.ServiceAssetStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.ServiceAsset?.ServiceAssetId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "NormalizeServiceAsset", (request.ServiceAsset?.ServiceAssetId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews a service asset.
    /// </summary>
    public override async Task<ApiItsm.PreviewServiceAssetResponse> PreviewServiceAsset(ApiItsm.PreviewServiceAssetRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PreviewServiceAsset for {Tenant}", requestContext.TenantId);
        if (request.ServiceAsset is { } record)
        {
            var snapshot = ServiceAssetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.NormalizeIncidentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.NormalizeIncidentRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                    Phone = (request.Details?.Contact?.Phone ?? ""),
                },
            },
            Incident = new ApiItsm.Incident
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.ServiceAsset?.Audit?.CreatedBy ?? ""),
                },
                Comments = { new ApiItsm.IncidentComment
                {
                    CreatedAt = (request.ServiceAsset?.Audit?.CreatedAt ?? new Timestamp()),
                } },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _itsmIncidentValidationService.NormalizeIncidentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.CompareIncidentOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Incident = new ApiItsm.Incident
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply0.Incident?.Requester?.Email ?? ""),
                },
                Customer = new ApiEmployees.EmployeeRef
                {
                    EmployeeId = (reply0.Incident ?? new ApiItsm.Incident()).OriginCase switch
                    {
                        ApiItsm.Incident.OriginOneofCase.Customer => (reply0.Incident?.Customer?.EmployeeId ?? ""),
                        _ => ""
                    },
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Incident?.Assignee?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _itsmIncidentCalculationService.CompareIncidentOptionsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.PreviewServiceAssetResponse
        {
            Outcome = new ApiItsm.PreviewServiceAssetResponse.Types.Outcome
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Capacity = (request.ServiceAsset?.Capacity ?? 0L),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.Incident?.Customer?.DisplayName ?? ""),
                },
                ServiceAssetId = (request.ServiceAssetId ?? ""),
                Tags = { (reply0.Incident?.Tags ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.ServiceAsset?.ServiceAssetId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "PreviewServiceAsset", (request.ServiceAsset?.ServiceAssetId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores a service asset.
    /// </summary>
    public override async Task<ApiItsm.ScoreServiceAssetResponse> ScoreServiceAsset(ApiItsm.ScoreServiceAssetRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ScoreServiceAsset for {Tenant}", requestContext.TenantId);
        if (request.ServiceAsset is { } record)
        {
            var snapshot = ServiceAssetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPerformers.CreatePerformerRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Performer = new ApiPerformers.Performer
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.ServiceAsset?.Audit?.Etag ?? ""),
                },
            },
        };
        var reply0 = await SendScoreServiceAsset0Async(input0, token);
        var input1 = new ApiFeatureflags.ValidateFeatureFlagRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            FeatureFlag = new ApiFeatureflags.FeatureFlag
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.ServiceAsset?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (request.ServiceAsset?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (reply0.Performer?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.ServiceAsset?.Owner?.Email ?? ""),
                },
            },
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _featureflagsFeatureFlagValidationService.ValidateFeatureFlagAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.ScoreServiceAssetResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Capacity = (request.ServiceAsset?.Capacity ?? 0L),
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply1.FeatureFlag?.Owner?.Email ?? ""),
                    UserId = (reply0.Performer?.RelationshipManager?.UserId ?? ""),
                },
                ServiceAssetId = (request.ServiceAssetId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.ServiceAsset?.ServiceAssetId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "ScoreServiceAsset", (request.ServiceAsset?.ServiceAssetId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates a service asset.
    /// </summary>
    public override async Task<ApiItsm.ValidateServiceAssetResponse> ValidateServiceAsset(ApiItsm.ValidateServiceAssetRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ValidateServiceAsset for {Tenant}", requestContext.TenantId);
        if (request.ServiceAsset is { } record)
        {
            var snapshot = ServiceAssetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.NormalizeSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.NormalizeSoftwareLicenseRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                    Phone = (request.Details?.Contact?.Phone ?? ""),
                },
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiLicensing.NormalizeSoftwareLicenseRequest>();
        using var rpc0 = _licensingSoftwareLicenseValidationService.NormalizeSoftwareLicenseAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.BrowseDocumentFoldersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.BrowseDocumentFoldersRequest.Types.Filter
            {
                CreatedAfter = (request.ServiceAsset?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (reply0.SoftwareLicense?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Region = (request.ServiceAsset ?? new ApiItsm.ServiceAsset()).LocatorCase switch
                {
                    ApiItsm.ServiceAsset.LocatorOneofCase.Address => (request.ServiceAsset?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc1 = _documentsDocumentFolderQueryService.BrowseDocumentFoldersAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.ValidateServiceAssetResponse
        {
            Outcome = new ApiItsm.ValidateServiceAssetResponse.Types.Outcome
            {
                ReferenceId = (reply0.Outcome?.ReferenceId ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                ServiceAssetId = (request.ServiceAssetId ?? ""),
                Specs = { (request.ServiceAsset?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.ServiceAsset?.ServiceAssetId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "ValidateServiceAsset", (request.ServiceAsset?.ServiceAssetId ?? ""), token);
        return response;
    }
    private async Task<ApiPerformers.CreatePerformerResponse> SendScoreServiceAsset0Async(ApiPerformers.CreatePerformerRequest request, CancellationToken token)
    {
        return await _serviceAssetValidationEndpointSendScoreServiceAsset0Port.SendAsync(request, token);
    }
}
