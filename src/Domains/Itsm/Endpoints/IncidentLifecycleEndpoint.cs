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
using ApiConcessions = Acme.Concessions.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiShipping = Acme.Shipping.V1;
using ApiSponsorships = Acme.Sponsorships.V1;

namespace Acme.Workforce.Itsm;

/// <summary>
/// Moves an incident between the states of its lifecycle.
/// </summary>
public sealed class IncidentLifecycleEndpoint : ApiItsm.IncidentLifecycleService.IncidentLifecycleServiceBase
{
    private readonly ApiConcessions.ConcessionStandCalculationService.ConcessionStandCalculationServiceClient _concessionsConcessionStandCalculationService;
    private readonly ApiDevices.ComplianceScanWorkflowService.ComplianceScanWorkflowServiceClient _devicesComplianceScanWorkflowService;
    private readonly ApiDevices.ManagedDeviceWorkflowService.ManagedDeviceWorkflowServiceClient _devicesManagedDeviceWorkflowService;
    private readonly ApiDocuments.DocumentSyncService.DocumentSyncServiceClient _documentsDocumentSyncService;
    private readonly ApiDocuments.DocumentTemplateCalculationService.DocumentTemplateCalculationServiceClient _documentsDocumentTemplateCalculationService;
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _employeesEmployeeService;
    private readonly IIncidentLifecycleEndpointSendReinstateIncident1Port _incidentLifecycleEndpointSendReinstateIncident1Port;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ChangeRequestQueryService.ChangeRequestQueryServiceClient _itsmChangeRequestQueryService;
    private readonly ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient _itsmChangeRequestValidationService;
    private readonly ApiItsm.IncidentCalculationService.IncidentCalculationServiceClient _itsmIncidentCalculationService;
    private readonly ApiItsm.IncidentQueryService.IncidentQueryServiceClient _itsmIncidentQueryService;
    private readonly ApiItsm.IncidentService.IncidentServiceClient _itsmIncidentService;
    private readonly ApiItsm.ProblemValidationService.ProblemValidationServiceClient _itsmProblemValidationService;
    private readonly ILogger<IncidentLifecycleEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient _metricsMetricSeriesQueryService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiMetrics.SummarizeMetricSeriesesRequest, CallOptions, AsyncUnaryCall<ApiMetrics.SummarizeMetricSeriesesResponse>> _sendSuspendIncident0;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiShipping.ShipmentService.ShipmentServiceClient _shippingShipmentService;
    private readonly ApiSponsorships.SponsorCalculationService.SponsorCalculationServiceClient _sponsorshipsSponsorCalculationService;

