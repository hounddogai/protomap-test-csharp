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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Corporateevents;

/// <summary>
/// Runs the long-lived workflow that takes a registration from start to finish.
/// </summary>
public sealed class RegistrationWorkflowEndpoint : ApiCorporateevents.RegistrationWorkflowService.RegistrationWorkflowServiceBase
{
    private readonly ApiCorporateevents.CorporateEventApprovalService.CorporateEventApprovalServiceClient _corporateeventsCorporateEventApprovalService;
    private readonly ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient _corporateeventsCorporateEventCalculationService;
    private readonly ApiCorporateevents.CorporateEventReportingService.CorporateEventReportingServiceClient _corporateeventsCorporateEventReportingService;
    private readonly ApiCorporateevents.RegistrationApprovalService.RegistrationApprovalServiceClient _corporateeventsRegistrationApprovalService;
    private readonly ApiCorporateevents.RegistrationLifecycleService.RegistrationLifecycleServiceClient _corporateeventsRegistrationLifecycleService;
    private readonly ApiCorporateevents.RegistrationReportingService.RegistrationReportingServiceClient _corporateeventsRegistrationReportingService;
    private readonly ApiCorporateevents.RegistrationValidationService.RegistrationValidationServiceClient _corporateeventsRegistrationValidationService;
    private readonly ApiCorporateevents.VenueApprovalService.VenueApprovalServiceClient _corporateeventsVenueApprovalService;
    private readonly ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceClient _devicesDevicePolicyWorkflowService;
    private readonly ApiDocuments.DocumentHistoryService.DocumentHistoryServiceClient _documentsDocumentHistoryService;
    private readonly ApiHelpdesk.TicketWorkflowService.TicketWorkflowServiceClient _helpdeskTicketWorkflowService;
    private readonly ApiIdentity.UserQueryService.UserQueryServiceClient _identityUserQueryService;
    private readonly CallInvoker _invoker;
    private readonly ApiLicensing.LicenseAssignmentWorkflowService.LicenseAssignmentWorkflowServiceClient _licensingLicenseAssignmentWorkflowService;
    private readonly ILogger<RegistrationWorkflowEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly IRegistrationWorkflowEndpointSendSignalRegistrationWorkflow0Port _registrationWorkflowEndpointSendSignalRegistrationWorkflow0Port;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiDevices.ResumeDevicePolicyWorkflowRequest, CallOptions, AsyncUnaryCall<ApiDevices.ResumeDevicePolicyWorkflowResponse>> _sendListRegistrationWorkflows0;

