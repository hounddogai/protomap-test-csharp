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
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Scheduling;

public sealed partial class SwapRequestApprovalEndpoint
{
    /// <summary>
    /// Approves a swap request.
    /// </summary>
    public override async Task<ApiScheduling.ApproveSwapRequestResponse> ApproveSwapRequest(ApiScheduling.ApproveSwapRequestRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ApproveSwapRequest for {Tenant}", requestContext.TenantId);
        if (request.SwapRequest is { } record)
        {
            var snapshot = SwapRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiScheduling.SignalShiftWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Shift = new ApiScheduling.Shift
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.SwapRequest?.Audit?.Etag ?? ""),
                },
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (request.SwapRequest?.Requester?.Email ?? ""),
                },
                Participants = { new ApiScheduling.ShiftParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Phone = (request.SwapRequest?.Requester?.Phone ?? ""),
                    },
                } },
            },
        };
        var reply0 = await SendApproveSwapRequest0Async(input0, token);
        var response = new ApiScheduling.ApproveSwapRequestResponse
        {
            Outcome = new ApiScheduling.ApproveSwapRequestResponse.Types.Outcome
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
            SwapRequest = new ApiScheduling.SwapRequest
            {
                Priority = (request.SwapRequest?.Priority ?? 0),
                SwapRequestId = (request.SwapRequestId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SwapRequest?.SwapRequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "swap_request",
            "audit", "ApproveSwapRequest", (request.SwapRequest?.SwapRequestId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments a swap request.
    /// </summary>
    public override async Task<ApiScheduling.CommentSwapRequestApprovalResponse> CommentSwapRequestApproval(ApiScheduling.CommentSwapRequestApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CommentSwapRequestApproval for {Tenant}", requestContext.TenantId);
        if (request.SwapRequest is { } record)
        {
            var snapshot = SwapRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.CommentVenueApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Venue = new ApiCorporateevents.Venue
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.SwapRequest?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (request.SwapRequest?.Audit?.CreatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.SwapRequest ?? new ApiScheduling.SwapRequest()).OriginCase switch
                    {
                        ApiScheduling.SwapRequest.OriginOneofCase.Customer => (request.SwapRequest?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                    UserId = (request.SwapRequest?.Assignee?.UserId ?? ""),
                },
            },
        };
        using var rpc0 = _corporateeventsVenueApprovalService.CommentVenueApprovalAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var reply1 = new ApiCorporateevents.SubmitVenueResponse();
        foreach (var item in (request.SwapRequest?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.SwapRequestComment>()))
        {
            var input1 = new ApiCorporateevents.SubmitVenueRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiCorporateevents.SubmitVenueRequest.Types.Details
                {
                    Approver = new ApiIdentity.UserRef
                    {
                        UserId = (request.SwapRequest?.Assignee?.UserId ?? ""),
                    },
                },
                Venue = new ApiCorporateevents.Venue
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (item.CreatedAt ?? new Timestamp()),
                    },
                    Owner = new ApiIdentity.UserRef
                    {
                        DisplayName = (request.SwapRequest?.Customer?.DisplayName ?? ""),
                        Email = (item.AuthorEmail ?? ""),
                    },
                },
            };
            using var rpc1 = _corporateeventsVenueApprovalService.SubmitVenueAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var input2 = new ApiEmployees.ValidatePositionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Venue?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (reply0.Venue?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (reply0.Venue?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.SwapRequest?.Assignee?.DisplayName ?? ""),
                },
            },
        };
        var json2 = JsonFormatter.Default.Format(input2);
        input2 = JsonParser.Default.Parse<ApiEmployees.ValidatePositionRequest>(json2);
        using var rpc2 = _employeesPositionValidationService.ValidatePositionAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiScheduling.ExplainSwapRequestDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SwapRequest = new ApiScheduling.SwapRequest
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.SwapRequest?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedBy = (reply1.Venue?.Audit?.UpdatedBy ?? ""),
                },
                Customer = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.SwapRequest ?? new ApiScheduling.SwapRequest()).OriginCase switch
                    {
                        ApiScheduling.SwapRequest.OriginOneofCase.Customer => (request.SwapRequest?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.SwapRequest ?? new ApiScheduling.SwapRequest()).OriginCase switch
                    {
                        ApiScheduling.SwapRequest.OriginOneofCase.Customer => (request.SwapRequest?.Customer?.Email ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc3 = _schedulingSwapRequestValidationService.ExplainSwapRequestDecisionAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiScheduling.CommentSwapRequestApprovalResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Outcome?.Status ?? ""),
            },
            SwapRequest = new ApiScheduling.SwapRequest
            {
                Priority = (request.SwapRequest?.Priority ?? 0),
                Requester = (reply3.SwapRequest?.Requester ?? new ApiCommon.ContactInfo()),
                SwapRequestId = (request.SwapRequestId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SwapRequest?.SwapRequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "swap_request",
            "audit", "CommentSwapRequestApproval", (request.SwapRequest?.SwapRequestId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates a swap request.
    /// </summary>
    public override async Task<ApiScheduling.DelegateSwapRequestApprovalResponse> DelegateSwapRequestApproval(ApiScheduling.DelegateSwapRequestApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling DelegateSwapRequestApproval for {Tenant}", requestContext.TenantId);
        if (request.SwapRequest is { } record)
        {
            var snapshot = SwapRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiScheduling.RecallRosterRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Roster = new ApiScheduling.Roster
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.SwapRequest?.Assignee?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _schedulingRosterApprovalService.RecallRosterAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.AdvanceSoftwareLicenseWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Roster?.Audit?.CreatedBy ?? ""),
                    UpdatedBy = (request.SwapRequest?.Audit?.UpdatedBy ?? ""),
                },
                Card = new ApiLicensing.SoftwareLicenseCardFunding
                {
                    HolderName = (reply0.Roster ?? new ApiScheduling.Roster()).PaymentCase switch
                    {
                        ApiScheduling.Roster.SettlementOneofCase.Card => (reply0.Roster?.Card?.HolderName ?? ""),
                        _ => ""
                    },
                },
                Holder = new ApiLicensing.LicenseAssignmentRef
                {
                    DisplayName = (request.SwapRequest?.Assignee?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _licensingSoftwareLicenseWorkflowService.AdvanceSoftwareLicenseWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var batch2 = (request.SwapRequest?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.SwapRequestComment>()).Select(async item =>
        {
            var input2 = new ApiScheduling.ExplainSwapRequestCalculationRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                SwapRequest = new ApiScheduling.SwapRequest
                {
                    Comments = { new ApiScheduling.SwapRequestComment
                    {
                        Body = (item.Body ?? ""),
                        CreatedAt = (item.CreatedAt ?? new Timestamp()),
                    } },
                },
            };
            using var rpc2 = _schedulingSwapRequestCalculationService.ExplainSwapRequestCalculationAsync(input2, cancellationToken: token);
            var reply2 = await rpc2.ResponseAsync;
            return reply2;
        }).ToArray();
        var replies2 = await Task.WhenAll(batch2);
        var reply2 = replies2.LastOrDefault() ?? new ApiScheduling.ExplainSwapRequestCalculationResponse();
        var response = new ApiScheduling.DelegateSwapRequestApprovalResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Outcome?.Status ?? ""),
            },
            SwapRequest = new ApiScheduling.SwapRequest
            {
                Assignee = (request.SwapRequest?.Assignee ?? new ApiIdentity.UserRef()),
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Roster?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (reply0.Roster?.Audit?.Etag ?? ""),
                },
                SwapRequestId = (request.SwapRequestId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SwapRequest?.SwapRequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "swap_request",
            "audit", "DelegateSwapRequestApproval", (request.SwapRequest?.SwapRequestId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates a swap request.
    /// </summary>
    public override async Task<ApiScheduling.EscalateSwapRequestResponse> EscalateSwapRequest(ApiScheduling.EscalateSwapRequestRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EscalateSwapRequest for {Tenant}", requestContext.TenantId);
        if (request.SwapRequest is { } record)
        {
            var snapshot = SwapRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPerformance.PauseReviewWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.PauseReviewWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.SwapRequest?.Assignee?.Email ?? ""),
                    Name = (request.SwapRequest?.Requester?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.SwapRequest?.Requester?.Phone ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _performanceReviewWorkflowService.PauseReviewWorkflowAsync);
        var reply1 = new ApiEmployees.NormalizeEmploymentContractResponse();
        foreach (var item in (request.SwapRequest?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.SwapRequestComment>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input1 = new ApiEmployees.NormalizeEmploymentContractRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                EmploymentContract = new ApiEmployees.EmploymentContract
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (item.CreatedAt ?? new Timestamp()),
                    },
                    BillingAddress = new ApiCommon.PostalAddress
                    {
                        Lines = { (reply0.Review?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    },
                    Counterparty = new ApiEmployees.EmployeeRef
                    {
                        Email = (item.AuthorEmail ?? ""),
                    },
                },
            };
            using var rpc1 = _employeesEmploymentContractValidationService.NormalizeEmploymentContractAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var response = new ApiScheduling.EscalateSwapRequestResponse
        {
            SwapRequest = new ApiScheduling.SwapRequest
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Review?.Audit?.CreatedBy ?? ""),
                },
                Priority = (request.SwapRequest?.Priority ?? 0),
                SwapRequestId = (request.SwapRequestId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SwapRequest?.SwapRequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "swap_request",
            "audit", "EscalateSwapRequest", (request.SwapRequest?.SwapRequestId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls a swap request.
    /// </summary>
    public override async Task<ApiScheduling.RecallSwapRequestResponse> RecallSwapRequest(ApiScheduling.RecallSwapRequestRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecallSwapRequest for {Tenant}", requestContext.TenantId);
        if (request.SwapRequest is { } record)
        {
            var snapshot = SwapRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.ScheduleItineraryReportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.ScheduleItineraryReportRequest.Types.Details
            {
                Filters = { (request.SwapRequest?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Reason = (request.Reason ?? ""),
        };
        var named0 = _registry.Get<ApiTravel.ItineraryReportingService.ItineraryReportingServiceClient>("acme.travel.v1.ItineraryReportingService");
        using var rpc0 = named0.ScheduleItineraryReportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiScheduling.SimulateSwapRequestRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SwapRequest = new ApiScheduling.SwapRequest
            {
                Customer = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                    Email = (request.SwapRequest ?? new ApiScheduling.SwapRequest()).OriginCase switch
                    {
                        ApiScheduling.SwapRequest.OriginOneofCase.Customer => (request.SwapRequest?.Customer?.Email ?? ""),
                        _ => ""
                    },
                },
                Sla = new ApiScheduling.SwapRequestSla
                {
                    PolicyId = (request.SwapRequest?.Sla?.PolicyId ?? ""),
                },
            },
        };
        using var rpc1 = _schedulingSwapRequestCalculationService.SimulateSwapRequestAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiScheduling.RecallSwapRequestResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            SwapRequest = new ApiScheduling.SwapRequest
            {
                Priority = (reply1.SwapRequest?.Priority ?? 0),
                Status = (request.SwapRequest?.Status ?? (ApiScheduling.SwapRequestStatus)0),
                SwapRequestId = (request.SwapRequestId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SwapRequest?.SwapRequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "swap_request",
            "audit", "RecallSwapRequest", (request.SwapRequest?.SwapRequestId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects a swap request.
    /// </summary>
    public override async Task<ApiScheduling.RejectSwapRequestResponse> RejectSwapRequest(ApiScheduling.RejectSwapRequestRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RejectSwapRequest for {Tenant}", requestContext.TenantId);
        if (request.SwapRequest is { } record)
        {
            var snapshot = SwapRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.LintPositionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.LintPositionsRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.SwapRequest?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (request.SwapRequest?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var command0 = new DispatchCommand<ApiEmployees.LintPositionsRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _employeesPositionValidationService.LintPositionsAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiScheduling.RejectSwapRequestRequest, ApiEmployees.RecalculateEmploymentContractsRequest>(value => new ApiEmployees.RecalculateEmploymentContractsRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (value.SwapRequest?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _employeesEmploymentContractCalculationService.RecalculateEmploymentContractsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiScheduling.RejectSwapRequestResponse
        {
            Outcome = new ApiScheduling.RejectSwapRequestResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            SwapRequest = new ApiScheduling.SwapRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply1.EmploymentContract?.Counterparty?.Email ?? ""),
                    UserId = (reply0.Position?.Owner?.UserId ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.EmploymentContract?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Subject = (request.SwapRequest?.Subject ?? ""),
                SwapRequestId = (request.SwapRequestId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SwapRequest?.SwapRequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "swap_request",
            "audit", "RejectSwapRequest", (request.SwapRequest?.SwapRequestId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits a swap request.
    /// </summary>
    public override async Task<ApiScheduling.SubmitSwapRequestResponse> SubmitSwapRequest(ApiScheduling.SubmitSwapRequestRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitSwapRequest for {Tenant}", requestContext.TenantId);
        if (request.SwapRequest is { } record)
        {
            var snapshot = SwapRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiScheduling.ApproveAvailabilityWindowResponse();
        foreach (var item in (request.SwapRequest?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.SwapRequestComment>()))
        {
            var input0 = new ApiScheduling.ApproveAvailabilityWindowRequest
            {
                AvailabilityWindow = new ApiScheduling.AvailabilityWindow
                {
                    Participants = { new ApiScheduling.AvailabilityWindowParticipant
                    {
                        Contact = new ApiCommon.ContactInfo
                        {
                            Email = (item.AuthorEmail ?? ""),
                        },
                    } },
                },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
            };
            using var rpc0 = _schedulingAvailabilityWindowApprovalService.ApproveAvailabilityWindowAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var reply1 = new ApiLedger.CreateLedgerAccountResponse();
        foreach (var item in (request.SwapRequest?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.SwapRequestComment>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input1 = new ApiLedger.CreateLedgerAccountRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                LedgerAccount = new ApiLedger.LedgerAccount
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (item.CreatedAt ?? new Timestamp()),
                        UpdatedAt = (request.SwapRequest?.Audit?.UpdatedAt ?? new Timestamp()),
                    },
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (item.AuthorEmail ?? ""),
                    },
                },
            };
            using var rpc1 = _ledgerLedgerAccountService.CreateLedgerAccountAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var response = new ApiScheduling.SubmitSwapRequestResponse
        {
            Outcome = new ApiScheduling.SubmitSwapRequestResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            SwapRequest = new ApiScheduling.SwapRequest
            {
                SwapRequestId = (request.SwapRequestId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SwapRequest?.SwapRequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "swap_request",
            "audit", "SubmitSwapRequest", (request.SwapRequest?.SwapRequestId ?? ""), token);
        return response;
    }
    private async Task<ApiScheduling.SignalShiftWorkflowResponse> SendApproveSwapRequest0Async(ApiScheduling.SignalShiftWorkflowRequest request, CancellationToken token)
    {
        return await _swapRequestApprovalEndpointSendApproveSwapRequest0Port.SendAsync(request, token);
    }
}
