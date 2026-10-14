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
using ApiCustomers = Acme.Customers.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiInvoicing = Acme.Invoicing.V1;
using ApiLoyalty = Acme.Loyalty.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

/// <summary>
/// Runs the long-lived workflow that takes a desk from start to finish.
/// </summary>
public sealed class DeskWorkflowEndpoint : ApiWorkplaces.DeskWorkflowService.DeskWorkflowServiceBase
{
    private readonly IDeskWorkflowEndpointSendAdvanceDeskWorkflow0Port _deskWorkflowEndpointSendAdvanceDeskWorkflow0Port;
    private readonly IDeskWorkflowEndpointSendSignalDeskWorkflow2Port _deskWorkflowEndpointSendSignalDeskWorkflow2Port;
    private readonly ApiDocuments.DocumentSyncService.DocumentSyncServiceClient _documentsDocumentSyncService;
    private readonly ApiDocuments.DocumentTemplateExportService.DocumentTemplateExportServiceClient _documentsDocumentTemplateExportService;
    private readonly ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceClient _documentsDocumentTemplateWorkflowService;
    private readonly ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient _employeesEmployeeQueryService;
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _employeesEmployeeService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient _featureflagsFeatureFlagValidationService;
    private readonly ApiInvoicing.InvoiceBatchCalculationService.InvoiceBatchCalculationServiceClient _invoicingInvoiceBatchCalculationService;
    private readonly CallInvoker _invoker;
    private readonly ILogger<DeskWorkflowEndpoint> _logger;
    private readonly ApiLoyalty.LoyaltyMemberQueryService.LoyaltyMemberQueryServiceClient _loyaltyLoyaltyMemberQueryService;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;
    private readonly ApiTravel.TravelPolicyReportingService.TravelPolicyReportingServiceClient _travelTravelPolicyReportingService;
    private readonly ApiTravel.TripWorkflowService.TripWorkflowServiceClient _travelTripWorkflowService;
    private readonly ApiWorkplaces.BuildingWorkflowService.BuildingWorkflowServiceClient _workplacesBuildingWorkflowService;
    private readonly ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient _workplacesDeskCalculationService;
    private readonly ApiWorkplaces.DeskQueryService.DeskQueryServiceClient _workplacesDeskQueryService;
    private readonly ApiWorkplaces.RoomBookingReportingService.RoomBookingReportingServiceClient _workplacesRoomBookingReportingService;
    private readonly ApiWorkplaces.VisitorPassLifecycleService.VisitorPassLifecycleServiceClient _workplacesVisitorPassLifecycleService;
    private readonly ApiWorkplaces.VisitorPassSyncService.VisitorPassSyncServiceClient _workplacesVisitorPassSyncService;
    private readonly ApiWorkplaces.VisitorPassValidationService.VisitorPassValidationServiceClient _workplacesVisitorPassValidationService;

