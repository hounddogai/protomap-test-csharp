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
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiCustomers = Acme.Customers.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFeaturestore = Acme.Featurestore.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLabeling = Acme.Labeling.V1;
using ApiPayroll = Acme.Payroll.V1;
using ApiRatelimiting = Acme.Ratelimiting.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Scheduling;

public sealed partial class RosterWorkflowEndpoint
{
    /// <summary>
    /// Advances a roster.
    /// </summary>
    public override async Task<ApiScheduling.AdvanceRosterWorkflowResponse> AdvanceRosterWorkflow(ApiScheduling.AdvanceRosterWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling AdvanceRosterWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Roster is { } record)
        {
            var snapshot = RosterMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiBenefits.SignalDependentWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Dependent = new ApiBenefits.Dependent
            {
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Locality = (request.Roster?.BillingAddress?.Locality ?? ""),
                },
            },
            Details = new ApiBenefits.SignalDependentWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        using var rpc0 = _benefitsDependentWorkflowService.SignalDependentWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiScheduling.LintShiftsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Shift = new ApiScheduling.Shift
            {
                Participants = { new ApiScheduling.ShiftParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply0.Dependent?.RelationshipManager?.Email ?? ""),
                        Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                        Phone = (request.Details?.Notify?.Phone ?? ""),
                        PreferredChannel = (reply0.Dependent?.Contact?.PreferredChannel ?? ""),
                    },
                } },
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _schedulingShiftValidationService.LintShiftsAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiScheduling.AdvanceRosterWorkflowResponse
        {
            Outcome = new ApiScheduling.AdvanceRosterWorkflowResponse.Types.Outcome
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
                Status = (reply0.Outcome?.Status ?? ""),
            },
            Roster = new ApiScheduling.Roster
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (reply1.Shift?.Location?.Recipient ?? ""),
                },
                IssuedAt = (request.Roster?.IssuedAt ?? new Timestamp()),
                RosterId = (request.RosterId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Roster?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "roster",
            "audit", "AdvanceRosterWorkflow", (request.Roster?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels a roster.
    /// </summary>
    public override async Task<ApiScheduling.CancelRosterWorkflowResponse> CancelRosterWorkflow(ApiScheduling.CancelRosterWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CancelRosterWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Roster is { } record)
        {
            var snapshot = RosterMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiScheduling.ExportRostersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiScheduling.ExportRostersRequest.Types.Filter
            {
                CreatedAfter = (request.Roster?.IssuedAt ?? new Timestamp()),
                Labels = { (request.Details?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.RosterId ?? ""),
                Region = (request.Roster?.BillingAddress?.Region ?? ""),
            },
        };
        using var stream0 = _schedulingRosterQueryService.ExportRosters(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportRosters frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiEmployees.ExplainEmploymentContractDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Roster?.BillingAddress?.CountryCode ?? ""),
                    Lines = { (request.Roster?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Recipient = (request.Roster?.BillingAddress?.Recipient ?? ""),
                },
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _employeesEmploymentContractValidationService.ExplainEmploymentContractDecisionAsync);
        var response = new ApiScheduling.CancelRosterWorkflowResponse
        {
            Roster = new ApiScheduling.Roster
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.EmploymentContract?.Audit?.UpdatedBy ?? ""),
                },
                Metadata = { (request.Roster?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                RosterId = (request.RosterId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Roster?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "roster",
            "audit", "CancelRosterWorkflow", (request.Roster?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a roster.
    /// </summary>
    public override async Task<ApiScheduling.InspectRosterWorkflowResponse> InspectRosterWorkflow(ApiScheduling.InspectRosterWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.RosterId, "roster_id");
        _logger.LogInformation("Handling InspectRosterWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "roster", request.RosterId,
            "InspectRosterWorkflow", "accepted", token);
        var response = new ApiScheduling.InspectRosterWorkflowResponse
        {
            Roster = new ApiScheduling.Roster
            {
                RosterId = (request.RosterId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists rosters a page at a time.
    /// </summary>
    public override async Task<ApiScheduling.ListRosterWorkflowsResponse> ListRosterWorkflows(ApiScheduling.ListRosterWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListRosterWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "roster", "batch",
            "ListRosterWorkflows", "accepted", token);
        var input0 = new ApiScheduling.ListShiftWorkflowsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiScheduling.ListShiftWorkflowsRequest.Types.Filter
            {
                OwnerId = (request.Filter?.OwnerId ?? ""),
                Region = (request.Filter?.Region ?? ""),
            },
            Page = new ApiCommon.PageRequest
            {
                PageToken = (request.Page?.PageToken ?? ""),
            },
        };
        using var rpc0 = _sendListRosterWorkflows0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiScheduling.ListRosterWorkflowsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.OrderBy ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "roster",
            "audit", "ListRosterWorkflows", (request.OrderBy ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pauses a roster.
    /// </summary>
    public override async Task<ApiScheduling.PauseRosterWorkflowResponse> PauseRosterWorkflow(ApiScheduling.PauseRosterWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PauseRosterWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Roster is { } record)
        {
            var snapshot = RosterMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiScheduling.SignalShiftWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiScheduling.SignalShiftWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
            },
            Shift = new ApiScheduling.Shift
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Roster?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (request.Roster?.Counterparty?.Email ?? ""),
                },
                Participants = { new ApiScheduling.ShiftParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Details?.Notify?.Email ?? ""),
                    },
                } },
            },
        };
        var named0 = _registry.Get<ApiScheduling.ShiftWorkflowService.ShiftWorkflowServiceClient>("acme.scheduling.v1.ShiftWorkflowService");
        using var rpc0 = named0.SignalShiftWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiScheduling.ListPendingShiftsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiScheduling.ListPendingShiftsRequest.Types.Filter
            {
                CreatedAfter = (reply0.Shift?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (reply0.Shift?.ShiftId ?? ""),
                Region = (reply0.Shift?.Location?.Region ?? ""),
            },
        };
        var descriptor1 = new Method<ApiScheduling.ListPendingShiftsRequest, ApiScheduling.ListPendingShiftsResponse>(
            MethodType.Unary, "acme.scheduling.v1" + ".ShiftApprovalService", "ListPendingShifts",
            Wire.Marshaller<ApiScheduling.ListPendingShiftsRequest>(), Wire.Marshaller<ApiScheduling.ListPendingShiftsResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var reply2 = new ApiCustomers.RecalculateCustomersResponse();
        foreach (var item in (request.Roster?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.RosterLine>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input2 = new ApiCustomers.RecalculateCustomersRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Customer = new ApiCustomers.Customer
                {
                    Attributes = { (item.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedBy = (reply0.Shift?.Audit?.CreatedBy ?? ""),
                    },
                    Contact = new ApiCommon.ContactInfo
                    {
                        Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                    },
                    PrimaryAddress = new ApiCommon.PostalAddress
                    {
                        Region = (reply0.Shift?.Location?.Region ?? ""),
                    },
                },
            };
            using var rpc2 = _customersCustomerCalculationService.RecalculateCustomersAsync(input2, cancellationToken: token);
            reply2 = await rpc2.ResponseAsync;
        }
        var profile3 = new MappingProfile<ApiScheduling.PauseRosterWorkflowRequest, ApiScheduling.ScheduleAvailabilityWindowExpiryRequest>(value => new ApiScheduling.ScheduleAvailabilityWindowExpiryRequest
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                Participants = { new ApiScheduling.AvailabilityWindowParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply2.Customer?.Contact?.Email ?? ""),
                        Name = (value.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                    },
                } },
            },
            Context = (value.Context ?? new ApiCommon.RequestContext()),
        });
        var input3 = profile3.Map(request);
        using var rpc3 = _schedulingAvailabilityWindowLifecycleService.ScheduleAvailabilityWindowExpiryAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiScheduling.PauseRosterWorkflowResponse
        {
            Outcome = new ApiScheduling.PauseRosterWorkflowResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
                NextSteps = { (reply3.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Result?.Status ?? ""),
            },
            Roster = new ApiScheduling.Roster
            {
                DueAt = (request.Roster?.DueAt ?? new Timestamp()),
                RosterId = (request.RosterId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Roster?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "roster",
            "audit", "PauseRosterWorkflow", (request.Roster?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys a roster.
    /// </summary>
    public override async Task<ApiScheduling.RetryRosterWorkflowResponse> RetryRosterWorkflow(ApiScheduling.RetryRosterWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RetryRosterWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Roster is { } record)
        {
            var snapshot = RosterMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLabeling.CalculateLabelingTaskRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLabeling.CalculateLabelingTaskRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Roster?.BillingAddress?.PostalCode ?? ""),
                },
            },
            LabelingTask = new ApiLabeling.LabelingTask
            {
                Requester = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _labelingLabelingTaskCalculationService.CalculateLabelingTaskAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.RetryRosterWorkflowToCheckPositionEligibility1();
        using var rpc1 = _employeesPositionValidationService.CheckPositionEligibilityAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiScheduling.LintAvailabilityWindowsRequest
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Roster?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Organizer = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.Position?.Owner?.DisplayName ?? ""),
                },
                Participants = { new ApiScheduling.AvailabilityWindowParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply1.Position?.Owner?.Email ?? ""),
                    },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply2 = await SendRetryRosterWorkflow2Async(input2, token);
        var input3 = new ApiFeaturestore.QuoteFeatureViewRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiFeaturestore.QuoteFeatureViewRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (reply2.AvailabilityWindow?.Location?.Region ?? ""),
                },
            },
            FeatureView = new ApiFeaturestore.FeatureView
            {
                Address = new ApiCommon.PostalAddress
                {
                    Lines = { (reply2.AvailabilityWindow?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Recipient = (request.Roster?.BillingAddress?.Recipient ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (reply2.AvailabilityWindow?.Organizer?.UserId ?? ""),
                },
            },
        };
        var labels3 = input3.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input3.Context.Baggage.Clear();
        input3.Context.Baggage.Add(labels3);
        using var rpc3 = _featurestoreFeatureViewCalculationService.QuoteFeatureViewAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiScheduling.RetryRosterWorkflowResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Details = { (reply3.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Roster = new ApiScheduling.Roster
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply2.AvailabilityWindow?.Audit?.Etag ?? ""),
                },
                BillingAddress = (request.Roster?.BillingAddress ?? new ApiCommon.PostalAddress()),
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply0.LabelingTask?.Customer?.DisplayName ?? ""),
                },
                RosterId = (request.RosterId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Roster?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "roster",
            "audit", "RetryRosterWorkflow", (request.Roster?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals a roster.
    /// </summary>
    public override async Task<ApiScheduling.SignalRosterWorkflowResponse> SignalRosterWorkflow(ApiScheduling.SignalRosterWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SignalRosterWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Roster is { } record)
        {
            var snapshot = RosterMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiRatelimiting.SimulateRateLimitResponse();
        foreach (var item in (request.Roster?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.RosterLine>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input0 = new ApiRatelimiting.SimulateRateLimitRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiRatelimiting.SimulateRateLimitRequest.Types.Details
                {
                    Budget = (item.Tax?.Amount ?? new ApiCommon.Money()),
                },
                RateLimit = new ApiRatelimiting.RateLimit
                {
                    Description = (item.Description ?? ""),
                    Discount = new ApiRatelimiting.RateLimitDiscountAction
                    {
                        Amount = (item.Amount ?? new ApiCommon.Money()),
                    },
                },
            };
            using var rpc0 = _ratelimitingRateLimitCalculationService.SimulateRateLimitAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiEmployees.BrowseOrgUnitsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.BrowseOrgUnitsRequest.Types.Filter
            {
                CreatedAfter = (request.Roster?.Audit?.UpdatedAt ?? new Timestamp()),
                OwnerId = (reply0.RateLimit?.Owner?.UserId ?? ""),
                Region = (request.Roster?.BillingAddress?.Region ?? ""),
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiEmployees.BrowseOrgUnitsRequest>(json1);
        using var rpc1 = _employeesOrgUnitQueryService.BrowseOrgUnitsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiScheduling.SignalRosterWorkflowResponse
        {
            Outcome = new ApiScheduling.SignalRosterWorkflowResponse.Types.Outcome
            {
                Status = (reply1.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            Roster = new ApiScheduling.Roster
            {
                DueAt = (request.Roster?.DueAt ?? new Timestamp()),
                RosterId = (request.RosterId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Roster?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "roster",
            "audit", "SignalRosterWorkflow", (request.Roster?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts a roster.
    /// </summary>
    public override async Task<ApiScheduling.StartRosterWorkflowResponse> StartRosterWorkflow(ApiScheduling.StartRosterWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StartRosterWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Roster is { } record)
        {
            var snapshot = RosterMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.ResumeChangeRequestWorkflowRequest();
        input0.ChangeRequest = new ApiItsm.ChangeRequest
        {
            Audit = new ApiCommon.AuditInfo
            {
                CreatedBy = (request.Roster?.Audit?.CreatedBy ?? ""),
            },
        };
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        using var rpc0 = _itsmChangeRequestWorkflowService.ResumeChangeRequestWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiPayroll.BatchGetPayRunsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _payrollPayRunService.BatchGetPayRunsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiScheduling.StartRosterWorkflowResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
            Roster = new ApiScheduling.Roster
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (reply0.ChangeRequest?.Assignee?.Email ?? ""),
                },
                RosterId = (request.RosterId ?? ""),
                Total = (request.Roster?.Total ?? new ApiCommon.Money()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Roster?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "roster",
            "audit", "StartRosterWorkflow", (request.Roster?.VoucherCode ?? ""), token);
        return response;
    }
    private async Task<ApiScheduling.LintAvailabilityWindowsResponse> SendRetryRosterWorkflow2Async(ApiScheduling.LintAvailabilityWindowsRequest request, CancellationToken token)
    {
        return await _rosterWorkflowEndpointSendRetryRosterWorkflow2Port.SendAsync(request, token);
    }
}
