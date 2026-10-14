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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiSearch = Acme.Search.V1;
using ApiTours = Acme.Tours.V1;

namespace Acme.Workforce.Itsm;

/// <summary>
/// Checks incidents against the rules before anything acts on them.
/// </summary>
public sealed class IncidentValidationEndpoint : ApiItsm.IncidentValidationService.IncidentValidationServiceBase
{
    private readonly ApiAudit.AuditEventService.AuditEventServiceClient _auditAuditEventService;
    private readonly ApiDevices.ManagedDeviceService.ManagedDeviceServiceClient _devicesManagedDeviceService;
    private readonly ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient _documentsDocumentFolderQueryService;
    private readonly IIncidentValidationEndpointSendPreviewIncident0Port _incidentValidationEndpointSendPreviewIncident0Port;
    private readonly ApiItsm.ChangeRequestCalculationService.ChangeRequestCalculationServiceClient _itsmChangeRequestCalculationService;
    private readonly ApiItsm.IncidentCalculationService.IncidentCalculationServiceClient _itsmIncidentCalculationService;
    private readonly ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient _licensingEntitlementValidationService;
    private readonly ApiLicensing.LicenseAssignmentCalculationService.LicenseAssignmentCalculationServiceClient _licensingLicenseAssignmentCalculationService;
    private readonly ILogger<IncidentValidationEndpoint> _logger;
    private readonly ApiNotifications.NotificationQueryService.NotificationQueryServiceClient _notificationsNotificationQueryService;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiSearch.SearchIndexService.SearchIndexServiceClient _searchSearchIndexService;
    private readonly Func<ApiItsm.QuoteChangeRequestRequest, CallOptions, AsyncUnaryCall<ApiItsm.QuoteChangeRequestResponse>> _sendGetIncidentRules0;
    private readonly Func<ApiItsm.GetIncidentRatesRequest, CallOptions, AsyncUnaryCall<ApiItsm.GetIncidentRatesResponse>> _sendValidateIncident3;
    private readonly ApiTours.TourQueryService.TourQueryServiceClient _toursTourQueryService;

