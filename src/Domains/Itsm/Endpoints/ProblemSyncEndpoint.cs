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
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPatients = Acme.Patients.V1;
using ApiPrescriptions = Acme.Prescriptions.V1;

namespace Acme.Workforce.Itsm;

/// <summary>
/// Keeps problems in step with the systems of record in other regions.
/// </summary>
public sealed class ProblemSyncEndpoint : ApiItsm.ProblemSyncService.ProblemSyncServiceBase
{
    private readonly ApiDevices.ManagedDeviceApprovalService.ManagedDeviceApprovalServiceClient _devicesManagedDeviceApprovalService;
    private readonly ApiDevices.ManagedDeviceWorkflowService.ManagedDeviceWorkflowServiceClient _devicesManagedDeviceWorkflowService;
    private readonly ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient _documentsDocumentTemplateSyncService;
    private readonly ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient _employeesOrgUnitQueryService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ChangeRequestReportingService.ChangeRequestReportingServiceClient _itsmChangeRequestReportingService;
    private readonly ApiItsm.ProblemValidationService.ProblemValidationServiceClient _itsmProblemValidationService;
    private readonly ApiItsm.ServiceAssetValidationService.ServiceAssetValidationServiceClient _itsmServiceAssetValidationService;
    private readonly ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient _ledgerLedgerAccountValidationService;
    private readonly ApiLicensing.EntitlementWorkflowService.EntitlementWorkflowServiceClient _licensingEntitlementWorkflowService;
    private readonly ILogger<ProblemSyncEndpoint> _logger;
    private readonly ApiPrescriptions.PrescriptionCalculationService.PrescriptionCalculationServiceClient _prescriptionsPrescriptionCalculationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;