    public RegistrationWorkflowEndpoint(
        ApiCorporateevents.CorporateEventApprovalService.CorporateEventApprovalServiceClient corporateeventsCorporateEventApprovalService,
        ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient corporateeventsCorporateEventCalculationService,
        ApiCorporateevents.CorporateEventReportingService.CorporateEventReportingServiceClient corporateeventsCorporateEventReportingService,
        ApiCorporateevents.RegistrationApprovalService.RegistrationApprovalServiceClient corporateeventsRegistrationApprovalService,
        ApiCorporateevents.RegistrationLifecycleService.RegistrationLifecycleServiceClient corporateeventsRegistrationLifecycleService,
        ApiCorporateevents.RegistrationReportingService.RegistrationReportingServiceClient corporateeventsRegistrationReportingService,
        ApiCorporateevents.RegistrationValidationService.RegistrationValidationServiceClient corporateeventsRegistrationValidationService,
        ApiCorporateevents.VenueApprovalService.VenueApprovalServiceClient corporateeventsVenueApprovalService,
        ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceClient devicesDevicePolicyWorkflowService,
        ApiDocuments.DocumentHistoryService.DocumentHistoryServiceClient documentsDocumentHistoryService,
        ApiHelpdesk.TicketWorkflowService.TicketWorkflowServiceClient helpdeskTicketWorkflowService,
        ApiIdentity.UserQueryService.UserQueryServiceClient identityUserQueryService,
        CallInvoker invoker,
        ApiLicensing.LicenseAssignmentWorkflowService.LicenseAssignmentWorkflowServiceClient licensingLicenseAssignmentWorkflowService,
        ILogger<RegistrationWorkflowEndpoint> logger,
        RetryPolicy policy,
        IRegistrationWorkflowEndpointSendSignalRegistrationWorkflow0Port registrationWorkflowEndpointSendSignalRegistrationWorkflow0PortAdapter,
        ClientRegistry registry,
        IRecordRepository repository)
    {
        _corporateeventsCorporateEventApprovalService = corporateeventsCorporateEventApprovalService;
        _corporateeventsCorporateEventCalculationService = corporateeventsCorporateEventCalculationService;
        _corporateeventsCorporateEventReportingService = corporateeventsCorporateEventReportingService;
        _corporateeventsRegistrationApprovalService = corporateeventsRegistrationApprovalService;
        _corporateeventsRegistrationLifecycleService = corporateeventsRegistrationLifecycleService;
        _corporateeventsRegistrationReportingService = corporateeventsRegistrationReportingService;
        _corporateeventsRegistrationValidationService = corporateeventsRegistrationValidationService;
        _corporateeventsVenueApprovalService = corporateeventsVenueApprovalService;
        _devicesDevicePolicyWorkflowService = devicesDevicePolicyWorkflowService;
        _documentsDocumentHistoryService = documentsDocumentHistoryService;
        _helpdeskTicketWorkflowService = helpdeskTicketWorkflowService;
        _identityUserQueryService = identityUserQueryService;
        _invoker = invoker;
        _licensingLicenseAssignmentWorkflowService = licensingLicenseAssignmentWorkflowService;
        _logger = logger;
        _policy = policy;
        _registrationWorkflowEndpointSendSignalRegistrationWorkflow0Port = registrationWorkflowEndpointSendSignalRegistrationWorkflow0PortAdapter;
        _registry = registry;
        _repository = repository;
        _sendListRegistrationWorkflows0 = _devicesDevicePolicyWorkflowService.ResumeDevicePolicyWorkflowAsync;
    }
    /// <summary>
    /// Advances a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.AdvanceRegistrationWorkflowResponse> AdvanceRegistrationWorkflow(ApiCorporateevents.AdvanceRegistrationWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling AdvanceRegistrationWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Registration is { } record)
        {
            var snapshot = RegistrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.EscalateVenueRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Details = new ApiCorporateevents.EscalateVenueRequest.Types.Details
        {
            Approver = new ApiIdentity.UserRef
            {
                Email = (request.Details?.Notify?.Email ?? ""),
            },
        };
        input0.Venue = new ApiCorporateevents.Venue
        {
            Audit = new ApiCommon.AuditInfo
            {
                Etag = (request.Registration?.Audit?.Etag ?? ""),
                UpdatedAt = (request.Registration?.Audit?.UpdatedAt ?? new Timestamp()),
            },
            Owner = new ApiIdentity.UserRef
            {
                DisplayName = (request.Registration?.Holder?.DisplayName ?? ""),
            },
        };
        using var rpc0 = _corporateeventsVenueApprovalService.EscalateVenueAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiCorporateevents.AdvanceRegistrationWorkflowResponse
        {
            Registration = new ApiCorporateevents.Registration
            {
                Balance = (request.Registration?.Balance ?? new ApiCommon.Money()),
                Holder = new ApiCorporateevents.CorporateEventRef
                {
                    DisplayName = (reply0.Venue?.Owner?.DisplayName ?? ""),
                },
                RegistrationId = (request.RegistrationId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Registration?.RegistrationId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "AdvanceRegistrationWorkflow", (request.Registration?.RegistrationId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.CancelRegistrationWorkflowResponse> CancelRegistrationWorkflow(ApiCorporateevents.CancelRegistrationWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CancelRegistrationWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Registration is { } record)
        {
            var snapshot = RegistrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.DeleteCorporateEventReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEventId = (request.Registration?.Holder?.CorporateEventId ?? ""),
            Etag = (request.Registration?.Audit?.Etag ?? ""),
            Reason = (request.Reason ?? ""),
        };
        using var rpc0 = _corporateeventsCorporateEventReportingService.DeleteCorporateEventReportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.NormalizeRegistrationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.NormalizeRegistrationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Registration?.Contact?.Phone ?? ""),
                },
            },
            Registration = new ApiCorporateevents.Registration
            {
                Bank = new ApiCorporateevents.RegistrationBankFunding
                {
                    Bic = (request.Registration ?? new ApiCorporateevents.Registration()).PaymentCase switch
                    {
                        ApiCorporateevents.Registration.FundingOneofCase.Bank => (request.Registration?.Bank?.Bic ?? ""),
                        _ => ""
                    },
                },
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Registration?.Contact?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc1 = _corporateeventsRegistrationValidationService.NormalizeRegistrationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.CancelRegistrationWorkflowResponse
        {
            Registration = new ApiCorporateevents.Registration
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply1.Registration?.Contact?.Email ?? ""),
                },
                CurrencyCode = (request.Registration?.CurrencyCode ?? ""),
                Labels = { (reply1.Registration?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                RegistrationId = (request.RegistrationId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Registration?.RegistrationId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "CancelRegistrationWorkflow", (request.Registration?.RegistrationId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.InspectRegistrationWorkflowResponse> InspectRegistrationWorkflow(ApiCorporateevents.InspectRegistrationWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.RegistrationId, "registration_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling InspectRegistrationWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "registration", request.RegistrationId,
            "InspectRegistrationWorkflow", "accepted", token);
        var input0 = new ApiCorporateevents.ScheduleRegistrationExpiryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Registration = new ApiCorporateevents.Registration
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            RegistrationId = (request.RegistrationId ?? ""),
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _corporateeventsRegistrationLifecycleService.ScheduleRegistrationExpiryAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiCorporateevents.GenerateCorporateEventReportRequest
        {
            Annotations = { (reply0.Registration?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEventId = (reply0.Registration?.Holder?.CorporateEventId ?? ""),
            Details = new ApiCorporateevents.GenerateCorporateEventReportRequest.Types.Details
            {
                Filters = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _corporateeventsCorporateEventReportingService.GenerateCorporateEventReportAsync);
        var response = new ApiCorporateevents.InspectRegistrationWorkflowResponse
        {
            Registration = new ApiCorporateevents.Registration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Registration?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Available = (reply0.Registration?.Available ?? new ApiCommon.Money()),
                RegistrationId = (request.RegistrationId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists registrations a page at a time.
    /// </summary>
    public override async Task<ApiCorporateevents.ListRegistrationWorkflowsResponse> ListRegistrationWorkflows(ApiCorporateevents.ListRegistrationWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListRegistrationWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "registration", "batch",
            "ListRegistrationWorkflows", "accepted", token);
        var input0 = new ApiDevices.ResumeDevicePolicyWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.ResumeDevicePolicyWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        using var rpc0 = _sendListRegistrationWorkflows0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiCorporateevents.ListRegistrationWorkflowsResponse
        {
            Registrations = { new ApiCorporateevents.Registration
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.DevicePolicy?.Owner?.Email ?? ""),
                },
                Limits = { new ApiCorporateevents.RegistrationLimit
                {
                    Amount = (reply0.DevicePolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                } },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "ListRegistrationWorkflows", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pauses a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.PauseRegistrationWorkflowResponse> PauseRegistrationWorkflow(ApiCorporateevents.PauseRegistrationWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PauseRegistrationWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Registration is { } record)
        {
            var snapshot = RegistrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.GenerateRegistrationReportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            RegistrationId = (request.Registration?.RegistrationId ?? ""),
        };
        var named0 = _registry.Get<ApiCorporateevents.RegistrationReportingService.RegistrationReportingServiceClient>("acme.corporateevents.v1.RegistrationReportingService");
        using var rpc0 = named0.GenerateRegistrationReportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.EscalateVenueRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Venue = new ApiCorporateevents.Venue
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Registration?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var descriptor1 = new Method<ApiCorporateevents.EscalateVenueRequest, ApiCorporateevents.EscalateVenueResponse>(
            MethodType.Unary, "acme.corporateevents.v1" + ".VenueApprovalService", "EscalateVenue",
            Wire.Marshaller<ApiCorporateevents.EscalateVenueRequest>(), Wire.Marshaller<ApiCorporateevents.EscalateVenueResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.PauseRegistrationWorkflowResponse
        {
            Registration = new ApiCorporateevents.Registration
            {
                Available = (request.Registration?.Available ?? new ApiCommon.Money()),
                RegistrationId = (request.RegistrationId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Registration?.Holder?.DisplayName ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "PauseRegistrationWorkflow", (request.Registration?.Holder?.DisplayName ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resumes a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.ResumeRegistrationWorkflowResponse> ResumeRegistrationWorkflow(ApiCorporateevents.ResumeRegistrationWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ResumeRegistrationWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Registration is { } record)
        {
            var snapshot = RegistrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.AnnotateDocumentVersionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.AnnotateDocumentVersionRequest.Types.Details
            {
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Document = new ApiDocuments.Document
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Registration?.Audit?.Etag ?? ""),
                },
                Counterparty = new ApiDocuments.DocumentFolderRef
                {
                    DisplayName = (request.Registration?.Holder?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _documentsDocumentHistoryService.AnnotateDocumentVersionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiCorporateevents.ResumeRegistrationWorkflowRequest, ApiCorporateevents.GenerateRegistrationReportRequest>(value => new ApiCorporateevents.GenerateRegistrationReportRequest
        {
            Annotations = { (value.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Reason = (value.Reason ?? ""),
            RegistrationId = (value.Registration?.RegistrationId ?? ""),
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _corporateeventsRegistrationReportingService.GenerateRegistrationReportAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.ResumeRegistrationWorkflowResponse
        {
            Outcome = new ApiCorporateevents.ResumeRegistrationWorkflowResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
            Registration = new ApiCorporateevents.Registration
            {
                Balance = (request.Registration?.Balance ?? new ApiCommon.Money()),
                RegistrationId = (request.RegistrationId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Registration?.Holder?.CorporateEventId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "ResumeRegistrationWorkflow", (request.Registration?.Holder?.CorporateEventId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.RetryRegistrationWorkflowResponse> RetryRegistrationWorkflow(ApiCorporateevents.RetryRegistrationWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RetryRegistrationWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Registration is { } record)
        {
            var snapshot = RegistrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.GetCorporateEventApprovalStatusRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEventId = (request.Registration?.Holder?.CorporateEventId ?? ""),
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _corporateeventsCorporateEventApprovalService.GetCorporateEventApprovalStatusAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.RetryRegistrationWorkflowToAdvanceLicenseAssignmentWorkflow1(reply0);
        using var rpc1 = _licensingLicenseAssignmentWorkflowService.AdvanceLicenseAssignmentWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.RetryRegistrationWorkflowResponse
        {
            Outcome = new ApiCorporateevents.RetryRegistrationWorkflowResponse.Types.Outcome
            {
                Status = (reply1.Result?.Status ?? ""),
            },
            Registration = new ApiCorporateevents.Registration
            {
                Contact = (request.Registration?.Contact ?? new ApiCommon.ContactInfo()),
                Holder = new ApiCorporateevents.CorporateEventRef
                {
                    DisplayName = (reply0.CorporateEvent?.Organizer?.DisplayName ?? ""),
                },
                RegistrationId = (request.RegistrationId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Registration?.RegistrationId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "RetryRegistrationWorkflow", (request.Registration?.RegistrationId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.SignalRegistrationWorkflowResponse> SignalRegistrationWorkflow(ApiCorporateevents.SignalRegistrationWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SignalRegistrationWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Registration is { } record)
        {
            var snapshot = RegistrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.SignalTicketWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.SignalTicketWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Notify?.Phone ?? ""),
                },
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.Registration?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        var reply0 = await SendSignalRegistrationWorkflow0Async(input0, token);
        var response = new ApiCorporateevents.SignalRegistrationWorkflowResponse
        {
            Registration = new ApiCorporateevents.Registration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Ticket?.Audit?.Etag ?? ""),
                },
                Contact = (request.Registration?.Contact ?? new ApiCommon.ContactInfo()),
                RegistrationId = (request.RegistrationId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Registration?.RegistrationId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "SignalRegistrationWorkflow", (request.Registration?.RegistrationId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.StartRegistrationWorkflowResponse> StartRegistrationWorkflow(ApiCorporateevents.StartRegistrationWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling StartRegistrationWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Registration is { } record)
        {
            var snapshot = RegistrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.GetCorporateEventApprovalStatusRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEventId = (request.Registration?.Holder?.CorporateEventId ?? ""),
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _corporateeventsCorporateEventApprovalService.GetCorporateEventApprovalStatusAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.RecallRegistrationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.RecallRegistrationRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (request.Registration?.Contact?.Email ?? ""),
                },
            },
            Registration = new ApiCorporateevents.Registration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Registration?.Audit?.Etag ?? ""),
                },
                Holder = new ApiCorporateevents.CorporateEventRef
                {
                    DisplayName = (reply0.CorporateEvent?.Organizer?.DisplayName ?? ""),
                },
            },
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiCorporateevents.RecallRegistrationRequest>();
        using var rpc1 = _corporateeventsRegistrationApprovalService.RecallRegistrationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiCorporateevents.CompareCorporateEventOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Registration?.Audit?.CreatedBy ?? ""),
                },
                Daily = new ApiCorporateevents.CorporateEventDailyRule
                {
                    Interval = (reply0.CorporateEvent ?? new ApiCorporateevents.CorporateEvent()).RecurrenceCase switch
                    {
                        ApiCorporateevents.CorporateEvent.RecurrenceOneofCase.Daily => (reply0.CorporateEvent?.Daily?.Interval ?? 0),
                        _ => 0
                    },
                },
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply0.CorporateEvent?.Organizer?.Email ?? ""),
                    },
                } },
            },
            Details = new ApiCorporateevents.CompareCorporateEventOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.CorporateEvent?.Location?.PostalCode ?? ""),
                },
            },
        };
        using var rpc2 = _corporateeventsCorporateEventCalculationService.CompareCorporateEventOptionsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiIdentity.PeekUserRequest();
        input3.Context = (request.Context ?? new ApiCommon.RequestContext());
        input3.UserId = (request.Context?.Actor?.UserId ?? "");
        using var rpc3 = _identityUserQueryService.PeekUserAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiCorporateevents.StartRegistrationWorkflowResponse
        {
            Registration = new ApiCorporateevents.Registration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Registration?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (reply3.User?.Audit?.Etag ?? ""),
                },
                Contact = (request.Registration?.Contact ?? new ApiCommon.ContactInfo()),
                Holder = new ApiCorporateevents.CorporateEventRef
                {
                    DisplayName = (reply0.CorporateEvent?.Organizer?.DisplayName ?? ""),
                },
                RegistrationId = (request.RegistrationId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply1.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Registration?.RegistrationId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "StartRegistrationWorkflow", (request.Registration?.RegistrationId ?? ""), token);
        return response;
    }
    private async Task<ApiHelpdesk.SignalTicketWorkflowResponse> SendSignalRegistrationWorkflow0Async(ApiHelpdesk.SignalTicketWorkflowRequest request, CancellationToken token)
    {
        return await _registrationWorkflowEndpointSendSignalRegistrationWorkflow0Port.SendAsync(request, token);
    }
}