    public IncidentValidationEndpoint(
        ApiAudit.AuditEventService.AuditEventServiceClient auditAuditEventService,
        ApiDevices.ManagedDeviceService.ManagedDeviceServiceClient devicesManagedDeviceService,
        ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient documentsDocumentFolderQueryService,
        IIncidentValidationEndpointSendPreviewIncident0Port incidentValidationEndpointSendPreviewIncident0PortAdapter,
        ApiItsm.ChangeRequestCalculationService.ChangeRequestCalculationServiceClient itsmChangeRequestCalculationService,
        ApiItsm.IncidentCalculationService.IncidentCalculationServiceClient itsmIncidentCalculationService,
        ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient licensingEntitlementValidationService,
        ApiLicensing.LicenseAssignmentCalculationService.LicenseAssignmentCalculationServiceClient licensingLicenseAssignmentCalculationService,
        ILogger<IncidentValidationEndpoint> logger,
        ApiNotifications.NotificationQueryService.NotificationQueryServiceClient notificationsNotificationQueryService,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiSearch.SearchIndexService.SearchIndexServiceClient searchSearchIndexService,
        ApiTours.TourQueryService.TourQueryServiceClient toursTourQueryService)
    {
        _auditAuditEventService = auditAuditEventService;
        _devicesManagedDeviceService = devicesManagedDeviceService;
        _documentsDocumentFolderQueryService = documentsDocumentFolderQueryService;
        _incidentValidationEndpointSendPreviewIncident0Port = incidentValidationEndpointSendPreviewIncident0PortAdapter;
        _itsmChangeRequestCalculationService = itsmChangeRequestCalculationService;
        _itsmIncidentCalculationService = itsmIncidentCalculationService;
        _licensingEntitlementValidationService = licensingEntitlementValidationService;
        _licensingLicenseAssignmentCalculationService = licensingLicenseAssignmentCalculationService;
        _logger = logger;
        _notificationsNotificationQueryService = notificationsNotificationQueryService;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _searchSearchIndexService = searchSearchIndexService;
        _toursTourQueryService = toursTourQueryService;
        _sendGetIncidentRules0 = _itsmChangeRequestCalculationService.QuoteChangeRequestAsync;
        _sendValidateIncident3 = _itsmIncidentCalculationService.GetIncidentRatesAsync;
    }
    /// <summary>
    /// Checks an incident.
    /// </summary>
    public override async Task<ApiItsm.CheckIncidentEligibilityResponse> CheckIncidentEligibility(ApiItsm.CheckIncidentEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CheckIncidentEligibility for {Tenant}", requestContext.TenantId);
        if (request.Incident is { } record)
        {
            var snapshot = IncidentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiItsm.RecalculateIncidentsResponse();
        foreach (var item in (request.Incident?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiItsm.IncidentComment>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input0 = new ApiItsm.RecalculateIncidentsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Incident = new ApiItsm.Incident
                {
                    Comments = { new ApiItsm.IncidentComment
                    {
                        Body = (item.Body ?? ""),
                        CreatedAt = (item.CreatedAt ?? new Timestamp()),
                        Visibility = (item.Visibility ?? ""),
                    } },
                },
            };
            using var rpc0 = _itsmIncidentCalculationService.RecalculateIncidentsAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiAudit.WatchAuditEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
            },
        };
        using var stream1 = _auditAuditEventService.WatchAuditEvents(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchAuditEvents frame {Type}", change1.GetType().Name);
        }
        var input2 = new ApiItsm.ExplainIncidentCalculationRequest();
        input2.Context = (request.Context ?? new ApiCommon.RequestContext());
        input2.Incident = new ApiItsm.Incident
        {
            Assignee = new ApiIdentity.UserRef
            {
                Email = (request.Incident?.Assignee?.Email ?? ""),
            },
        };
        using var rpc2 = _itsmIncidentCalculationService.ExplainIncidentCalculationAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiItsm.CheckIncidentEligibilityResponse
        {
            Incident = new ApiItsm.Incident
            {
                Assignee = (reply2.Incident?.Assignee ?? new ApiIdentity.UserRef()),
                CustomFields = { (request.Incident?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                IncidentId = (request.IncidentId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "CheckIncidentEligibility", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains an incident.
    /// </summary>
    public override async Task<ApiItsm.ExplainIncidentDecisionResponse> ExplainIncidentDecision(ApiItsm.ExplainIncidentDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExplainIncidentDecision for {Tenant}", requestContext.TenantId);
        if (request.Incident is { } record)
        {
            var snapshot = IncidentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.GetIncidentRatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            IncidentId = (request.IncidentId ?? ""),
        };
        using var rpc0 = _itsmIncidentCalculationService.GetIncidentRatesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.SimulateLicenseAssignmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Incident?.Audit?.Etag ?? ""),
                    UpdatedBy = (request.Incident?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _licensingLicenseAssignmentCalculationService.SimulateLicenseAssignmentAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiDocuments.FindDocumentFoldersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.FindDocumentFoldersRequest.Types.Filter
            {
                Region = (reply1.LicenseAssignment?.BillingAddress?.Region ?? ""),
            },
        };
        var reply2 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _documentsDocumentFolderQueryService.FindDocumentFoldersAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input3 = new ApiItsm.CompareChangeRequestOptionsRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Incident?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Customer = new ApiItsm.IncidentRef
                {
                    DisplayName = (reply0.Incident?.Assignee?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply3 = await Workflow.SendAsync(input3, token, _itsmChangeRequestCalculationService.CompareChangeRequestOptionsAsync);
        var response = new ApiItsm.ExplainIncidentDecisionResponse
        {
            Incident = new ApiItsm.Incident
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (reply0.Incident?.Assignee?.UserId ?? ""),
                },
                CustomFields = { (request.Incident?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                IncidentId = (request.IncidentId ?? ""),
                Priority = (reply3.ChangeRequest?.Priority ?? 0),
            },
            Outcome = new ApiItsm.ExplainIncidentDecisionResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "ExplainIncidentDecision", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an incident.
    /// </summary>
    public override async Task<ApiItsm.GetIncidentRulesResponse> GetIncidentRules(ApiItsm.GetIncidentRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.IncidentId, "incident_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetIncidentRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "incident", request.IncidentId,
            "GetIncidentRules", "accepted", token);
        var input0 = new ApiItsm.QuoteChangeRequestRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
                Customer = new ApiItsm.IncidentRef
                {
                    IncidentId = (request.IncidentId ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _sendGetIncidentRules0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiItsm.GetIncidentRulesResponse
        {
            Incident = new ApiItsm.Incident
            {
                IncidentId = (reply0.ChangeRequest?.Customer?.IncidentId ?? ""),
                Requester = (reply0.ChangeRequest?.Requester ?? new ApiCommon.ContactInfo()),
            },
        };
        return response;
    }
    /// <summary>
    /// Lints an incident.
    /// </summary>
    public override async Task<ApiItsm.LintIncidentsResponse> LintIncidents(ApiItsm.LintIncidentsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling LintIncidents for {Tenant}", requestContext.TenantId);
        if (request.Incident is { } record)
        {
            var snapshot = IncidentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.ExplainIncidentCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Incident = new ApiItsm.Incident
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Incident?.Requester?.Email ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Contact?.Phone ?? ""),
                },
            },
        };
        var named0 = _registry.Get<ApiItsm.IncidentCalculationService.IncidentCalculationServiceClient>("acme.itsm.v1.IncidentCalculationService");
        using var rpc0 = named0.ExplainIncidentCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var reply1 = new ApiItsm.QuoteChangeRequestResponse();
        foreach (var item in (request.Incident?.History ?? new Google.Protobuf.Collections.RepeatedField<ApiItsm.IncidentStatusChange>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input1 = new ApiItsm.QuoteChangeRequestRequest
            {
                ChangeRequest = new ApiItsm.ChangeRequest
                {
                    History = { new ApiItsm.ChangeRequestStatusChange
                    {
                        ChangedBy = (item.ChangedBy ?? ""),
                        ToStatus = (item.ToStatus ?? ""),
                    } },
                },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
            };
            using var rpc1 = _itsmChangeRequestCalculationService.QuoteChangeRequestAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var response = new ApiItsm.LintIncidentsResponse
        {
            Incident = new ApiItsm.Incident
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Incident?.Assignee?.DisplayName ?? ""),
                },
                CustomFields = { (reply0.Incident?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                IncidentId = (request.IncidentId ?? ""),
                Status = (request.Incident?.Status ?? (ApiItsm.IncidentStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "LintIncidents", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Normalizes an incident.
    /// </summary>
    public override async Task<ApiItsm.NormalizeIncidentResponse> NormalizeIncident(ApiItsm.NormalizeIncidentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling NormalizeIncident for {Tenant}", requestContext.TenantId);
        if (request.Incident is { } record)
        {
            var snapshot = IncidentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        foreach (var item in (request.Incident?.History ?? new Google.Protobuf.Collections.RepeatedField<ApiItsm.IncidentStatusChange>()))
        {
            var input0 = new ApiTours.ExportToursRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiTours.ExportToursRequest.Types.Filter
                {
                    CreatedAfter = (item.ChangedAt ?? new Timestamp()),
                    OwnerId = (request.Incident?.Sla?.PolicyId ?? ""),
                },
            };
            using var stream0 = _toursTourQueryService.ExportTours(input0, cancellationToken: token);
            await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
            {
                _logger.LogDebug("Received ExportTours frame {Type}", change0.GetType().Name);
            }
        }
        var profile1 = new MappingProfile<ApiItsm.NormalizeIncidentRequest, ApiSearch.BatchGetSearchIndexesRequest>(value => new ApiSearch.BatchGetSearchIndexesRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _searchSearchIndexService.BatchGetSearchIndexesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiItsm.CalculateIncidentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Incident = new ApiItsm.Incident
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (request.Incident?.Assignee?.UserId ?? ""),
                },
                Channel = new ApiItsm.IncidentChannelOrigin
                {
                    Address = (request.Incident ?? new ApiItsm.Incident()).OriginCase switch
                    {
                        ApiItsm.Incident.OriginOneofCase.Channel => (request.Incident?.Channel?.Address ?? ""),
                        _ => ""
                    },
                },
                Sla = new ApiItsm.IncidentSla
                {
                    PausedFor = (request.Incident?.Sla?.PausedFor ?? new Duration()),
                    PolicyId = (request.Incident?.Sla?.PolicyId ?? ""),
                },
            },
        };
        using var rpc2 = _itsmIncidentCalculationService.CalculateIncidentAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiItsm.SimulateIncidentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Incident = new ApiItsm.Incident
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Incident ?? new ApiItsm.Incident()).OriginCase switch
                    {
                        ApiItsm.Incident.OriginOneofCase.Customer => (request.Incident?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                    UserId = (reply2.Incident?.Assignee?.UserId ?? ""),
                },
                Customer = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Incident?.Assignee?.Email ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Incident ?? new ApiItsm.Incident()).OriginCase switch
                    {
                        ApiItsm.Incident.OriginOneofCase.Customer => (request.Incident?.Customer?.Email ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc3 = _itsmIncidentCalculationService.SimulateIncidentAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiItsm.NormalizeIncidentResponse
        {
            Incident = new ApiItsm.Incident
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (reply2.Incident?.Assignee?.UserId ?? ""),
                },
                CustomFields = { (request.Incident?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                IncidentId = (request.IncidentId ?? ""),
                Subject = (reply2.Incident?.Subject ?? ""),
                Tags = { (reply3.Incident?.Tags ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Message = (reply3.Result?.Message ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "NormalizeIncident", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews an incident.
    /// </summary>
    public override async Task<ApiItsm.PreviewIncidentResponse> PreviewIncident(ApiItsm.PreviewIncidentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PreviewIncident for {Tenant}", requestContext.TenantId);
        if (request.Incident is { } record)
        {
            var snapshot = IncidentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.ExplainIncidentCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.ExplainIncidentCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        var reply0 = await SendPreviewIncident0Async(input0, token);
        var input1 = new ApiDevices.UpdateManagedDeviceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Incident?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.Incident?.Requester?.Email ?? ""),
                },
            },
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _devicesManagedDeviceService.UpdateManagedDeviceAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiNotifications.BrowseNotificationsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiNotifications.BrowseNotificationsRequest.Types.Filter
            {
                CreatedAfter = (reply0.Incident?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (request.Incident?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply1.ManagedDevice?.Components?.GetValueOrDefault("primary")?.ComponentId ?? ""),
                Region = (reply1.ManagedDevice?.Address?.Region ?? ""),
            },
        };
        using var rpc2 = _notificationsNotificationQueryService.BrowseNotificationsAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        var input3 = new ApiLicensing.PreviewEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.PreviewEntitlementRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply1.ManagedDevice?.Owner?.Email ?? ""),
                    Phone = (request.Incident?.Requester?.Phone ?? ""),
                    PreferredChannel = (request.Incident?.Requester?.PreferredChannel ?? ""),
                },
            },
            Entitlement = new ApiLicensing.Entitlement
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.ManagedDevice?.Owner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc3 = _licensingEntitlementValidationService.PreviewEntitlementAsync(input3, cancellationToken: token);
        var pending3 = rpc3.ResponseAsync;
        await Task.WhenAll(pending2, pending3);
        var reply2 = await pending2;
        var reply3 = await pending3;
        var response = new ApiItsm.PreviewIncidentResponse
        {
            Incident = new ApiItsm.Incident
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.Incident?.Audit?.UpdatedBy ?? ""),
                },
                CustomFields = { (request.Incident?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                IncidentId = (request.IncidentId ?? ""),
            },
            Outcome = new ApiItsm.PreviewIncidentResponse.Types.Outcome
            {
                Message = (reply2.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply3.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Result?.Message ?? ""),
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply3.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "PreviewIncident", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores an incident.
    /// </summary>
    public override async Task<ApiItsm.ScoreIncidentResponse> ScoreIncident(ApiItsm.ScoreIncidentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ScoreIncident for {Tenant}", requestContext.TenantId);
        if (request.Incident is { } record)
        {
            var snapshot = IncidentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiNotifications.CountNotificationsRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Filter = new ApiNotifications.CountNotificationsRequest.Types.Filter
        {
            CreatedAfter = (request.Incident?.Sla?.DueAt ?? new Timestamp()),
            OwnerId = (request.IncidentId ?? ""),
            Region = (request.Details?.Destination?.Region ?? ""),
        };
        using var rpc0 = _notificationsNotificationQueryService.CountNotificationsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiSearch.BatchGetSearchIndexesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _searchSearchIndexService.BatchGetSearchIndexesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.ScoreIncidentResponse
        {
            Incident = new ApiItsm.Incident
            {
                IncidentId = (request.IncidentId ?? ""),
                Priority = (request.Incident?.Priority ?? 0),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "ScoreIncident", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates an incident.
    /// </summary>
    public override async Task<ApiItsm.ValidateIncidentResponse> ValidateIncident(ApiItsm.ValidateIncidentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ValidateIncident for {Tenant}", requestContext.TenantId);
        if (request.Incident is { } record)
        {
            var snapshot = IncidentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiItsm.ExplainIncidentCalculationResponse();
        foreach (var item in (request.Incident?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiItsm.IncidentComment>()))
        {
            var input0 = new ApiItsm.ExplainIncidentCalculationRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Incident = new ApiItsm.Incident
                {
                    Comments = { new ApiItsm.IncidentComment
                    {
                        Body = (item.Body ?? ""),
                    } },
                },
            };
            using var rpc0 = _itsmIncidentCalculationService.ExplainIncidentCalculationAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiItsm.CalculateIncidentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Incident = new ApiItsm.Incident
            {
                Comments = { new ApiItsm.IncidentComment
                {
                    CreatedAt = (request.Incident?.Audit?.CreatedAt ?? new Timestamp()),
                } },
            },
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _itsmIncidentCalculationService.CalculateIncidentAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiItsm.CompareIncidentOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Incident = new ApiItsm.Incident
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply0.Incident ?? new ApiItsm.Incident()).OriginCase switch
                    {
                        ApiItsm.Incident.OriginOneofCase.Customer => (reply0.Incident?.Customer?.Email ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc2 = _itsmIncidentCalculationService.CompareIncidentOptionsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiItsm.GetIncidentRatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            IncidentId = (request.Incident?.IncidentId ?? ""),
        };
        using var rpc3 = _sendValidateIncident3(input3, new CallOptions(cancellationToken: token));
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiItsm.ValidateIncidentResponse
        {
            Incident = new ApiItsm.Incident
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply2.Incident?.Customer?.DisplayName ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply2.Incident?.Audit?.CreatedAt ?? new Timestamp()),
                },
                CustomFields = { (request.Incident?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                IncidentId = (request.IncidentId ?? ""),
                Requester = new ApiCommon.ContactInfo
                {
                    Name = (reply3.Incident?.Requester?.Name ?? new ApiCommon.PersonName()),
                },
                Status = (reply1.Incident?.Status ?? (ApiItsm.IncidentStatus)0),
                Tags = { (reply3.Incident?.Tags ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "ValidateIncident", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiItsm.ExplainIncidentCalculationResponse> SendPreviewIncident0Async(ApiItsm.ExplainIncidentCalculationRequest request, CancellationToken token)
    {
        return await _incidentValidationEndpointSendPreviewIncident0Port.SendAsync(request, token);
    }
}