    public ProblemSyncEndpoint(
        ApiDevices.ManagedDeviceApprovalService.ManagedDeviceApprovalServiceClient devicesManagedDeviceApprovalService,
        ApiDevices.ManagedDeviceWorkflowService.ManagedDeviceWorkflowServiceClient devicesManagedDeviceWorkflowService,
        ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient documentsDocumentTemplateSyncService,
        ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient employeesOrgUnitQueryService,
        CallInvoker invoker,
        ApiItsm.ChangeRequestReportingService.ChangeRequestReportingServiceClient itsmChangeRequestReportingService,
        ApiItsm.ProblemValidationService.ProblemValidationServiceClient itsmProblemValidationService,
        ApiItsm.ServiceAssetValidationService.ServiceAssetValidationServiceClient itsmServiceAssetValidationService,
        ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient ledgerLedgerAccountValidationService,
        ApiLicensing.EntitlementWorkflowService.EntitlementWorkflowServiceClient licensingEntitlementWorkflowService,
        ILogger<ProblemSyncEndpoint> logger,
        ApiPrescriptions.PrescriptionCalculationService.PrescriptionCalculationServiceClient prescriptionsPrescriptionCalculationService,
        ClientRegistry registry,
        IRecordRepository repository)
    {
        _devicesManagedDeviceApprovalService = devicesManagedDeviceApprovalService;
        _devicesManagedDeviceWorkflowService = devicesManagedDeviceWorkflowService;
        _documentsDocumentTemplateSyncService = documentsDocumentTemplateSyncService;
        _employeesOrgUnitQueryService = employeesOrgUnitQueryService;
        _invoker = invoker;
        _itsmChangeRequestReportingService = itsmChangeRequestReportingService;
        _itsmProblemValidationService = itsmProblemValidationService;
        _itsmServiceAssetValidationService = itsmServiceAssetValidationService;
        _ledgerLedgerAccountValidationService = ledgerLedgerAccountValidationService;
        _licensingEntitlementWorkflowService = licensingEntitlementWorkflowService;
        _logger = logger;
        _prescriptionsPrescriptionCalculationService = prescriptionsPrescriptionCalculationService;
        _registry = registry;
        _repository = repository;
    }
    /// <summary>
    /// Returns a problem.
    /// </summary>
    public override async Task<ApiItsm.GetProblemSyncStateResponse> GetProblemSyncState(ApiItsm.GetProblemSyncStateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ProblemId, "problem_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetProblemSyncState for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "problem", request.ProblemId,
            "GetProblemSyncState", "accepted", token);
        var input0 = new ApiItsm.ScoreProblemRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Problem = new ApiItsm.Problem
            {
                ProblemId = (request.ProblemId ?? ""),
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiItsm.ScoreProblemRequest>(json0);
        using var rpc0 = _itsmProblemValidationService.ScoreProblemAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiItsm.GetProblemSyncStateResponse
        {
            Problem = new ApiItsm.Problem
            {
                Comments = { new ApiItsm.ProblemComment
                {
                    CreatedAt = (reply0.Problem?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                ProblemId = (request.ProblemId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists problems a page at a time.
    /// </summary>
    public override async Task<ApiItsm.ListProblemConflictsResponse> ListProblemConflicts(ApiItsm.ListProblemConflictsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListProblemConflicts for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "problem", "batch",
            "ListProblemConflicts", "accepted", token);
        var input0 = new ApiItsm.ValidateServiceAssetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Address = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        using var rpc0 = _itsmServiceAssetValidationService.ValidateServiceAssetAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiEmployees.BrowseOrgUnitsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.BrowseOrgUnitsRequest.Types.Filter
            {
                CreatedAfter = (request.Filter?.CreatedAfter ?? new Timestamp()),
            },
        };
        using var rpc1 = _employeesOrgUnitQueryService.BrowseOrgUnitsAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiItsm.ListProblemConflictsResponse
        {
            Problems = { new ApiItsm.Problem
            {
                Comments = { new ApiItsm.ProblemComment
                {
                    CreatedAt = (reply0.ServiceAsset?.Audit?.CreatedAt ?? new Timestamp()),
                } },
            } },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "problem",
            "audit", "ListProblemConflicts", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pulls a problem.
    /// </summary>
    public override async Task<ApiItsm.PullProblemsResponse> PullProblems(ApiItsm.PullProblemsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PullProblems for {Tenant}", requestContext.TenantId);
        if (request.Problem is { } record)
        {
            var snapshot = ProblemMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.ResolveDocumentTemplateConflictRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ResolveDocumentTemplateConflictRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Problem?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (request.Problem?.Audit?.Etag ?? ""),
                },
            },
        };
        using var rpc0 = _documentsDocumentTemplateSyncService.ResolveDocumentTemplateConflictAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.StartEntitlementWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.StartEntitlementWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Problem ?? new ApiItsm.Problem()).OriginCase switch
                    {
                        ApiItsm.Problem.OriginOneofCase.Customer => (request.Problem?.Customer?.Email ?? ""),
                        _ => ""
                    },
                    Phone = (request.Problem?.Requester?.Phone ?? ""),
                },
            },
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Problem?.Audit?.Etag ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.DocumentTemplate?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _licensingEntitlementWorkflowService.StartEntitlementWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.PullProblemsResponse
        {
            Problem = new ApiItsm.Problem
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.DocumentTemplate?.Audit?.UpdatedBy ?? ""),
                },
                Comments = { new ApiItsm.ProblemComment
                {
                    CreatedAt = (reply1.Entitlement?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                CustomFields = { (request.Problem?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ProblemId = (request.ProblemId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "problem",
            "audit", "PullProblems", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pushs a problem.
    /// </summary>
    public override async Task<ApiItsm.PushProblemResponse> PushProblem(ApiItsm.PushProblemRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PushProblem for {Tenant}", requestContext.TenantId);
        if (request.Problem is { } record)
        {
            var snapshot = ProblemMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.PauseManagedDeviceWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.PauseManagedDeviceWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Phone = (request.Problem?.Requester?.Phone ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _devicesManagedDeviceWorkflowService.PauseManagedDeviceWorkflowAsync);
        var input1 = new ApiDevices.ListPendingManagedDevicesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDevices.ListPendingManagedDevicesRequest.Types.Filter
            {
                Region = (reply0.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                {
                    ApiDevices.ManagedDevice.LocatorOneofCase.Address => (reply0.ManagedDevice?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc1 = _devicesManagedDeviceApprovalService.ListPendingManagedDevicesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.PushProblemResponse
        {
            Problem = new ApiItsm.Problem
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.ManagedDevice?.Audit?.CreatedBy ?? ""),
                },
                ProblemId = (request.ProblemId ?? ""),
                Status = (request.Problem?.Status ?? (ApiItsm.ProblemStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "problem",
            "audit", "PushProblem", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Reconciles a problem.
    /// </summary>
    public override async Task<ApiItsm.ReconcileProblemsResponse> ReconcileProblems(ApiItsm.ReconcileProblemsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ReconcileProblems for {Tenant}", requestContext.TenantId);
        if (request.Problem is { } record)
        {
            var snapshot = ProblemMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPrescriptions.RecalculatePrescriptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Prescription = new ApiPrescriptions.Prescription
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Problem?.Audit?.Etag ?? ""),
                },
                Counterparty = new ApiPatients.PatientRef
                {
                    DisplayName = (request.Problem?.Assignee?.DisplayName ?? ""),
                    Email = (request.Problem?.Assignee?.Email ?? ""),
                },
            },
        };
        var named0 = _registry.Get<ApiPrescriptions.PrescriptionCalculationService.PrescriptionCalculationServiceClient>("acme.prescriptions.v1.PrescriptionCalculationService");
        using var rpc0 = named0.RecalculatePrescriptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiItsm.ReconcileProblemsResponse
        {
            Problem = new ApiItsm.Problem
            {
                ProblemId = (request.ProblemId ?? ""),
                Subject = (request.Problem?.Subject ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "problem",
            "audit", "ReconcileProblems", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resolves a problem.
    /// </summary>
    public override async Task<ApiItsm.ResolveProblemConflictResponse> ResolveProblemConflict(ApiItsm.ResolveProblemConflictRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ResolveProblemConflict for {Tenant}", requestContext.TenantId);
        if (request.Problem is { } record)
        {
            var snapshot = ProblemMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLedger.PreviewLedgerAccountRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LedgerAccount = new ApiLedger.LedgerAccount
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Problem?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (request.Problem?.Audit?.CreatedBy ?? ""),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Problem?.Assignee?.Email ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiLedger.PreviewLedgerAccountRequest, ApiLedger.PreviewLedgerAccountResponse>(
            MethodType.Unary, "acme.ledger.v1" + ".LedgerAccountValidationService", "PreviewLedgerAccount",
            Wire.Marshaller<ApiLedger.PreviewLedgerAccountRequest>(), Wire.Marshaller<ApiLedger.PreviewLedgerAccountResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiItsm.ResolveProblemConflictResponse
        {
            Problem = new ApiItsm.Problem
            {
                Assignee = (request.Problem?.Assignee ?? new ApiIdentity.UserRef()),
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.LedgerAccount?.Audit?.UpdatedBy ?? ""),
                },
                ProblemId = (request.ProblemId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "problem",
            "audit", "ResolveProblemConflict", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Exchanges changes of problems with a peer.
    /// </summary>
    public override async Task StreamProblemChanges(IAsyncStreamReader<ApiItsm.StreamProblemChangesRequest> requests, IServerStreamWriter<ApiItsm.StreamProblemChangesResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        await foreach (var request in requests.ReadAllAsync(token))
        {
            RequestGuard.Tenant(request.Context);
            var requestContext = request.Context;
            using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
                requestContext.TenantId, requestContext.TraceId);
            _logger.LogInformation("Handling StreamProblemChanges for {Tenant}", requestContext.TenantId);
            if (request.Change is { } record)
            {
                var snapshot = ProblemMapper.ToRecord(requestContext.TenantId, record);
                await _repository.SaveAsync(snapshot, token);
            }
            var input0 = new ApiItsm.ScoreProblemRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Problem = new ApiItsm.Problem
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        Etag = request.FrameCase switch
                        {
                            ApiItsm.StreamProblemChangesRequest.FrameOneofCase.Change => (request.Change?.Audit?.Etag ?? ""),
                            _ => ""
                        },
                        UpdatedAt = request.FrameCase switch
                        {
                            ApiItsm.StreamProblemChangesRequest.FrameOneofCase.Change => (request.Change?.Audit?.UpdatedAt ?? new Timestamp()),
                            _ => new Timestamp()
                        },
                    },
                    Channel = new ApiItsm.ProblemChannelOrigin
                    {
                        Channel = request.FrameCase switch
                        {
                            ApiItsm.StreamProblemChangesRequest.FrameOneofCase.Change => (request.Change?.Channel?.Channel ?? ""),
                            _ => ""
                        },
                    },
                },
            };
            using var rpc0 = _itsmProblemValidationService.ScoreProblemAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            var response = new ApiItsm.StreamProblemChangesResponse
            {
                Problem = new ApiItsm.Problem
                {
                    Assignee = new ApiIdentity.UserRef
                    {
                        Email = (reply0.Problem?.Requester?.Email ?? ""),
                    },
                    Priority = (reply0.Problem?.Priority ?? 0),
                    ProblemId = (request.Change?.ProblemId ?? ""),
                    Subject = (request.Change?.Subject ?? ""),
                },
            };
            _logger.LogDebug("Local audit detail {Detail}", (request.Cursor ?? ""));
            await _repository.RecordAsync(requestContext.TenantId, "problem",
                "audit", "StreamProblemChanges", (request.Cursor ?? ""), token);
            await responses.WriteAsync(response, token);
        }
    }
    /// <summary>
    /// Syncs a problem.
    /// </summary>
    public override async Task<ApiItsm.SyncProblemsResponse> SyncProblems(ApiItsm.SyncProblemsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SyncProblems for {Tenant}", requestContext.TenantId);
        if (request.Problem is { } record)
        {
            var snapshot = ProblemMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiItsm.ListChangeRequestReportsResponse();
        foreach (var item in (request.Problem?.History ?? new Google.Protobuf.Collections.RepeatedField<ApiItsm.ProblemStatusChange>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input0 = new ApiItsm.ListChangeRequestReportsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiItsm.ListChangeRequestReportsRequest.Types.Filter
                {
                    CreatedAfter = (item.ChangedAt ?? new Timestamp()),
                    Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    OwnerId = (request.Problem?.Sla?.PolicyId ?? ""),
                },
            };
            using var rpc0 = _itsmChangeRequestReportingService.ListChangeRequestReportsAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var response = new ApiItsm.SyncProblemsResponse
        {
            Problem = new ApiItsm.Problem
            {
                ProblemId = (request.ProblemId ?? ""),
                Status = (request.Problem?.Status ?? (ApiItsm.ProblemStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "problem",
            "audit", "SyncProblems", (request.Reason ?? ""), token);
        return response;
    }
}
