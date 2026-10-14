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
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiSearch = Acme.Search.V1;

namespace Acme.Workforce.Employees;

public sealed partial class EmployeeQueryEndpoint
{
    /// <summary>
    /// Lists employees a page at a time.
    /// </summary>
    public override async Task<ApiEmployees.BrowseEmployeesResponse> BrowseEmployees(ApiEmployees.BrowseEmployeesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling BrowseEmployees for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "employee", "batch",
            "BrowseEmployees", "accepted", token);
        var input0 = new ApiLicensing.CountEntitlementsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLicensing.CountEntitlementsRequest.Types.Filter
            {
                Region = (request.Filter?.Region ?? ""),
            },
            Page = new ApiCommon.PageRequest
            {
                PageSize = (request.Page?.PageSize ?? 0),
            },
        };
        var named0 = _registry.Get<ApiLicensing.EntitlementQueryService.EntitlementQueryServiceClient>("acme.licensing.v1.EntitlementQueryService");
        using var rpc0 = named0.CountEntitlementsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployees.BrowseEmployeesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employee",
            "audit", "BrowseEmployees", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Counts employees by status.
    /// </summary>
    public override async Task<ApiEmployees.CountEmployeesResponse> CountEmployees(ApiEmployees.CountEmployeesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CountEmployees for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "employee", "batch",
            "CountEmployees", "accepted", token);
        var response = new ApiEmployees.CountEmployeesResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employee",
            "audit", "CountEmployees", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams every matching employee.
    /// </summary>
    public override async Task ExportEmployees(ApiEmployees.ExportEmployeesRequest request, IServerStreamWriter<ApiEmployees.ExportEmployeesResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExportEmployees for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "employee", "batch",
            "ExportEmployees", "accepted", token);
        var input0 = new ApiSearch.CheckSearchIndexEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SearchIndex = new ApiSearch.SearchIndex
            {
                Address = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiSearch.CheckSearchIndexEligibilityRequest, ApiSearch.CheckSearchIndexEligibilityResponse>(
            MethodType.Unary, "acme.search.v1" + ".SearchIndexValidationService", "CheckSearchIndexEligibility",
            Wire.Marshaller<ApiSearch.CheckSearchIndexEligibilityRequest>(), Wire.Marshaller<ApiSearch.CheckSearchIndexEligibilityResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployees.ExportEmployeesResponse
        {
            Employee = new ApiEmployees.Employee
            {
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.SearchIndex?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Region = (request.Filter?.Region ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employee",
            "audit", "ExportEmployees", (request.Format ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    /// <summary>
    /// Lists employees a page at a time.
    /// </summary>
    public override async Task<ApiEmployees.FindEmployeesResponse> FindEmployees(ApiEmployees.FindEmployeesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling FindEmployees for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "employee", "batch",
            "FindEmployees", "accepted", token);
        var response = new ApiEmployees.FindEmployeesResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employee",
            "audit", "FindEmployees", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an employee.
    /// </summary>
    public override async Task<ApiEmployees.LookupEmployeeResponse> LookupEmployee(ApiEmployees.LookupEmployeeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EmployeeId, "employee_id");
        _logger.LogInformation("Handling LookupEmployee for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "employee", request.EmployeeId,
            "LookupEmployee", "accepted", token);
        var response = new ApiEmployees.LookupEmployeeResponse
        {
            Employee = new ApiEmployees.Employee
            {
                EmployeeId = (request.EmployeeId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns an employee.
    /// </summary>
    public override async Task<ApiEmployees.PeekEmployeeResponse> PeekEmployee(ApiEmployees.PeekEmployeeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EmployeeId, "employee_id");
        _logger.LogInformation("Handling PeekEmployee for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "employee", request.EmployeeId,
            "PeekEmployee", "accepted", token);
        var response = new ApiEmployees.PeekEmployeeResponse
        {
            Employee = new ApiEmployees.Employee
            {
                EmployeeId = (request.EmployeeId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns several employees at once.
    /// </summary>
    public override async Task<ApiEmployees.ResolveEmployeesResponse> ResolveEmployees(ApiEmployees.ResolveEmployeesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResolveEmployees for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "employee", "batch",
            "ResolveEmployees", "accepted", token);
        var input0 = new ApiEmployees.PreviewPositionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.PreviewPositionRequest.Types.Details
            {
                Locale = (request.Context?.Locale ?? ""),
            },
            Position = new ApiEmployees.Position
            {
                Block = new ApiEmployees.PositionBlockAction
                {
                    NotifyEmail = (request.Context?.Actor?.Email ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiEmployees.PreviewPositionRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _employeesPositionValidationService.PreviewPositionAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployees.ResolveEmployeesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists employees a page at a time.
    /// </summary>
    public override async Task<ApiEmployees.SuggestEmployeesResponse> SuggestEmployees(ApiEmployees.SuggestEmployeesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SuggestEmployees for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "employee", "batch",
            "SuggestEmployees", "accepted", token);
        var input0 = new ApiDocuments.ExplainSignatureDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ExplainSignatureDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
        };
        using var rpc0 = _documentsSignatureValidationService.ExplainSignatureDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployees.SuggestEmployeesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employee",
            "audit", "SuggestEmployees", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Summarizes an employee.
    /// </summary>
    public override async Task<ApiEmployees.SummarizeEmployeesResponse> SummarizeEmployees(ApiEmployees.SummarizeEmployeesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SummarizeEmployees for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "employee", request.EmployeeId,
            "SummarizeEmployees", "accepted", token);
        var input0 = new ApiEmployees.ExplainEmploymentContractDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    EmployeeId = (request.EmployeeId ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _employeesEmploymentContractValidationService.ExplainEmploymentContractDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.SummarizeEmployeesToBatchGetNotifications1();
        using var rpc1 = _notificationsNotificationService.BatchGetNotificationsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployees.SummarizeEmployeesResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employee",
            "audit", "SummarizeEmployees", (request.Reason ?? ""), token);
        return response;
    }
}
