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
using ApiBudgeting = Acme.Budgeting.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLabeling = Acme.Labeling.V1;
using ApiMaintenance = Acme.Maintenance.V1;
using ApiSearch = Acme.Search.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;
using ApiYards = Acme.Yards.V1;

namespace Acme.Workforce.Workplaces;

/// <summary>
/// Moves a desk between the states of its lifecycle.
/// </summary>
public sealed class DeskLifecycleEndpoint : ApiWorkplaces.DeskLifecycleService.DeskLifecycleServiceBase
{
    private readonly ApiBudgeting.BudgetQueryService.BudgetQueryServiceClient _budgetingBudgetQueryService;
    private readonly ApiDevices.ComplianceScanWorkflowService.ComplianceScanWorkflowServiceClient _devicesComplianceScanWorkflowService;
    private readonly ApiDocuments.DocumentExportService.DocumentExportServiceClient _documentsDocumentExportService;
    private readonly ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient _documentsDocumentFolderQueryService;
    private readonly ApiDocuments.SignatureAdminService.SignatureAdminServiceClient _documentsSignatureAdminService;
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient _employeesEmploymentContractValidationService;
    private readonly CallInvoker _invoker;
    private readonly ApiLabeling.LabelingTaskQueryService.LabelingTaskQueryServiceClient _labelingLabelingTaskQueryService;
    private readonly ILogger<DeskLifecycleEndpoint> _logger;
    private readonly ApiMaintenance.WorkOrderService.WorkOrderServiceClient _maintenanceWorkOrderService;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;
    private readonly ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient _searchSearchIndexQueryService;
    private readonly Func<ApiDocuments.CountDocumentFoldersRequest, CallOptions, AsyncUnaryCall<ApiDocuments.CountDocumentFoldersResponse>> _sendRenewDesk1;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTravel.TravelBookingWorkflowService.TravelBookingWorkflowServiceClient _travelTravelBookingWorkflowService;
    private readonly ApiWorkplaces.DeskValidationService.DeskValidationServiceClient _workplacesDeskValidationService;
    private readonly ApiWorkplaces.RoomBookingCalculationService.RoomBookingCalculationServiceClient _workplacesRoomBookingCalculationService;
    private readonly ApiWorkplaces.VisitorPassSyncService.VisitorPassSyncServiceClient _workplacesVisitorPassSyncService;
    private readonly ApiWorkplaces.VisitorPassValidationService.VisitorPassValidationServiceClient _workplacesVisitorPassValidationService;
    private readonly ApiYards.YardService.YardServiceClient _yardsYardService;

