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
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiSurveys = Acme.Surveys.V1;

namespace Acme.Workforce.Scheduling;

public sealed partial class RosterApprovalEndpoint
{
    /// <summary>
    /// Approves a roster.
    /// </summary>
    public override async Task<ApiScheduling.ApproveRosterResponse> ApproveRoster(ApiScheduling.ApproveRosterRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ApproveRoster for {Tenant}", requestContext.TenantId);
        if (request.Roster is { } record)
        {
            var snapshot = RosterMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.SummarizeOrgUnitsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var descriptor0 = new Method<ApiEmployees.SummarizeOrgUnitsRequest, ApiEmployees.SummarizeOrgUnitsResponse>(
            MethodType.Unary, "acme.employees.v1" + ".OrgUnitQueryService", "SummarizeOrgUnits",
            Wire.Marshaller<ApiEmployees.SummarizeOrgUnitsRequest>(), Wire.Marshaller<ApiEmployees.SummarizeOrgUnitsResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiSurveys.ListSurveysRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiSurveys.ListSurveysRequest.Types.Filter
            {
                Region = (request.Roster?.BillingAddress?.Region ?? ""),
            },
        };
        var command1 = new DispatchCommand<ApiSurveys.ListSurveysRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _surveysSurveyService.ListSurveysAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var profile2 = new MappingProfile<ApiScheduling.ApproveRosterRequest, ApiNotifications.BatchGetNotificationsRequest>(value => new ApiNotifications.BatchGetNotificationsRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
        });
        var input2 = profile2.Map(request);
        using var rpc2 = _notificationsNotificationService.BatchGetNotificationsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiMetrics.PreviewMetricSeriesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiMetrics.PreviewMetricSeriesRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
            MetricSeries = new ApiMetrics.MetricSeries
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Roster?.Audit?.CreatedBy ?? ""),
                },
            },
        };
        input3 = PropertyMap.Copy(input3);
        using var rpc3 = _metricsMetricSeriesValidationService.PreviewMetricSeriesAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiScheduling.ApproveRosterResponse
        {
            Outcome = new ApiScheduling.ApproveRosterResponse.Types.Outcome
            {
                NextSteps = { (reply3.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
            Roster = new ApiScheduling.Roster
            {
                Notes = (request.Roster?.Notes ?? ""),
                RosterId = (request.RosterId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Roster?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "roster",
            "audit", "ApproveRoster", (request.Roster?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments a roster.
    /// </summary>
    public override async Task<ApiScheduling.CommentRosterApprovalResponse> CommentRosterApproval(ApiScheduling.CommentRosterApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CommentRosterApproval for {Tenant}", requestContext.TenantId);
        if (request.Roster is { } record)
        {
            var snapshot = RosterMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.CommentRosterApprovalToBrowseOrgUnits0();
        using var rpc0 = _employeesOrgUnitQueryService.BrowseOrgUnitsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var reply1 = new ApiEmployees.SimulateEmploymentContractResponse();
        foreach (var item in (request.Roster?.Attachments ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.RosterAttachment>()))
        {
            var input1 = new ApiEmployees.SimulateEmploymentContractRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                EmploymentContract = new ApiEmployees.EmploymentContract
                {
                    Attachments = { new ApiEmployees.EmploymentContractAttachment
                    {
                        FileName = (item.FileName ?? ""),
                    } },
                },
            };
            using var rpc1 = _employeesEmploymentContractCalculationService.SimulateEmploymentContractAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var input2 = new ApiLicensing.RetryLicenseAssignmentWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.RetryLicenseAssignmentWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (reply1.EmploymentContract?.Counterparty?.Email ?? ""),
                },
            },
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Card = new ApiLicensing.LicenseAssignmentCardSettlement
                {
                    Network = (request.Roster ?? new ApiScheduling.Roster()).PaymentCase switch
                    {
                        ApiScheduling.Roster.SettlementOneofCase.Card => (request.Roster?.Card?.Network ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc2 = _licensingLicenseAssignmentWorkflowService.RetryLicenseAssignmentWorkflowAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiScheduling.CommentRosterApprovalResponse
        {
            Outcome = new ApiScheduling.CommentRosterApprovalResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            Roster = new ApiScheduling.Roster
            {
                Counterparty = (request.Roster?.Counterparty ?? new ApiEmployees.EmployeeRef()),
                DueAt = (reply2.LicenseAssignment?.DueAt ?? new Timestamp()),
                RosterId = (request.RosterId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Roster?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "roster",
            "audit", "CommentRosterApproval", (request.Roster?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates a roster.
    /// </summary>
    public override async Task<ApiScheduling.DelegateRosterApprovalResponse> DelegateRosterApproval(ApiScheduling.DelegateRosterApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling DelegateRosterApproval for {Tenant}", requestContext.TenantId);
        if (request.Roster is { } record)
        {
            var snapshot = RosterMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPerformance.ApproveCalibrationRequest
        {
            Calibration = new ApiPerformance.Calibration
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
                Customer = new ApiPerformance.ReviewRef
                {
                    DisplayName = (request.Roster?.Counterparty?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.ApproveCalibrationRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiPerformance.ApproveCalibrationRequest>();
        using var rpc0 = _performanceCalibrationApprovalService.ApproveCalibrationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiScheduling.ScoreShiftRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Shift = new ApiScheduling.Shift
            {
                Participants = { new ApiScheduling.ShiftParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Roster?.Counterparty?.Email ?? ""),
                    },
                } },
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiScheduling.ScoreShiftRequest>(json1);
        using var rpc1 = _schedulingShiftValidationService.ScoreShiftAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiScheduling.DelegateRosterApprovalResponse
        {
            Outcome = new ApiScheduling.DelegateRosterApprovalResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            Roster = new ApiScheduling.Roster
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (reply0.Calibration?.Assignee?.Email ?? ""),
                },
                Metadata = { (reply1.Shift?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                RosterId = (request.RosterId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Roster?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "roster",
            "audit", "DelegateRosterApproval", (request.Roster?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates a roster.
    /// </summary>
    public override async Task<ApiScheduling.EscalateRosterResponse> EscalateRoster(ApiScheduling.EscalateRosterRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EscalateRoster for {Tenant}", requestContext.TenantId);
        if (request.Roster is { } record)
        {
            var snapshot = RosterMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiScheduling.RecalculateSwapRequestsRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.SwapRequest = new ApiScheduling.SwapRequest
        {
            Customer = new ApiEmployees.EmployeeRef
            {
                DisplayName = (request.Roster?.Counterparty?.DisplayName ?? ""),
            },
            Requester = new ApiCommon.ContactInfo
            {
                Email = (request.Details?.Approver?.Email ?? ""),
            },
        };
        using var rpc0 = _schedulingSwapRequestCalculationService.RecalculateSwapRequestsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiScheduling.EscalateRosterResponse
        {
            Roster = new ApiScheduling.Roster
            {
                BillingAddress = (request.Roster?.BillingAddress ?? new ApiCommon.PostalAddress()),
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (reply0.SwapRequest?.Requester?.Email ?? ""),
                },
                RosterId = (request.RosterId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Roster?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "roster",
            "audit", "EscalateRoster", (request.Roster?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a roster.
    /// </summary>
    public override async Task<ApiScheduling.GetRosterApprovalStatusResponse> GetRosterApprovalStatus(ApiScheduling.GetRosterApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.RosterId, "roster_id");
        _logger.LogInformation("Handling GetRosterApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "roster", request.RosterId,
            "GetRosterApprovalStatus", "accepted", token);
        var input0 = new ApiHelpdesk.StreamEscalationMetricsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Since = (request.Context?.Deadline ?? new Timestamp()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (request.Context?.Actor?.Email ?? ""),
            },
        };
        using var stream0 = _helpdeskEscalationReportingService.StreamEscalationMetrics(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamEscalationMetrics frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiIdentity.SuggestUsersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiIdentity.SuggestUsersRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                OwnerId = (request.RosterId ?? ""),
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _identityUserQueryService.SuggestUsersAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiScheduling.GetRosterApprovalStatusResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Message = (reply1.Result?.Message ?? ""),
            },
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
    public override async Task<ApiScheduling.ListPendingRostersResponse> ListPendingRosters(ApiScheduling.ListPendingRostersRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListPendingRosters for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "roster", "batch",
            "ListPendingRosters", "accepted", token);
        var response = new ApiScheduling.ListPendingRostersResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "roster",
            "audit", "ListPendingRosters", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls a roster.
    /// </summary>
    public override async Task<ApiScheduling.RecallRosterResponse> RecallRoster(ApiScheduling.RecallRosterRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecallRoster for {Tenant}", requestContext.TenantId);
        if (request.Roster is { } record)
        {
            var snapshot = RosterMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiScheduling.ExplainRosterCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Roster = new ApiScheduling.Roster
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Roster?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Card = new ApiScheduling.RosterCardSettlement
                {
                    Last4 = (request.Roster ?? new ApiScheduling.Roster()).PaymentCase switch
                    {
                        ApiScheduling.Roster.SettlementOneofCase.Card => (request.Roster?.Card?.Last4 ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _schedulingRosterCalculationService.ExplainRosterCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiScheduling.ExplainSwapRequestCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SwapRequest = new ApiScheduling.SwapRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Roster?.Counterparty?.DisplayName ?? ""),
                },
                Customer = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Roster?.Counterparty?.Email ?? ""),
                    EmployeeId = (reply0.Roster?.Counterparty?.EmployeeId ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Roster?.Counterparty?.Email ?? ""),
                },
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _schedulingSwapRequestCalculationService.ExplainSwapRequestCalculationAsync);
        var response = new ApiScheduling.RecallRosterResponse
        {
            Outcome = new ApiScheduling.RecallRosterResponse.Types.Outcome
            {
                EffectiveAt = (reply1.Outcome?.EffectiveAt ?? new Timestamp()),
            },
            Roster = new ApiScheduling.Roster
            {
                IssuedAt = (reply0.Roster?.IssuedAt ?? new Timestamp()),
                Metadata = { (request.Roster?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                RosterId = (request.RosterId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Roster?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "roster",
            "audit", "RecallRoster", (request.Roster?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects a roster.
    /// </summary>
    public override async Task<ApiScheduling.RejectRosterResponse> RejectRoster(ApiScheduling.RejectRosterRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RejectRoster for {Tenant}", requestContext.TenantId);
        if (request.Roster is { } record)
        {
            var snapshot = RosterMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiEmployees.BrowseEmployeesResponse();
        foreach (var item in (request.Roster?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.RosterLine>()))
        {
            var input0 = new ApiEmployees.BrowseEmployeesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiEmployees.BrowseEmployeesRequest.Types.Filter
                {
                    Labels = { (item.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                },
            };
            using var rpc0 = _employeesEmployeeQueryService.BrowseEmployeesAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiScheduling.CalculateRosterRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiScheduling.CalculateRosterRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Recipient = (request.Roster?.BillingAddress?.Recipient ?? ""),
                    Region = (request.Roster?.BillingAddress?.Region ?? ""),
                },
            },
            Roster = new ApiScheduling.Roster
            {
                Card = new ApiScheduling.RosterCardSettlement
                {
                    HolderName = (request.Roster ?? new ApiScheduling.Roster()).PaymentCase switch
                    {
                        ApiScheduling.Roster.SettlementOneofCase.Card => (request.Roster?.Card?.HolderName ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _schedulingRosterCalculationService.CalculateRosterAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiBenefits.EscalateDependentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Dependent = new ApiBenefits.Dependent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Roster?.Audit?.CreatedAt ?? new Timestamp()),
                },
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (reply1.Roster?.BillingAddress?.Recipient ?? ""),
                },
            },
        };
        var descriptor2 = new Method<ApiBenefits.EscalateDependentRequest, ApiBenefits.EscalateDependentResponse>(
            MethodType.Unary, "acme.benefits.v1" + ".DependentApprovalService", "EscalateDependent",
            Wire.Marshaller<ApiBenefits.EscalateDependentRequest>(), Wire.Marshaller<ApiBenefits.EscalateDependentResponse>());
        using var rpc2 = _invoker.AsyncUnaryCall(descriptor2, null,
            new CallOptions(cancellationToken: token), input2);
        var reply2 = await rpc2.ResponseAsync;
        var reply3 = new ApiEmployees.QuoteEmploymentContractResponse();
        foreach (var item in (request.Roster?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.RosterLine>()))
        {
            var input3 = new ApiEmployees.QuoteEmploymentContractRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                EmploymentContract = new ApiEmployees.EmploymentContract
                {
                    Lines = { new ApiEmployees.EmploymentContractLine
                    {
                        Amount = (item.Tax?.Amount ?? new ApiCommon.Money()),
                        Tax = new ApiEmployees.EmploymentContractLineTax
                        {
                            Amount = (item.Amount ?? new ApiCommon.Money()),
                            Exemptions = { (item.Tax?.Exemptions ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                        },
                    } },
                },
            };
            using var rpc3 = _employeesEmploymentContractCalculationService.QuoteEmploymentContractAsync(input3, cancellationToken: token);
            reply3 = await rpc3.ResponseAsync;
        }
        var response = new ApiScheduling.RejectRosterResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply2.Result?.Status ?? ""),
            },
            Roster = new ApiScheduling.Roster
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Roster?.Audit?.CreatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply1.Roster?.BillingAddress?.PostalCode ?? ""),
                },
                Number = (request.Roster?.Number ?? ""),
                RosterId = (request.RosterId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Roster?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "roster",
            "audit", "RejectRoster", (request.Roster?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits a roster.
    /// </summary>
    public override async Task<ApiScheduling.SubmitRosterResponse> SubmitRoster(ApiScheduling.SubmitRosterRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitRoster for {Tenant}", requestContext.TenantId);
        if (request.Roster is { } record)
        {
            var snapshot = RosterMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiScheduling.SubmitRosterRequest, ApiScheduling.SuggestRostersRequest>(value => new ApiScheduling.SuggestRostersRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiScheduling.SuggestRostersRequest.Types.Filter
            {
                CreatedAfter = (value.Roster?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (value.Roster?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (value.Details?.Approver?.UserId ?? ""),
                Region = (value.Roster?.BillingAddress?.Region ?? ""),
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _schedulingRosterQueryService.SuggestRostersAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var batch1 = (request.Roster?.Attachments ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.RosterAttachment>()).Select(async item =>
        {
            var input1 = new ApiDocuments.ResolveDocumentFolderConflictRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiDocuments.ResolveDocumentFolderConflictRequest.Types.Details
                {
                    Checksum = (item.Checksum ?? ""),
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
                                CreatedBy = (request.Roster?.Audit?.CreatedBy ?? ""),
                                Etag = (request.Roster?.Audit?.Etag ?? ""),
                            },
                        } },
                        Manager = new ApiIdentity.UserRef
                        {
                            Email = (request.Roster?.Counterparty?.Email ?? ""),
                        },
                    } },
                },
            };
            using var rpc1 = _documentsDocumentFolderSyncService.ResolveDocumentFolderConflictAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            return reply1;
        }).ToArray();
        var replies1 = await Task.WhenAll(batch1);
        var reply1 = replies1.LastOrDefault() ?? new ApiDocuments.ResolveDocumentFolderConflictResponse();
        var input2 = new ApiScheduling.LintRostersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiScheduling.LintRostersRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply1.DocumentFolder?.Manager?.Email ?? ""),
                },
            },
            Roster = new ApiScheduling.Roster
            {
                Bank = new ApiScheduling.RosterBankSettlement
                {
                    AccountHolder = (request.Roster ?? new ApiScheduling.Roster()).PaymentCase switch
                    {
                        ApiScheduling.Roster.SettlementOneofCase.Bank => (request.Roster?.Bank?.AccountHolder ?? ""),
                        _ => ""
                    },
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    EmployeeId = (request.Roster?.Counterparty?.EmployeeId ?? ""),
                },
            },
        };
        using var rpc2 = _schedulingRosterValidationService.LintRostersAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiScheduling.ScoreAvailabilityWindowRequest
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                Participants = { new ApiScheduling.AvailabilityWindowParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Roster?.Counterparty?.Email ?? ""),
                    },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply3 = await SendSubmitRoster3Async(input3, token);
        var response = new ApiScheduling.SubmitRosterResponse
        {
            Outcome = new ApiScheduling.SubmitRosterResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Roster = new ApiScheduling.Roster
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (reply2.Roster?.BillingAddress?.Region ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply3.AvailabilityWindow?.Organizer?.DisplayName ?? ""),
                },
                IssuedAt = (request.Roster?.IssuedAt ?? new Timestamp()),
                RosterId = (request.RosterId ?? ""),
                Status = (reply2.Roster?.Status ?? (ApiScheduling.RosterStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Roster?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "roster",
            "audit", "SubmitRoster", (request.Roster?.VoucherCode ?? ""), token);
        return response;
    }
    private async Task<ApiScheduling.ScoreAvailabilityWindowResponse> SendSubmitRoster3Async(ApiScheduling.ScoreAvailabilityWindowRequest request, CancellationToken token)
    {
        return await _rosterApprovalEndpointSendSubmitRoster3Port.SendAsync(request, token);
    }
}