    public IncidentLifecycleEndpoint(
        ApiConcessions.ConcessionStandCalculationService.ConcessionStandCalculationServiceClient concessionsConcessionStandCalculationService,
        ApiDevices.ComplianceScanWorkflowService.ComplianceScanWorkflowServiceClient devicesComplianceScanWorkflowService,
        ApiDevices.ManagedDeviceWorkflowService.ManagedDeviceWorkflowServiceClient devicesManagedDeviceWorkflowService,
        ApiDocuments.DocumentSyncService.DocumentSyncServiceClient documentsDocumentSyncService,
        ApiDocuments.DocumentTemplateCalculationService.DocumentTemplateCalculationServiceClient documentsDocumentTemplateCalculationService,
        ApiEmployees.EmployeeService.EmployeeServiceClient employeesEmployeeService,
        IIncidentLifecycleEndpointSendReinstateIncident1Port incidentLifecycleEndpointSendReinstateIncident1PortAdapter,
        CallInvoker invoker,
        ApiItsm.ChangeRequestQueryService.ChangeRequestQueryServiceClient itsmChangeRequestQueryService,
        ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient itsmChangeRequestValidationService,
        ApiItsm.IncidentCalculationService.IncidentCalculationServiceClient itsmIncidentCalculationService,
        ApiItsm.IncidentQueryService.IncidentQueryServiceClient itsmIncidentQueryService,
        ApiItsm.IncidentService.IncidentServiceClient itsmIncidentService,
        ApiItsm.ProblemValidationService.ProblemValidationServiceClient itsmProblemValidationService,
        ILogger<IncidentLifecycleEndpoint> logger,
        ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient metricsMetricSeriesQueryService,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiShipping.ShipmentService.ShipmentServiceClient shippingShipmentService,
        ApiSponsorships.SponsorCalculationService.SponsorCalculationServiceClient sponsorshipsSponsorCalculationService)
    {
        _concessionsConcessionStandCalculationService = concessionsConcessionStandCalculationService;
        _devicesComplianceScanWorkflowService = devicesComplianceScanWorkflowService;
        _devicesManagedDeviceWorkflowService = devicesManagedDeviceWorkflowService;
        _documentsDocumentSyncService = documentsDocumentSyncService;
        _documentsDocumentTemplateCalculationService = documentsDocumentTemplateCalculationService;
        _employeesEmployeeService = employeesEmployeeService;
        _incidentLifecycleEndpointSendReinstateIncident1Port = incidentLifecycleEndpointSendReinstateIncident1PortAdapter;
        _invoker = invoker;
        _itsmChangeRequestQueryService = itsmChangeRequestQueryService;
        _itsmChangeRequestValidationService = itsmChangeRequestValidationService;
        _itsmIncidentCalculationService = itsmIncidentCalculationService;
        _itsmIncidentQueryService = itsmIncidentQueryService;
        _itsmIncidentService = itsmIncidentService;
        _itsmProblemValidationService = itsmProblemValidationService;
        _logger = logger;
        _metricsMetricSeriesQueryService = metricsMetricSeriesQueryService;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _shippingShipmentService = shippingShipmentService;
        _sponsorshipsSponsorCalculationService = sponsorshipsSponsorCalculationService;
        _sendSuspendIncident0 = _metricsMetricSeriesQueryService.SummarizeMetricSeriesesAsync;
    }
    /// <summary>
    /// Activates an incident.
    /// </summary>
    public override async Task<ApiItsm.ActivateIncidentResponse> ActivateIncident(ApiItsm.ActivateIncidentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.IncidentId, "incident_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ActivateIncident for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "incident", request.IncidentId,
            "ActivateIncident", "accepted", token);
        var input0 = new ApiEmployees.BatchGetEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _employeesEmployeeService.BatchGetEmployeesAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiDocuments.CompareDocumentTemplateOptionsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                },
                Counterparty = new ApiDocuments.DocumentFolderRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc1 = _documentsDocumentTemplateCalculationService.CompareDocumentTemplateOptionsAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var input2 = new ApiItsm.FindIncidentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.FindIncidentsRequest.Types.Filter
            {
                Region = (reply1.DocumentTemplate?.BillingAddress?.Region ?? ""),
            },
        };
        var command2 = new DispatchCommand<ApiItsm.FindIncidentsRequest>(input2,
            "primary", requestContext.TenantId);
        var routed2 = command2 with { Region = _settings.Value.Region };
        using var rpc2 = _itsmIncidentQueryService.FindIncidentsAsync(routed2.Payload, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiItsm.ActivateIncidentResponse
        {
            Incident = new ApiItsm.Incident
            {
                IncidentId = (request.IncidentId ?? ""),
            },
            Outcome = new ApiItsm.ActivateIncidentResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply1.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.ReasonCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "ActivateIncident", (request.Details?.ReasonCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Expires an incident.
    /// </summary>
    public override async Task<ApiItsm.ExpireIncidentResponse> ExpireIncident(ApiItsm.ExpireIncidentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.IncidentId, "incident_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExpireIncident for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "incident", request.IncidentId,
            "ExpireIncident", "accepted", token);
        var profile0 = new MappingProfile<ApiItsm.ExpireIncidentRequest, ApiSponsorships.SimulateSponsorRequest>(value => new ApiSponsorships.SimulateSponsorRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Sponsor = new ApiSponsorships.Sponsor
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (value.Details?.NotifyContact?.Phone ?? ""),
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _sponsorshipsSponsorCalculationService.SimulateSponsorAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.CheckProblemEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Problem = new ApiItsm.Problem
            {
                Comments = { new ApiItsm.ProblemComment
                {
                    CreatedAt = (reply0.Sponsor?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _itsmProblemValidationService.CheckProblemEligibilityAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.ExpireIncidentResponse
        {
            Incident = new ApiItsm.Incident
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Sponsor?.Audit?.Etag ?? ""),
                },
                IncidentId = (request.IncidentId ?? ""),
            },
            Outcome = new ApiItsm.ExpireIncidentResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Outcome?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "ExpireIncident", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an incident.
    /// </summary>
    public override async Task<ApiItsm.GetIncidentLifecycleResponse> GetIncidentLifecycle(ApiItsm.GetIncidentLifecycleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.IncidentId, "incident_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetIncidentLifecycle for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "incident", request.IncidentId,
            "GetIncidentLifecycle", "accepted", token);
        var response = new ApiItsm.GetIncidentLifecycleResponse
        {
            Incident = new ApiItsm.Incident
            {
                IncidentId = (request.IncidentId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Reinstates an incident.
    /// </summary>
    public override async Task<ApiItsm.ReinstateIncidentResponse> ReinstateIncident(ApiItsm.ReinstateIncidentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.IncidentId, "incident_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ReinstateIncident for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "incident", request.IncidentId,
            "ReinstateIncident", "accepted", token);
        var input0 = request.ReinstateIncidentToSearchShipments0();
        using var rpc0 = _shippingShipmentService.SearchShipmentsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.GetIncidentRatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            IncidentId = (request.IncidentId ?? ""),
        };
        var reply1 = await SendReinstateIncident1Async(input1, token);
        var input2 = new ApiDocuments.ReconcileDocumentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ReconcileDocumentsRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Document = new ApiDocuments.Document
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Incident?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiDocuments.DocumentFolderRef
                {
                    DisplayName = (reply1.Incident ?? new ApiItsm.Incident()).OriginCase switch
                    {
                        ApiItsm.Incident.OriginOneofCase.Customer => (reply1.Incident?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc2 = _documentsDocumentSyncService.ReconcileDocumentsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiEmployees.UpdateEmployeeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Employee = new ApiEmployees.Employee
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply2.Document?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (reply1.Incident?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply2.Document?.BillingAddress?.Locality ?? ""),
                },
            },
        };
        var packed3 = Any.Pack(input3);
        input3 = packed3.Unpack<ApiEmployees.UpdateEmployeeRequest>();
        using var rpc3 = _employeesEmployeeService.UpdateEmployeeAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiItsm.ReinstateIncidentResponse
        {
            Incident = new ApiItsm.Incident
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply2.Document?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (reply2.Document?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                IncidentId = (request.IncidentId ?? ""),
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply3.Employee?.Contact?.Email ?? ""),
                },
                Status = (reply1.Incident?.Status ?? (ApiItsm.IncidentStatus)0),
            },
            Outcome = new ApiItsm.ReinstateIncidentResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply3.Result?.CorrelationId ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "ReinstateIncident", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Renews an incident.
    /// </summary>
    public override async Task<ApiItsm.RenewIncidentResponse> RenewIncident(ApiItsm.RenewIncidentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.IncidentId, "incident_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RenewIncident for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "incident", request.IncidentId,
            "RenewIncident", "accepted", token);
        var input0 = new ApiItsm.ListIncidentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.ListIncidentsRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Details?.NewOwner?.UserId ?? ""),
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiItsm.ListIncidentsRequest>(json0);
        using var rpc0 = _itsmIncidentService.ListIncidentsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.CheckProblemEligibilityRequest();
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        input1.Details = new ApiItsm.CheckProblemEligibilityRequest.Types.Details
        {
            Contact = new ApiCommon.ContactInfo
            {
                Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
            },
        };
        input1.Problem = new ApiItsm.Problem
        {
            Assignee = new ApiIdentity.UserRef
            {
                Email = (request.Details?.NewOwner?.Email ?? ""),
            },
            Customer = new ApiEmployees.EmployeeRef
            {
                Email = (request.Details?.NotifyContact?.Email ?? ""),
            },
        };
        using var rpc1 = _itsmProblemValidationService.CheckProblemEligibilityAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.RenewIncidentResponse
        {
            Incident = new ApiItsm.Incident
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.Problem?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                IncidentId = (request.IncidentId ?? ""),
            },
            Outcome = new ApiItsm.RenewIncidentResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "RenewIncident", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules an incident.
    /// </summary>
    public override async Task<ApiItsm.ScheduleIncidentExpiryResponse> ScheduleIncidentExpiry(ApiItsm.ScheduleIncidentExpiryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ScheduleIncidentExpiry for {Tenant}", requestContext.TenantId);
        if (request.Incident is { } record)
        {
            var snapshot = IncidentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.Incident?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiItsm.IncidentComment>()).Select(async item =>
        {
            var input0 = new ApiItsm.ScoreChangeRequestRequest
            {
                ChangeRequest = new ApiItsm.ChangeRequest
                {
                    Comments = { new ApiItsm.ChangeRequestComment
                    {
                        AuthorEmail = (item.AuthorEmail ?? ""),
                        Body = (item.Body ?? ""),
                        CreatedAt = (item.CreatedAt ?? new Timestamp()),
                        Visibility = (item.Visibility ?? ""),
                    } },
                },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
            };
            using var rpc0 = _itsmChangeRequestValidationService.ScoreChangeRequestAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiItsm.ScoreChangeRequestResponse();
        var input1 = new ApiItsm.SummarizeChangeRequestsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            ChangeRequestId = (reply0.ChangeRequest?.ChangeRequestId ?? ""),
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _itsmChangeRequestQueryService.SummarizeChangeRequestsAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiDevices.RetryComplianceScanWorkflowRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (request.Incident ?? new ApiItsm.Incident()).OriginCase switch
                    {
                        ApiItsm.Incident.OriginOneofCase.Customer => (request.Incident?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc2 = _devicesComplianceScanWorkflowService.RetryComplianceScanWorkflowAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiItsm.CreateIncidentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Incident = new ApiItsm.Incident
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Incident?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        var reply3 = await Workflow.SendAsync(input3, token, _itsmIncidentService.CreateIncidentAsync);
        var response = new ApiItsm.ScheduleIncidentExpiryResponse
        {
            Incident = new ApiItsm.Incident
            {
                CustomFields = { (request.Incident?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                IncidentId = (request.IncidentId ?? ""),
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply3.Incident?.Customer?.Email ?? ""),
                },
            },
            Outcome = new ApiItsm.ScheduleIncidentExpiryResponse.Types.Outcome
            {
                EffectiveAt = (reply2.Outcome?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Incident?.Subject ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "ScheduleIncidentExpiry", (request.Incident?.Subject ?? ""), token);
        return response;
    }
    /// <summary>
    /// Suspends an incident.
    /// </summary>
    public override async Task<ApiItsm.SuspendIncidentResponse> SuspendIncident(ApiItsm.SuspendIncidentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.IncidentId, "incident_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SuspendIncident for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "incident", request.IncidentId,
            "SuspendIncident", "accepted", token);
        var input0 = new ApiMetrics.SummarizeMetricSeriesesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        using var rpc0 = _sendSuspendIncident0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.SummarizeChangeRequestsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.SummarizeChangeRequestsRequest.Types.Details
            {
                Period = (reply0.Period ?? new ApiCommon.TimeWindow()),
            },
        };
        var named1 = _registry.Get<ApiItsm.ChangeRequestQueryService.ChangeRequestQueryServiceClient>("acme.itsm.v1.ChangeRequestQueryService");
        using var rpc1 = named1.SummarizeChangeRequestsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiConcessions.CompareConcessionStandOptionsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            ConcessionStand = new ApiConcessions.ConcessionStand
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var descriptor2 = new Method<ApiConcessions.CompareConcessionStandOptionsRequest, ApiConcessions.CompareConcessionStandOptionsResponse>(
            MethodType.Unary, "acme.concessions.v1" + ".ConcessionStandCalculationService", "CompareConcessionStandOptions",
            Wire.Marshaller<ApiConcessions.CompareConcessionStandOptionsRequest>(), Wire.Marshaller<ApiConcessions.CompareConcessionStandOptionsResponse>());
        using var rpc2 = _invoker.AsyncUnaryCall(descriptor2, null,
            new CallOptions(cancellationToken: token), input2);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiItsm.SuspendIncidentResponse
        {
            Incident = new ApiItsm.Incident
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply2.ConcessionStand?.Owner?.Email ?? ""),
                },
                IncidentId = (request.IncidentId ?? ""),
            },
            Outcome = new ApiItsm.SuspendIncidentResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.ReasonCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "SuspendIncident", (request.Details?.ReasonCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Terminates an incident.
    /// </summary>
    public override async Task<ApiItsm.TerminateIncidentResponse> TerminateIncident(ApiItsm.TerminateIncidentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.IncidentId, "incident_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling TerminateIncident for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "incident", request.IncidentId,
            "TerminateIncident", "accepted", token);
        var input0 = new ApiDevices.PauseManagedDeviceWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.PauseManagedDeviceWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                },
            },
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiDevices.PauseManagedDeviceWorkflowRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _devicesManagedDeviceWorkflowService.PauseManagedDeviceWorkflowAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiItsm.TerminateIncidentResponse
        {
            Incident = new ApiItsm.Incident
            {
                IncidentId = (request.IncidentId ?? ""),
            },
            Outcome = new ApiItsm.TerminateIncidentResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "TerminateIncident", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiItsm.GetIncidentRatesResponse> SendReinstateIncident1Async(ApiItsm.GetIncidentRatesRequest request, CancellationToken token)
    {
        return await _incidentLifecycleEndpointSendReinstateIncident1Port.SendAsync(request, token);
    }
}
