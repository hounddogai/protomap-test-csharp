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
using ApiScheduling = Acme.Scheduling.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Scheduling;

public sealed partial class AvailabilityWindowApprovalEndpoint
{
    /// <summary>
    /// Approves an availability window.
    /// </summary>
    public override async Task<ApiScheduling.ApproveAvailabilityWindowResponse> ApproveAvailabilityWindow(ApiScheduling.ApproveAvailabilityWindowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ApproveAvailabilityWindow for {Tenant}", requestContext.TenantId);
        if (request.AvailabilityWindow is { } record)
        {
            var snapshot = AvailabilityWindowMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.EscalateVenueRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Venue = new ApiCorporateevents.Venue
            {
                Address = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.AvailabilityWindow?.Location?.CountryCode ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _corporateeventsVenueApprovalService.EscalateVenueAsync);
        var input1 = new ApiDocuments.PauseDocumentTemplateWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Venue?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (request.AvailabilityWindow?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                    {
                        ApiCorporateevents.Venue.LocatorOneofCase.Address => (reply0.Venue?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()),
                        _ => new Google.Protobuf.Collections.RepeatedField<string>()
                    } },
                },
            },
        };
        using var rpc1 = _documentsDocumentTemplateWorkflowService.PauseDocumentTemplateWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiScheduling.RecallShiftRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiScheduling.RecallShiftRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    UserId = (request.AvailabilityWindow?.Organizer?.UserId ?? ""),
                },
            },
            Shift = new ApiScheduling.Shift
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.DocumentTemplate?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Participants = { new ApiScheduling.ShiftParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply0.Venue?.Owner?.Email ?? ""),
                    },
                } },
            },
        };
        var named2 = _registry.Get<ApiScheduling.ShiftApprovalService.ShiftApprovalServiceClient>("acme.scheduling.v1.ShiftApprovalService");
        using var rpc2 = named2.RecallShiftAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var batch3 = (request.AvailabilityWindow?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.AvailabilityWindowParticipant>()).Select(async item =>
        {
            var input3 = new ApiWorkplaces.ListBuildingWorkflowsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiWorkplaces.ListBuildingWorkflowsRequest.Types.Filter
                {
                    CreatedAfter = (reply2.Shift?.Audit?.UpdatedAt ?? new Timestamp()),
                    OwnerId = (item.ParticipantId ?? ""),
                    Region = (request.AvailabilityWindow?.Location?.Region ?? ""),
                },
            };
            using var rpc3 = _workplacesBuildingWorkflowService.ListBuildingWorkflowsAsync(input3, cancellationToken: token);
            var reply3 = await rpc3.ResponseAsync;
            return reply3;
        }).ToArray();
        var replies3 = await Task.WhenAll(batch3);
        var reply3 = replies3.LastOrDefault() ?? new ApiWorkplaces.ListBuildingWorkflowsResponse();
        var response = new ApiScheduling.ApproveAvailabilityWindowResponse
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.DocumentTemplate?.Audit?.CreatedAt ?? new Timestamp()),
                },
                AvailabilityWindowId = (request.AvailabilityWindowId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.Venue?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Recipient = (reply1.DocumentTemplate?.BillingAddress?.Recipient ?? ""),
                },
                Metadata = { (reply2.Shift?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Organizer = new ApiIdentity.UserRef
                {
                    UserId = (reply0.Venue?.Owner?.UserId ?? ""),
                },
                Title = (request.AvailabilityWindow?.Title ?? ""),
            },
            Outcome = new ApiScheduling.ApproveAvailabilityWindowResponse.Types.Outcome
            {
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "availability_window",
            "audit", "ApproveAvailabilityWindow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments an availability window.
    /// </summary>
    public override async Task<ApiScheduling.CommentAvailabilityWindowApprovalResponse> CommentAvailabilityWindowApproval(ApiScheduling.CommentAvailabilityWindowApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CommentAvailabilityWindowApproval for {Tenant}", requestContext.TenantId);
        if (request.AvailabilityWindow is { } record)
        {
            var snapshot = AvailabilityWindowMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiEmployees.QuoteEmploymentContractResponse();
        foreach (var item in (request.AvailabilityWindow?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.AvailabilityWindowParticipant>()))
        {
            var input0 = new ApiEmployees.QuoteEmploymentContractRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                EmploymentContract = new ApiEmployees.EmploymentContract
                {
                    BillingAddress = new ApiCommon.PostalAddress
                    {
                        Locality = (request.AvailabilityWindow?.Location?.Locality ?? ""),
                    },
                    Counterparty = new ApiEmployees.EmployeeRef
                    {
                        Email = (item.Contact?.Email ?? ""),
                    },
                },
            };
            using var rpc0 = _employeesEmploymentContractCalculationService.QuoteEmploymentContractAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var profile1 = new MappingProfile<ApiScheduling.CommentAvailabilityWindowApprovalRequest, ApiScheduling.GetAvailabilityWindowRulesRequest>(value => new ApiScheduling.GetAvailabilityWindowRulesRequest
        {
            AvailabilityWindowId = (value.AvailabilityWindow?.AvailabilityWindowId ?? ""),
            Context = (value.Context ?? new ApiCommon.RequestContext()),
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _schedulingAvailabilityWindowValidationService.GetAvailabilityWindowRulesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiScheduling.CommentAvailabilityWindowApprovalResponse
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.AvailabilityWindow?.Audit?.CreatedAt ?? new Timestamp()),
                },
                AvailabilityWindowId = (request.AvailabilityWindowId ?? ""),
                Status = (request.AvailabilityWindow?.Status ?? (ApiScheduling.AvailabilityWindowStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "availability_window",
            "audit", "CommentAvailabilityWindowApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates an availability window.
    /// </summary>
    public override async Task<ApiScheduling.DelegateAvailabilityWindowApprovalResponse> DelegateAvailabilityWindowApproval(ApiScheduling.DelegateAvailabilityWindowApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling DelegateAvailabilityWindowApproval for {Tenant}", requestContext.TenantId);
        if (request.AvailabilityWindow is { } record)
        {
            var snapshot = AvailabilityWindowMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiScheduling.ResolveRostersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _schedulingRosterQueryService.ResolveRostersAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiScheduling.DelegateAvailabilityWindowApprovalResponse
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                AvailabilityWindowId = (request.AvailabilityWindowId ?? ""),
                Window = (request.AvailabilityWindow?.Window ?? new ApiCommon.TimeWindow()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "availability_window",
            "audit", "DelegateAvailabilityWindowApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists availability windows a page at a time.
    /// </summary>
    public override async Task<ApiScheduling.ListPendingAvailabilityWindowsResponse> ListPendingAvailabilityWindows(ApiScheduling.ListPendingAvailabilityWindowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListPendingAvailabilityWindows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "availability_window", "batch",
            "ListPendingAvailabilityWindows", "accepted", token);
        var response = new ApiScheduling.ListPendingAvailabilityWindowsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "availability_window",
            "audit", "ListPendingAvailabilityWindows", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls an availability window.
    /// </summary>
    public override async Task<ApiScheduling.RecallAvailabilityWindowResponse> RecallAvailabilityWindow(ApiScheduling.RecallAvailabilityWindowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecallAvailabilityWindow for {Tenant}", requestContext.TenantId);
        if (request.AvailabilityWindow is { } record)
        {
            var snapshot = AvailabilityWindowMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.RecallAvailabilityWindowToTerminateAvailabilityWindow0();
        using var rpc0 = _schedulingAvailabilityWindowLifecycleService.TerminateAvailabilityWindowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.NormalizePositionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.AvailabilityWindow?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var reply1 = await SendRecallAvailabilityWindow1Async(input1, token);
        var response = new ApiScheduling.RecallAvailabilityWindowResponse
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.AvailabilityWindow?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (reply1.Position?.Audit?.UpdatedBy ?? ""),
                },
                AvailabilityWindowId = (request.AvailabilityWindowId ?? ""),
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (reply0.AvailabilityWindow?.Organizer?.Email ?? ""),
                },
                Window = (request.AvailabilityWindow?.Window ?? new ApiCommon.TimeWindow()),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "availability_window",
            "audit", "RecallAvailabilityWindow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects an availability window.
    /// </summary>
    public override async Task<ApiScheduling.RejectAvailabilityWindowResponse> RejectAvailabilityWindow(ApiScheduling.RejectAvailabilityWindowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RejectAvailabilityWindow for {Tenant}", requestContext.TenantId);
        if (request.AvailabilityWindow is { } record)
        {
            var snapshot = AvailabilityWindowMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.AvailabilityWindow?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.AvailabilityWindowParticipant>()).Select(async item =>
        {
            var input0 = new ApiScheduling.DelegateRosterApprovalRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiScheduling.DelegateRosterApprovalRequest.Types.Details
                {
                    Approver = new ApiIdentity.UserRef
                    {
                        Email = (item.Contact?.Email ?? ""),
                        UserId = (request.Details?.Approver?.UserId ?? ""),
                    },
                },
                Roster = new ApiScheduling.Roster
                {
                    BillingAddress = new ApiCommon.PostalAddress
                    {
                        Recipient = (request.AvailabilityWindow?.Location?.Recipient ?? ""),
                    },
                    Counterparty = new ApiEmployees.EmployeeRef
                    {
                        DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                    },
                },
            };
            using var rpc0 = _schedulingRosterApprovalService.DelegateRosterApprovalAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiScheduling.DelegateRosterApprovalResponse();
        var input1 = new ApiScheduling.ExplainShiftDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Shift = new ApiScheduling.Shift
            {
                Organizer = new ApiIdentity.UserRef
                {
                    UserId = (request.AvailabilityWindow?.Organizer?.UserId ?? ""),
                },
            },
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiScheduling.ExplainShiftDecisionRequest>();
        using var rpc1 = _schedulingShiftValidationService.ExplainShiftDecisionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiScheduling.UpdateShiftRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Shift = new ApiScheduling.Shift
            {
                Participants = { new ApiScheduling.ShiftParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.AvailabilityWindow?.Organizer?.Email ?? ""),
                    },
                } },
            },
        };
        var json2 = JsonFormatter.Default.Format(input2);
        input2 = JsonParser.Default.Parse<ApiScheduling.UpdateShiftRequest>(json2);
        using var rpc2 = _schedulingShiftService.UpdateShiftAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiScheduling.RejectAvailabilityWindowResponse
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Shift?.Audit?.CreatedAt ?? new Timestamp()),
                },
                AvailabilityWindowId = (request.AvailabilityWindowId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply1.Shift?.Location?.PostalCode ?? ""),
                },
                Organizer = (request.AvailabilityWindow?.Organizer ?? new ApiIdentity.UserRef()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "availability_window",
            "audit", "RejectAvailabilityWindow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits an availability window.
    /// </summary>
    public override async Task<ApiScheduling.SubmitAvailabilityWindowResponse> SubmitAvailabilityWindow(ApiScheduling.SubmitAvailabilityWindowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitAvailabilityWindow for {Tenant}", requestContext.TenantId);
        if (request.AvailabilityWindow is { } record)
        {
            var snapshot = AvailabilityWindowMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.AvailabilityWindow?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.AvailabilityWindowParticipant>()).Select(async item =>
        {
            var input0 = new ApiScheduling.CalculateRosterRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Roster = new ApiScheduling.Roster
                {
                    Counterparty = new ApiEmployees.EmployeeRef
                    {
                        Email = (item.Contact?.Email ?? ""),
                    },
                },
            };
            using var rpc0 = _schedulingRosterCalculationService.CalculateRosterAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiScheduling.CalculateRosterResponse();
        var input1 = new ApiEmployees.SuggestEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.SuggestEmployeesRequest.Types.Filter
            {
                CreatedAfter = (request.AvailabilityWindow?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.AvailabilityWindow?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.Roster?.Counterparty?.EmployeeId ?? ""),
                Region = (request.AvailabilityWindow?.Location?.Region ?? ""),
            },
        };
        using var rpc1 = _employeesEmployeeQueryService.SuggestEmployeesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDocuments.StartDocumentTemplateWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (request.AvailabilityWindow?.Location?.Locality ?? ""),
                },
            },
        };
        var reply2 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _documentsDocumentTemplateWorkflowService.StartDocumentTemplateWorkflowAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        await _bus.PublishAsync(new TransportNotification<ApiDocuments.StartDocumentTemplateWorkflowRequest>("acme.scheduling.v1.AvailabilityWindowApprovalService/SubmitAvailabilityWindow/2", input2.Clone()), token);
        var input3 = new ApiScheduling.ExplainAvailabilityWindowCalculationRequest
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                Location = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.AvailabilityWindow?.Location?.CountryCode ?? ""),
                },
                Participants = { new ApiScheduling.AvailabilityWindowParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply0.Roster?.Counterparty?.Email ?? ""),
                    },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply3 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _schedulingAvailabilityWindowCalculationService.ExplainAvailabilityWindowCalculationAsync(input3, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiScheduling.SubmitAvailabilityWindowResponse
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                AvailabilityWindowId = (request.AvailabilityWindowId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply2.DocumentTemplate?.BillingAddress?.CountryCode ?? ""),
                },
                Window = (request.AvailabilityWindow?.Window ?? new ApiCommon.TimeWindow()),
            },
            Outcome = new ApiScheduling.SubmitAvailabilityWindowResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "availability_window",
            "audit", "SubmitAvailabilityWindow", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiEmployees.NormalizePositionResponse> SendRecallAvailabilityWindow1Async(ApiEmployees.NormalizePositionRequest request, CancellationToken token)
    {
        return await _availabilityWindowApprovalEndpointSendRecallAvailabilityWindow1Port.SendAsync(request, token);
    }
}
