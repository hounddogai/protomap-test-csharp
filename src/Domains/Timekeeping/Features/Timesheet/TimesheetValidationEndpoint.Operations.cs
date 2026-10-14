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
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMaintenance = Acme.Maintenance.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiTelehealth = Acme.Telehealth.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Timekeeping;

public sealed partial class TimesheetValidationEndpoint
{
    /// <summary>
    /// Explains a timesheet.
    /// </summary>
    public override async Task<ApiTimekeeping.ExplainTimesheetDecisionResponse> ExplainTimesheetDecision(ApiTimekeeping.ExplainTimesheetDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainTimesheetDecision for {Tenant}", requestContext.TenantId);
        if (request.Timesheet is { } record)
        {
            var snapshot = TimesheetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTimekeeping.EstimateClockEventRequest
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiTimekeeping.TimesheetRef
                {
                    DisplayName = (request.Timesheet?.Counterparty?.DisplayName ?? ""),
                    Number = (request.Timesheet?.Number ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.EstimateClockEventRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Recipient = (request.Timesheet?.BillingAddress?.Recipient ?? ""),
                },
            },
        };
        using var rpc0 = _timekeepingClockEventCalculationService.EstimateClockEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var batch1 = (request.Timesheet?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiTimekeeping.TimesheetLine>()).Select(async item =>
        {
            var input1 = new ApiHelpdesk.ValidateSlaPolicyRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                SlaPolicy = new ApiHelpdesk.SlaPolicy
                {
                    Description = (item.Description ?? ""),
                },
            };
            using var rpc1 = _helpdeskSlaPolicyValidationService.ValidateSlaPolicyAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            return reply1;
        }).ToArray();
        var replies1 = await Task.WhenAll(batch1);
        var reply1 = replies1.LastOrDefault() ?? new ApiHelpdesk.ValidateSlaPolicyResponse();
        var input2 = new ApiHelpdesk.QuoteKnowledgeArticleRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Timesheet?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var labels2 = input2.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input2.Context.Baggage.Clear();
        input2.Context.Baggage.Add(labels2);
        using var rpc2 = _helpdeskKnowledgeArticleCalculationService.QuoteKnowledgeArticleAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiMaintenance.QuoteWorkOrderRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiMaintenance.QuoteWorkOrderRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (reply2.KnowledgeArticle?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
        };
        var packed3 = Any.Pack(input3);
        input3 = packed3.Unpack<ApiMaintenance.QuoteWorkOrderRequest>();
        using var rpc3 = _maintenanceWorkOrderCalculationService.QuoteWorkOrderAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiTimekeeping.ExplainTimesheetDecisionResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply3.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply3.Result?.Status ?? ""),
            },
            Timesheet = new ApiTimekeeping.Timesheet
            {
                IssuedAt = (request.Timesheet?.IssuedAt ?? new Timestamp()),
                Status = (request.Timesheet?.Status ?? (ApiTimekeeping.TimesheetStatus)0),
                TimesheetId = (reply0.ClockEvent?.Subject?.TimesheetId ?? ""),
                Total = (reply2.KnowledgeArticle?.Total ?? new ApiCommon.Money()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TimesheetId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "timesheet",
            "audit", "ExplainTimesheetDecision", (request.TimesheetId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a timesheet.
    /// </summary>
    public override async Task<ApiTimekeeping.GetTimesheetRulesResponse> GetTimesheetRules(ApiTimekeeping.GetTimesheetRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TimesheetId, "timesheet_id");
        _logger.LogInformation("Handling GetTimesheetRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "timesheet", request.TimesheetId,
            "GetTimesheetRules", "accepted", token);
        var input0 = new ApiTravel.ValidateTravelPolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.ValidateTravelPolicyRequest.Types.Details
            {
                Locale = (request.Context?.Locale ?? ""),
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiTravel.ValidateTravelPolicyRequest>(json0);
        using var rpc0 = _travelTravelPolicyValidationService.ValidateTravelPolicyAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTimekeeping.GetTimesheetRulesResponse
        {
            Timesheet = new ApiTimekeeping.Timesheet
            {
                Lines = { new ApiTimekeeping.TimesheetLine
                {
                    Amount = (reply0.TravelPolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                } },
                TimesheetId = (request.TimesheetId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lints a timesheet.
    /// </summary>
    public override async Task<ApiTimekeeping.LintTimesheetsResponse> LintTimesheets(ApiTimekeeping.LintTimesheetsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling LintTimesheets for {Tenant}", requestContext.TenantId);
        if (request.Timesheet is { } record)
        {
            var snapshot = TimesheetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTimekeeping.SimulateClockEventRequest
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.SimulateClockEventRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (request.Timesheet?.BillingAddress?.Locality ?? ""),
                    PostalCode = (request.Timesheet?.BillingAddress?.PostalCode ?? ""),
                    Region = (request.Timesheet?.BillingAddress?.Region ?? ""),
                },
            },
        };
        using var rpc0 = _timekeepingClockEventCalculationService.SimulateClockEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTimekeeping.RecalculateClockEventsRequest
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiTimekeeping.TimesheetRef
                {
                    TimesheetId = (reply0.ClockEvent?.Subject?.TimesheetId ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.RecalculateClockEventsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Timesheet?.BillingAddress?.CountryCode ?? ""),
                    Locality = (request.Timesheet?.BillingAddress?.Locality ?? ""),
                    Recipient = (request.Timesheet?.BillingAddress?.Recipient ?? ""),
                },
            },
        };
        using var rpc1 = _timekeepingClockEventCalculationService.RecalculateClockEventsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTimekeeping.LintTimesheetsResponse
        {
            Outcome = new ApiTimekeeping.LintTimesheetsResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Timesheet = new ApiTimekeeping.Timesheet
            {
                Counterparty = (request.Timesheet?.Counterparty ?? new ApiEmployees.EmployeeRef()),
                TimesheetId = (request.TimesheetId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Timesheet?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "timesheet",
            "audit", "LintTimesheets", (request.Timesheet?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Normalizes a timesheet.
    /// </summary>
    public override async Task<ApiTimekeeping.NormalizeTimesheetResponse> NormalizeTimesheet(ApiTimekeeping.NormalizeTimesheetRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling NormalizeTimesheet for {Tenant}", requestContext.TenantId);
        if (request.Timesheet is { } record)
        {
            var snapshot = TimesheetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiScheduling.ExplainRosterCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiScheduling.ExplainRosterCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _schedulingRosterCalculationService.ExplainRosterCalculationAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiTimekeeping.CalculateClockEventRequest
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiTimekeeping.TimesheetRef
                {
                    DisplayName = (reply0.Roster?.Counterparty?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _timekeepingClockEventCalculationService.CalculateClockEventAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTimekeeping.NormalizeTimesheetResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.ClockEvent?.CorrelationId ?? ""),
            },
            Timesheet = new ApiTimekeeping.Timesheet
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.Roster?.BillingAddress?.Recipient ?? ""),
                    Region = (reply0.Roster?.BillingAddress?.Region ?? ""),
                },
                IssuedAt = (request.Timesheet?.IssuedAt ?? new Timestamp()),
                TimesheetId = (request.TimesheetId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Timesheet?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "timesheet",
            "audit", "NormalizeTimesheet", (request.Timesheet?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews a timesheet.
    /// </summary>
    public override async Task<ApiTimekeeping.PreviewTimesheetResponse> PreviewTimesheet(ApiTimekeeping.PreviewTimesheetRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PreviewTimesheet for {Tenant}", requestContext.TenantId);
        if (request.Timesheet is { } record)
        {
            var snapshot = TimesheetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTimekeeping.SimulateClockEventRequest
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiTimekeeping.TimesheetRef
                {
                    DisplayName = (request.Timesheet?.Counterparty?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _timekeepingClockEventCalculationService.SimulateClockEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiFeatureflags.PreviewFeatureFlagRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            FeatureFlag = new ApiFeatureflags.FeatureFlag
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.Timesheet?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        using var rpc1 = _sendPreviewTimesheet1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLedger.PreviewLedgerAccountRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLedger.PreviewLedgerAccountRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
            LedgerAccount = new ApiLedger.LedgerAccount
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Timesheet?.Audit?.Etag ?? ""),
                },
                Holder = new ApiLedger.PostingBatchRef
                {
                    DisplayName = (reply1.FeatureFlag?.Owner?.DisplayName ?? ""),
                    Number = (reply0.ClockEvent?.Subject?.Number ?? ""),
                },
            },
        };
        var named2 = _registry.Get<ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient>("acme.ledger.v1.LedgerAccountValidationService");
        using var rpc2 = named2.PreviewLedgerAccountAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiFeatureflags.CountFeatureFlagsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiFeatureflags.CountFeatureFlagsRequest.Types.Filter
            {
                CreatedAfter = (reply1.FeatureFlag?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (reply2.LedgerAccount?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Region = (request.Timesheet?.BillingAddress?.Region ?? ""),
            },
        };
        var descriptor3 = new Method<ApiFeatureflags.CountFeatureFlagsRequest, ApiFeatureflags.CountFeatureFlagsResponse>(
            MethodType.Unary, "acme.featureflags.v1" + ".FeatureFlagQueryService", "CountFeatureFlags",
            Wire.Marshaller<ApiFeatureflags.CountFeatureFlagsRequest>(), Wire.Marshaller<ApiFeatureflags.CountFeatureFlagsResponse>());
        using var rpc3 = _invoker.AsyncUnaryCall(descriptor3, null,
            new CallOptions(cancellationToken: token), input3);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiTimekeeping.PreviewTimesheetResponse
        {
            Outcome = new ApiTimekeeping.PreviewTimesheetResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply3.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.ClockEvent?.CorrelationId ?? ""),
                ReferenceIds = { (reply3.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Timesheet = new ApiTimekeeping.Timesheet
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply2.LedgerAccount?.Audit?.UpdatedBy ?? ""),
                },
                Lines = { new ApiTimekeeping.TimesheetLine
                {
                    Amount = (reply1.FeatureFlag?.Discount?.Amount ?? new ApiCommon.Money()),
                } },
                Status = (request.Timesheet?.Status ?? (ApiTimekeeping.TimesheetStatus)0),
                TimesheetId = (request.TimesheetId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Timesheet?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "timesheet",
            "audit", "PreviewTimesheet", (request.Timesheet?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores a timesheet.
    /// </summary>
    public override async Task<ApiTimekeeping.ScoreTimesheetResponse> ScoreTimesheet(ApiTimekeeping.ScoreTimesheetRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScoreTimesheet for {Tenant}", requestContext.TenantId);
        if (request.Timesheet is { } record)
        {
            var snapshot = TimesheetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTelehealth.CountVideoVisitsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiTelehealth.CountVideoVisitsRequest.Types.Filter
            {
                CreatedAfter = (request.Timesheet?.Audit?.UpdatedAt ?? new Timestamp()),
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        var command0 = new DispatchCommand<ApiTelehealth.CountVideoVisitsRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _telehealthVideoVisitQueryService.CountVideoVisitsAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTimekeeping.CalculateClockEventRequest
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.CalculateClockEventRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                    Recipient = (request.Details?.Destination?.Recipient ?? ""),
                },
            },
        };
        using var rpc1 = _timekeepingClockEventCalculationService.CalculateClockEventAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTimekeeping.ScoreTimesheetResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Timesheet = new ApiTimekeeping.Timesheet
            {
                Notes = (request.Timesheet?.Notes ?? ""),
                TimesheetId = (request.TimesheetId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Timesheet?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "timesheet",
            "audit", "ScoreTimesheet", (request.Timesheet?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates a timesheet.
    /// </summary>
    public override async Task<ApiTimekeeping.ValidateTimesheetResponse> ValidateTimesheet(ApiTimekeeping.ValidateTimesheetRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ValidateTimesheet for {Tenant}", requestContext.TenantId);
        if (request.Timesheet is { } record)
        {
            var snapshot = TimesheetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTimekeeping.QuoteClockEventRequest
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiTimekeeping.TimesheetRef
                {
                    DisplayName = (request.Timesheet?.Counterparty?.DisplayName ?? ""),
                    Number = (request.Timesheet?.Number ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.QuoteClockEventRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Timesheet?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    PostalCode = (request.Timesheet?.BillingAddress?.PostalCode ?? ""),
                },
            },
        };
        using var rpc0 = _timekeepingClockEventCalculationService.QuoteClockEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var batch1 = (request.Timesheet?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiTimekeeping.TimesheetLine>()).Select(async item =>
        {
            var input1 = new ApiTimekeeping.CalculateClockEventRequest
            {
                ClockEvent = new ApiTimekeeping.ClockEvent
                {
                    Attributes = { (item.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiTimekeeping.CalculateClockEventRequest.Types.Details
                {
                    Budget = (item.Tax?.Amount ?? new ApiCommon.Money()),
                    Destination = new ApiCommon.PostalAddress
                    {
                        Region = (request.Timesheet?.BillingAddress?.Region ?? ""),
                    },
                },
            };
            using var rpc1 = _timekeepingClockEventCalculationService.CalculateClockEventAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            return reply1;
        }).ToArray();
        var replies1 = await Task.WhenAll(batch1);
        var reply1 = replies1.LastOrDefault() ?? new ApiTimekeeping.CalculateClockEventResponse();
        var input2 = new ApiLicensing.ValidateSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Bank = new ApiLicensing.SoftwareLicenseBankFunding
                {
                    AccountHolder = (request.Timesheet ?? new ApiTimekeeping.Timesheet()).PaymentCase switch
                    {
                        ApiTimekeeping.Timesheet.SettlementOneofCase.Bank => (request.Timesheet?.Bank?.AccountHolder ?? ""),
                        _ => ""
                    },
                },
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc2 = _licensingSoftwareLicenseValidationService.ValidateSoftwareLicenseAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiTimekeeping.ValidateTimesheetResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply2.Outcome?.Status ?? ""),
            },
            Timesheet = new ApiTimekeeping.Timesheet
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply0.ClockEvent?.Subject?.DisplayName ?? ""),
                },
                Number = (request.Timesheet?.Number ?? ""),
                TimesheetId = (request.TimesheetId ?? ""),
                Total = (reply0.Breakdown?.Total ?? new ApiCommon.Money()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Timesheet?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "timesheet",
            "audit", "ValidateTimesheet", (request.Timesheet?.VoucherCode ?? ""), token);
        return response;
    }
}
