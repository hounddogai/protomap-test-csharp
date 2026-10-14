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
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

/// <summary>
/// Runs the long-lived workflow that takes a building from start to finish.
/// </summary>
public sealed class BuildingWorkflowEndpoint : ApiWorkplaces.BuildingWorkflowService.BuildingWorkflowServiceBase
{
    private readonly ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient _authorizationAccessPolicyQueryService;
    private readonly ApiDocuments.DocumentSyncService.DocumentSyncServiceClient _documentsDocumentSyncService;
    private readonly ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient _employeesEmployeeQueryService;
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _employeesEmployeeService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ProblemReportingService.ProblemReportingServiceClient _itsmProblemReportingService;
    private readonly ILogger<BuildingWorkflowEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient _metricsMetricSeriesQueryService;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiItsm.GenerateProblemReportRequest, CallOptions, AsyncUnaryCall<ApiItsm.GenerateProblemReportResponse>> _sendStartBuildingWorkflow1;
    private readonly ApiWorkplaces.BuildingApprovalService.BuildingApprovalServiceClient _workplacesBuildingApprovalService;
    private readonly ApiWorkplaces.DeskQueryService.DeskQueryServiceClient _workplacesDeskQueryService;
    private readonly ApiWorkplaces.VisitorPassLifecycleService.VisitorPassLifecycleServiceClient _workplacesVisitorPassLifecycleService;

