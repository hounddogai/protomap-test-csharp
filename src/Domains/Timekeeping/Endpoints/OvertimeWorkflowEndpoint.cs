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
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiTours = Acme.Tours.V1;

namespace Acme.Workforce.Timekeeping;

/// <summary>
/// Runs the long-lived workflow that takes an overtime from start to finish.
/// </summary>
public sealed class OvertimeWorkflowEndpoint : ApiTimekeeping.OvertimeWorkflowService.OvertimeWorkflowServiceBase
{
    private readonly ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient _documentsDocumentFolderNotificationService;
    private readonly ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient _documentsDocumentTemplateNotificationService;
    private readonly ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient _documentsSignatureHistoryService;
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _employeesEmployeeService;
    private readonly ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient _employeesOrgUnitQueryService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly ApiHelpdesk.SlaPolicyWorkflowService.SlaPolicyWorkflowServiceClient _helpdeskSlaPolicyWorkflowService;
    private readonly ApiLearning.LearningPathApprovalService.LearningPathApprovalServiceClient _learningLearningPathApprovalService;
    private readonly ILogger<OvertimeWorkflowEndpoint> _logger;
    private readonly ApiNotifications.NotificationService.NotificationServiceClient _notificationsNotificationService;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTimekeeping.ClockEventValidationService.ClockEventValidationServiceClient _timekeepingClockEventValidationService;
    private readonly ApiTimekeeping.ClockEventWorkflowService.ClockEventWorkflowServiceClient _timekeepingClockEventWorkflowService;
    private readonly ApiTours.TourQueryService.TourQueryServiceClient _toursTourQueryService;

