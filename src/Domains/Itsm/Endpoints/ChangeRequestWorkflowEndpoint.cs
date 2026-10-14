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
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPayroll = Acme.Payroll.V1;

namespace Acme.Workforce.Itsm;

/// <summary>
/// Runs the long-lived workflow that takes a change request from start to finish.
/// </summary>
public sealed class ChangeRequestWorkflowEndpoint : ApiItsm.ChangeRequestWorkflowService.ChangeRequestWorkflowServiceBase
{
    private readonly ApiAudit.AuditEventQueryService.AuditEventQueryServiceClient _auditAuditEventQueryService;
    private readonly NotificationBus _bus;
    private readonly ApiDocuments.DocumentExportService.DocumentExportServiceClient _documentsDocumentExportService;
    private readonly ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient _documentsDocumentFolderValidationService;
    private readonly ApiIdentity.UserService.UserServiceClient _identityUserService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ChangeRequestReportingService.ChangeRequestReportingServiceClient _itsmChangeRequestReportingService;
    private readonly ApiItsm.IncidentApprovalService.IncidentApprovalServiceClient _itsmIncidentApprovalService;
    private readonly ApiItsm.IncidentWorkflowService.IncidentWorkflowServiceClient _itsmIncidentWorkflowService;
    private readonly ApiItsm.ProblemSyncService.ProblemSyncServiceClient _itsmProblemSyncService;
    private readonly ApiItsm.ServiceAssetApprovalService.ServiceAssetApprovalServiceClient _itsmServiceAssetApprovalService;
    private readonly ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient _licensingLicenseAssignmentReportingService;
    private readonly ILogger<ChangeRequestWorkflowEndpoint> _logger;
    private readonly ApiPayroll.PayRunCalculationService.PayRunCalculationServiceClient _payrollPayRunCalculationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;

