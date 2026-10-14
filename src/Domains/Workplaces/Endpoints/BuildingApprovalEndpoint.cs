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
using ApiCustomers = Acme.Customers.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiInvoicing = Acme.Invoicing.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiSearch = Acme.Search.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

/// <summary>
/// Routes buildings through approval, with escalation and delegation.
/// </summary>
public sealed class BuildingApprovalEndpoint : ApiWorkplaces.BuildingApprovalService.BuildingApprovalServiceBase
{
    private readonly NotificationBus _bus;
    private readonly ApiCorporateevents.RegistrationWorkflowService.RegistrationWorkflowServiceClient _corporateeventsRegistrationWorkflowService;
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _employeesEmployeeService;
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ApiIdentity.UserService.UserServiceClient _identityUserService;
    private readonly ApiInvoicing.InvoiceBatchCalculationService.InvoiceBatchCalculationServiceClient _invoicingInvoiceBatchCalculationService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.IncidentApprovalService.IncidentApprovalServiceClient _itsmIncidentApprovalService;
    private readonly ApiItsm.ServiceAssetWorkflowService.ServiceAssetWorkflowServiceClient _itsmServiceAssetWorkflowService;
    private readonly ILogger<BuildingApprovalEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesService.MetricSeriesServiceClient _metricsMetricSeriesService;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiSearch.SearchIndexService.SearchIndexServiceClient _searchSearchIndexService;
    private readonly Func<ApiMetrics.ListMetricSeriesesRequest, CallOptions, AsyncUnaryCall<ApiMetrics.ListMetricSeriesesResponse>> _sendApproveBuilding2;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiWorkplaces.VisitorPassCalculationService.VisitorPassCalculationServiceClient _workplacesVisitorPassCalculationService;

