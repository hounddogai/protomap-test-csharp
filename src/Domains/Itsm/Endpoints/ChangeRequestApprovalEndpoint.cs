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
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Itsm;

/// <summary>
/// Routes change requests through approval, with escalation and delegation.
/// </summary>
public sealed class ChangeRequestApprovalEndpoint : ApiItsm.ChangeRequestApprovalService.ChangeRequestApprovalServiceBase
{
    private readonly NotificationBus _bus;
    private readonly ApiDevices.ComplianceScanApprovalService.ComplianceScanApprovalServiceClient _devicesComplianceScanApprovalService;
    private readonly ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceClient _devicesDevicePolicyWorkflowService;
    private readonly ApiDocuments.SignatureNotificationService.SignatureNotificationServiceClient _documentsSignatureNotificationService;
    private readonly ApiItsm.ChangeRequestQueryService.ChangeRequestQueryServiceClient _itsmChangeRequestQueryService;
    private readonly ApiItsm.IncidentService.IncidentServiceClient _itsmIncidentService;
    private readonly ApiItsm.IncidentValidationService.IncidentValidationServiceClient _itsmIncidentValidationService;
    private readonly ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient _licensingLicenseAssignmentApprovalService;
    private readonly ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient _licensingLicenseAssignmentReportingService;
    private readonly ILogger<ChangeRequestApprovalEndpoint> _logger;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiScheduling.ShiftQueryService.ShiftQueryServiceClient _schedulingShiftQueryService;
    private readonly Func<ApiDevices.PauseDevicePolicyWorkflowRequest, CallOptions, AsyncUnaryCall<ApiDevices.PauseDevicePolicyWorkflowResponse>> _sendRecallChangeRequest0;
    private readonly ApiVenues.VenueValidationService.VenueValidationServiceClient _venuesVenueValidationService;

