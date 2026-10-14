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
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiModelregistry = Acme.Modelregistry.V1;
using ApiPerformers = Acme.Performers.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiShows = Acme.Shows.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Employees;

/// <summary>
/// Checks positions against the rules before anything acts on them.
/// </summary>
public sealed class PositionValidationEndpoint : ApiEmployees.PositionValidationService.PositionValidationServiceBase
{
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly CallInvoker _invoker;
    private readonly ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient _ledgerLedgerAccountQueryService;
    private readonly ILogger<PositionValidationEndpoint> _logger;
    private readonly ApiModelregistry.RegisteredModelCalculationService.RegisteredModelCalculationServiceClient _modelregistryRegisteredModelCalculationService;
    private readonly ApiPerformers.PerformerService.PerformerServiceClient _performersPerformerService;
    private readonly RetryPolicy _policy;
    private readonly ApiRecruiting.JobOpeningQueryService.JobOpeningQueryServiceClient _recruitingJobOpeningQueryService;
    private readonly ApiRecruiting.OfferCalculationService.OfferCalculationServiceClient _recruitingOfferCalculationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiEmployees.RecalculateEmploymentContractsRequest, CallOptions, AsyncUnaryCall<ApiEmployees.RecalculateEmploymentContractsResponse>> _sendExplainPositionDecision0;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiShows.ShowService.ShowServiceClient _showsShowService;
    private readonly ApiTimekeeping.TimesheetValidationService.TimesheetValidationServiceClient _timekeepingTimesheetValidationService;