    public BuildingWorkflowEndpoint(
        ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient authorizationAccessPolicyQueryService,
        ApiDocuments.DocumentSyncService.DocumentSyncServiceClient documentsDocumentSyncService,
        ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient employeesEmployeeQueryService,
        ApiEmployees.EmployeeService.EmployeeServiceClient employeesEmployeeService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        CallInvoker invoker,
        ApiItsm.ProblemReportingService.ProblemReportingServiceClient itsmProblemReportingService,
        ILogger<BuildingWorkflowEndpoint> logger,
        ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient metricsMetricSeriesQueryService,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiWorkplaces.BuildingApprovalService.BuildingApprovalServiceClient workplacesBuildingApprovalService,
        ApiWorkplaces.DeskQueryService.DeskQueryServiceClient workplacesDeskQueryService,
        ApiWorkplaces.VisitorPassLifecycleService.VisitorPassLifecycleServiceClient workplacesVisitorPassLifecycleService)
    {
        _authorizationAccessPolicyQueryService = authorizationAccessPolicyQueryService;
        _documentsDocumentSyncService = documentsDocumentSyncService;
        _employeesEmployeeQueryService = employeesEmployeeQueryService;
        _employeesEmployeeService = employeesEmployeeService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _invoker = invoker;
        _itsmProblemReportingService = itsmProblemReportingService;
        _logger = logger;
        _metricsMetricSeriesQueryService = metricsMetricSeriesQueryService;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _workplacesBuildingApprovalService = workplacesBuildingApprovalService;
        _workplacesDeskQueryService = workplacesDeskQueryService;
        _workplacesVisitorPassLifecycleService = workplacesVisitorPassLifecycleService;
        _sendStartBuildingWorkflow1 = _itsmProblemReportingService.GenerateProblemReportAsync;
    }
    /// <summary>
    /// Advances a building.
    /// </summary>
    public override async Task<ApiWorkplaces.AdvanceBuildingWorkflowResponse> AdvanceBuildingWorkflow(ApiWorkplaces.AdvanceBuildingWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling AdvanceBuildingWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Building is { } record)
        {
            var snapshot = BuildingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiMetrics.CountMetricSeriesesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiMetrics.CountMetricSeriesesRequest.Types.Filter
            {
                CreatedAfter = (request.Building?.Audit?.UpdatedAt ?? new Timestamp()),
                OwnerId = (request.Building?.BuildingId ?? ""),
                Region = (request.Building?.Location?.Region ?? ""),
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _metricsMetricSeriesQueryService.CountMetricSeriesesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiAuthorization.CountAccessPoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiAuthorization.CountAccessPoliciesRequest.Types.Filter
            {
                CreatedAfter = (request.Building?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.Result?.CorrelationId ?? ""),
                Region = (request.Building?.Location?.Region ?? ""),
            },
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiAuthorization.CountAccessPoliciesRequest>();
        using var rpc1 = _authorizationAccessPolicyQueryService.CountAccessPoliciesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.AdvanceBuildingWorkflowResponse
        {
            Building = new ApiWorkplaces.Building
            {
                BuildingId = (request.BuildingId ?? ""),
                Location = (request.Building?.Location ?? new ApiCommon.PostalAddress()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "AdvanceBuildingWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels a building.
    /// </summary>
    public override async Task<ApiWorkplaces.CancelBuildingWorkflowResponse> CancelBuildingWorkflow(ApiWorkplaces.CancelBuildingWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CancelBuildingWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Building is { } record)
        {
            var snapshot = BuildingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.LintPositionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.LintPositionsRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
            },
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.Building?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Building?.Manager?.DisplayName ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiEmployees.LintPositionsRequest>(json0);
        using var rpc0 = _employeesPositionValidationService.LintPositionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiWorkplaces.CancelBuildingWorkflowResponse
        {
            Building = new ApiWorkplaces.Building
            {
                BuildingId = (request.BuildingId ?? ""),
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            CreatedBy = (reply0.Position?.Audit?.CreatedBy ?? ""),
                        },
                    } },
                } },
                Name = (request.Building?.Name ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "CancelBuildingWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a building.
    /// </summary>
    public override async Task<ApiWorkplaces.InspectBuildingWorkflowResponse> InspectBuildingWorkflow(ApiWorkplaces.InspectBuildingWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.BuildingId, "building_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling InspectBuildingWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "building", request.BuildingId,
            "InspectBuildingWorkflow", "accepted", token);
        var input0 = new ApiWorkplaces.ResolveDesksRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        using var rpc0 = _workplacesDeskQueryService.ResolveDesksAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiWorkplaces.InspectBuildingWorkflowResponse
        {
            Building = new ApiWorkplaces.Building
            {
                BuildingId = (request.BuildingId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Lists buildings a page at a time.
    /// </summary>
    public override async Task<ApiWorkplaces.ListBuildingWorkflowsResponse> ListBuildingWorkflows(ApiWorkplaces.ListBuildingWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListBuildingWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "building", "batch",
            "ListBuildingWorkflows", "accepted", token);
        var response = new ApiWorkplaces.ListBuildingWorkflowsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "ListBuildingWorkflows", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pauses a building.
    /// </summary>
    public override async Task<ApiWorkplaces.PauseBuildingWorkflowResponse> PauseBuildingWorkflow(ApiWorkplaces.PauseBuildingWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PauseBuildingWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Building is { } record)
        {
            var snapshot = BuildingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.UpdateEmployeeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Employee = new ApiEmployees.Employee
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Building?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (request.Building?.Audit?.Etag ?? ""),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc0 = _employeesEmployeeService.UpdateEmployeeAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiWorkplaces.PauseBuildingWorkflowResponse
        {
            Building = new ApiWorkplaces.Building
            {
                BuildingId = (request.BuildingId ?? ""),
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Location = new ApiCommon.PostalAddress
                        {
                            Lines = { (reply0.Employee?.PrimaryAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                        },
                    } },
                } },
                Manager = (request.Building?.Manager ?? new ApiIdentity.UserRef()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "PauseBuildingWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resumes a building.
    /// </summary>
    public override async Task<ApiWorkplaces.ResumeBuildingWorkflowResponse> ResumeBuildingWorkflow(ApiWorkplaces.ResumeBuildingWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ResumeBuildingWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Building is { } record)
        {
            var snapshot = BuildingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.ExplainPositionDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Building?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Building?.Manager?.DisplayName ?? ""),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _employeesPositionValidationService.ExplainPositionDecisionAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiWorkplaces.ResumeBuildingWorkflowResponse
        {
            Building = new ApiWorkplaces.Building
            {
                BuildingId = (request.BuildingId ?? ""),
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            Etag = (reply0.Position?.Audit?.Etag ?? ""),
                        },
                    } },
                } },
                Depth = (request.Building?.Depth ?? 0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "ResumeBuildingWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals a building.
    /// </summary>
    public override async Task<ApiWorkplaces.SignalBuildingWorkflowResponse> SignalBuildingWorkflow(ApiWorkplaces.SignalBuildingWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SignalBuildingWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Building is { } record)
        {
            var snapshot = BuildingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWorkplaces.ScheduleVisitorPassExpiryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiWorkplaces.ScheduleVisitorPassExpiryRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Building?.Manager?.Email ?? ""),
                    Phone = (request.Details?.Notify?.Phone ?? ""),
                },
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Building?.Location?.CountryCode ?? ""),
                    Region = (request.Building?.Location?.Region ?? ""),
                },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _workplacesVisitorPassLifecycleService.ScheduleVisitorPassExpiryAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiWorkplaces.SignalBuildingWorkflowResponse
        {
            Building = new ApiWorkplaces.Building
            {
                Attributes = { (request.Building?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                BuildingId = (request.BuildingId ?? ""),
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Location = new ApiCommon.PostalAddress
                        {
                            CountryCode = (reply0.VisitorPass?.BillingAddress?.CountryCode ?? ""),
                        },
                    } },
                } },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "SignalBuildingWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts a building.
    /// </summary>
    public override async Task<ApiWorkplaces.StartBuildingWorkflowResponse> StartBuildingWorkflow(ApiWorkplaces.StartBuildingWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling StartBuildingWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Building is { } record)
        {
            var snapshot = BuildingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.SummarizeEmployeesRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        var reply0 = await Workflow.SendAsync(input0, token, _employeesEmployeeQueryService.SummarizeEmployeesAsync);
        var input1 = new ApiItsm.GenerateProblemReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        using var rpc1 = _sendStartBuildingWorkflow1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDocuments.ListDocumentConflictsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentConflictsRequest.Types.Filter
            {
                Region = (request.Building?.Location?.Region ?? ""),
            },
        };
        var named2 = _registry.Get<ApiDocuments.DocumentSyncService.DocumentSyncServiceClient>("acme.documents.v1.DocumentSyncService");
        using var rpc2 = named2.ListDocumentConflictsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiWorkplaces.DelegateBuildingApprovalRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            Etag = (request.Building?.Audit?.Etag ?? ""),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            CountryCode = (request.Building?.Location?.CountryCode ?? ""),
                            Region = (request.Building?.Location?.Region ?? ""),
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var descriptor3 = new Method<ApiWorkplaces.DelegateBuildingApprovalRequest, ApiWorkplaces.DelegateBuildingApprovalResponse>(
            MethodType.Unary, "acme.workplaces.v1" + ".BuildingApprovalService", "DelegateBuildingApproval",
            Wire.Marshaller<ApiWorkplaces.DelegateBuildingApprovalRequest>(), Wire.Marshaller<ApiWorkplaces.DelegateBuildingApprovalResponse>());
        using var rpc3 = _invoker.AsyncUnaryCall(descriptor3, null,
            new CallOptions(cancellationToken: token), input3);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiWorkplaces.StartBuildingWorkflowResponse
        {
            Building = new ApiWorkplaces.Building
            {
                BuildingId = (request.BuildingId ?? ""),
                Depth = (request.Building?.Depth ?? 0),
                ParentId = (reply3.Building?.ParentId ?? ""),
            },
            Outcome = new ApiWorkplaces.StartBuildingWorkflowResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Step ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "StartBuildingWorkflow", (request.Details?.Step ?? ""), token);
        return response;
    }
}
