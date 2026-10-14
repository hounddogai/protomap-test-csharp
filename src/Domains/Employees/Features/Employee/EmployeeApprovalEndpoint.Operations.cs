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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Employees;

public sealed partial class EmployeeApprovalEndpoint
{
    /// <summary>
    /// Approves an employee.
    /// </summary>
    public override async Task<ApiEmployees.ApproveEmployeeResponse> ApproveEmployee(ApiEmployees.ApproveEmployeeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ApproveEmployee for {Tenant}", requestContext.TenantId);
        if (request.Employee is { } record)
        {
            var snapshot = EmployeeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.ScheduleDocumentExportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ScheduleDocumentExportRequest.Types.Details
            {
                Period = (request.Employee?.Preferences?.QuietHours ?? new ApiCommon.TimeWindow()),
            },
            Reason = (request.Reason ?? ""),
        };
        var named0 = _registry.Get<ApiDocuments.DocumentExportService.DocumentExportServiceClient>("acme.documents.v1.DocumentExportService");
        using var rpc0 = named0.ScheduleDocumentExportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.LookupEmployeeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmployeeId = (request.EmployeeId ?? ""),
        };
        var descriptor1 = new Method<ApiEmployees.LookupEmployeeRequest, ApiEmployees.LookupEmployeeResponse>(
            MethodType.Unary, "acme.employees.v1" + ".EmployeeQueryService", "LookupEmployee",
            Wire.Marshaller<ApiEmployees.LookupEmployeeRequest>(), Wire.Marshaller<ApiEmployees.LookupEmployeeResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiEmployees.ValidateEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Employee?.PrimaryAddress?.CountryCode ?? ""),
                    PostalCode = (reply1.Employee?.PrimaryAddress?.PostalCode ?? ""),
                    Region = (reply1.Employee?.PrimaryAddress?.Region ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply1.Employee?.RelationshipManager?.DisplayName ?? ""),
                },
            },
        };
        var command2 = new DispatchCommand<ApiEmployees.ValidateEmploymentContractRequest>(input2,
            "primary", requestContext.TenantId);
        var routed2 = command2 with { Region = _settings.Value.Region };
        using var rpc2 = _employeesEmploymentContractValidationService.ValidateEmploymentContractAsync(routed2.Payload, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiEmployees.SearchEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.SearchEmployeesRequest.Types.Filter
            {
                CreatedAfter = (reply1.Employee ?? new ApiEmployees.Employee()).IdentityDocumentCase switch
                {
                    ApiEmployees.Employee.IdentityDocumentOneofCase.Passport => (reply1.Employee?.Passport?.ExpiresAt ?? new Timestamp()),
                    _ => new Timestamp()
                },
                Labels = { (request.Employee?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Details?.Approver?.UserId ?? ""),
                Region = (reply2.EmploymentContract?.BillingAddress?.Region ?? ""),
            },
        };
        using var rpc3 = _employeesEmployeeService.SearchEmployeesAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiEmployees.ApproveEmployeeResponse
        {
            Employee = new ApiEmployees.Employee
            {
                Attributes = { (reply1.Employee?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                EmployeeId = (request.EmployeeId ?? ""),
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply1.Employee?.PrimaryAddress?.CountryCode ?? ""),
                    Region = (reply2.EmploymentContract?.BillingAddress?.Region ?? ""),
                },
                RelationshipManager = (request.Employee?.RelationshipManager ?? new ApiIdentity.UserRef()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply3.Result?.CorrelationId ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Employee?.TaxNumber ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employee",
            "audit", "ApproveEmployee", (request.Employee?.TaxNumber ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments an employee.
    /// </summary>
    public override async Task<ApiEmployees.CommentEmployeeApprovalResponse> CommentEmployeeApproval(ApiEmployees.CommentEmployeeApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CommentEmployeeApproval for {Tenant}", requestContext.TenantId);
        if (request.Employee is { } record)
        {
            var snapshot = EmployeeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.CompareEmploymentContractOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Employee?.RelationshipManager?.Email ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _employeesEmploymentContractCalculationService.CompareEmploymentContractOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.CommentEmployeeApprovalToListVisitorPassConflicts1(reply0);
        using var rpc1 = _workplacesVisitorPassSyncService.ListVisitorPassConflictsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployees.CommentEmployeeApprovalResponse
        {
            Employee = new ApiEmployees.Employee
            {
                Contact = (request.Employee?.Contact ?? new ApiCommon.ContactInfo()),
                EmployeeId = (request.EmployeeId ?? ""),
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.EmploymentContract?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employee",
            "audit", "CommentEmployeeApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates an employee.
    /// </summary>
    public override async Task<ApiEmployees.EscalateEmployeeResponse> EscalateEmployee(ApiEmployees.EscalateEmployeeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EscalateEmployee for {Tenant}", requestContext.TenantId);
        if (request.Employee is { } record)
        {
            var snapshot = EmployeeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.ExportOrgUnitsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.ExportOrgUnitsRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
                Labels = { (request.Employee?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Employee?.RelationshipManager?.UserId ?? ""),
                Region = (request.Employee?.PrimaryAddress?.Region ?? ""),
            },
        };
        using var stream0 = _employeesOrgUnitQueryService.ExportOrgUnits(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportOrgUnits frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiEmployees.SearchEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.SearchEmployeesRequest.Types.Filter
            {
                CreatedAfter = (request.Employee ?? new ApiEmployees.Employee()).IdentityDocumentCase switch
                {
                    ApiEmployees.Employee.IdentityDocumentOneofCase.Passport => (request.Employee?.Passport?.ExpiresAt ?? new Timestamp()),
                    _ => new Timestamp()
                },
                Region = (request.Employee?.PrimaryAddress?.Region ?? ""),
            },
        };
        using var rpc1 = _employeesEmployeeService.SearchEmployeesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployees.EscalateEmployeeResponse
        {
            Employee = new ApiEmployees.Employee
            {
                Contact = (request.Employee?.Contact ?? new ApiCommon.ContactInfo()),
                EmployeeId = (request.EmployeeId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employee",
            "audit", "EscalateEmployee", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists employees a page at a time.
    /// </summary>
    public override async Task<ApiEmployees.ListPendingEmployeesResponse> ListPendingEmployees(ApiEmployees.ListPendingEmployeesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListPendingEmployees for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "employee", "batch",
            "ListPendingEmployees", "accepted", token);
        var input0 = new ApiEmployees.SummarizeEmployeesRequest
        {
            Annotations = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.SummarizeEmployeesRequest.Types.Details
            {
                Filters = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiEmployees.SummarizeEmployeesRequest>();
        using var rpc0 = _employeesEmployeeQueryService.SummarizeEmployeesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.CompareEmploymentContractOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiEmployees.CompareEmploymentContractOptionsRequest>(json1);
        using var rpc1 = _employeesEmploymentContractCalculationService.CompareEmploymentContractOptionsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployees.ListPendingEmployeesResponse
        {
            Employees = { new ApiEmployees.Employee
            {
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (reply1.EmploymentContract?.BillingAddress?.Recipient ?? ""),
                },
            } },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employee",
            "audit", "ListPendingEmployees", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls an employee.
    /// </summary>
    public override async Task<ApiEmployees.RecallEmployeeResponse> RecallEmployee(ApiEmployees.RecallEmployeeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecallEmployee for {Tenant}", requestContext.TenantId);
        if (request.Employee is { } record)
        {
            var snapshot = EmployeeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.CompareEmploymentContractOptionsRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.EmploymentContract = new ApiEmployees.EmploymentContract
        {
            Audit = new ApiCommon.AuditInfo
            {
                UpdatedAt = (request.Employee?.Audit?.UpdatedAt ?? new Timestamp()),
            },
        };
        using var rpc0 = _employeesEmploymentContractCalculationService.CompareEmploymentContractOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.PauseSoftwareLicenseWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.PauseSoftwareLicenseWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Employee?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
                Holder = new ApiLicensing.LicenseAssignmentRef
                {
                    DisplayName = (reply0.EmploymentContract?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _licensingSoftwareLicenseWorkflowService.PauseSoftwareLicenseWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDevices.RecallComplianceScanRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.RecallComplianceScanRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.EmploymentContract?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        using var rpc2 = _devicesComplianceScanApprovalService.RecallComplianceScanAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiDevices.SubmitComplianceScanRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (request.Employee?.RelationshipManager?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.SubmitComplianceScanRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Employee?.RelationshipManager?.DisplayName ?? ""),
                    Email = (request.Employee?.Contact?.Email ?? ""),
                    UserId = (request.Details?.Approver?.UserId ?? ""),
                },
            },
        };
        using var rpc3 = _devicesComplianceScanApprovalService.SubmitComplianceScanAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiEmployees.RecallEmployeeResponse
        {
            Employee = new ApiEmployees.Employee
            {
                Attributes = { (request.Employee?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.EmploymentContract?.Audit?.CreatedAt ?? new Timestamp()),
                },
                EmployeeId = (request.EmployeeId ?? ""),
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.EmploymentContract?.BillingAddress?.Locality ?? ""),
                },
            },
            Outcome = new ApiEmployees.RecallEmployeeResponse.Types.Outcome
            {
                Message = (reply2.Outcome?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply3.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employee",
            "audit", "RecallEmployee", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits an employee.
    /// </summary>
    public override async Task<ApiEmployees.SubmitEmployeeResponse> SubmitEmployee(ApiEmployees.SubmitEmployeeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitEmployee for {Tenant}", requestContext.TenantId);
        if (request.Employee is { } record)
        {
            var snapshot = EmployeeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.ExplainEmploymentContractCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.ExplainEmploymentContractCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Employee?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
            },
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Employee?.Audit?.Etag ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Employee?.PrimaryAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _employeesEmploymentContractCalculationService.ExplainEmploymentContractCalculationAsync);
        var input1 = new ApiEmployees.PreviewPositionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.EmploymentContract?.Audit?.CreatedBy ?? ""),
                    Etag = (reply0.EmploymentContract?.Audit?.Etag ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Employee?.RelationshipManager?.DisplayName ?? ""),
                    UserId = (request.Details?.Approver?.UserId ?? ""),
                },
            },
        };
        using var rpc1 = _sendSubmitEmployee1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployees.SubmitEmployeeResponse
        {
            Employee = new ApiEmployees.Employee
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.EmploymentContract?.Audit?.CreatedAt ?? new Timestamp()),
                },
                EmployeeId = (request.EmployeeId ?? ""),
                RelationshipManager = (request.Employee?.RelationshipManager ?? new ApiIdentity.UserRef()),
            },
            Outcome = new ApiEmployees.SubmitEmployeeResponse.Types.Outcome
            {
                ReferenceId = (reply1.Outcome?.ReferenceId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employee",
            "audit", "SubmitEmployee", (request.Reason ?? ""), token);
        return response;
    }
}