    public OvertimeWorkflowEndpoint(
        ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient documentsDocumentFolderNotificationService,
        ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient documentsDocumentTemplateNotificationService,
        ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient documentsSignatureHistoryService,
        ApiEmployees.EmployeeService.EmployeeServiceClient employeesEmployeeService,
        ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient employeesOrgUnitQueryService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        ApiHelpdesk.SlaPolicyWorkflowService.SlaPolicyWorkflowServiceClient helpdeskSlaPolicyWorkflowService,
        ApiLearning.LearningPathApprovalService.LearningPathApprovalServiceClient learningLearningPathApprovalService,
        ILogger<OvertimeWorkflowEndpoint> logger,
        ApiNotifications.NotificationService.NotificationServiceClient notificationsNotificationService,
        RetryPolicy policy,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiTimekeeping.ClockEventValidationService.ClockEventValidationServiceClient timekeepingClockEventValidationService,
        ApiTimekeeping.ClockEventWorkflowService.ClockEventWorkflowServiceClient timekeepingClockEventWorkflowService,
        ApiTours.TourQueryService.TourQueryServiceClient toursTourQueryService)
    {
        _documentsDocumentFolderNotificationService = documentsDocumentFolderNotificationService;
        _documentsDocumentTemplateNotificationService = documentsDocumentTemplateNotificationService;
        _documentsSignatureHistoryService = documentsSignatureHistoryService;
        _employeesEmployeeService = employeesEmployeeService;
        _employeesOrgUnitQueryService = employeesOrgUnitQueryService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _helpdeskSlaPolicyWorkflowService = helpdeskSlaPolicyWorkflowService;
        _learningLearningPathApprovalService = learningLearningPathApprovalService;
        _logger = logger;
        _notificationsNotificationService = notificationsNotificationService;
        _policy = policy;
        _repository = repository;
        _settings = settings;
        _timekeepingClockEventValidationService = timekeepingClockEventValidationService;
        _timekeepingClockEventWorkflowService = timekeepingClockEventWorkflowService;
        _toursTourQueryService = toursTourQueryService;
    }
    /// <summary>
    /// Advances an overtime.
    /// </summary>
    public override async Task<ApiTimekeeping.AdvanceOvertimeWorkflowResponse> AdvanceOvertimeWorkflow(ApiTimekeeping.AdvanceOvertimeWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling AdvanceOvertimeWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Overtime is { } record)
        {
            var snapshot = OvertimeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTimekeeping.ScoreClockEventRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiTimekeeping.TimesheetRef
                {
                    DisplayName = (request.Overtime?.Owner?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        using var rpc0 = _timekeepingClockEventValidationService.ScoreClockEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTimekeeping.AdvanceOvertimeWorkflowResponse
        {
            Overtime = new ApiTimekeeping.Overtime
            {
                OvertimeId = (request.OvertimeId ?? ""),
                Priority = (request.Overtime?.Priority ?? 0),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Overtime?.Route?.Target ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "overtime",
            "audit", "AdvanceOvertimeWorkflow", (request.Overtime?.Route?.Target ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels an overtime.
    /// </summary>
    public override async Task<ApiTimekeeping.CancelOvertimeWorkflowResponse> CancelOvertimeWorkflow(ApiTimekeeping.CancelOvertimeWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CancelOvertimeWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Overtime is { } record)
        {
            var snapshot = OvertimeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.FindOrgUnitsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.FindOrgUnitsRequest.Types.Filter
            {
                CreatedAfter = (request.Overtime?.Audit?.UpdatedAt ?? new Timestamp()),
                OwnerId = (request.Overtime?.Owner?.UserId ?? ""),
            },
        };
        var command0 = new DispatchCommand<ApiEmployees.FindOrgUnitsRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _employeesOrgUnitQueryService.FindOrgUnitsAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.SubscribeDocumentTemplateUpdatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SubscribeDocumentTemplateUpdatesRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.Notify?.Phone ?? ""),
                },
            },
            Reason = (request.Overtime ?? new ApiTimekeeping.Overtime()).ActionCase switch
            {
                ApiTimekeeping.Overtime.ActionOneofCase.Block => (request.Overtime?.Block?.Reason ?? ""),
                _ => ""
            },
        };
        using var rpc1 = _documentsDocumentTemplateNotificationService.SubscribeDocumentTemplateUpdatesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTimekeeping.CancelOvertimeWorkflowResponse
        {
            Outcome = new ApiTimekeeping.CancelOvertimeWorkflowResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Overtime = new ApiTimekeeping.Overtime
            {
                Effective = (request.Overtime?.Effective ?? new ApiCommon.TimeWindow()),
                OvertimeId = (request.OvertimeId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "overtime",
            "audit", "CancelOvertimeWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an overtime.
    /// </summary>
    public override async Task<ApiTimekeeping.InspectOvertimeWorkflowResponse> InspectOvertimeWorkflow(ApiTimekeeping.InspectOvertimeWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OvertimeId, "overtime_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling InspectOvertimeWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "overtime", request.OvertimeId,
            "InspectOvertimeWorkflow", "accepted", token);
        var input0 = new ApiLearning.EscalateLearningPathRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LearningPath = new ApiLearning.LearningPath
            {
                Children = { new ApiLearning.LearningPath
                {
                    Children = { new ApiLearning.LearningPath
                    {
                        Children = { new ApiLearning.LearningPath
                        {
                            ParentId = (request.OvertimeId ?? ""),
                        } },
                        Manager = new ApiIdentity.UserRef
                        {
                            UserId = (request.Context?.Actor?.UserId ?? ""),
                        },
                    } },
                    Manager = new ApiIdentity.UserRef
                    {
                        Email = (request.Context?.Actor?.Email ?? ""),
                    },
                } },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _learningLearningPathApprovalService.EscalateLearningPathAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.InspectOvertimeWorkflowToDelegateLearningPathApproval1(reply0);
        using var rpc1 = _learningLearningPathApprovalService.DelegateLearningPathApprovalAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTimekeeping.InspectOvertimeWorkflowResponse
        {
            Overtime = new ApiTimekeeping.Overtime
            {
                OvertimeId = (request.OvertimeId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists overtimes a page at a time.
    /// </summary>
    public override async Task<ApiTimekeeping.ListOvertimeWorkflowsResponse> ListOvertimeWorkflows(ApiTimekeeping.ListOvertimeWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListOvertimeWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "overtime", "batch",
            "ListOvertimeWorkflows", "accepted", token);
        var response = new ApiTimekeeping.ListOvertimeWorkflowsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "overtime",
            "audit", "ListOvertimeWorkflows", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pauses an overtime.
    /// </summary>
    public override async Task<ApiTimekeeping.PauseOvertimeWorkflowResponse> PauseOvertimeWorkflow(ApiTimekeeping.PauseOvertimeWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PauseOvertimeWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Overtime is { } record)
        {
            var snapshot = OvertimeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.NormalizePositionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.NormalizePositionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Overtime?.Owner?.Email ?? ""),
                },
            },
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Overtime?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (request.Overtime?.Audit?.UpdatedBy ?? ""),
                },
                Route = new ApiEmployees.PositionRouteAction
                {
                    Target = (request.Overtime ?? new ApiTimekeeping.Overtime()).ActionCase switch
                    {
                        ApiTimekeeping.Overtime.ActionOneofCase.Route => (request.Overtime?.Route?.Target ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _employeesPositionValidationService.NormalizePositionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTours.SummarizeToursRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (reply0.Position ?? new ApiEmployees.Position()).ActionCase switch
            {
                ApiEmployees.Position.ActionOneofCase.Block => (reply0.Position?.Block?.Reason ?? ""),
                _ => ""
            },
        };
        using var rpc1 = _toursTourQueryService.SummarizeToursAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTimekeeping.PauseOvertimeWorkflowResponse
        {
            Overtime = new ApiTimekeeping.Overtime
            {
                OvertimeId = (request.OvertimeId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.Position?.Owner?.Email ?? ""),
                },
                Version = (request.Overtime?.Version ?? 0L),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "overtime",
            "audit", "PauseOvertimeWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resumes an overtime.
    /// </summary>
    public override async Task<ApiTimekeeping.ResumeOvertimeWorkflowResponse> ResumeOvertimeWorkflow(ApiTimekeeping.ResumeOvertimeWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ResumeOvertimeWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Overtime is { } record)
        {
            var snapshot = OvertimeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.CompareSignatureVersionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.CompareSignatureVersionsRequest.Types.Details
            {
                AuthorEmail = (request.Details?.Notify?.Email ?? ""),
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Reason = (request.Overtime ?? new ApiTimekeeping.Overtime()).ActionCase switch
            {
                ApiTimekeeping.Overtime.ActionOneofCase.Block => (request.Overtime?.Block?.Reason ?? ""),
                _ => ""
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
        };
        using var rpc0 = _documentsSignatureHistoryService.CompareSignatureVersionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.StreamDocumentFolderNoticesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (request.Overtime?.Owner?.Email ?? ""),
                Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                Phone = (request.Details?.Notify?.Phone ?? ""),
                PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
            },
        };
        using var stream1 = _documentsDocumentFolderNotificationService.StreamDocumentFolderNotices(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamDocumentFolderNotices frame {Type}", change1.GetType().Name);
        }
        var response = new ApiTimekeeping.ResumeOvertimeWorkflowResponse
        {
            Outcome = new ApiTimekeeping.ResumeOvertimeWorkflowResponse.Types.Outcome
            {
                EffectiveAt = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
            },
            Overtime = new ApiTimekeeping.Overtime
            {
                Description = (request.Overtime?.Description ?? ""),
                OvertimeId = (request.OvertimeId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "overtime",
            "audit", "ResumeOvertimeWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys an overtime.
    /// </summary>
    public override async Task<ApiTimekeeping.RetryOvertimeWorkflowResponse> RetryOvertimeWorkflow(ApiTimekeeping.RetryOvertimeWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RetryOvertimeWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Overtime is { } record)
        {
            var snapshot = OvertimeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTimekeeping.ValidateClockEventRequest
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.ValidateClockEventRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Overtime?.Owner?.Email ?? ""),
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.Notify?.Phone ?? ""),
                },
            },
        };
        using var rpc0 = _timekeepingClockEventValidationService.ValidateClockEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTimekeeping.CancelClockEventWorkflowRequest
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Kind = (reply0.ClockEvent?.Kind ?? ""),
                Subject = new ApiTimekeeping.TimesheetRef
                {
                    TimesheetId = (reply0.ClockEvent?.Subject?.TimesheetId ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.CancelClockEventWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Overtime?.Owner?.Email ?? ""),
                    Phone = (request.Details?.Notify?.Phone ?? ""),
                },
            },
        };
        using var rpc1 = _timekeepingClockEventWorkflowService.CancelClockEventWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiHelpdesk.PauseSlaPolicyWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.Overtime?.Audit?.UpdatedBy ?? ""),
                },
                Discount = new ApiHelpdesk.SlaPolicyDiscountAction
                {
                    Amount = (request.Overtime ?? new ApiTimekeeping.Overtime()).ActionCase switch
                    {
                        ApiTimekeeping.Overtime.ActionOneofCase.Discount => (request.Overtime?.Discount?.Amount ?? new ApiCommon.Money()),
                        _ => new ApiCommon.Money()
                    },
                    Percent = (request.Overtime ?? new ApiTimekeeping.Overtime()).ActionCase switch
                    {
                        ApiTimekeeping.Overtime.ActionOneofCase.Discount => (request.Overtime?.Discount?.Percent ?? 0d),
                        _ => 0d
                    },
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.ClockEvent?.Subject?.DisplayName ?? ""),
                },
            },
        };
        using var rpc2 = _helpdeskSlaPolicyWorkflowService.PauseSlaPolicyWorkflowAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiEmployees.CreateEmployeeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Employee = new ApiEmployees.Employee
            {
                RelationshipManager = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.ClockEvent?.Subject?.DisplayName ?? ""),
                },
            },
        };
        var reply3 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _employeesEmployeeService.CreateEmployeeAsync(input3, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiTimekeeping.RetryOvertimeWorkflowResponse
        {
            Outcome = new ApiTimekeeping.RetryOvertimeWorkflowResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Overtime = new ApiTimekeeping.Overtime
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply2.SlaPolicy?.Audit?.CreatedBy ?? ""),
                },
                Name = (request.Overtime?.Name ?? ""),
                OvertimeId = (request.OvertimeId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply3.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "overtime",
            "audit", "RetryOvertimeWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals an overtime.
    /// </summary>
    public override async Task<ApiTimekeeping.SignalOvertimeWorkflowResponse> SignalOvertimeWorkflow(ApiTimekeeping.SignalOvertimeWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SignalOvertimeWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Overtime is { } record)
        {
            var snapshot = OvertimeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiNotifications.BatchGetNotificationsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _notificationsNotificationService.BatchGetNotificationsAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiDocuments.ListDocumentFolderSubscriptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentFolderSubscriptionsRequest.Types.Filter
            {
                CreatedAfter = (request.Overtime?.Audit?.UpdatedAt ?? new Timestamp()),
                OwnerId = (request.Overtime?.Owner?.UserId ?? ""),
            },
        };
        using var rpc1 = _documentsDocumentFolderNotificationService.ListDocumentFolderSubscriptionsAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiTimekeeping.SignalOvertimeWorkflowResponse
        {
            Overtime = new ApiTimekeeping.Overtime
            {
                OvertimeId = (request.OvertimeId ?? ""),
                Status = (request.Overtime?.Status ?? (ApiTimekeeping.OvertimeStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "overtime",
            "audit", "SignalOvertimeWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts an overtime.
    /// </summary>
    public override async Task<ApiTimekeeping.StartOvertimeWorkflowResponse> StartOvertimeWorkflow(ApiTimekeeping.StartOvertimeWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling StartOvertimeWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Overtime is { } record)
        {
            var snapshot = OvertimeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.AnnotateSignatureVersionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.AnnotateSignatureVersionRequest.Types.Details
            {
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
                Version = (request.Overtime?.Version ?? 0L),
            },
            Reason = (request.Overtime ?? new ApiTimekeeping.Overtime()).ActionCase switch
            {
                ApiTimekeeping.Overtime.ActionOneofCase.Block => (request.Overtime?.Block?.Reason ?? ""),
                _ => ""
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (request.Overtime?.Owner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _documentsSignatureHistoryService.AnnotateSignatureVersionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTimekeeping.StartOvertimeWorkflowResponse
        {
            Outcome = new ApiTimekeeping.StartOvertimeWorkflowResponse.Types.Outcome
            {
                EffectiveAt = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
            },
            Overtime = new ApiTimekeeping.Overtime
            {
                OvertimeId = (request.OvertimeId ?? ""),
                Parameters = { (request.Overtime?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "overtime",
            "audit", "StartOvertimeWorkflow", (request.Reason ?? ""), token);
        return response;
    }
}
