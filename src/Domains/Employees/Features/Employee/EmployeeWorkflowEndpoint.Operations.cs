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
using ApiKeymanagement = Acme.Keymanagement.V1;
using ApiLabeling = Acme.Labeling.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiProcurement = Acme.Procurement.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiWebhooks = Acme.Webhooks.V1;

namespace Acme.Workforce.Employees;

public sealed partial class EmployeeWorkflowEndpoint
{
    /// <summary>
    /// Advances an employee.
    /// </summary>
    public override async Task<ApiEmployees.AdvanceEmployeeWorkflowResponse> AdvanceEmployeeWorkflow(ApiEmployees.AdvanceEmployeeWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling AdvanceEmployeeWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Employee is { } record)
        {
            var snapshot = EmployeeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.RecallManagedDeviceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.RecallManagedDeviceRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (request.Employee?.Contact?.Email ?? ""),
                },
            },
        };
        var reply0 = await SendAdvanceEmployeeWorkflow0Async(input0, token);
        var input1 = new ApiProcurement.QuotePurchaseRequisitionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            PurchaseRequisition = new ApiProcurement.PurchaseRequisition
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.ManagedDevice?.Audit?.CreatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Employee?.PrimaryAddress?.CountryCode ?? ""),
                    Recipient = (request.Employee?.PrimaryAddress?.Recipient ?? ""),
                },
                Counterparty = new ApiProcurement.PurchaseOrderRef
                {
                    Number = (request.Employee ?? new ApiEmployees.Employee()).IdentityDocumentCase switch
                    {
                        ApiEmployees.Employee.IdentityDocumentOneofCase.NationalId => (request.Employee?.NationalId?.Number ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _procurementPurchaseRequisitionCalculationService.QuotePurchaseRequisitionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployees.AdvanceEmployeeWorkflowResponse
        {
            Employee = new ApiEmployees.Employee
            {
                Contact = (request.Employee?.Contact ?? new ApiCommon.ContactInfo()),
                EmployeeId = (request.EmployeeId ?? ""),
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Region = (reply0.ManagedDevice?.Address?.Region ?? ""),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.PurchaseRequisition?.Counterparty?.DisplayName ?? ""),
                },
            },
            Outcome = new ApiEmployees.AdvanceEmployeeWorkflowResponse.Types.Outcome
            {
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employee",
            "audit", "AdvanceEmployeeWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels an employee.
    /// </summary>
    public override async Task<ApiEmployees.CancelEmployeeWorkflowResponse> CancelEmployeeWorkflow(ApiEmployees.CancelEmployeeWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CancelEmployeeWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Employee is { } record)
        {
            var snapshot = EmployeeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.RejectOrgUnitRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OrgUnit = new ApiEmployees.OrgUnit
            {
                Children = { new ApiEmployees.OrgUnit
                {
                    Children = { new ApiEmployees.OrgUnit
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            CreatedBy = (request.Employee?.Audit?.CreatedBy ?? ""),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            Recipient = (request.Employee?.PrimaryAddress?.Recipient ?? ""),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            Email = (request.Details?.Notify?.Email ?? ""),
                        },
                    } },
                } },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiEmployees.RejectOrgUnitRequest>();
        using var rpc0 = _employeesOrgUnitApprovalService.RejectOrgUnitAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.CheckPositionEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.OrgUnit?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (reply0.OrgUnit?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiEmployees.CheckPositionEligibilityRequest>(json1);
        using var rpc1 = _employeesPositionValidationService.CheckPositionEligibilityAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiEmployees.CheckPositionEligibilityRequest>("acme.employees.v1.EmployeeWorkflowService/CancelEmployeeWorkflow/1", input1.Clone()), token);
        var input2 = new ApiDocuments.PullSignaturesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.PullSignaturesRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Signature = new ApiDocuments.Signature
            {
                Attributes = { (request.Employee?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (request.Employee?.RelationshipManager?.DisplayName ?? ""),
                    Number = (request.Employee ?? new ApiEmployees.Employee()).IdentityDocumentCase switch
                    {
                        ApiEmployees.Employee.IdentityDocumentOneofCase.NationalId => (request.Employee?.NationalId?.Number ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc2 = _documentsSignatureSyncService.PullSignaturesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiDocuments.StartDocumentTemplateWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Employee?.PrimaryAddress?.PostalCode ?? ""),
                },
            },
        };
        using var rpc3 = _documentsDocumentTemplateWorkflowService.StartDocumentTemplateWorkflowAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiEmployees.CancelEmployeeWorkflowResponse
        {
            Employee = new ApiEmployees.Employee
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.OrgUnit?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Contact = (request.Employee?.Contact ?? new ApiCommon.ContactInfo()),
                EmployeeId = (request.EmployeeId ?? ""),
            },
            Outcome = new ApiEmployees.CancelEmployeeWorkflowResponse.Types.Outcome
            {
                Status = (reply3.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply2.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employee",
            "audit", "CancelEmployeeWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an employee.
    /// </summary>
    public override async Task<ApiEmployees.InspectEmployeeWorkflowResponse> InspectEmployeeWorkflow(ApiEmployees.InspectEmployeeWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EmployeeId, "employee_id");
        _logger.LogInformation("Handling InspectEmployeeWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "employee", request.EmployeeId,
            "InspectEmployeeWorkflow", "accepted", token);
        var input0 = new ApiDevices.ScheduleDevicePolicyExpiryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _devicesDevicePolicyLifecycleService.ScheduleDevicePolicyExpiryAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiEmployees.InspectEmployeeWorkflowResponse
        {
            Employee = new ApiEmployees.Employee
            {
                EmployeeId = (request.EmployeeId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Lists employees a page at a time.
    /// </summary>
    public override async Task<ApiEmployees.ListEmployeeWorkflowsResponse> ListEmployeeWorkflows(ApiEmployees.ListEmployeeWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListEmployeeWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "employee", "batch",
            "ListEmployeeWorkflows", "accepted", token);
        var input0 = new ApiScheduling.EscalateRosterRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiScheduling.EscalateRosterRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
            Roster = new ApiScheduling.Roster
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _schedulingRosterApprovalService.EscalateRosterAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiEmployees.ListEmployeeWorkflowsResponse
        {
            Employees = { new ApiEmployees.Employee
            {
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Region = (reply0.Roster?.BillingAddress?.Region ?? ""),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Roster?.Counterparty?.DisplayName ?? ""),
                },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employee",
            "audit", "ListEmployeeWorkflows", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pauses an employee.
    /// </summary>
    public override async Task<ApiEmployees.PauseEmployeeWorkflowResponse> PauseEmployeeWorkflow(ApiEmployees.PauseEmployeeWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PauseEmployeeWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Employee is { } record)
        {
            var snapshot = EmployeeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiIdentity.LintUsersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiIdentity.LintUsersRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Employee?.Contact?.Email ?? ""),
                    PreferredChannel = (request.Employee?.Contact?.PreferredChannel ?? ""),
                },
            },
            User = new ApiIdentity.User
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Employee?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _identityUserValidationService.LintUsersAsync);
        var input1 = new ApiDevices.AdvanceComplianceScanWorkflowRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (reply0.User?.Contact?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.AdvanceComplianceScanWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Name = (reply0.User?.Contact?.Name ?? new ApiCommon.PersonName()),
                    Phone = (reply0.User?.Contact?.Phone ?? ""),
                },
            },
        };
        using var rpc1 = _devicesComplianceScanWorkflowService.AdvanceComplianceScanWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiWebhooks.ExportWebhookEndpointsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiWebhooks.ExportWebhookEndpointsRequest.Types.Filter
            {
                CreatedAfter = (reply1.ComplianceScan?.OccurredAt ?? new Timestamp()),
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.User?.RelationshipManager?.UserId ?? ""),
                Region = (reply0.User?.PrimaryAddress?.Region ?? ""),
            },
        };
        using var stream2 = _webhooksWebhookEndpointQueryService.ExportWebhookEndpoints(input2, cancellationToken: token);
        await foreach (var change2 in stream2.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportWebhookEndpoints frame {Type}", change2.GetType().Name);
        }
        var input3 = new ApiKeymanagement.RecalculateCryptoKeysRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiKeymanagement.RecalculateCryptoKeysRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Employee?.PrimaryAddress?.PostalCode ?? ""),
                    Region = (reply0.User?.PrimaryAddress?.Region ?? ""),
                },
            },
        };
        var descriptor3 = new Method<ApiKeymanagement.RecalculateCryptoKeysRequest, ApiKeymanagement.RecalculateCryptoKeysResponse>(
            MethodType.Unary, "acme.keymanagement.v1" + ".CryptoKeyCalculationService", "RecalculateCryptoKeys",
            Wire.Marshaller<ApiKeymanagement.RecalculateCryptoKeysRequest>(), Wire.Marshaller<ApiKeymanagement.RecalculateCryptoKeysResponse>());
        using var rpc3 = _invoker.AsyncUnaryCall(descriptor3, null,
            new CallOptions(cancellationToken: token), input3);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiEmployees.PauseEmployeeWorkflowResponse
        {
            Employee = new ApiEmployees.Employee
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.User?.RelationshipManager?.Email ?? ""),
                },
                EmployeeId = (request.EmployeeId ?? ""),
                Name = (reply0.User?.Contact?.Name ?? new ApiCommon.PersonName()),
                PrimaryAddress = (request.Employee?.PrimaryAddress ?? new ApiCommon.PostalAddress()),
            },
            Outcome = new ApiEmployees.PauseEmployeeWorkflowResponse.Types.Outcome
            {
                ReferenceId = (reply1.Outcome?.ReferenceId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employee",
            "audit", "PauseEmployeeWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resumes an employee.
    /// </summary>
    public override async Task<ApiEmployees.ResumeEmployeeWorkflowResponse> ResumeEmployeeWorkflow(ApiEmployees.ResumeEmployeeWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResumeEmployeeWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Employee is { } record)
        {
            var snapshot = EmployeeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLabeling.SimulateLabelingTaskRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LabelingTask = new ApiLabeling.LabelingTask
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Employee?.RelationshipManager?.DisplayName ?? ""),
                },
                Comments = { new ApiLabeling.LabelingTaskComment
                {
                    CreatedAt = (request.Employee?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Employee?.RelationshipManager?.Email ?? ""),
                    Phone = (request.Employee?.Contact?.Phone ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiLabeling.SimulateLabelingTaskRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _labelingLabelingTaskCalculationService.SimulateLabelingTaskAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiMetrics.UpdateMetricSeriesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            MetricSeries = new ApiMetrics.MetricSeries
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.LabelingTask?.Audit?.CreatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.LabelingTask ?? new ApiLabeling.LabelingTask()).OriginCase switch
                    {
                        ApiLabeling.LabelingTask.OriginOneofCase.Customer => (reply0.LabelingTask?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                    Email = (reply0.LabelingTask?.Requester?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _metricsMetricSeriesService.UpdateMetricSeriesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiEmployees.LintPositionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (reply1.MetricSeries?.Owner?.UserId ?? ""),
                },
            },
        };
        input2 = PropertyMap.Copy(input2);
        using var rpc2 = _employeesPositionValidationService.LintPositionsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiDocuments.RetryDocumentExportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Document = new ApiDocuments.Document
            {
                Counterparty = new ApiDocuments.DocumentFolderRef
                {
                    Path = (reply0.LabelingTask ?? new ApiLabeling.LabelingTask()).OriginCase switch
                    {
                        ApiLabeling.LabelingTask.OriginOneofCase.Customer => (reply0.LabelingTask?.Customer?.Path ?? ""),
                        _ => ""
                    },
                },
                Lines = { new ApiDocuments.DocumentLine
                {
                    Tax = new ApiDocuments.DocumentLineTax
                    {
                        Amount = (reply2.Position ?? new ApiEmployees.Position()).ActionCase switch
                        {
                            ApiEmployees.Position.ActionOneofCase.Discount => (reply2.Position?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc3 = _documentsDocumentExportService.RetryDocumentExportAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiEmployees.ResumeEmployeeWorkflowResponse
        {
            Employee = new ApiEmployees.Employee
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.LabelingTask?.Audit?.CreatedBy ?? ""),
                },
                Contact = (request.Employee?.Contact ?? new ApiCommon.ContactInfo()),
                EmployeeId = (request.EmployeeId ?? ""),
                RelationshipManager = new ApiIdentity.UserRef
                {
                    UserId = (reply0.LabelingTask?.Assignee?.UserId ?? ""),
                },
            },
            Outcome = new ApiEmployees.ResumeEmployeeWorkflowResponse.Types.Outcome
            {
                EffectiveAt = (reply2.Outcome?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employee",
            "audit", "ResumeEmployeeWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys an employee.
    /// </summary>
    public override async Task<ApiEmployees.RetryEmployeeWorkflowResponse> RetryEmployeeWorkflow(ApiEmployees.RetryEmployeeWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RetryEmployeeWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Employee is { } record)
        {
            var snapshot = EmployeeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.SubmitEmployeeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Employee = new ApiEmployees.Employee
            {
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
                NationalId = new ApiEmployees.EmployeeNationalId
                {
                    IssuingCountry = (request.Employee ?? new ApiEmployees.Employee()).IdentityDocumentCase switch
                    {
                        ApiEmployees.Employee.IdentityDocumentOneofCase.NationalId => (request.Employee?.NationalId?.IssuingCountry ?? ""),
                        _ => ""
                    },
                },
                Preferences = new ApiEmployees.EmployeePreferences
                {
                    MarketingOptIn = (request.Employee?.Preferences?.MarketingOptIn ?? false),
                    QuietHours = (request.Employee?.Preferences?.QuietHours ?? new ApiCommon.TimeWindow()),
                },
            },
        };
        using var rpc0 = _employeesEmployeeApprovalService.SubmitEmployeeAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.ResolveEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _employeesEmployeeQueryService.ResolveEmployeesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDocuments.SubscribeDocumentFolderUpdatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SubscribeDocumentFolderUpdatesRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Employee?.RelationshipManager?.Email ?? ""),
                    Name = (request.Employee?.Contact?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.Notify?.Phone ?? ""),
                },
            },
        };
        using var rpc2 = _documentsDocumentFolderNotificationService.SubscribeDocumentFolderUpdatesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiEmployees.RetryEmployeeWorkflowResponse
        {
            Employee = new ApiEmployees.Employee
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply2.DocumentFolder?.Manager?.Email ?? ""),
                    Name = (request.Employee?.Name ?? new ApiCommon.PersonName()),
                },
                EmployeeId = (request.EmployeeId ?? ""),
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.Employee?.PrimaryAddress?.Recipient ?? ""),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    DisplayName = (reply2.DocumentFolder?.Manager?.DisplayName ?? ""),
                },
            },
            Outcome = new ApiEmployees.RetryEmployeeWorkflowResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employee",
            "audit", "RetryEmployeeWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals an employee.
    /// </summary>
    public override async Task<ApiEmployees.SignalEmployeeWorkflowResponse> SignalEmployeeWorkflow(ApiEmployees.SignalEmployeeWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SignalEmployeeWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Employee is { } record)
        {
            var snapshot = EmployeeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.PreviewPositionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.PreviewPositionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Employee?.RelationshipManager?.Email ?? ""),
                },
            },
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Employee?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (request.Employee?.Audit?.Etag ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiEmployees.PreviewPositionRequest>(json0);
        using var rpc0 = _employeesPositionValidationService.PreviewPositionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.SuggestEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.SuggestEmployeesRequest.Types.Filter
            {
                CreatedAfter = (reply0.Position?.Audit?.UpdatedAt ?? new Timestamp()),
                Region = (request.Employee?.PrimaryAddress?.Region ?? ""),
            },
        };
        using var rpc1 = _employeesEmployeeQueryService.SuggestEmployeesAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        var input2 = new ApiEmployees.ValidatePositionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Employee?.RelationshipManager?.UserId ?? ""),
                },
            },
        };
        using var rpc2 = _employeesPositionValidationService.ValidatePositionAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        await Task.WhenAll(pending1, pending2);
        var reply1 = await pending1;
        var reply2 = await pending2;
        var response = new ApiEmployees.SignalEmployeeWorkflowResponse
        {
            Employee = new ApiEmployees.Employee
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Position?.Audit?.Etag ?? ""),
                    UpdatedBy = (reply0.Position?.Audit?.UpdatedBy ?? ""),
                },
                EmployeeId = (request.EmployeeId ?? ""),
                Status = (request.Employee?.Status ?? (ApiEmployees.EmployeeStatus)0),
            },
            Outcome = new ApiEmployees.SignalEmployeeWorkflowResponse.Types.Outcome
            {
                Message = (reply2.Outcome?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employee",
            "audit", "SignalEmployeeWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts an employee.
    /// </summary>
    public override async Task<ApiEmployees.StartEmployeeWorkflowResponse> StartEmployeeWorkflow(ApiEmployees.StartEmployeeWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StartEmployeeWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Employee is { } record)
        {
            var snapshot = EmployeeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.LookupEmployeeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmployeeId = (request.EmployeeId ?? ""),
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _employeesEmployeeQueryService.LookupEmployeeAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiDocuments.ResolveDocumentConflictRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ResolveDocumentConflictRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Document = new ApiDocuments.Document
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Employee?.Audit?.Etag ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.Employee?.PrimaryAddress?.CountryCode ?? ""),
                    Region = (request.Employee?.PrimaryAddress?.Region ?? ""),
                },
            },
        };
        using var rpc1 = _documentsDocumentSyncService.ResolveDocumentConflictAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployees.StartEmployeeWorkflowResponse
        {
            Employee = new ApiEmployees.Employee
            {
                EmployeeId = (request.EmployeeId ?? ""),
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply1.Document?.BillingAddress?.Locality ?? ""),
                },
                RelationshipManager = (request.Employee?.RelationshipManager ?? new ApiIdentity.UserRef()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employee",
            "audit", "StartEmployeeWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiDevices.RecallManagedDeviceResponse> SendAdvanceEmployeeWorkflow0Async(ApiDevices.RecallManagedDeviceRequest request, CancellationToken token)
    {
        return await _employeeWorkflowEndpointSendAdvanceEmployeeWorkflow0Port.SendAsync(request, token);
    }
}
