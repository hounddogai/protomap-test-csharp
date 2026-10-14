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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLending = Acme.Lending.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;

namespace Acme.Workforce.Corporateevents;

/// <summary>
/// Routes registrations through approval, with escalation and delegation.
/// </summary>
public sealed class RegistrationApprovalEndpoint : ApiCorporateevents.RegistrationApprovalService.RegistrationApprovalServiceBase
{
    private readonly ApiCorporateevents.CorporateEventApprovalService.CorporateEventApprovalServiceClient _corporateeventsCorporateEventApprovalService;
    private readonly ApiCorporateevents.CorporateEventLifecycleService.CorporateEventLifecycleServiceClient _corporateeventsCorporateEventLifecycleService;
    private readonly ApiCorporateevents.CorporateEventReportingService.CorporateEventReportingServiceClient _corporateeventsCorporateEventReportingService;
    private readonly ApiCorporateevents.CorporateEventValidationService.CorporateEventValidationServiceClient _corporateeventsCorporateEventValidationService;
    private readonly ApiCorporateevents.VenueReportingService.VenueReportingServiceClient _corporateeventsVenueReportingService;
    private readonly ApiDevices.ComplianceScanApprovalService.ComplianceScanApprovalServiceClient _devicesComplianceScanApprovalService;
    private readonly ApiDocuments.DocumentFolderHistoryService.DocumentFolderHistoryServiceClient _documentsDocumentFolderHistoryService;
    private readonly ApiDocuments.DocumentService.DocumentServiceClient _documentsDocumentService;
    private readonly ApiHelpdesk.SlaPolicyWorkflowService.SlaPolicyWorkflowServiceClient _helpdeskSlaPolicyWorkflowService;
    private readonly ApiItsm.ProblemSyncService.ProblemSyncServiceClient _itsmProblemSyncService;
    private readonly ApiItsm.ServiceAssetWorkflowService.ServiceAssetWorkflowServiceClient _itsmServiceAssetWorkflowService;
    private readonly ApiLending.LoanApplicationCalculationService.LoanApplicationCalculationServiceClient _lendingLoanApplicationCalculationService;
    private readonly ApiLicensing.SoftwareLicenseWorkflowService.SoftwareLicenseWorkflowServiceClient _licensingSoftwareLicenseWorkflowService;
    private readonly ILogger<RegistrationApprovalEndpoint> _logger;
    private readonly ApiNotifications.NotificationQueryService.NotificationQueryServiceClient _notificationsNotificationQueryService;
    private readonly ApiNotifications.NotificationValidationService.NotificationValidationServiceClient _notificationsNotificationValidationService;
    private readonly IRegistrationApprovalEndpointSendSubmitRegistration1Port _registrationApprovalEndpointSendSubmitRegistration1Port;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiHelpdesk.ResumeSlaPolicyWorkflowRequest, CallOptions, AsyncUnaryCall<ApiHelpdesk.ResumeSlaPolicyWorkflowResponse>> _sendGetRegistrationApprovalStatus1;