    public DeskLifecycleEndpoint(
        ApiBudgeting.BudgetQueryService.BudgetQueryServiceClient budgetingBudgetQueryService,
        ApiDevices.ComplianceScanWorkflowService.ComplianceScanWorkflowServiceClient devicesComplianceScanWorkflowService,
        ApiDocuments.DocumentExportService.DocumentExportServiceClient documentsDocumentExportService,
        ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient documentsDocumentFolderQueryService,
        ApiDocuments.SignatureAdminService.SignatureAdminServiceClient documentsSignatureAdminService,
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient employeesEmploymentContractValidationService,
        CallInvoker invoker,
        ApiLabeling.LabelingTaskQueryService.LabelingTaskQueryServiceClient labelingLabelingTaskQueryService,
        ILogger<DeskLifecycleEndpoint> logger,
        ApiMaintenance.WorkOrderService.WorkOrderServiceClient maintenanceWorkOrderService,
        RetryPolicy policy,
        IRecordRepository repository,
        ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient searchSearchIndexQueryService,
        IOptions<MeshOptions> settings,
        ApiTravel.TravelBookingWorkflowService.TravelBookingWorkflowServiceClient travelTravelBookingWorkflowService,
        ApiWorkplaces.DeskValidationService.DeskValidationServiceClient workplacesDeskValidationService,
        ApiWorkplaces.RoomBookingCalculationService.RoomBookingCalculationServiceClient workplacesRoomBookingCalculationService,
        ApiWorkplaces.VisitorPassSyncService.VisitorPassSyncServiceClient workplacesVisitorPassSyncService,
        ApiWorkplaces.VisitorPassValidationService.VisitorPassValidationServiceClient workplacesVisitorPassValidationService,
        ApiYards.YardService.YardServiceClient yardsYardService)
    {
        _budgetingBudgetQueryService = budgetingBudgetQueryService;
        _devicesComplianceScanWorkflowService = devicesComplianceScanWorkflowService;
        _documentsDocumentExportService = documentsDocumentExportService;
        _documentsDocumentFolderQueryService = documentsDocumentFolderQueryService;
        _documentsSignatureAdminService = documentsSignatureAdminService;
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _employeesEmploymentContractValidationService = employeesEmploymentContractValidationService;
        _invoker = invoker;
        _labelingLabelingTaskQueryService = labelingLabelingTaskQueryService;
        _logger = logger;
        _maintenanceWorkOrderService = maintenanceWorkOrderService;
        _policy = policy;
        _repository = repository;
        _searchSearchIndexQueryService = searchSearchIndexQueryService;
        _settings = settings;
        _travelTravelBookingWorkflowService = travelTravelBookingWorkflowService;
        _workplacesDeskValidationService = workplacesDeskValidationService;
        _workplacesRoomBookingCalculationService = workplacesRoomBookingCalculationService;
        _workplacesVisitorPassSyncService = workplacesVisitorPassSyncService;
        _workplacesVisitorPassValidationService = workplacesVisitorPassValidationService;
        _yardsYardService = yardsYardService;
        _sendRenewDesk1 = _documentsDocumentFolderQueryService.CountDocumentFoldersAsync;
    }
    /// <summary>
    /// Activates a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.ActivateDeskResponse> ActivateDesk(ApiWorkplaces.ActivateDeskRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DeskId, "desk_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ActivateDesk for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "desk", request.DeskId,
            "ActivateDesk", "accepted", token);
        var input0 = new ApiWorkplaces.PushVisitorPassRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiWorkplaces.PushVisitorPassRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _workplacesVisitorPassSyncService.PushVisitorPassAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiWorkplaces.PreviewVisitorPassRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiWorkplaces.PreviewVisitorPassRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.VisitorPass?.Audit?.UpdatedBy ?? ""),
                },
                Card = new ApiWorkplaces.VisitorPassCardSettlement
                {
                    HolderName = (reply0.VisitorPass ?? new ApiWorkplaces.VisitorPass()).PaymentCase switch
                    {
                        ApiWorkplaces.VisitorPass.SettlementOneofCase.Card => (reply0.VisitorPass?.Card?.HolderName ?? ""),
                        _ => ""
                    },
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _workplacesVisitorPassValidationService.PreviewVisitorPassAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.ActivateDeskResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                DeskId = (request.DeskId ?? ""),
            },
            Outcome = new ApiWorkplaces.ActivateDeskResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "desk",
            "audit", "ActivateDesk", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Expires a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.ExpireDeskResponse> ExpireDesk(ApiWorkplaces.ExpireDeskRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DeskId, "desk_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExpireDesk for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "desk", request.DeskId,
            "ExpireDesk", "accepted", token);
        var input0 = new ApiDevices.ListComplianceScanWorkflowsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDevices.ListComplianceScanWorkflowsRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiDevices.ListComplianceScanWorkflowsRequest>();
        using var rpc0 = _devicesComplianceScanWorkflowService.ListComplianceScanWorkflowsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.RetryDocumentExportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.RetryDocumentExportRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
            Document = new ApiDocuments.Document
            {
                Counterparty = new ApiDocuments.DocumentFolderRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                },
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiDocuments.RetryDocumentExportRequest>(json1);
        using var rpc1 = _documentsDocumentExportService.RetryDocumentExportAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.ExpireDeskResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                DeskId = (request.DeskId ?? ""),
            },
            Outcome = new ApiWorkplaces.ExpireDeskResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply1.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "desk",
            "audit", "ExpireDesk", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.GetDeskLifecycleResponse> GetDeskLifecycle(ApiWorkplaces.GetDeskLifecycleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DeskId, "desk_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetDeskLifecycle for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "desk", request.DeskId,
            "GetDeskLifecycle", "accepted", token);
        var input0 = new ApiBudgeting.FindBudgetsRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Filter = new ApiBudgeting.FindBudgetsRequest.Types.Filter
        {
            CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
            Labels = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            OwnerId = (request.DeskId ?? ""),
        };
        using var rpc0 = _budgetingBudgetQueryService.FindBudgetsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiWorkplaces.GetDeskLifecycleResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                DeskId = (request.DeskId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Reinstates a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.ReinstateDeskResponse> ReinstateDesk(ApiWorkplaces.ReinstateDeskRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DeskId, "desk_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ReinstateDesk for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "desk", request.DeskId,
            "ReinstateDesk", "accepted", token);
        var input0 = new ApiMaintenance.CreateWorkOrderRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            WorkOrder = new ApiMaintenance.WorkOrder
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc0 = _maintenanceWorkOrderService.CreateWorkOrderAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.ResumeTravelBookingWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.ResumeTravelBookingWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                },
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.WorkOrder?.Audit?.Etag ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply0.WorkOrder?.Assignee?.DisplayName ?? ""),
                },
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _travelTravelBookingWorkflowService.ResumeTravelBookingWorkflowAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiWorkplaces.ExplainRoomBookingCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Location = new ApiCommon.PostalAddress
                {
                    Lines = { (reply1.TravelBooking?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                Participants = { new ApiWorkplaces.RoomBookingParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Details?.NewOwner?.Email ?? ""),
                        Name = (reply0.WorkOrder?.Requester?.Name ?? new ApiCommon.PersonName()),
                    },
                } },
            },
        };
        var reply2 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _workplacesRoomBookingCalculationService.ExplainRoomBookingCalculationAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiWorkplaces.ReinstateDeskResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.WorkOrder?.Audit?.Etag ?? ""),
                },
                DeskId = (request.DeskId ?? ""),
            },
            Outcome = new ApiWorkplaces.ReinstateDeskResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply2.Outcome?.Message ?? ""),
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "desk",
            "audit", "ReinstateDesk", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Renews a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.RenewDeskResponse> RenewDesk(ApiWorkplaces.RenewDeskRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DeskId, "desk_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RenewDesk for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "desk", request.DeskId,
            "RenewDesk", "accepted", token);
        var input0 = new ApiSearch.SuggestSearchIndexesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiSearch.SuggestSearchIndexesRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Details?.NewOwner?.UserId ?? ""),
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _searchSearchIndexQueryService.SuggestSearchIndexesAsync);
        var input1 = new ApiDocuments.CountDocumentFoldersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.CountDocumentFoldersRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        using var rpc1 = _sendRenewDesk1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.RenewDeskResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                DeskId = (request.DeskId ?? ""),
            },
            Outcome = new ApiWorkplaces.RenewDeskResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "desk",
            "audit", "RenewDesk", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.ScheduleDeskExpiryResponse> ScheduleDeskExpiry(ApiWorkplaces.ScheduleDeskExpiryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ScheduleDeskExpiry for {Tenant}", requestContext.TenantId);
        if (request.Desk is { } record)
        {
            var snapshot = DeskMapper.ToRecord(requestContext.TenantId, record);
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
                Measurements = { new ApiDocuments.SignatureMeasurement
                {
                    Unit = (request.Desk?.Dimensions?.Unit ?? ""),
                } },
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (request.Desk?.Owner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _documentsSignatureAdminService.RepairSignatureAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiWorkplaces.ExplainDeskDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Desk = new ApiWorkplaces.Desk
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Desk?.Owner?.UserId ?? ""),
                },
            },
            Details = new ApiWorkplaces.ExplainDeskDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Notify?.Phone ?? ""),
                },
            },
        };
        var descriptor1 = new Method<ApiWorkplaces.ExplainDeskDecisionRequest, ApiWorkplaces.ExplainDeskDecisionResponse>(
            MethodType.Unary, "acme.workplaces.v1" + ".DeskValidationService", "ExplainDeskDecision",
            Wire.Marshaller<ApiWorkplaces.ExplainDeskDecisionRequest>(), Wire.Marshaller<ApiWorkplaces.ExplainDeskDecisionResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.ScheduleDeskExpiryResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.Desk?.Audit?.UpdatedBy ?? ""),
                },
                DeskId = (request.DeskId ?? ""),
                Owner = (request.Desk?.Owner ?? new ApiIdentity.UserRef()),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Step ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "desk",
            "audit", "ScheduleDeskExpiry", (request.Details?.Step ?? ""), token);
        return response;
    }
    /// <summary>
    /// Suspends a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.SuspendDeskResponse> SuspendDesk(ApiWorkplaces.SuspendDeskRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DeskId, "desk_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SuspendDesk for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "desk", request.DeskId,
            "SuspendDesk", "accepted", token);
        var input0 = new ApiWorkplaces.NormalizeDeskRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiWorkplaces.NormalizeDeskRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiWorkplaces.NormalizeDeskRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _workplacesDeskValidationService.NormalizeDeskAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.CalculateEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.Desk?.Audit?.UpdatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                    {
                        ApiWorkplaces.Desk.LocatorOneofCase.Address => (reply0.Desk?.Address?.Recipient ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _employeesEmploymentContractCalculationService.CalculateEmploymentContractAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.SuspendDeskResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.EmploymentContract?.Audit?.Etag ?? ""),
                },
                Components = { { "primary", new ApiWorkplaces.DeskComponent
                {
                    Description = (reply0.Desk?.Components?.GetValueOrDefault("primary")?.Description ?? ""),
                } } },
                DeskId = (request.DeskId ?? ""),
            },
            Outcome = new ApiWorkplaces.SuspendDeskResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "desk",
            "audit", "SuspendDesk", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Terminates a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.TerminateDeskResponse> TerminateDesk(ApiWorkplaces.TerminateDeskRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DeskId, "desk_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling TerminateDesk for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "desk", request.DeskId,
            "TerminateDesk", "accepted", token);
        var input0 = new ApiDocuments.RepairSignatureRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.RepairSignatureRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _documentsSignatureAdminService.RepairSignatureAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.TerminateDeskToCloneYard1();
        using var rpc1 = _yardsYardService.CloneYardAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLabeling.FindLabelingTasksRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLabeling.FindLabelingTasksRequest.Types.Filter
            {
                CreatedAfter = (reply1.Yard?.Audit?.UpdatedAt ?? new Timestamp()),
                Region = (reply1.Yard ?? new ApiYards.Yard()).LocatorCase switch
                {
                    ApiYards.Yard.LocatorOneofCase.Address => (reply1.Yard?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc2 = _labelingLabelingTaskQueryService.FindLabelingTasksAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiEmployees.PreviewEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.PreviewEmploymentContractRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply1.Yard?.Owner?.Email ?? ""),
                },
            },
        };
        var labels3 = input3.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input3.Context.Baggage.Clear();
        input3.Context.Baggage.Add(labels3);
        using var rpc3 = _employeesEmploymentContractValidationService.PreviewEmploymentContractAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiWorkplaces.TerminateDeskResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                Components = { { "primary", new ApiWorkplaces.DeskComponent
                {
                    ComponentId = (reply1.Yard?.Components?.GetValueOrDefault("primary")?.ComponentId ?? ""),
                } } },
                DeskId = (request.DeskId ?? ""),
                Dimensions = new ApiWorkplaces.DeskDimensions
                {
                    Weight = (reply1.Yard?.Dimensions?.Weight ?? 0d),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Signature?.Subject?.DisplayName ?? ""),
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
            },
            Outcome = new ApiWorkplaces.TerminateDeskResponse.Types.Outcome
            {
                EffectiveAt = (reply3.Outcome?.EffectiveAt ?? new Timestamp()),
                Message = (reply2.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "desk",
            "audit", "TerminateDesk", (request.Reason ?? ""), token);
        return response;
    }
}
