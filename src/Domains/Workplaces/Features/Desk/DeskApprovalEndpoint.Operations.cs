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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiFleet = Acme.Fleet.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

public sealed partial class DeskApprovalEndpoint
{
    /// <summary>
    /// Approves a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.ApproveDeskResponse> ApproveDesk(ApiWorkplaces.ApproveDeskRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ApproveDesk for {Tenant}", requestContext.TenantId);
        if (request.Desk is { } record)
        {
            var snapshot = DeskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiNotifications.ValidateNotificationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Notification = new ApiNotifications.Notification
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Desk?.Owner?.DisplayName ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Desk?.Audit?.Etag ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiNotifications.ValidateNotificationRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _notificationsNotificationValidationService.ValidateNotificationAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiWorkplaces.ApproveDeskResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                DeskId = (request.DeskId ?? ""),
                Status = (request.Desk?.Status ?? (ApiWorkplaces.DeskStatus)0),
            },
            Outcome = new ApiWorkplaces.ApproveDeskResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "desk",
            "audit", "ApproveDesk", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.CommentDeskApprovalResponse> CommentDeskApproval(ApiWorkplaces.CommentDeskApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CommentDeskApproval for {Tenant}", requestContext.TenantId);
        if (request.Desk is { } record)
        {
            var snapshot = DeskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiWorkplaces.CommentDeskApprovalRequest, ApiCorporateevents.GenerateCorporateEventReportRequest>(value => new ApiCorporateevents.GenerateCorporateEventReportRequest
        {
            Annotations = { (value.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.GenerateCorporateEventReportRequest.Types.Details
            {
                Filters = { (value.Desk?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Reason = (value.Reason ?? ""),
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _corporateeventsCorporateEventReportingService.GenerateCorporateEventReportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiFeatureflags.SummarizeFeatureFlagsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _featureflagsFeatureFlagQueryService.SummarizeFeatureFlagsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = request.CommentDeskApprovalToExplainDeskCalculation2();
        using var rpc2 = _workplacesDeskCalculationService.ExplainDeskCalculationAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiLicensing.ListPendingLicenseAssignmentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLicensing.ListPendingLicenseAssignmentsRequest.Types.Filter
            {
                CreatedAfter = (reply2.Desk?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Region = (request.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                {
                    ApiWorkplaces.Desk.LocatorOneofCase.Address => (request.Desk?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc3 = _licensingLicenseAssignmentApprovalService.ListPendingLicenseAssignmentsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiWorkplaces.CommentDeskApprovalResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                DeskId = (request.DeskId ?? ""),
                Status = (reply2.Desk?.Status ?? (ApiWorkplaces.DeskStatus)0),
            },
            Outcome = new ApiWorkplaces.CommentDeskApprovalResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply3.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Comment ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "desk",
            "audit", "CommentDeskApproval", (request.Details?.Comment ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.DelegateDeskApprovalResponse> DelegateDeskApproval(ApiWorkplaces.DelegateDeskApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling DelegateDeskApproval for {Tenant}", requestContext.TenantId);
        if (request.Desk is { } record)
        {
            var snapshot = DeskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.SearchEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.SearchEmployeesRequest.Types.Filter
            {
                Region = (request.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                {
                    ApiWorkplaces.Desk.LocatorOneofCase.Address => (request.Desk?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc0 = _employeesEmployeeService.SearchEmployeesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.ListSignatureConflictsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListSignatureConflictsRequest.Types.Filter
            {
                CreatedAfter = (request.Desk?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.Desk?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Desk?.Components?.GetValueOrDefault("primary")?.ComponentId ?? ""),
                Region = (request.Desk?.Address?.Region ?? ""),
            },
        };
        using var rpc1 = _documentsSignatureSyncService.ListSignatureConflictsAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        var input2 = new ApiItsm.SignalIncidentWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Incident = new ApiItsm.Incident
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (request.Desk?.Owner?.UserId ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.Desk?.Audit?.UpdatedBy ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Desk?.Owner?.Email ?? ""),
                },
            },
        };
        using var rpc2 = _itsmIncidentWorkflowService.SignalIncidentWorkflowAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        await Task.WhenAll(pending1, pending2);
        var reply1 = await pending1;
        var reply2 = await pending2;
        var response = new ApiWorkplaces.DelegateDeskApprovalResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply2.Incident?.Audit?.UpdatedBy ?? ""),
                },
                DeskId = (request.DeskId ?? ""),
                Name = (request.Desk?.Name ?? ""),
            },
            Outcome = new ApiWorkplaces.DelegateDeskApprovalResponse.Types.Outcome
            {
                Message = (reply2.Outcome?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "desk",
            "audit", "DelegateDeskApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.GetDeskApprovalStatusResponse> GetDeskApprovalStatus(ApiWorkplaces.GetDeskApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DeskId, "desk_id");
        _logger.LogInformation("Handling GetDeskApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "desk", request.DeskId,
            "GetDeskApprovalStatus", "accepted", token);
        var response = new ApiWorkplaces.GetDeskApprovalStatusResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                DeskId = (request.DeskId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Recalls a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.RecallDeskResponse> RecallDesk(ApiWorkplaces.RecallDeskRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecallDesk for {Tenant}", requestContext.TenantId);
        if (request.Desk is { } record)
        {
            var snapshot = DeskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.CloneEmployeeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Employee = new ApiEmployees.Employee
            {
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (request.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                    {
                        ApiWorkplaces.Desk.LocatorOneofCase.Address => (request.Desk?.Address?.Recipient ?? ""),
                        _ => ""
                    },
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    UserId = (request.Details?.Approver?.UserId ?? ""),
                },
            },
        };
        using var rpc0 = _employeesEmployeeService.CloneEmployeeAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiWorkplaces.ExportBuildingsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiWorkplaces.ExportBuildingsRequest.Types.Filter
            {
                CreatedAfter = (request.Desk?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Details?.Approver?.UserId ?? ""),
                Region = (reply0.Employee?.PrimaryAddress?.Region ?? ""),
            },
        };
        using var stream1 = _workplacesBuildingQueryService.ExportBuildings(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportBuildings frame {Type}", change1.GetType().Name);
        }
        var input2 = new ApiWorkplaces.ListRoomBookingReportsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiWorkplaces.ListRoomBookingReportsRequest.Types.Filter
            {
                Region = (request.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                {
                    ApiWorkplaces.Desk.LocatorOneofCase.Address => (request.Desk?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc2 = _workplacesRoomBookingReportingService.ListRoomBookingReportsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiFleet.ListVehiclesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiFleet.ListVehiclesRequest.Types.Filter
            {
                CreatedAfter = (request.Desk?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.Employee?.RelationshipManager?.UserId ?? ""),
                Region = (reply0.Employee?.PrimaryAddress?.Region ?? ""),
            },
        };
        var reply3 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _fleetVehicleService.ListVehiclesAsync(input3, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiWorkplaces.RecallDeskResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                DeskId = (request.DeskId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.Employee?.Contact?.Email ?? ""),
                },
                Status = (request.Desk?.Status ?? (ApiWorkplaces.DeskStatus)0),
            },
            Outcome = new ApiWorkplaces.RecallDeskResponse.Types.Outcome
            {
                Status = (reply3.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "desk",
            "audit", "RecallDesk", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.RejectDeskResponse> RejectDesk(ApiWorkplaces.RejectDeskRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RejectDesk for {Tenant}", requestContext.TenantId);
        if (request.Desk is { } record)
        {
            var snapshot = DeskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.UpdateEmployeeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Employee = new ApiEmployees.Employee
            {
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                    {
                        ApiWorkplaces.Desk.LocatorOneofCase.Address => (request.Desk?.Address?.CountryCode ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _employeesEmployeeService.UpdateEmployeeAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiWorkplaces.EstimateVisitorPassRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                    {
                        ApiWorkplaces.Desk.LocatorOneofCase.Address => (request.Desk?.Address?.CountryCode ?? ""),
                        _ => ""
                    },
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (reply0.Employee?.Contact?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _workplacesVisitorPassCalculationService.EstimateVisitorPassAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.RejectDeskResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                Capacity = (request.Desk?.Capacity ?? 0L),
                DeskId = (request.DeskId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.VisitorPass?.Counterparty?.DisplayName ?? ""),
                },
            },
            Outcome = new ApiWorkplaces.RejectDeskResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "desk",
            "audit", "RejectDesk", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.SubmitDeskResponse> SubmitDesk(ApiWorkplaces.SubmitDeskRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitDesk for {Tenant}", requestContext.TenantId);
        if (request.Desk is { } record)
        {
            var snapshot = DeskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWorkplaces.ExpireDeskRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DeskId = (request.Desk?.DeskId ?? ""),
            Details = new ApiWorkplaces.ExpireDeskRequest.Types.Details
            {
                NewOwner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                    UserId = (request.Desk?.Owner?.UserId ?? ""),
                },
            },
        };
        var named0 = _registry.Get<ApiWorkplaces.DeskLifecycleService.DeskLifecycleServiceClient>("acme.workplaces.v1.DeskLifecycleService");
        using var rpc0 = named0.ExpireDeskAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.BatchGetEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var descriptor1 = new Method<ApiEmployees.BatchGetEmployeesRequest, ApiEmployees.BatchGetEmployeesResponse>(
            MethodType.Unary, "acme.employees.v1" + ".EmployeeService", "BatchGetEmployees",
            Wire.Marshaller<ApiEmployees.BatchGetEmployeesRequest>(), Wire.Marshaller<ApiEmployees.BatchGetEmployeesResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLicensing.SubmitLicenseAssignmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Desk?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var command2 = new DispatchCommand<ApiLicensing.SubmitLicenseAssignmentRequest>(input2,
            "primary", requestContext.TenantId);
        var routed2 = command2 with { Region = _settings.Value.Region };
        using var rpc2 = _licensingLicenseAssignmentApprovalService.SubmitLicenseAssignmentAsync(routed2.Payload, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var profile3 = new MappingProfile<ApiWorkplaces.SubmitDeskRequest, ApiWorkplaces.GenerateRoomBookingReportRequest>(value => new ApiWorkplaces.GenerateRoomBookingReportRequest
        {
            Annotations = { (value.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Reason = (value.Reason ?? ""),
        });
        var input3 = profile3.Map(request);
        using var rpc3 = _workplacesRoomBookingReportingService.GenerateRoomBookingReportAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiWorkplaces.SubmitDeskResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                Code = (request.Desk?.Code ?? ""),
                DeskId = (reply0.Desk?.DeskId ?? ""),
                Name = (request.Desk?.Name ?? ""),
            },
            Outcome = new ApiWorkplaces.SubmitDeskResponse.Types.Outcome
            {
                Message = (reply2.Result?.Message ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply3.Result?.CorrelationId ?? ""),
                Status = (reply2.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Comment ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "desk",
            "audit", "SubmitDesk", (request.Details?.Comment ?? ""), token);
        return response;
    }
}