    public DeskWorkflowEndpoint(
        IDeskWorkflowEndpointSendAdvanceDeskWorkflow0Port deskWorkflowEndpointSendAdvanceDeskWorkflow0PortAdapter,
        IDeskWorkflowEndpointSendSignalDeskWorkflow2Port deskWorkflowEndpointSendSignalDeskWorkflow2PortAdapter,
        ApiDocuments.DocumentSyncService.DocumentSyncServiceClient documentsDocumentSyncService,
        ApiDocuments.DocumentTemplateExportService.DocumentTemplateExportServiceClient documentsDocumentTemplateExportService,
        ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceClient documentsDocumentTemplateWorkflowService,
        ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient employeesEmployeeQueryService,
        ApiEmployees.EmployeeService.EmployeeServiceClient employeesEmployeeService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient featureflagsFeatureFlagValidationService,
        ApiInvoicing.InvoiceBatchCalculationService.InvoiceBatchCalculationServiceClient invoicingInvoiceBatchCalculationService,
        CallInvoker invoker,
        ILogger<DeskWorkflowEndpoint> logger,
        ApiLoyalty.LoyaltyMemberQueryService.LoyaltyMemberQueryServiceClient loyaltyLoyaltyMemberQueryService,
        RetryPolicy policy,
        IRecordRepository repository,
        ApiTravel.TravelPolicyReportingService.TravelPolicyReportingServiceClient travelTravelPolicyReportingService,
        ApiTravel.TripWorkflowService.TripWorkflowServiceClient travelTripWorkflowService,
        ApiWorkplaces.BuildingWorkflowService.BuildingWorkflowServiceClient workplacesBuildingWorkflowService,
        ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient workplacesDeskCalculationService,
        ApiWorkplaces.DeskQueryService.DeskQueryServiceClient workplacesDeskQueryService,
        ApiWorkplaces.RoomBookingReportingService.RoomBookingReportingServiceClient workplacesRoomBookingReportingService,
        ApiWorkplaces.VisitorPassLifecycleService.VisitorPassLifecycleServiceClient workplacesVisitorPassLifecycleService,
        ApiWorkplaces.VisitorPassSyncService.VisitorPassSyncServiceClient workplacesVisitorPassSyncService,
        ApiWorkplaces.VisitorPassValidationService.VisitorPassValidationServiceClient workplacesVisitorPassValidationService)
    {
        _deskWorkflowEndpointSendAdvanceDeskWorkflow0Port = deskWorkflowEndpointSendAdvanceDeskWorkflow0PortAdapter;
        _deskWorkflowEndpointSendSignalDeskWorkflow2Port = deskWorkflowEndpointSendSignalDeskWorkflow2PortAdapter;
        _documentsDocumentSyncService = documentsDocumentSyncService;
        _documentsDocumentTemplateExportService = documentsDocumentTemplateExportService;
        _documentsDocumentTemplateWorkflowService = documentsDocumentTemplateWorkflowService;
        _employeesEmployeeQueryService = employeesEmployeeQueryService;
        _employeesEmployeeService = employeesEmployeeService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _featureflagsFeatureFlagValidationService = featureflagsFeatureFlagValidationService;
        _invoicingInvoiceBatchCalculationService = invoicingInvoiceBatchCalculationService;
        _invoker = invoker;
        _logger = logger;
        _loyaltyLoyaltyMemberQueryService = loyaltyLoyaltyMemberQueryService;
        _policy = policy;
        _repository = repository;
        _travelTravelPolicyReportingService = travelTravelPolicyReportingService;
        _travelTripWorkflowService = travelTripWorkflowService;
        _workplacesBuildingWorkflowService = workplacesBuildingWorkflowService;
        _workplacesDeskCalculationService = workplacesDeskCalculationService;
        _workplacesDeskQueryService = workplacesDeskQueryService;
        _workplacesRoomBookingReportingService = workplacesRoomBookingReportingService;
        _workplacesVisitorPassLifecycleService = workplacesVisitorPassLifecycleService;
        _workplacesVisitorPassSyncService = workplacesVisitorPassSyncService;
        _workplacesVisitorPassValidationService = workplacesVisitorPassValidationService;
    }
    /// <summary>
    /// Advances a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.AdvanceDeskWorkflowResponse> AdvanceDeskWorkflow(ApiWorkplaces.AdvanceDeskWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling AdvanceDeskWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Desk is { } record)
        {
            var snapshot = DeskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.CheckPositionEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.CheckPositionEligibilityRequest.Types.Details
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
                    CreatedBy = (request.Desk?.Audit?.CreatedBy ?? ""),
                },
            },
        };
        var reply0 = await SendAdvanceDeskWorkflow0Async(input0, token);
        var input1 = new ApiWorkplaces.CountRoomBookingTotalsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiWorkplaces.CountRoomBookingTotalsRequest.Types.Filter
            {
                CreatedAfter = (request.Desk?.Audit?.CreatedAt ?? new Timestamp()),
                Region = (request.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                {
                    ApiWorkplaces.Desk.LocatorOneofCase.Address => (request.Desk?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc1 = _workplacesRoomBookingReportingService.CountRoomBookingTotalsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.AdvanceDeskWorkflowResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                Code = (request.Desk?.Code ?? ""),
                DeskId = (request.DeskId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.Position?.Owner?.Email ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "desk",
            "audit", "AdvanceDeskWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.CancelDeskWorkflowResponse> CancelDeskWorkflow(ApiWorkplaces.CancelDeskWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CancelDeskWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Desk is { } record)
        {
            var snapshot = DeskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWorkplaces.AdvanceBuildingWorkflowRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Location = new ApiCommon.PostalAddress
                        {
                            PostalCode = (request.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                            {
                                ApiWorkplaces.Desk.LocatorOneofCase.Address => (request.Desk?.Address?.PostalCode ?? ""),
                                _ => ""
                            },
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _workplacesBuildingWorkflowService.AdvanceBuildingWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.PullDocumentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.PullDocumentsRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Document = new ApiDocuments.Document
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Building?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        using var rpc1 = _documentsDocumentSyncService.PullDocumentsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.CancelDeskWorkflowResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                DeskId = (request.DeskId ?? ""),
                Name = (request.Desk?.Name ?? ""),
            },
            Outcome = new ApiWorkplaces.CancelDeskWorkflowResponse.Types.Outcome
            {
                Message = (reply1.Outcome?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "desk",
            "audit", "CancelDeskWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.InspectDeskWorkflowResponse> InspectDeskWorkflow(ApiWorkplaces.InspectDeskWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DeskId, "desk_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling InspectDeskWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "desk", request.DeskId,
            "InspectDeskWorkflow", "accepted", token);
        var input0 = new ApiTravel.ListTravelPolicyReportsRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Filter = new ApiTravel.ListTravelPolicyReportsRequest.Types.Filter
        {
            OwnerId = (request.DeskId ?? ""),
        };
        using var rpc0 = _travelTravelPolicyReportingService.ListTravelPolicyReportsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.UpdateEmployeeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Employee = new ApiEmployees.Employee
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        using var rpc1 = _employeesEmployeeService.UpdateEmployeeAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.InspectDeskWorkflowResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.Employee?.Audit?.UpdatedBy ?? ""),
                },
                DeskId = (request.DeskId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (reply1.Employee?.RelationshipManager?.UserId ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Lists desks a page at a time.
    /// </summary>
    public override async Task<ApiWorkplaces.ListDeskWorkflowsResponse> ListDeskWorkflows(ApiWorkplaces.ListDeskWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListDeskWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "desk", "batch",
            "ListDeskWorkflows", "accepted", token);
        var input0 = new ApiInvoicing.RecalculateInvoiceBatchesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            InvoiceBatch = new ApiInvoicing.InvoiceBatch
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Counterparty = new ApiCustomers.CustomerRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                IssuedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _invoicingInvoiceBatchCalculationService.RecalculateInvoiceBatchesAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiWorkplaces.ListDeskWorkflowsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "desk",
            "audit", "ListDeskWorkflows", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pauses a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.PauseDeskWorkflowResponse> PauseDeskWorkflow(ApiWorkplaces.PauseDeskWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PauseDeskWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Desk is { } record)
        {
            var snapshot = DeskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWorkplaces.SummarizeDesksRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DeskId = (request.DeskId ?? ""),
            Details = new ApiWorkplaces.SummarizeDesksRequest.Types.Details
            {
                Filters = { (request.Desk?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Reason = (request.Reason ?? ""),
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _workplacesDeskQueryService.SummarizeDesksAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiEmployees.CountEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.CountEmployeesRequest.Types.Filter
            {
                CreatedAfter = (request.Desk?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (request.Details?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Desk?.Components?.GetValueOrDefault("primary")?.ComponentId ?? ""),
                Region = (request.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                {
                    ApiWorkplaces.Desk.LocatorOneofCase.Address => (request.Desk?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc1 = _employeesEmployeeQueryService.CountEmployeesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiWorkplaces.SyncVisitorPassesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiWorkplaces.SyncVisitorPassesRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                    {
                        ApiWorkplaces.Desk.LocatorOneofCase.Address => (request.Desk?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()),
                        _ => new Google.Protobuf.Collections.RepeatedField<string>()
                    } },
                    Recipient = (request.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                    {
                        ApiWorkplaces.Desk.LocatorOneofCase.Address => (request.Desk?.Address?.Recipient ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc2 = _workplacesVisitorPassSyncService.SyncVisitorPassesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiTravel.ShareTravelPolicyReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.ShareTravelPolicyReportRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc3 = _travelTravelPolicyReportingService.ShareTravelPolicyReportAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiWorkplaces.PauseDeskWorkflowResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                DeskId = (request.DeskId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply3.TravelPolicy?.Owner?.Email ?? ""),
                },
                Specs = { (request.Desk?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply2.Outcome?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Step ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "desk",
            "audit", "PauseDeskWorkflow", (request.Details?.Step ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resumes a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.ResumeDeskWorkflowResponse> ResumeDeskWorkflow(ApiWorkplaces.ResumeDeskWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ResumeDeskWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Desk is { } record)
        {
            var snapshot = DeskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.ScheduleDocumentTemplateExportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ScheduleDocumentTemplateExportRequest.Types.Details
            {
                Filters = { (request.Desk?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Reason = (request.Reason ?? ""),
        };
        var descriptor0 = new Method<ApiDocuments.ScheduleDocumentTemplateExportRequest, ApiDocuments.ScheduleDocumentTemplateExportResponse>(
            MethodType.Unary, "acme.documents.v1" + ".DocumentTemplateExportService", "ScheduleDocumentTemplateExport",
            Wire.Marshaller<ApiDocuments.ScheduleDocumentTemplateExportRequest>(), Wire.Marshaller<ApiDocuments.ScheduleDocumentTemplateExportResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLoyalty.ExportLoyaltyMembersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLoyalty.ExportLoyaltyMembersRequest.Types.Filter
            {
                CreatedAfter = (request.Desk?.Audit?.UpdatedAt ?? new Timestamp()),
                Region = (request.Desk?.Address?.Region ?? ""),
            },
        };
        using var stream1 = _loyaltyLoyaltyMemberQueryService.ExportLoyaltyMembers(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportLoyaltyMembers frame {Type}", change1.GetType().Name);
        }
        var response = new ApiWorkplaces.ResumeDeskWorkflowResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                DeskId = (request.DeskId ?? ""),
                Specs = { (request.Desk?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Step ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "desk",
            "audit", "ResumeDeskWorkflow", (request.Details?.Step ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.RetryDeskWorkflowResponse> RetryDeskWorkflow(ApiWorkplaces.RetryDeskWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RetryDeskWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Desk is { } record)
        {
            var snapshot = DeskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWorkplaces.ScheduleVisitorPassExpiryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiWorkplaces.ScheduleVisitorPassExpiryRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Desk?.Audit?.CreatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                    {
                        ApiWorkplaces.Desk.LocatorOneofCase.Address => (request.Desk?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()),
                        _ => new Google.Protobuf.Collections.RepeatedField<string>()
                    } },
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _workplacesVisitorPassLifecycleService.ScheduleVisitorPassExpiryAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiWorkplaces.RetryDeskWorkflowResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.VisitorPass?.Audit?.UpdatedBy ?? ""),
                },
                DeskId = (request.DeskId ?? ""),
                Specs = { (request.Desk?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Outcome = new ApiWorkplaces.RetryDeskWorkflowResponse.Types.Outcome
            {
                EffectiveAt = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "desk",
            "audit", "RetryDeskWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.SignalDeskWorkflowResponse> SignalDeskWorkflow(ApiWorkplaces.SignalDeskWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SignalDeskWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Desk is { } record)
        {
            var snapshot = DeskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.PauseTripWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Trip = new ApiTravel.Trip
            {
                Lines = { new ApiTravel.TripLine
                {
                    Quantity = (request.Desk?.Components?.GetValueOrDefault("primary")?.Quantity ?? 0L),
                } },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _travelTripWorkflowService.PauseTripWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.SignalDeskWorkflowToPreviewVisitorPass1(reply0);
        using var rpc1 = _workplacesVisitorPassValidationService.PreviewVisitorPassAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiWorkplaces.GetDeskRatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DeskId = (request.DeskId ?? ""),
        };
        var reply2 = await SendSignalDeskWorkflow2Async(input2, token);
        var response = new ApiWorkplaces.SignalDeskWorkflowResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Trip?.Audit?.CreatedBy ?? ""),
                },
                DeskId = (request.DeskId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (reply2.Desk?.Owner?.UserId ?? ""),
                },
                Specs = { (request.Desk?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "desk",
            "audit", "SignalDeskWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.StartDeskWorkflowResponse> StartDeskWorkflow(ApiWorkplaces.StartDeskWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling StartDeskWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Desk is { } record)
        {
            var snapshot = DeskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiFeatureflags.NormalizeFeatureFlagRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            FeatureFlag = new ApiFeatureflags.FeatureFlag
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Desk?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedBy = (request.Desk?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _featureflagsFeatureFlagValidationService.NormalizeFeatureFlagAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.ResumeDocumentTemplateWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Desk?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Lines = { new ApiDocuments.DocumentTemplateLine
                {
                    Tax = new ApiDocuments.DocumentTemplateLineTax
                    {
                        Amount = (reply0.FeatureFlag ?? new ApiFeatureflags.FeatureFlag()).ActionCase switch
                        {
                            ApiFeatureflags.FeatureFlag.ActionOneofCase.Discount => (reply0.FeatureFlag?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc1 = _documentsDocumentTemplateWorkflowService.ResumeDocumentTemplateWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.StartDeskWorkflowResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                DeskId = (request.DeskId ?? ""),
                Specs = { (request.Desk?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Outcome = new ApiWorkplaces.StartDeskWorkflowResponse.Types.Outcome
            {
                Status = (reply1.Outcome?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "desk",
            "audit", "StartDeskWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiEmployees.CheckPositionEligibilityResponse> SendAdvanceDeskWorkflow0Async(ApiEmployees.CheckPositionEligibilityRequest request, CancellationToken token)
    {
        return await _deskWorkflowEndpointSendAdvanceDeskWorkflow0Port.SendAsync(request, token);
    }
    private async Task<ApiWorkplaces.GetDeskRatesResponse> SendSignalDeskWorkflow2Async(ApiWorkplaces.GetDeskRatesRequest request, CancellationToken token)
    {
        return await _deskWorkflowEndpointSendSignalDeskWorkflow2Port.SendAsync(request, token);
    }
}