    public BuildingApprovalEndpoint(
        NotificationBus bus,
        ApiCorporateevents.RegistrationWorkflowService.RegistrationWorkflowServiceClient corporateeventsRegistrationWorkflowService,
        ApiEmployees.EmployeeService.EmployeeServiceClient employeesEmployeeService,
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ApiIdentity.UserService.UserServiceClient identityUserService,
        ApiInvoicing.InvoiceBatchCalculationService.InvoiceBatchCalculationServiceClient invoicingInvoiceBatchCalculationService,
        CallInvoker invoker,
        ApiItsm.IncidentApprovalService.IncidentApprovalServiceClient itsmIncidentApprovalService,
        ApiItsm.ServiceAssetWorkflowService.ServiceAssetWorkflowServiceClient itsmServiceAssetWorkflowService,
        ILogger<BuildingApprovalEndpoint> logger,
        ApiMetrics.MetricSeriesService.MetricSeriesServiceClient metricsMetricSeriesService,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiSearch.SearchIndexService.SearchIndexServiceClient searchSearchIndexService,
        IOptions<MeshOptions> settings,
        ApiWorkplaces.VisitorPassCalculationService.VisitorPassCalculationServiceClient workplacesVisitorPassCalculationService)
    {
        _bus = bus;
        _corporateeventsRegistrationWorkflowService = corporateeventsRegistrationWorkflowService;
        _employeesEmployeeService = employeesEmployeeService;
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _identityUserService = identityUserService;
        _invoicingInvoiceBatchCalculationService = invoicingInvoiceBatchCalculationService;
        _invoker = invoker;
        _itsmIncidentApprovalService = itsmIncidentApprovalService;
        _itsmServiceAssetWorkflowService = itsmServiceAssetWorkflowService;
        _logger = logger;
        _metricsMetricSeriesService = metricsMetricSeriesService;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _searchSearchIndexService = searchSearchIndexService;
        _settings = settings;
        _workplacesVisitorPassCalculationService = workplacesVisitorPassCalculationService;
        _sendApproveBuilding2 = _metricsMetricSeriesService.ListMetricSeriesesAsync;
    }
    /// <summary>
    /// Approves a building.
    /// </summary>
    public override async Task<ApiWorkplaces.ApproveBuildingResponse> ApproveBuilding(ApiWorkplaces.ApproveBuildingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ApproveBuilding for {Tenant}", requestContext.TenantId);
        if (request.Building is { } record)
        {
            var snapshot = BuildingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiSearch.SearchSearchIndexesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiSearch.SearchSearchIndexesRequest.Types.Filter
            {
                Region = (request.Building?.Location?.Region ?? ""),
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _searchSearchIndexService.SearchSearchIndexesAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiInvoicing.CompareInvoiceBatchOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiInvoicing.CompareInvoiceBatchOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Building?.Location?.PostalCode ?? ""),
                },
            },
            InvoiceBatch = new ApiInvoicing.InvoiceBatch
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Building?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiCustomers.CustomerRef
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _invoicingInvoiceBatchCalculationService.CompareInvoiceBatchOptionsAsync);
        await _bus.PublishAsync(new TransportNotification<ApiInvoicing.CompareInvoiceBatchOptionsRequest>("acme.workplaces.v1.BuildingApprovalService/ApproveBuilding/1", input1.Clone()), token);
        var input2 = new ApiMetrics.ListMetricSeriesesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiMetrics.ListMetricSeriesesRequest.Types.Filter
            {
                CreatedAfter = (request.Building?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Details?.Approver?.UserId ?? ""),
                Region = (reply1.InvoiceBatch?.BillingAddress?.Region ?? ""),
            },
        };
        using var rpc2 = _sendApproveBuilding2(input2, new CallOptions(cancellationToken: token));
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiWorkplaces.ApproveBuildingResponse
        {
            Building = new ApiWorkplaces.Building
            {
                BuildingId = (request.BuildingId ?? ""),
                Depth = (request.Building?.Depth ?? 0),
                Location = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply1.InvoiceBatch?.BillingAddress?.PostalCode ?? ""),
                },
            },
            Outcome = new ApiWorkplaces.ApproveBuildingResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "ApproveBuilding", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments a building.
    /// </summary>
    public override async Task<ApiWorkplaces.CommentBuildingApprovalResponse> CommentBuildingApproval(ApiWorkplaces.CommentBuildingApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CommentBuildingApproval for {Tenant}", requestContext.TenantId);
        if (request.Building is { } record)
        {
            var snapshot = BuildingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.AdvanceRegistrationWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.AdvanceRegistrationWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
            Registration = new ApiCorporateevents.Registration
            {
                Holder = new ApiCorporateevents.CorporateEventRef
                {
                    DisplayName = (request.Building?.Manager?.DisplayName ?? ""),
                },
            },
        };
        var named0 = _registry.Get<ApiCorporateevents.RegistrationWorkflowService.RegistrationWorkflowServiceClient>("acme.corporateevents.v1.RegistrationWorkflowService");
        using var rpc0 = named0.AdvanceRegistrationWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiWorkplaces.CommentBuildingApprovalResponse
        {
            Building = new ApiWorkplaces.Building
            {
                BuildingId = (request.BuildingId ?? ""),
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (reply0.Registration?.Holder?.DisplayName ?? ""),
                        },
                    } },
                } },
            },
            Outcome = new ApiWorkplaces.CommentBuildingApprovalResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "CommentBuildingApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates a building.
    /// </summary>
    public override async Task<ApiWorkplaces.DelegateBuildingApprovalResponse> DelegateBuildingApproval(ApiWorkplaces.DelegateBuildingApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling DelegateBuildingApproval for {Tenant}", requestContext.TenantId);
        if (request.Building is { } record)
        {
            var snapshot = BuildingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.ListPendingIncidentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.ListPendingIncidentsRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
                Labels = { (request.Building?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Building?.Manager?.UserId ?? ""),
                Region = (request.Building?.Location?.Region ?? ""),
            },
        };
        var descriptor0 = new Method<ApiItsm.ListPendingIncidentsRequest, ApiItsm.ListPendingIncidentsResponse>(
            MethodType.Unary, "acme.itsm.v1" + ".IncidentApprovalService", "ListPendingIncidents",
            Wire.Marshaller<ApiItsm.ListPendingIncidentsRequest>(), Wire.Marshaller<ApiItsm.ListPendingIncidentsResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.RecalculateEmploymentContractsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Building?.Audit?.CreatedBy ?? ""),
                    Etag = (request.Building?.Audit?.Etag ?? ""),
                    UpdatedAt = (request.Building?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Building?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
        };
        var command1 = new DispatchCommand<ApiEmployees.RecalculateEmploymentContractsRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _employeesEmploymentContractCalculationService.RecalculateEmploymentContractsAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.DelegateBuildingApprovalResponse
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
                            Region = (reply1.EmploymentContract?.BillingAddress?.Region ?? ""),
                        },
                    } },
                } },
                Name = (request.Building?.Name ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "DelegateBuildingApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a building.
    /// </summary>
    public override async Task<ApiWorkplaces.GetBuildingApprovalStatusResponse> GetBuildingApprovalStatus(ApiWorkplaces.GetBuildingApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.BuildingId, "building_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetBuildingApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "building", request.BuildingId,
            "GetBuildingApprovalStatus", "accepted", token);
        var profile0 = new MappingProfile<ApiWorkplaces.GetBuildingApprovalStatusRequest, ApiMetrics.CloneMetricSeriesRequest>(value => new ApiMetrics.CloneMetricSeriesRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            MetricSeries = new ApiMetrics.MetricSeries
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (value.Context?.Deadline ?? new Timestamp()),
                },
                Components = { { "primary", new ApiMetrics.MetricSeriesComponent
                {
                    ComponentId = (value.BuildingId ?? ""),
                } } },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (value.Context?.Actor?.Email ?? ""),
                    UserId = (value.Context?.Actor?.UserId ?? ""),
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _metricsMetricSeriesService.CloneMetricSeriesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.CreateEmployeeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Employee = new ApiEmployees.Employee
            {
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.MetricSeries ?? new ApiMetrics.MetricSeries()).LocatorCase switch
                    {
                        ApiMetrics.MetricSeries.LocatorOneofCase.Address => (reply0.MetricSeries?.Address?.CountryCode ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _employeesEmployeeService.CreateEmployeeAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.GetBuildingApprovalStatusResponse
        {
            Building = new ApiWorkplaces.Building
            {
                BuildingId = (request.BuildingId ?? ""),
                Children = { new ApiWorkplaces.Building
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedBy = (reply1.Employee?.Audit?.CreatedBy ?? ""),
                    },
                    Children = { new ApiWorkplaces.Building
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            CreatedBy = (reply1.Employee?.Audit?.CreatedBy ?? ""),
                        },
                        Children = { new ApiWorkplaces.Building
                        {
                            Name = (reply0.MetricSeries?.Name ?? ""),
                        } },
                        Location = new ApiCommon.PostalAddress
                        {
                            Recipient = (reply0.MetricSeries?.Address?.Recipient ?? ""),
                        },
                    } },
                } },
            },
        };
        return response;
    }
    /// <summary>
    /// Lists buildings a page at a time.
    /// </summary>
    public override async Task<ApiWorkplaces.ListPendingBuildingsResponse> ListPendingBuildings(ApiWorkplaces.ListPendingBuildingsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListPendingBuildings for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "building", "batch",
            "ListPendingBuildings", "accepted", token);
        var response = new ApiWorkplaces.ListPendingBuildingsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "ListPendingBuildings", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits a building.
    /// </summary>
    public override async Task<ApiWorkplaces.SubmitBuildingResponse> SubmitBuilding(ApiWorkplaces.SubmitBuildingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SubmitBuilding for {Tenant}", requestContext.TenantId);
        if (request.Building is { } record)
        {
            var snapshot = BuildingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.SubmitBuildingToCalculateVisitorPass0();
        using var rpc0 = _workplacesVisitorPassCalculationService.CalculateVisitorPassAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiIdentity.GetUserRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            UserId = (request.Details?.Approver?.UserId ?? ""),
        };
        using var rpc1 = _identityUserService.GetUserAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        var input2 = new ApiItsm.StartServiceAssetWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Building?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (reply0.VisitorPass?.Audit?.CreatedBy ?? ""),
                },
            },
        };
        using var rpc2 = _itsmServiceAssetWorkflowService.StartServiceAssetWorkflowAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        await Task.WhenAll(pending1, pending2);
        var reply1 = await pending1;
        var reply2 = await pending2;
        var response = new ApiWorkplaces.SubmitBuildingResponse
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
                            Etag = (reply1.User?.Audit?.Etag ?? ""),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            PostalCode = (reply2.ServiceAsset?.Address?.PostalCode ?? ""),
                        },
                    } },
                } },
                Name = (request.Building?.Name ?? ""),
            },
            Outcome = new ApiWorkplaces.SubmitBuildingResponse.Types.Outcome
            {
                Message = (reply2.Outcome?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "SubmitBuilding", (request.Reason ?? ""), token);
        return response;
    }
}