    public PositionValidationEndpoint(
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        CallInvoker invoker,
        ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient ledgerLedgerAccountQueryService,
        ILogger<PositionValidationEndpoint> logger,
        ApiModelregistry.RegisteredModelCalculationService.RegisteredModelCalculationServiceClient modelregistryRegisteredModelCalculationService,
        ApiPerformers.PerformerService.PerformerServiceClient performersPerformerService,
        RetryPolicy policy,
        ApiRecruiting.JobOpeningQueryService.JobOpeningQueryServiceClient recruitingJobOpeningQueryService,
        ApiRecruiting.OfferCalculationService.OfferCalculationServiceClient recruitingOfferCalculationService,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiShows.ShowService.ShowServiceClient showsShowService,
        ApiTimekeeping.TimesheetValidationService.TimesheetValidationServiceClient timekeepingTimesheetValidationService)
    {
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _invoker = invoker;
        _ledgerLedgerAccountQueryService = ledgerLedgerAccountQueryService;
        _logger = logger;
        _modelregistryRegisteredModelCalculationService = modelregistryRegisteredModelCalculationService;
        _performersPerformerService = performersPerformerService;
        _policy = policy;
        _recruitingJobOpeningQueryService = recruitingJobOpeningQueryService;
        _recruitingOfferCalculationService = recruitingOfferCalculationService;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _showsShowService = showsShowService;
        _timekeepingTimesheetValidationService = timekeepingTimesheetValidationService;
        _sendExplainPositionDecision0 = _employeesEmploymentContractCalculationService.RecalculateEmploymentContractsAsync;
    }
    /// <summary>
    /// Checks a position.
    /// </summary>
    public override async Task<ApiEmployees.CheckPositionEligibilityResponse> CheckPositionEligibility(ApiEmployees.CheckPositionEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CheckPositionEligibility for {Tenant}", requestContext.TenantId);
        if (request.Position is { } record)
        {
            var snapshot = PositionMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.QuoteEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Position?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Position?.Owner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _employeesEmploymentContractCalculationService.QuoteEmploymentContractAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.CalculateEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Bank = new ApiEmployees.EmploymentContractBankSettlement
                {
                    AccountHolder = (reply0.EmploymentContract ?? new ApiEmployees.EmploymentContract()).PaymentCase switch
                    {
                        ApiEmployees.EmploymentContract.SettlementOneofCase.Bank => (reply0.EmploymentContract?.Bank?.AccountHolder ?? ""),
                        _ => ""
                    },
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.EmploymentContract?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                Lines = { new ApiEmployees.EmploymentContractLine
                {
                    Tax = new ApiEmployees.EmploymentContractLineTax
                    {
                        Amount = (request.Position ?? new ApiEmployees.Position()).ActionCase switch
                        {
                            ApiEmployees.Position.ActionOneofCase.Discount => (request.Position?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc1 = _employeesEmploymentContractCalculationService.CalculateEmploymentContractAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiModelregistry.ExplainRegisteredModelCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiModelregistry.ExplainRegisteredModelCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.EmploymentContract?.Counterparty?.Email ?? ""),
                },
            },
            RegisteredModel = new ApiModelregistry.RegisteredModel
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.EmploymentContract?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Position?.Owner?.UserId ?? ""),
                },
            },
        };
        var reply2 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _modelregistryRegisteredModelCalculationService.ExplainRegisteredModelCalculationAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input3 = new ApiPerformers.ListPerformersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiPerformers.ListPerformersRequest.Types.Filter
            {
                CreatedAfter = (reply0.EmploymentContract?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply2.RegisteredModel?.Components?.GetValueOrDefault("primary")?.ComponentId ?? ""),
                Region = (reply2.RegisteredModel ?? new ApiModelregistry.RegisteredModel()).LocatorCase switch
                {
                    ApiModelregistry.RegisteredModel.LocatorOneofCase.Address => (reply2.RegisteredModel?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc3 = _performersPerformerService.ListPerformersAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiEmployees.CheckPositionEligibilityResponse
        {
            Outcome = new ApiEmployees.CheckPositionEligibilityResponse.Types.Outcome
            {
                Message = (reply3.Result?.Message ?? ""),
            },
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.EmploymentContract?.Audit?.Etag ?? ""),
                    UpdatedAt = (reply2.RegisteredModel?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                PositionId = (request.PositionId ?? ""),
                Status = (request.Position?.Status ?? (ApiEmployees.PositionStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "position",
            "audit", "CheckPositionEligibility", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a position.
    /// </summary>
    public override async Task<ApiEmployees.ExplainPositionDecisionResponse> ExplainPositionDecision(ApiEmployees.ExplainPositionDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExplainPositionDecision for {Tenant}", requestContext.TenantId);
        if (request.Position is { } record)
        {
            var snapshot = PositionMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.RecalculateEmploymentContractsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Position?.Audit?.Etag ?? ""),
                    UpdatedAt = (request.Position?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Position?.Owner?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _sendExplainPositionDecision0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTimekeeping.LintTimesheetsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Timesheet = new ApiTimekeeping.Timesheet
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.EmploymentContract?.BillingAddress?.PostalCode ?? ""),
                },
            },
        };
        var named1 = _registry.Get<ApiTimekeeping.TimesheetValidationService.TimesheetValidationServiceClient>("acme.timekeeping.v1.TimesheetValidationService");
        using var rpc1 = named1.LintTimesheetsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployees.ExplainPositionDecisionResponse
        {
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.EmploymentContract?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.EmploymentContract?.Counterparty?.Email ?? ""),
                },
                PositionId = (request.PositionId ?? ""),
                Version = (request.Position?.Version ?? 0L),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "position",
            "audit", "ExplainPositionDecision", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a position.
    /// </summary>
    public override async Task<ApiEmployees.GetPositionRulesResponse> GetPositionRules(ApiEmployees.GetPositionRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.PositionId, "position_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetPositionRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "position", request.PositionId,
            "GetPositionRules", "accepted", token);
        var input0 = new ApiEmployees.ExplainEmploymentContractCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.ExplainEmploymentContractCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Lines = { new ApiEmployees.EmploymentContractLine
                {
                    Attributes = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                } },
            },
        };
        var descriptor0 = new Method<ApiEmployees.ExplainEmploymentContractCalculationRequest, ApiEmployees.ExplainEmploymentContractCalculationResponse>(
            MethodType.Unary, "acme.employees.v1" + ".EmploymentContractCalculationService", "ExplainEmploymentContractCalculation",
            Wire.Marshaller<ApiEmployees.ExplainEmploymentContractCalculationRequest>(), Wire.Marshaller<ApiEmployees.ExplainEmploymentContractCalculationResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.QuoteEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.EmploymentContract?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (reply0.EmploymentContract?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var command1 = new DispatchCommand<ApiEmployees.QuoteEmploymentContractRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _employeesEmploymentContractCalculationService.QuoteEmploymentContractAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployees.GetPositionRulesResponse
        {
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.EmploymentContract?.Audit?.CreatedBy ?? ""),
                },
                PositionId = (request.PositionId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lints a position.
    /// </summary>
    public override async Task<ApiEmployees.LintPositionsResponse> LintPositions(ApiEmployees.LintPositionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling LintPositions for {Tenant}", requestContext.TenantId);
        if (request.Position is { } record)
        {
            var snapshot = PositionMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.ExplainEmploymentContractCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.ExplainEmploymentContractCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
            },
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Lines = { new ApiEmployees.EmploymentContractLine
                {
                    Tax = new ApiEmployees.EmploymentContractLineTax
                    {
                        Amount = (request.Position ?? new ApiEmployees.Position()).ActionCase switch
                        {
                            ApiEmployees.Position.ActionOneofCase.Discount => (request.Position?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc0 = _employeesEmploymentContractCalculationService.ExplainEmploymentContractCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployees.LintPositionsResponse
        {
            Position = new ApiEmployees.Position
            {
                Name = (request.Position?.Name ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.EmploymentContract?.Counterparty?.DisplayName ?? ""),
                },
                PositionId = (request.PositionId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "position",
            "audit", "LintPositions", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Normalizes a position.
    /// </summary>
    public override async Task<ApiEmployees.NormalizePositionResponse> NormalizePosition(ApiEmployees.NormalizePositionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling NormalizePosition for {Tenant}", requestContext.TenantId);
        if (request.Position is { } record)
        {
            var snapshot = PositionMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.QuoteEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Position?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (request.Position?.Audit?.UpdatedBy ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Position?.Owner?.Email ?? ""),
                },
                Lines = { new ApiEmployees.EmploymentContractLine
                {
                    Tax = new ApiEmployees.EmploymentContractLineTax
                    {
                        Amount = (request.Position ?? new ApiEmployees.Position()).ActionCase switch
                        {
                            ApiEmployees.Position.ActionOneofCase.Discount => (request.Position?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc0 = _employeesEmploymentContractCalculationService.QuoteEmploymentContractAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.NormalizePositionToCloneShow1(reply0);
        using var rpc1 = _showsShowService.CloneShowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployees.NormalizePositionResponse
        {
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.Show?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.EmploymentContract?.Counterparty?.DisplayName ?? ""),
                },
                Parameters = { (request.Position?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                PositionId = (request.PositionId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "position",
            "audit", "NormalizePosition", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews a position.
    /// </summary>
    public override async Task<ApiEmployees.PreviewPositionResponse> PreviewPosition(ApiEmployees.PreviewPositionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PreviewPosition for {Tenant}", requestContext.TenantId);
        if (request.Position is { } record)
        {
            var snapshot = PositionMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.ExplainEmploymentContractCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Position?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (request.Position?.Audit?.UpdatedBy ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
                Lines = { new ApiEmployees.EmploymentContractLine
                {
                    Tax = new ApiEmployees.EmploymentContractLineTax
                    {
                        Amount = (request.Position ?? new ApiEmployees.Position()).ActionCase switch
                        {
                            ApiEmployees.Position.ActionOneofCase.Discount => (request.Position?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc0 = _employeesEmploymentContractCalculationService.ExplainEmploymentContractCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployees.PreviewPositionResponse
        {
            Position = new ApiEmployees.Position
            {
                Description = (request.Position?.Description ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.EmploymentContract?.Counterparty?.Email ?? ""),
                },
                PositionId = (request.PositionId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "position",
            "audit", "PreviewPosition", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores a position.
    /// </summary>
    public override async Task<ApiEmployees.ScorePositionResponse> ScorePosition(ApiEmployees.ScorePositionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ScorePosition for {Tenant}", requestContext.TenantId);
        if (request.Position is { } record)
        {
            var snapshot = PositionMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.RecalculateOffersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Offer = new ApiRecruiting.Offer
            {
                Lines = { new ApiRecruiting.OfferLine
                {
                    Tax = new ApiRecruiting.OfferLineTax
                    {
                        Amount = (request.Position ?? new ApiEmployees.Position()).ActionCase switch
                        {
                            ApiEmployees.Position.ActionOneofCase.Discount => (request.Position?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc0 = _recruitingOfferCalculationService.RecalculateOffersAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLedger.SummarizeLedgerAccountsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Position ?? new ApiEmployees.Position()).ActionCase switch
            {
                ApiEmployees.Position.ActionOneofCase.Block => (request.Position?.Block?.Reason ?? ""),
                _ => ""
            },
        };
        using var rpc1 = _ledgerLedgerAccountQueryService.SummarizeLedgerAccountsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployees.ScorePositionResponse
        {
            Breakdown = new ApiEmployees.ScorePositionResponse.Types.Breakdown
            {
                CurrencyCode = (request.Details?.CurrencyCode ?? ""),
                Total = (reply0.Offer?.Total ?? new ApiCommon.Money()),
            },
            Position = new ApiEmployees.Position
            {
                PositionId = (request.PositionId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "position",
            "audit", "ScorePosition", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates a position.
    /// </summary>
    public override async Task<ApiEmployees.ValidatePositionResponse> ValidatePosition(ApiEmployees.ValidatePositionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ValidatePosition for {Tenant}", requestContext.TenantId);
        if (request.Position is { } record)
        {
            var snapshot = PositionMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.RecalculateEmploymentContractsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Position?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (request.Position?.Audit?.UpdatedBy ?? ""),
                },
                Lines = { new ApiEmployees.EmploymentContractLine
                {
                    Tax = new ApiEmployees.EmploymentContractLineTax
                    {
                        Amount = (request.Position ?? new ApiEmployees.Position()).ActionCase switch
                        {
                            ApiEmployees.Position.ActionOneofCase.Discount => (request.Position?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc0 = _employeesEmploymentContractCalculationService.RecalculateEmploymentContractsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.CountJobOpeningsRequest();
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        input1.Filter = new ApiRecruiting.CountJobOpeningsRequest.Types.Filter
        {
            Region = (reply0.EmploymentContract?.BillingAddress?.Region ?? ""),
        };
        using var rpc1 = _recruitingJobOpeningQueryService.CountJobOpeningsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployees.ValidatePositionResponse
        {
            Outcome = new ApiEmployees.ValidatePositionResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Position = new ApiEmployees.Position
            {
                Owner = (request.Position?.Owner ?? new ApiIdentity.UserRef()),
                PositionId = (request.PositionId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "position",
            "audit", "ValidatePosition", (request.Reason ?? ""), token);
        return response;
    }
}
