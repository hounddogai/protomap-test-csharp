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
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

public sealed partial class RoomBookingApprovalEndpoint
{
    /// <summary>
    /// Approves a room booking.
    /// </summary>
    public override async Task<ApiWorkplaces.ApproveRoomBookingResponse> ApproveRoomBooking(ApiWorkplaces.ApproveRoomBookingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ApproveRoomBooking for {Tenant}", requestContext.TenantId);
        if (request.RoomBooking is { } record)
        {
            var snapshot = RoomBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWorkplaces.DelegateDeskApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Desk = new ApiWorkplaces.Desk
            {
                Address = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.RoomBooking?.Location?.CountryCode ?? ""),
                    Recipient = (request.RoomBooking?.Location?.Recipient ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiWorkplaces.DelegateDeskApprovalRequest>(json0);
        using var rpc0 = _workplacesDeskApprovalService.DelegateDeskApprovalAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.ListSignatureVersionsRequest();
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        input1.Filter = new ApiDocuments.ListSignatureVersionsRequest.Types.Filter
        {
            CreatedAfter = (reply0.Desk?.Audit?.CreatedAt ?? new Timestamp()),
            Region = (request.RoomBooking?.Location?.Region ?? ""),
        };
        using var rpc1 = _documentsSignatureHistoryService.ListSignatureVersionsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.ApproveRoomBookingResponse
        {
            Outcome = new ApiWorkplaces.ApproveRoomBookingResponse.Types.Outcome
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Result?.Status ?? ""),
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Organizer = (request.RoomBooking?.Organizer ?? new ApiIdentity.UserRef()),
                Participants = { new ApiWorkplaces.RoomBookingParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply0.Desk?.Owner?.Email ?? ""),
                    },
                } },
                RoomBookingId = (request.RoomBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RoomBooking?.Title ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "room_booking",
            "audit", "ApproveRoomBooking", (request.RoomBooking?.Title ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments a room booking.
    /// </summary>
    public override async Task<ApiWorkplaces.CommentRoomBookingApprovalResponse> CommentRoomBookingApproval(ApiWorkplaces.CommentRoomBookingApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CommentRoomBookingApproval for {Tenant}", requestContext.TenantId);
        if (request.RoomBooking is { } record)
        {
            var snapshot = RoomBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.RevertDocumentFolderRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.RevertDocumentFolderRequest.Types.Details
            {
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Location = new ApiCommon.PostalAddress
                        {
                            Recipient = (request.RoomBooking?.Location?.Recipient ?? ""),
                        },
                    } },
                    Location = new ApiCommon.PostalAddress
                    {
                        Lines = { (request.RoomBooking?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    },
                } },
            },
        };
        using var rpc0 = _documentsDocumentFolderHistoryService.RevertDocumentFolderAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.ResolveDocumentTemplateConflictRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ResolveDocumentTemplateConflictRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.RoomBooking?.Location?.CountryCode ?? ""),
                    Locality = (request.RoomBooking?.Location?.Locality ?? ""),
                },
            },
        };
        using var rpc1 = _documentsDocumentTemplateSyncService.ResolveDocumentTemplateConflictAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiEmployees.CountEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.CountEmployeesRequest.Types.Filter
            {
                CreatedAfter = (request.RoomBooking?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (reply1.DocumentTemplate?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply1.DocumentTemplate?.Counterparty?.DocumentFolderId ?? ""),
                Region = (reply0.DocumentFolder?.Location?.Region ?? ""),
            },
        };
        var reply2 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _employeesEmployeeQueryService.CountEmployeesAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var reply3 = new ApiLedger.SearchLedgerAccountsResponse();
        foreach (var item in (request.RoomBooking?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiWorkplaces.RoomBookingParticipant>()))
        {
            var input3 = new ApiLedger.SearchLedgerAccountsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiLedger.SearchLedgerAccountsRequest.Types.Filter
                {
                    OwnerId = (item.ParticipantId ?? ""),
                    Region = (reply1.DocumentTemplate?.BillingAddress?.Region ?? ""),
                },
            };
            using var rpc3 = _ledgerLedgerAccountService.SearchLedgerAccountsAsync(input3, cancellationToken: token);
            reply3 = await rpc3.ResponseAsync;
        }
        var response = new ApiWorkplaces.CommentRoomBookingApprovalResponse
        {
            Outcome = new ApiWorkplaces.CommentRoomBookingApprovalResponse.Types.Outcome
            {
                EffectiveAt = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.DocumentTemplate?.Audit?.CreatedAt ?? new Timestamp()),
                },
                RoomBookingId = (request.RoomBookingId ?? ""),
                Title = (request.RoomBooking?.Title ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RoomBooking?.RoomBookingId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "room_booking",
            "audit", "CommentRoomBookingApproval", (request.RoomBooking?.RoomBookingId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates a room booking.
    /// </summary>
    public override async Task<ApiWorkplaces.DelegateRoomBookingApprovalResponse> DelegateRoomBookingApproval(ApiWorkplaces.DelegateRoomBookingApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling DelegateRoomBookingApproval for {Tenant}", requestContext.TenantId);
        if (request.RoomBooking is { } record)
        {
            var snapshot = RoomBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.ResolveEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _sendDelegateRoomBookingApproval0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.SimulateEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
        };
        var named1 = _registry.Get<ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient>("acme.employees.v1.EmploymentContractCalculationService");
        using var rpc1 = named1.SimulateEmploymentContractAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.DelegateRoomBookingApprovalResponse
        {
            Outcome = new ApiWorkplaces.DelegateRoomBookingApprovalResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply0.Result?.Message ?? ""),
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Location = new ApiCommon.PostalAddress
                {
                    Locality = (reply1.EmploymentContract?.BillingAddress?.Locality ?? ""),
                },
                Participants = { new ApiWorkplaces.RoomBookingParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply1.EmploymentContract?.Counterparty?.Email ?? ""),
                    },
                } },
                RoomBookingId = (request.RoomBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RoomBooking?.Title ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "room_booking",
            "audit", "DelegateRoomBookingApproval", (request.RoomBooking?.Title ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates a room booking.
    /// </summary>
    public override async Task<ApiWorkplaces.EscalateRoomBookingResponse> EscalateRoomBooking(ApiWorkplaces.EscalateRoomBookingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EscalateRoomBooking for {Tenant}", requestContext.TenantId);
        if (request.RoomBooking is { } record)
        {
            var snapshot = RoomBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.PullDocumentFoldersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.PullDocumentFoldersRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            CreatedAt = (request.RoomBooking?.Audit?.CreatedAt ?? new Timestamp()),
                        },
                    } },
                } },
            },
        };
        using var rpc0 = _documentsDocumentFolderSyncService.PullDocumentFoldersAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiWorkplaces.EscalateRoomBookingResponse
        {
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Location = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.DocumentFolder?.Location?.Locality ?? ""),
                },
                RoomBookingId = (request.RoomBookingId ?? ""),
                Window = (request.RoomBooking?.Window ?? new ApiCommon.TimeWindow()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RoomBooking?.Title ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "room_booking",
            "audit", "EscalateRoomBooking", (request.RoomBooking?.Title ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a room booking.
    /// </summary>
    public override async Task<ApiWorkplaces.GetRoomBookingApprovalStatusResponse> GetRoomBookingApprovalStatus(ApiWorkplaces.GetRoomBookingApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.RoomBookingId, "room_booking_id");
        _logger.LogInformation("Handling GetRoomBookingApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "room_booking", request.RoomBookingId,
            "GetRoomBookingApprovalStatus", "accepted", token);
        var input0 = new ApiDocuments.PushDocumentFolderRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.PushDocumentFolderRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Manager = new ApiIdentity.UserRef
                        {
                            UserId = (request.Context?.Actor?.UserId ?? ""),
                        },
                    } },
                } },
            },
        };
        using var rpc0 = _documentsDocumentFolderSyncService.PushDocumentFolderAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiWorkplaces.GetRoomBookingApprovalStatusResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                RoomBookingId = (request.RoomBookingId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists room bookings a page at a time.
    /// </summary>
    public override async Task<ApiWorkplaces.ListPendingRoomBookingsResponse> ListPendingRoomBookings(ApiWorkplaces.ListPendingRoomBookingsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListPendingRoomBookings for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "room_booking", "batch",
            "ListPendingRoomBookings", "accepted", token);
        var input0 = new ApiCorporateevents.StreamVenueMetricsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiCorporateevents.StreamVenueMetricsRequest.Types.Filter
            {
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Filter?.OwnerId ?? ""),
                Query = (request.Filter?.Query ?? ""),
                Region = (request.Filter?.Region ?? ""),
            },
        };
        using var stream0 = _corporateeventsVenueReportingService.StreamVenueMetrics(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamVenueMetrics frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiEmployees.ListEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.ListEmployeesRequest.Types.Filter
            {
                OwnerId = (request.Filter?.OwnerId ?? ""),
            },
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _employeesEmployeeService.ListEmployeesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.ListPendingRoomBookingsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "room_booking",
            "audit", "ListPendingRoomBookings", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls a room booking.
    /// </summary>
    public override async Task<ApiWorkplaces.RecallRoomBookingResponse> RecallRoomBooking(ApiWorkplaces.RecallRoomBookingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecallRoomBooking for {Tenant}", requestContext.TenantId);
        if (request.RoomBooking is { } record)
        {
            var snapshot = RoomBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.RecallRoomBookingToPauseBuildingWorkflow0();
        using var rpc0 = _workplacesBuildingWorkflowService.PauseBuildingWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiWorkplaces.PauseBuildingWorkflowRequest>("acme.workplaces.v1.RoomBookingApprovalService/RecallRoomBooking/0", input0.Clone()), token);
        var input1 = new ApiWorkplaces.ListVisitorPassConflictsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiWorkplaces.ListVisitorPassConflictsRequest.Types.Filter
            {
                CreatedAfter = (reply0.Building?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (request.Details?.Approver?.UserId ?? ""),
                Region = (reply0.Building?.Location?.Region ?? ""),
            },
        };
        var reply1 = await SendRecallRoomBooking1Async(input1, token);
        var response = new ApiWorkplaces.RecallRoomBookingResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Building?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Location = (request.RoomBooking?.Location ?? new ApiCommon.PostalAddress()),
                RoomBookingId = (request.RoomBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RoomBooking?.Title ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "room_booking",
            "audit", "RecallRoomBooking", (request.RoomBooking?.Title ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits a room booking.
    /// </summary>
    public override async Task<ApiWorkplaces.SubmitRoomBookingResponse> SubmitRoomBooking(ApiWorkplaces.SubmitRoomBookingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitRoomBooking for {Tenant}", requestContext.TenantId);
        if (request.RoomBooking is { } record)
        {
            var snapshot = RoomBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiEmployees.SuggestEmployeesResponse();
        foreach (var item in (request.RoomBooking?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiWorkplaces.RoomBookingParticipant>()))
        {
            var input0 = new ApiEmployees.SuggestEmployeesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiEmployees.SuggestEmployeesRequest.Types.Filter
                {
                    CreatedAfter = (request.RoomBooking?.Audit?.UpdatedAt ?? new Timestamp()),
                    Labels = { (request.RoomBooking?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    OwnerId = (item.ParticipantId ?? ""),
                    Region = (request.RoomBooking?.Location?.Region ?? ""),
                },
            };
            using var rpc0 = _employeesEmployeeQueryService.SuggestEmployeesAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var response = new ApiWorkplaces.SubmitRoomBookingResponse
        {
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                RoomBookingId = (request.RoomBookingId ?? ""),
                Title = (request.RoomBooking?.Title ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RoomBooking?.RoomBookingId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "room_booking",
            "audit", "SubmitRoomBooking", (request.RoomBooking?.RoomBookingId ?? ""), token);
        return response;
    }
    private async Task<ApiWorkplaces.ListVisitorPassConflictsResponse> SendRecallRoomBooking1Async(ApiWorkplaces.ListVisitorPassConflictsRequest request, CancellationToken token)
    {
        return await _roomBookingApprovalEndpointSendRecallRoomBooking1Port.SendAsync(request, token);
    }
}