    public ChangeRequestWorkflowEndpoint(
        ApiAudit.AuditEventQueryService.AuditEventQueryServiceClient auditAuditEventQueryService,
        NotificationBus bus,
        ApiDocuments.DocumentExportService.DocumentExportServiceClient documentsDocumentExportService,
        ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient documentsDocumentFolderValidationService,
        ApiIdentity.UserService.UserServiceClient identityUserService,
        CallInvoker invoker,
        ApiItsm.ChangeRequestReportingService.ChangeRequestReportingServiceClient itsmChangeRequestReportingService,
        ApiItsm.IncidentApprovalService.IncidentApprovalServiceClient itsmIncidentApprovalService,
        ApiItsm.IncidentWorkflowService.IncidentWorkflowServiceClient itsmIncidentWorkflowService,
        ApiItsm.ProblemSyncService.ProblemSyncServiceClient itsmProblemSyncService,
        ApiItsm.ServiceAssetApprovalService.ServiceAssetApprovalServiceClient itsmServiceAssetApprovalService,
        ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient licensingLicenseAssignmentReportingService,
        ILogger<ChangeRequestWorkflowEndpoint> logger,
        ApiPayroll.PayRunCalculationService.PayRunCalculationServiceClient payrollPayRunCalculationService,
        ClientRegistry registry,
        IRecordRepository repository)
    {
        _auditAuditEventQueryService = auditAuditEventQueryService;
        _bus = bus;
        _documentsDocumentExportService = documentsDocumentExportService;
        _documentsDocumentFolderValidationService = documentsDocumentFolderValidationService;
        _identityUserService = identityUserService;
        _invoker = invoker;
        _itsmChangeRequestReportingService = itsmChangeRequestReportingService;
        _itsmIncidentApprovalService = itsmIncidentApprovalService;
        _itsmIncidentWorkflowService = itsmIncidentWorkflowService;
        _itsmProblemSyncService = itsmProblemSyncService;
        _itsmServiceAssetApprovalService = itsmServiceAssetApprovalService;
        _licensingLicenseAssignmentReportingService = licensingLicenseAssignmentReportingService;
        _logger = logger;
        _payrollPayRunCalculationService = payrollPayRunCalculationService;
        _registry = registry;
        _repository = repository;
    }
    /// <summary>
    /// Advances a change request.
    /// </summary>
    public override async Task<ApiItsm.AdvanceChangeRequestWorkflowResponse> AdvanceChangeRequestWorkflow(ApiItsm.AdvanceChangeRequestWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling AdvanceChangeRequestWorkflow for {Tenant}", requestContext.TenantId);
        if (request.ChangeRequest is { } record)
        {
            var snapshot = ChangeRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.PushProblemRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.PushProblemRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Problem = new ApiItsm.Problem
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.ChangeRequest?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (request.ChangeRequest?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Notify?.Phone ?? ""),
                },
            },
        };
        using var rpc0 = _itsmProblemSyncService.PushProblemAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiItsm.AdvanceChangeRequestWorkflowResponse
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                ChangeRequestId = (request.ChangeRequestId ?? ""),
                CustomFields = { (request.ChangeRequest?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Problem?.Customer?.Email ?? ""),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "AdvanceChangeRequestWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels a change request.
    /// </summary>
    public override async Task<ApiItsm.CancelChangeRequestWorkflowResponse> CancelChangeRequestWorkflow(ApiItsm.CancelChangeRequestWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CancelChangeRequestWorkflow for {Tenant}", requestContext.TenantId);
        if (request.ChangeRequest is { } record)
        {
            var snapshot = ChangeRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.RecallIncidentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Incident = new ApiItsm.Incident
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.ChangeRequest?.Audit?.Etag ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Phone = (request.ChangeRequest?.Requester?.Phone ?? ""),
                },
            },
        };
        var named0 = _registry.Get<ApiItsm.IncidentApprovalService.IncidentApprovalServiceClient>("acme.itsm.v1.IncidentApprovalService");
        using var rpc0 = named0.RecallIncidentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiItsm.CancelChangeRequestWorkflowResponse
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Incident?.Audit?.CreatedBy ?? ""),
                },
                ChangeRequestId = (request.ChangeRequestId ?? ""),
                Subject = (request.ChangeRequest?.Subject ?? ""),
            },
            Outcome = new ApiItsm.CancelChangeRequestWorkflowResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "CancelChangeRequestWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a change request.
    /// </summary>
    public override async Task<ApiItsm.InspectChangeRequestWorkflowResponse> InspectChangeRequestWorkflow(ApiItsm.InspectChangeRequestWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ChangeRequestId, "change_request_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling InspectChangeRequestWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "change_request", request.ChangeRequestId,
            "InspectChangeRequestWorkflow", "accepted", token);
        var input0 = new ApiItsm.ListChangeRequestReportsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.ListChangeRequestReportsRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                Labels = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.ChangeRequestId ?? ""),
            },
        };
        var descriptor0 = new Method<ApiItsm.ListChangeRequestReportsRequest, ApiItsm.ListChangeRequestReportsResponse>(
            MethodType.Unary, "acme.itsm.v1" + ".ChangeRequestReportingService", "ListChangeRequestReports",
            Wire.Marshaller<ApiItsm.ListChangeRequestReportsRequest>(), Wire.Marshaller<ApiItsm.ListChangeRequestReportsResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiItsm.InspectChangeRequestWorkflowResponse
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                ChangeRequestId = (request.ChangeRequestId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists change requests a page at a time.
    /// </summary>
    public override async Task<ApiItsm.ListChangeRequestWorkflowsResponse> ListChangeRequestWorkflows(ApiItsm.ListChangeRequestWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListChangeRequestWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "change_request", "batch",
            "ListChangeRequestWorkflows", "accepted", token);
        var response = new ApiItsm.ListChangeRequestWorkflowsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "ListChangeRequestWorkflows", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pauses a change request.
    /// </summary>
    public override async Task<ApiItsm.PauseChangeRequestWorkflowResponse> PauseChangeRequestWorkflow(ApiItsm.PauseChangeRequestWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PauseChangeRequestWorkflow for {Tenant}", requestContext.TenantId);
        if (request.ChangeRequest is { } record)
        {
            var snapshot = ChangeRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.ChangeRequest?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiItsm.ChangeRequestComment>()).Select(async item =>
        {
            var input0 = new ApiLicensing.ShareLicenseAssignmentReportRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiLicensing.ShareLicenseAssignmentReportRequest.Types.Details
                {
                    Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                    Recipient = new ApiCommon.ContactInfo
                    {
                        Email = (item.AuthorEmail ?? ""),
                        PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                    },
                },
            };
            using var rpc0 = _licensingLicenseAssignmentReportingService.ShareLicenseAssignmentReportAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiLicensing.ShareLicenseAssignmentReportResponse();
        var input1 = new ApiDocuments.RetryDocumentExportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Document = new ApiDocuments.Document
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.LicenseAssignment?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (request.ChangeRequest?.Audit?.UpdatedBy ?? ""),
                },
                Bank = new ApiDocuments.DocumentBankSettlement
                {
                    AccountHolder = (reply0.LicenseAssignment ?? new ApiLicensing.LicenseAssignment()).PaymentCase switch
                    {
                        ApiLicensing.LicenseAssignment.SettlementOneofCase.Bank => (reply0.LicenseAssignment?.Bank?.AccountHolder ?? ""),
                        _ => ""
                    },
                },
                Counterparty = new ApiDocuments.DocumentFolderRef
                {
                    DisplayName = (reply0.LicenseAssignment?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _documentsDocumentExportService.RetryDocumentExportAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiDocuments.RetryDocumentExportRequest>("acme.itsm.v1.ChangeRequestWorkflowService/PauseChangeRequestWorkflow/1", input1.Clone()), token);
        var response = new ApiItsm.PauseChangeRequestWorkflowResponse
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Document?.Audit?.CreatedAt ?? new Timestamp()),
                },
                ChangeRequestId = (request.ChangeRequestId ?? ""),
                Status = (request.ChangeRequest?.Status ?? (ApiItsm.ChangeRequestStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "PauseChangeRequestWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resumes a change request.
    /// </summary>
    public override async Task<ApiItsm.ResumeChangeRequestWorkflowResponse> ResumeChangeRequestWorkflow(ApiItsm.ResumeChangeRequestWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ResumeChangeRequestWorkflow for {Tenant}", requestContext.TenantId);
        if (request.ChangeRequest is { } record)
        {
            var snapshot = ChangeRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiIdentity.CreateUserRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            User = new ApiIdentity.User
            {
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.ChangeRequest?.Requester?.PreferredChannel ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _identityUserService.CreateUserAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiItsm.ResumeChangeRequestWorkflowResponse
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.User?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                ChangeRequestId = (request.ChangeRequestId ?? ""),
                Status = (request.ChangeRequest?.Status ?? (ApiItsm.ChangeRequestStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "ResumeChangeRequestWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys a change request.
    /// </summary>
    public override async Task<ApiItsm.RetryChangeRequestWorkflowResponse> RetryChangeRequestWorkflow(ApiItsm.RetryChangeRequestWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RetryChangeRequestWorkflow for {Tenant}", requestContext.TenantId);
        if (request.ChangeRequest is { } record)
        {
            var snapshot = ChangeRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.RetryChangeRequestWorkflowToEscalateServiceAsset0();
        using var rpc0 = _itsmServiceAssetApprovalService.EscalateServiceAssetAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiAudit.BrowseAuditEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiAudit.BrowseAuditEventsRequest.Types.Filter
            {
                Region = (reply0.ServiceAsset ?? new ApiItsm.ServiceAsset()).LocatorCase switch
                {
                    ApiItsm.ServiceAsset.LocatorOneofCase.Address => (reply0.ServiceAsset?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc1 = _auditAuditEventQueryService.BrowseAuditEventsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.RetryChangeRequestWorkflowResponse
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                ChangeRequestId = (request.ChangeRequestId ?? ""),
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply0.ServiceAsset?.Owner?.Email ?? ""),
                },
                Subject = (request.ChangeRequest?.Subject ?? ""),
            },
            Outcome = new ApiItsm.RetryChangeRequestWorkflowResponse.Types.Outcome
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "RetryChangeRequestWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals a change request.
    /// </summary>
    public override async Task<ApiItsm.SignalChangeRequestWorkflowResponse> SignalChangeRequestWorkflow(ApiItsm.SignalChangeRequestWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SignalChangeRequestWorkflow for {Tenant}", requestContext.TenantId);
        if (request.ChangeRequest is { } record)
        {
            var snapshot = ChangeRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.AdvanceIncidentWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.AdvanceIncidentWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.ChangeRequest?.Assignee?.Email ?? ""),
                    PreferredChannel = (request.ChangeRequest?.Requester?.PreferredChannel ?? ""),
                },
            },
            Incident = new ApiItsm.Incident
            {
                Channel = new ApiItsm.IncidentChannelOrigin
                {
                    Address = (request.ChangeRequest ?? new ApiItsm.ChangeRequest()).OriginCase switch
                    {
                        ApiItsm.ChangeRequest.OriginOneofCase.Channel => (request.ChangeRequest?.Channel?.Address ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _itsmIncidentWorkflowService.AdvanceIncidentWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiItsm.SignalChangeRequestWorkflowResponse
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply0.Incident?.Requester?.Email ?? ""),
                },
                ChangeRequestId = (request.ChangeRequestId ?? ""),
                Status = (request.ChangeRequest?.Status ?? (ApiItsm.ChangeRequestStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "SignalChangeRequestWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts a change request.
    /// </summary>
    public override async Task<ApiItsm.StartChangeRequestWorkflowResponse> StartChangeRequestWorkflow(ApiItsm.StartChangeRequestWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling StartChangeRequestWorkflow for {Tenant}", requestContext.TenantId);
        if (request.ChangeRequest is { } record)
        {
            var snapshot = ChangeRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.NormalizeDocumentFolderRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            UpdatedAt = (request.ChangeRequest?.Audit?.UpdatedAt ?? new Timestamp()),
                        },
                    } },
                } },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiDocuments.NormalizeDocumentFolderRequest>();
        using var rpc0 = _documentsDocumentFolderValidationService.NormalizeDocumentFolderAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiPayroll.SimulatePayRunRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPayroll.SimulatePayRunRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.DocumentFolder?.Location?.Recipient ?? ""),
                    Region = (reply0.DocumentFolder?.Location?.Region ?? ""),
                },
            },
            PayRun = new ApiPayroll.PayRun
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.DocumentFolder?.Location?.PostalCode ?? ""),
                },
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiPayroll.SimulatePayRunRequest>(json1);
        using var rpc1 = _payrollPayRunCalculationService.SimulatePayRunAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.StartChangeRequestWorkflowResponse
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                ChangeRequestId = (request.ChangeRequestId ?? ""),
                CustomFields = { (request.ChangeRequest?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "StartChangeRequestWorkflow", (request.Reason ?? ""), token);
        return response;
    }
}
