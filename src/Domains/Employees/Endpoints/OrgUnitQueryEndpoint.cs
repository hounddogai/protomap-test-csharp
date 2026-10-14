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
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Employees;

/// <summary>
/// Answers read-only questions about org units, for screens and reports.
/// </summary>
public sealed class OrgUnitQueryEndpoint : ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceBase
{
    private readonly ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient _employeesEmployeeQueryService;
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient _employeesEmploymentContractValidationService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly ApiLicensing.LicenseAssignmentValidationService.LicenseAssignmentValidationServiceClient _licensingLicenseAssignmentValidationService;
    private readonly ILogger<OrgUnitQueryEndpoint> _logger;
    private readonly IOrgUnitQueryEndpointSendFindOrgUnits0Port _orgUnitQueryEndpointSendFindOrgUnits0Port;
    private readonly IRecordRepository _repository;

    public OrgUnitQueryEndpoint(
        ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient employeesEmployeeQueryService,
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient employeesEmploymentContractValidationService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        ApiLicensing.LicenseAssignmentValidationService.LicenseAssignmentValidationServiceClient licensingLicenseAssignmentValidationService,
        ILogger<OrgUnitQueryEndpoint> logger,
        IOrgUnitQueryEndpointSendFindOrgUnits0Port orgUnitQueryEndpointSendFindOrgUnits0PortAdapter,
        IRecordRepository repository)
    {
        _employeesEmployeeQueryService = employeesEmployeeQueryService;
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _employeesEmploymentContractValidationService = employeesEmploymentContractValidationService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _licensingLicenseAssignmentValidationService = licensingLicenseAssignmentValidationService;
        _logger = logger;
        _orgUnitQueryEndpointSendFindOrgUnits0Port = orgUnitQueryEndpointSendFindOrgUnits0PortAdapter;
        _repository = repository;
    }
    /// <summary>
    /// Lists org units a page at a time.
    /// </summary>
    public override async Task<ApiEmployees.BrowseOrgUnitsResponse> BrowseOrgUnits(ApiEmployees.BrowseOrgUnitsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling BrowseOrgUnits for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "org_unit", "batch",
            "BrowseOrgUnits", "accepted", token);
        var input0 = new ApiEmployees.ValidateEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.ValidateEmploymentContractRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _employeesEmploymentContractValidationService.ValidateEmploymentContractAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployees.BrowseOrgUnitsResponse
        {
            OrgUnits = { new ApiEmployees.OrgUnit
            {
                Children = { new ApiEmployees.OrgUnit
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        UpdatedBy = (reply0.EmploymentContract?.Audit?.UpdatedBy ?? ""),
                    },
                } },
            } },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "org_unit",
            "audit", "BrowseOrgUnits", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams every matching org unit.
    /// </summary>
    public override async Task ExportOrgUnits(ApiEmployees.ExportOrgUnitsRequest request, IServerStreamWriter<ApiEmployees.ExportOrgUnitsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExportOrgUnits for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "org_unit", "batch",
            "ExportOrgUnits", "accepted", token);
        var input0 = request.ExportOrgUnitsToCheckPositionEligibility0();
        using var rpc0 = _employeesPositionValidationService.CheckPositionEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployees.ExportOrgUnitsResponse
        {
            OrgUnit = new ApiEmployees.OrgUnit
            {
                Children = { new ApiEmployees.OrgUnit
                {
                    Children = { new ApiEmployees.OrgUnit
                    {
                        Manager = new ApiIdentity.UserRef
                        {
                            UserId = (reply0.Position?.Owner?.UserId ?? ""),
                        },
                    } },
                } },
                Location = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "org_unit",
            "audit", "ExportOrgUnits", (request.Format ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    /// <summary>
    /// Lists org units a page at a time.
    /// </summary>
    public override async Task<ApiEmployees.FindOrgUnitsResponse> FindOrgUnits(ApiEmployees.FindOrgUnitsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling FindOrgUnits for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "org_unit", "batch",
            "FindOrgUnits", "accepted", token);
        var input0 = new ApiEmployees.LintPositionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.LintPositionsRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
            Position = new ApiEmployees.Position
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var reply0 = await SendFindOrgUnits0Async(input0, token);
        var response = new ApiEmployees.FindOrgUnitsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "org_unit",
            "audit", "FindOrgUnits", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an org unit.
    /// </summary>
    public override async Task<ApiEmployees.LookupOrgUnitResponse> LookupOrgUnit(ApiEmployees.LookupOrgUnitRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OrgUnitId, "org_unit_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling LookupOrgUnit for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "org_unit", request.OrgUnitId,
            "LookupOrgUnit", "accepted", token);
        var response = new ApiEmployees.LookupOrgUnitResponse
        {
            OrgUnit = new ApiEmployees.OrgUnit
            {
                OrgUnitId = (request.OrgUnitId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns an org unit.
    /// </summary>
    public override async Task<ApiEmployees.PeekOrgUnitResponse> PeekOrgUnit(ApiEmployees.PeekOrgUnitRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OrgUnitId, "org_unit_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PeekOrgUnit for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "org_unit", request.OrgUnitId,
            "PeekOrgUnit", "accepted", token);
        var input0 = new ApiEmployees.ExportEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.ExportEmployeesRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                Labels = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.OrgUnitId ?? ""),
            },
        };
        using var stream0 = _employeesEmployeeQueryService.ExportEmployees(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportEmployees frame {Type}", change0.GetType().Name);
        }
        var response = new ApiEmployees.PeekOrgUnitResponse
        {
            OrgUnit = new ApiEmployees.OrgUnit
            {
                OrgUnitId = (request.OrgUnitId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns several org units at once.
    /// </summary>
    public override async Task<ApiEmployees.ResolveOrgUnitsResponse> ResolveOrgUnits(ApiEmployees.ResolveOrgUnitsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ResolveOrgUnits for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "org_unit", "batch",
            "ResolveOrgUnits", "accepted", token);
        var input0 = new ApiEmployees.CompareEmploymentContractOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Lines = { new ApiEmployees.EmploymentContractLine
                {
                    Attributes = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                } },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiEmployees.CompareEmploymentContractOptionsRequest>();
        using var rpc0 = _employeesEmploymentContractCalculationService.CompareEmploymentContractOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployees.ResolveOrgUnitsResponse
        {
            OrgUnits = { new ApiEmployees.OrgUnit
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.EmploymentContract?.Audit?.Etag ?? ""),
                },
            } },
        };
        return response;
    }
    /// <summary>
    /// Lists org units a page at a time.
    /// </summary>
    public override async Task<ApiEmployees.SuggestOrgUnitsResponse> SuggestOrgUnits(ApiEmployees.SuggestOrgUnitsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SuggestOrgUnits for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "org_unit", "batch",
            "SuggestOrgUnits", "accepted", token);
        var response = new ApiEmployees.SuggestOrgUnitsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "org_unit",
            "audit", "SuggestOrgUnits", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Summarizes an org unit.
    /// </summary>
    public override async Task<ApiEmployees.SummarizeOrgUnitsResponse> SummarizeOrgUnits(ApiEmployees.SummarizeOrgUnitsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SummarizeOrgUnits for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "org_unit", request.OrgUnitId,
            "SummarizeOrgUnits", "accepted", token);
        var input0 = new ApiEmployees.PreviewPositionRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Block = new ApiEmployees.PositionBlockAction
                {
                    Reason = (request.Reason ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiEmployees.PreviewPositionRequest>(json0);
        using var rpc0 = _employeesPositionValidationService.PreviewPositionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.ValidateLicenseAssignmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Position?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiLicensing.EntitlementRef
                {
                    DisplayName = (reply0.Position?.Owner?.DisplayName ?? ""),
                },
                Lines = { new ApiLicensing.LicenseAssignmentLine
                {
                    Amount = (reply0.Position ?? new ApiEmployees.Position()).ActionCase switch
                    {
                        ApiEmployees.Position.ActionOneofCase.Discount => (reply0.Position?.Discount?.Amount ?? new ApiCommon.Money()),
                        _ => new ApiCommon.Money()
                    },
                } },
            },
        };
        using var rpc1 = _licensingLicenseAssignmentValidationService.ValidateLicenseAssignmentAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployees.SummarizeOrgUnitsResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Outcome?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.OrgUnitId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "org_unit",
            "audit", "SummarizeOrgUnits", (request.OrgUnitId ?? ""), token);
        return response;
    }
    private async Task<ApiEmployees.LintPositionsResponse> SendFindOrgUnits0Async(ApiEmployees.LintPositionsRequest request, CancellationToken token)
    {
        return await _orgUnitQueryEndpointSendFindOrgUnits0Port.SendAsync(request, token);
    }
}