    public RegistrationApprovalEndpoint(
        ApiCorporateevents.CorporateEventApprovalService.CorporateEventApprovalServiceClient corporateeventsCorporateEventApprovalService,
        ApiCorporateevents.CorporateEventLifecycleService.CorporateEventLifecycleServiceClient corporateeventsCorporateEventLifecycleService,
        ApiCorporateevents.CorporateEventReportingService.CorporateEventReportingServiceClient corporateeventsCorporateEventReportingService,
        ApiCorporateevents.CorporateEventValidationService.CorporateEventValidationServiceClient corporateeventsCorporateEventValidationService,
        ApiCorporateevents.VenueReportingService.VenueReportingServiceClient corporateeventsVenueReportingService,
        ApiDevices.ComplianceScanApprovalService.ComplianceScanApprovalServiceClient devicesComplianceScanApprovalService,
        ApiDocuments.DocumentFolderHistoryService.DocumentFolderHistoryServiceClient documentsDocumentFolderHistoryService,
        ApiDocuments.DocumentService.DocumentServiceClient documentsDocumentService,
        ApiHelpdesk.SlaPolicyWorkflowService.SlaPolicyWorkflowServiceClient helpdeskSlaPolicyWorkflowService,
        ApiItsm.ProblemSyncService.ProblemSyncServiceClient itsmProblemSyncService,
        ApiItsm.ServiceAssetWorkflowService.ServiceAssetWorkflowServiceClient itsmServiceAssetWorkflowService,
        ApiLending.LoanApplicationCalculationService.LoanApplicationCalculationServiceClient lendingLoanApplicationCalculationService,
        ApiLicensing.SoftwareLicenseWorkflowService.SoftwareLicenseWorkflowServiceClient licensingSoftwareLicenseWorkflowService,
        ILogger<RegistrationApprovalEndpoint> logger,
        ApiNotifications.NotificationQueryService.NotificationQueryServiceClient notificationsNotificationQueryService,
        ApiNotifications.NotificationValidationService.NotificationValidationServiceClient notificationsNotificationValidationService,
        IRegistrationApprovalEndpointSendSubmitRegistration1Port registrationApprovalEndpointSendSubmitRegistration1PortAdapter,
        IRecordRepository repository)
    {
        _corporateeventsCorporateEventApprovalService = corporateeventsCorporateEventApprovalService;
        _corporateeventsCorporateEventLifecycleService = corporateeventsCorporateEventLifecycleService;
        _corporateeventsCorporateEventReportingService = corporateeventsCorporateEventReportingService;
        _corporateeventsCorporateEventValidationService = corporateeventsCorporateEventValidationService;
        _corporateeventsVenueReportingService = corporateeventsVenueReportingService;
        _devicesComplianceScanApprovalService = devicesComplianceScanApprovalService;
        _documentsDocumentFolderHistoryService = documentsDocumentFolderHistoryService;
        _documentsDocumentService = documentsDocumentService;
        _helpdeskSlaPolicyWorkflowService = helpdeskSlaPolicyWorkflowService;
        _itsmProblemSyncService = itsmProblemSyncService;
        _itsmServiceAssetWorkflowService = itsmServiceAssetWorkflowService;
        _lendingLoanApplicationCalculationService = lendingLoanApplicationCalculationService;
        _licensingSoftwareLicenseWorkflowService = licensingSoftwareLicenseWorkflowService;
        _logger = logger;
        _notificationsNotificationQueryService = notificationsNotificationQueryService;
        _notificationsNotificationValidationService = notificationsNotificationValidationService;
        _registrationApprovalEndpointSendSubmitRegistration1Port = registrationApprovalEndpointSendSubmitRegistration1PortAdapter;
        _repository = repository;
        _sendGetRegistrationApprovalStatus1 = _helpdeskSlaPolicyWorkflowService.ResumeSlaPolicyWorkflowAsync;
    }
    /// <summary>
    /// Approves a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.ApproveRegistrationResponse> ApproveRegistration(ApiCorporateevents.ApproveRegistrationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ApproveRegistration for {Tenant}", requestContext.TenantId);
        if (request.Registration is { } record)
        {
            var snapshot = RegistrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.ShareVenueReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.ShareVenueReportRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                    PreferredChannel = (request.Registration?.Contact?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc0 = _corporateeventsVenueReportingService.ShareVenueReportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.RevertDocumentFolderRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.RevertDocumentFolderRequest.Types.Details
            {
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            UpdatedAt = (reply0.Venue?.Audit?.UpdatedAt ?? new Timestamp()),
                            UpdatedBy = (request.Registration?.Audit?.UpdatedBy ?? ""),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            Recipient = (reply0.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                            {
                                ApiCorporateevents.Venue.LocatorOneofCase.Address => (reply0.Venue?.Address?.Recipient ?? ""),
                                _ => ""
                            },
                            Region = (reply0.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                            {
                                ApiCorporateevents.Venue.LocatorOneofCase.Address => (reply0.Venue?.Address?.Region ?? ""),
                                _ => ""
                            },
                        },
                    } },
                } },
            },
        };
        using var rpc1 = _documentsDocumentFolderHistoryService.RevertDocumentFolderAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.ApproveRegistrationResponse
        {
            Outcome = new ApiCorporateevents.ApproveRegistrationResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Registration = new ApiCorporateevents.Registration
            {
                Holder = (request.Registration?.Holder ?? new ApiCorporateevents.CorporateEventRef()),
                Limits = { new ApiCorporateevents.RegistrationLimit
                {
                    Name = (reply1.DocumentFolder?.Name ?? ""),
                } },
                RegistrationId = (request.RegistrationId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Registration?.RegistrationId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "ApproveRegistration", (request.Registration?.RegistrationId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.DelegateRegistrationApprovalResponse> DelegateRegistrationApproval(ApiCorporateevents.DelegateRegistrationApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling DelegateRegistrationApproval for {Tenant}", requestContext.TenantId);
        if (request.Registration is { } record)
        {
            var snapshot = RegistrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.Registration?.Limits ?? new Google.Protobuf.Collections.RepeatedField<ApiCorporateevents.RegistrationLimit>()).Select(async item =>
        {
            var input0 = new ApiLending.CalculateLoanApplicationRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                LoanApplication = new ApiLending.LoanApplication
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        UpdatedBy = (request.Registration?.Audit?.UpdatedBy ?? ""),
                    },
                    Counterparty = new ApiCustomers.CustomerRef
                    {
                        DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                    },
                    Lines = { new ApiLending.LoanApplicationLine
                    {
                        Amount = (item.Amount ?? new ApiCommon.Money()),
                    } },
                },
            };
            using var rpc0 = _lendingLoanApplicationCalculationService.CalculateLoanApplicationAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiLending.CalculateLoanApplicationResponse();
        var reply1 = new ApiLicensing.SignalSoftwareLicenseWorkflowResponse();
        foreach (var item in (request.Registration?.Limits ?? new Google.Protobuf.Collections.RepeatedField<ApiCorporateevents.RegistrationLimit>()))
        {
            var input1 = new ApiLicensing.SignalSoftwareLicenseWorkflowRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                SoftwareLicense = new ApiLicensing.SoftwareLicense
                {
                    Limits = { new ApiLicensing.SoftwareLicenseLimit
                    {
                        Amount = (item.Amount ?? new ApiCommon.Money()),
                        Name = (item.Name ?? ""),
                        Period = (item.Period ?? new Duration()),
                    } },
                },
            };
            using var rpc1 = _licensingSoftwareLicenseWorkflowService.SignalSoftwareLicenseWorkflowAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var response = new ApiCorporateevents.DelegateRegistrationApprovalResponse
        {
            Registration = new ApiCorporateevents.Registration
            {
                Available = (request.Registration?.Available ?? new ApiCommon.Money()),
                RegistrationId = (request.RegistrationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Registration?.RegistrationId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "DelegateRegistrationApproval", (request.Registration?.RegistrationId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.EscalateRegistrationResponse> EscalateRegistration(ApiCorporateevents.EscalateRegistrationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling EscalateRegistration for {Tenant}", requestContext.TenantId);
        if (request.Registration is { } record)
        {
            var snapshot = RegistrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiNotifications.SummarizeNotificationsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiNotifications.SummarizeNotificationsRequest.Types.Details
            {
                Filters = { (request.Registration?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Reason = (request.Reason ?? ""),
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _notificationsNotificationQueryService.SummarizeNotificationsAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var batch1 = (request.Registration?.Limits ?? new Google.Protobuf.Collections.RepeatedField<ApiCorporateevents.RegistrationLimit>()).Select(async item =>
        {
            var input1 = new ApiItsm.PauseServiceAssetWorkflowRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiItsm.PauseServiceAssetWorkflowRequest.Types.Details
                {
                    Notify = new ApiCommon.ContactInfo
                    {
                        Email = (request.Registration?.Contact?.Email ?? ""),
                    },
                },
                ServiceAsset = new ApiItsm.ServiceAsset
                {
                    Name = (item.Name ?? ""),
                },
            };
            using var rpc1 = _itsmServiceAssetWorkflowService.PauseServiceAssetWorkflowAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            return reply1;
        }).ToArray();
        var replies1 = await Task.WhenAll(batch1);
        var reply1 = replies1.LastOrDefault() ?? new ApiItsm.PauseServiceAssetWorkflowResponse();
        var response = new ApiCorporateevents.EscalateRegistrationResponse
        {
            Registration = new ApiCorporateevents.Registration
            {
                OpenedAt = (request.Registration?.OpenedAt ?? new Timestamp()),
                RegistrationId = (request.RegistrationId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Registration?.RegistrationId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "EscalateRegistration", (request.Registration?.RegistrationId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.GetRegistrationApprovalStatusResponse> GetRegistrationApprovalStatus(ApiCorporateevents.GetRegistrationApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.RegistrationId, "registration_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetRegistrationApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "registration", request.RegistrationId,
            "GetRegistrationApprovalStatus", "accepted", token);
        var input0 = new ApiCorporateevents.EscalateCorporateEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
            },
            Details = new ApiCorporateevents.EscalateCorporateEventRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _corporateeventsCorporateEventApprovalService.EscalateCorporateEventAsync);
        var input1 = new ApiHelpdesk.ResumeSlaPolicyWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.CorporateEvent?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.CorporateEvent?.Organizer?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _sendGetRegistrationApprovalStatus1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.GetRegistrationApprovalStatusResponse
        {
            Registration = new ApiCorporateevents.Registration
            {
                RegistrationId = (request.RegistrationId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Outcome?.Message ?? ""),
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists registrations a page at a time.
    /// </summary>
    public override async Task<ApiCorporateevents.ListPendingRegistrationsResponse> ListPendingRegistrations(ApiCorporateevents.ListPendingRegistrationsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListPendingRegistrations for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "registration", "batch",
            "ListPendingRegistrations", "accepted", token);
        var input0 = new ApiDevices.DelegateComplianceScanApprovalRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.DelegateComplianceScanApprovalRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        using var rpc0 = _devicesComplianceScanApprovalService.DelegateComplianceScanApprovalAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiCorporateevents.ListPendingRegistrationsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.ComplianceScan?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "ListPendingRegistrations", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.RecallRegistrationResponse> RecallRegistration(ApiCorporateevents.RecallRegistrationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RecallRegistration for {Tenant}", requestContext.TenantId);
        if (request.Registration is { } record)
        {
            var snapshot = RegistrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.ListCorporateEventReportsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiCorporateevents.ListCorporateEventReportsRequest.Types.Filter
            {
                CreatedAfter = (request.Registration?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.Registration?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        using var rpc0 = _corporateeventsCorporateEventReportingService.ListCorporateEventReportsAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiNotifications.CheckNotificationEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Notification = new ApiNotifications.Notification
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Registration?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        using var rpc1 = _notificationsNotificationValidationService.CheckNotificationEligibilityAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var profile2 = new MappingProfile<ApiCorporateevents.RecallRegistrationRequest, ApiCorporateevents.GetCorporateEventRulesRequest>(value => new ApiCorporateevents.GetCorporateEventRulesRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            CorporateEventId = (value.Registration?.Holder?.CorporateEventId ?? ""),
        });
        var input2 = profile2.Map(request);
        using var rpc2 = _corporateeventsCorporateEventValidationService.GetCorporateEventRulesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiCorporateevents.RecallRegistrationResponse
        {
            Registration = new ApiCorporateevents.Registration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply2.CorporateEvent?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Available = (request.Registration?.Available ?? new ApiCommon.Money()),
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (reply1.Notification?.Requester?.Name ?? new ApiCommon.PersonName()),
                },
                RegistrationId = (request.RegistrationId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Registration?.RegistrationId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "RecallRegistration", (request.Registration?.RegistrationId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.RejectRegistrationResponse> RejectRegistration(ApiCorporateevents.RejectRegistrationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RejectRegistration for {Tenant}", requestContext.TenantId);
        if (request.Registration is { } record)
        {
            var snapshot = RegistrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.ListProblemConflictsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.ListProblemConflictsRequest.Types.Filter
            {
                CreatedAfter = (request.Registration?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (request.Registration?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Registration?.Holder?.CorporateEventId ?? ""),
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _itsmProblemSyncService.ListProblemConflictsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiCorporateevents.RejectRegistrationResponse
        {
            Registration = new ApiCorporateevents.Registration
            {
                OpenedAt = (request.Registration?.OpenedAt ?? new Timestamp()),
                RegistrationId = (request.RegistrationId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Registration?.RegistrationId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "RejectRegistration", (request.Registration?.RegistrationId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.SubmitRegistrationResponse> SubmitRegistration(ApiCorporateevents.SubmitRegistrationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SubmitRegistration for {Tenant}", requestContext.TenantId);
        if (request.Registration is { } record)
        {
            var snapshot = RegistrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.SubmitRegistrationToTerminateCorporateEvent0();
        using var rpc0 = _corporateeventsCorporateEventLifecycleService.TerminateCorporateEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.BatchGetDocumentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply1 = await SendSubmitRegistration1Async(input1, token);
        var response = new ApiCorporateevents.SubmitRegistrationResponse
        {
            Registration = new ApiCorporateevents.Registration
            {
                Holder = (request.Registration?.Holder ?? new ApiCorporateevents.CorporateEventRef()),
                RegistrationId = (request.RegistrationId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Registration?.RegistrationId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "SubmitRegistration", (request.Registration?.RegistrationId ?? ""), token);
        return response;
    }
    private async Task<ApiDocuments.BatchGetDocumentsResponse> SendSubmitRegistration1Async(ApiDocuments.BatchGetDocumentsRequest request, CancellationToken token)
    {
        return await _registrationApprovalEndpointSendSubmitRegistration1Port.SendAsync(request, token);
    }
}