    public ChangeRequestApprovalEndpoint(
        NotificationBus bus,
        ApiDevices.ComplianceScanApprovalService.ComplianceScanApprovalServiceClient devicesComplianceScanApprovalService,
        ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceClient devicesDevicePolicyWorkflowService,
        ApiDocuments.SignatureNotificationService.SignatureNotificationServiceClient documentsSignatureNotificationService,
        ApiItsm.ChangeRequestQueryService.ChangeRequestQueryServiceClient itsmChangeRequestQueryService,
        ApiItsm.IncidentService.IncidentServiceClient itsmIncidentService,
        ApiItsm.IncidentValidationService.IncidentValidationServiceClient itsmIncidentValidationService,
        ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient licensingLicenseAssignmentApprovalService,
        ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient licensingLicenseAssignmentReportingService,
        ILogger<ChangeRequestApprovalEndpoint> logger,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiScheduling.ShiftQueryService.ShiftQueryServiceClient schedulingShiftQueryService,
        ApiVenues.VenueValidationService.VenueValidationServiceClient venuesVenueValidationService)
    {
        _bus = bus;
        _devicesComplianceScanApprovalService = devicesComplianceScanApprovalService;
        _devicesDevicePolicyWorkflowService = devicesDevicePolicyWorkflowService;
        _documentsSignatureNotificationService = documentsSignatureNotificationService;
        _itsmChangeRequestQueryService = itsmChangeRequestQueryService;
        _itsmIncidentService = itsmIncidentService;
        _itsmIncidentValidationService = itsmIncidentValidationService;
        _licensingLicenseAssignmentApprovalService = licensingLicenseAssignmentApprovalService;
        _licensingLicenseAssignmentReportingService = licensingLicenseAssignmentReportingService;
        _logger = logger;
        _registry = registry;
        _repository = repository;
        _schedulingShiftQueryService = schedulingShiftQueryService;
        _venuesVenueValidationService = venuesVenueValidationService;
        _sendRecallChangeRequest0 = _devicesDevicePolicyWorkflowService.PauseDevicePolicyWorkflowAsync;
    }
    /// <summary>
    /// Approves a change request.
    /// </summary>
    public override async Task<ApiItsm.ApproveChangeRequestResponse> ApproveChangeRequest(ApiItsm.ApproveChangeRequestRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ApproveChangeRequest for {Tenant}", requestContext.TenantId);
        if (request.ChangeRequest is { } record)
        {
            var snapshot = ChangeRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.ListLicenseAssignmentReportsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLicensing.ListLicenseAssignmentReportsRequest.Types.Filter
            {
                CreatedAfter = (request.ChangeRequest?.Sla?.DueAt ?? new Timestamp()),
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _licensingLicenseAssignmentReportingService.ListLicenseAssignmentReportsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiVenues.ScoreVenueRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Venue = new ApiVenues.Venue
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.ChangeRequest ?? new ApiItsm.ChangeRequest()).OriginCase switch
                    {
                        ApiItsm.ChangeRequest.OriginOneofCase.Customer => (request.ChangeRequest?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _venuesVenueValidationService.ScoreVenueAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLicensing.CommentLicenseAssignmentApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.CommentLicenseAssignmentApprovalRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                },
            },
        };
        var json2 = JsonFormatter.Default.Format(input2);
        input2 = JsonParser.Default.Parse<ApiLicensing.CommentLicenseAssignmentApprovalRequest>(json2);
        using var rpc2 = _licensingLicenseAssignmentApprovalService.CommentLicenseAssignmentApprovalAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiItsm.ApproveChangeRequestResponse
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.Venue?.Owner?.DisplayName ?? ""),
                },
                ChangeRequestId = (request.ChangeRequestId ?? ""),
                Requester = (request.ChangeRequest?.Requester ?? new ApiCommon.ContactInfo()),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "ApproveChangeRequest", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments a change request.
    /// </summary>
    public override async Task<ApiItsm.CommentChangeRequestApprovalResponse> CommentChangeRequestApproval(ApiItsm.CommentChangeRequestApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CommentChangeRequestApproval for {Tenant}", requestContext.TenantId);
        if (request.ChangeRequest is { } record)
        {
            var snapshot = ChangeRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.ExportChangeRequestsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.ExportChangeRequestsRequest.Types.Filter
            {
                CreatedAfter = (request.ChangeRequest?.Audit?.UpdatedAt ?? new Timestamp()),
                OwnerId = (request.ChangeRequest?.ChangeRequestId ?? ""),
            },
        };
        using var stream0 = _itsmChangeRequestQueryService.ExportChangeRequests(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportChangeRequests frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiItsm.SearchIncidentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.SearchIncidentsRequest.Types.Filter
            {
                CreatedAfter = (request.ChangeRequest?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.ChangeRequest?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.ChangeRequest?.Sla?.PolicyId ?? ""),
            },
        };
        using var rpc1 = _itsmIncidentService.SearchIncidentsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.CommentChangeRequestApprovalResponse
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                ChangeRequestId = (request.ChangeRequestId ?? ""),
                Requester = (request.ChangeRequest?.Requester ?? new ApiCommon.ContactInfo()),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "CommentChangeRequestApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates a change request.
    /// </summary>
    public override async Task<ApiItsm.DelegateChangeRequestApprovalResponse> DelegateChangeRequestApproval(ApiItsm.DelegateChangeRequestApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling DelegateChangeRequestApproval for {Tenant}", requestContext.TenantId);
        if (request.ChangeRequest is { } record)
        {
            var snapshot = ChangeRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.SubmitComplianceScanRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (request.ChangeRequest ?? new ApiItsm.ChangeRequest()).OriginCase switch
                    {
                        ApiItsm.ChangeRequest.OriginOneofCase.Customer => (request.ChangeRequest?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                    Email = (request.ChangeRequest?.Requester?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.SubmitComplianceScanRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    UserId = (request.Details?.Approver?.UserId ?? ""),
                },
            },
        };
        using var rpc0 = _devicesComplianceScanApprovalService.SubmitComplianceScanAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDevices.ApproveComplianceScanRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (reply0.ComplianceScan?.Subject?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.ApproveComplianceScanRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (reply0.ComplianceScan?.Subject?.Email ?? ""),
                    UserId = (request.ChangeRequest?.Assignee?.UserId ?? ""),
                },
            },
        };
        using var rpc1 = _devicesComplianceScanApprovalService.ApproveComplianceScanAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiDevices.ApproveComplianceScanRequest>("acme.itsm.v1.ChangeRequestApprovalService/DelegateChangeRequestApproval/1", input1.Clone()), token);
        var input2 = new ApiScheduling.SummarizeShiftsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply2 = await Workflow.SendAsync(input2, token, _schedulingShiftQueryService.SummarizeShiftsAsync);
        var response = new ApiItsm.DelegateChangeRequestApprovalResponse
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                ChangeRequestId = (request.ChangeRequestId ?? ""),
                Priority = (request.ChangeRequest?.Priority ?? 0),
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply1.ComplianceScan?.Subject?.Email ?? ""),
                },
            },
            Outcome = new ApiItsm.DelegateChangeRequestApprovalResponse.Types.Outcome
            {
                Message = (reply0.Outcome?.Message ?? ""),
                NextSteps = { (reply0.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply2.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "DelegateChangeRequestApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a change request.
    /// </summary>
    public override async Task<ApiItsm.GetChangeRequestApprovalStatusResponse> GetChangeRequestApprovalStatus(ApiItsm.GetChangeRequestApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ChangeRequestId, "change_request_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetChangeRequestApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "change_request", request.ChangeRequestId,
            "GetChangeRequestApprovalStatus", "accepted", token);
        var response = new ApiItsm.GetChangeRequestApprovalStatusResponse
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                ChangeRequestId = (request.ChangeRequestId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Recalls a change request.
    /// </summary>
    public override async Task<ApiItsm.RecallChangeRequestResponse> RecallChangeRequest(ApiItsm.RecallChangeRequestRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RecallChangeRequest for {Tenant}", requestContext.TenantId);
        if (request.ChangeRequest is { } record)
        {
            var snapshot = ChangeRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.PauseDevicePolicyWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.PauseDevicePolicyWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                    Phone = (request.ChangeRequest?.Requester?.Phone ?? ""),
                },
            },
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.ChangeRequest?.Audit?.CreatedBy ?? ""),
                    Etag = (request.ChangeRequest?.Audit?.Etag ?? ""),
                },
            },
        };
        using var rpc0 = _sendRecallChangeRequest0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiItsm.RecallChangeRequestResponse
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (reply0.DevicePolicy?.Owner?.UserId ?? ""),
                },
                ChangeRequestId = (request.ChangeRequestId ?? ""),
                Requester = (request.ChangeRequest?.Requester ?? new ApiCommon.ContactInfo()),
            },
            Outcome = new ApiItsm.RecallChangeRequestResponse.Types.Outcome
            {
                ReferenceId = (reply0.Outcome?.ReferenceId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "RecallChangeRequest", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits a change request.
    /// </summary>
    public override async Task<ApiItsm.SubmitChangeRequestResponse> SubmitChangeRequest(ApiItsm.SubmitChangeRequestRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SubmitChangeRequest for {Tenant}", requestContext.TenantId);
        if (request.ChangeRequest is { } record)
        {
            var snapshot = ChangeRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.RecallLicenseAssignmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.RecallLicenseAssignmentRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (request.ChangeRequest?.Assignee?.Email ?? ""),
                    UserId = (request.ChangeRequest?.Assignee?.UserId ?? ""),
                },
            },
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.ChangeRequest?.Audit?.Etag ?? ""),
                },
            },
        };
        var named0 = _registry.Get<ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient>("acme.licensing.v1.LicenseAssignmentApprovalService");
        using var rpc0 = named0.RecallLicenseAssignmentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var batch1 = (request.ChangeRequest?.History ?? new Google.Protobuf.Collections.RepeatedField<ApiItsm.ChangeRequestStatusChange>()).Select(async item =>
        {
            var input1 = new ApiItsm.PreviewIncidentRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Incident = new ApiItsm.Incident
                {
                    History = { new ApiItsm.IncidentStatusChange
                    {
                        ChangedAt = (item.ChangedAt ?? new Timestamp()),
                        ChangedBy = (item.ChangedBy ?? ""),
                        ToStatus = (item.ToStatus ?? ""),
                    } },
                },
            };
            using var rpc1 = _itsmIncidentValidationService.PreviewIncidentAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            return reply1;
        }).ToArray();
        var replies1 = await Task.WhenAll(batch1);
        var reply1 = replies1.LastOrDefault() ?? new ApiItsm.PreviewIncidentResponse();
        var input2 = new ApiDocuments.SendSignatureTestNoticeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SendSignatureTestNoticeRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                    Phone = (request.ChangeRequest?.Requester?.Phone ?? ""),
                    PreferredChannel = (request.ChangeRequest?.Requester?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc2 = _documentsSignatureNotificationService.SendSignatureTestNoticeAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiItsm.SubmitChangeRequestResponse
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.LicenseAssignment?.Audit?.CreatedBy ?? ""),
                },
                ChangeRequestId = (request.ChangeRequestId ?? ""),
                Status = (request.ChangeRequest?.Status ?? (ApiItsm.ChangeRequestStatus)0),
            },
            Outcome = new ApiItsm.SubmitChangeRequestResponse.Types.Outcome
            {
                Message = (reply2.Outcome?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "SubmitChangeRequest", (request.Reason ?? ""), token);
        return response;
    }
}
