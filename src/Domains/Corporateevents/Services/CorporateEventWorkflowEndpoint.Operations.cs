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
using ApiCareplans = Acme.Careplans.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLocalization = Acme.Localization.V1;
using ApiSellers = Acme.Sellers.V1;

namespace Acme.Workforce.Corporateevents;

public sealed partial class CorporateEventWorkflowEndpoint
{
    /// <summary>
    /// Advances a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.AdvanceCorporateEventWorkflowResponse> AdvanceCorporateEventWorkflow(ApiCorporateevents.AdvanceCorporateEventWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling AdvanceCorporateEventWorkflow for {Tenant}", requestContext.TenantId);
        if (request.CorporateEvent is { } record)
        {
            var snapshot = CorporateEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.CorporateEvent?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiCorporateevents.CorporateEventParticipant>()).Select(async item =>
        {
            var input0 = new ApiDevices.StartComplianceScanWorkflowRequest
            {
                ComplianceScan = new ApiDevices.ComplianceScan
                {
                    Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiDevices.StartComplianceScanWorkflowRequest.Types.Details
                {
                    Notify = new ApiCommon.ContactInfo
                    {
                        Name = (item.Contact?.Name ?? new ApiCommon.PersonName()),
                        PreferredChannel = (item.Contact?.PreferredChannel ?? ""),
                    },
                },
            };
            using var rpc0 = _devicesComplianceScanWorkflowService.StartComplianceScanWorkflowAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiDevices.StartComplianceScanWorkflowResponse();
        var input1 = new ApiItsm.EscalateServiceAssetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Address = new ApiCommon.PostalAddress
                {
                    Lines = { (request.CorporateEvent?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
        };
        var reply1 = await SendAdvanceCorporateEventWorkflow1Async(input1, token);
        var response = new ApiCorporateevents.AdvanceCorporateEventWorkflowResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                CorporateEventId = (request.CorporateEventId ?? ""),
                Location = (request.CorporateEvent?.Location ?? new ApiCommon.PostalAddress()),
            },
            Outcome = new ApiCorporateevents.AdvanceCorporateEventWorkflowResponse.Types.Outcome
            {
                EffectiveAt = (reply1.Outcome?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "AdvanceCorporateEventWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.CancelCorporateEventWorkflowResponse> CancelCorporateEventWorkflow(ApiCorporateevents.CancelCorporateEventWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CancelCorporateEventWorkflow for {Tenant}", requestContext.TenantId);
        if (request.CorporateEvent is { } record)
        {
            var snapshot = CorporateEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.UnsubscribeDocumentTemplateUpdatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.UnsubscribeDocumentTemplateUpdatesRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.CorporateEvent?.Organizer?.Email ?? ""),
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.Notify?.Phone ?? ""),
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc0 = _documentsDocumentTemplateNotificationService.UnsubscribeDocumentTemplateUpdatesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.RecallRegistrationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.RecallRegistrationRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (request.CorporateEvent?.Organizer?.Email ?? ""),
                },
            },
            Registration = new ApiCorporateevents.Registration
            {
                Card = new ApiCorporateevents.RegistrationCardFunding
                {
                    HolderName = (reply0.DocumentTemplate ?? new ApiDocuments.DocumentTemplate()).PaymentCase switch
                    {
                        ApiDocuments.DocumentTemplate.SettlementOneofCase.Card => (reply0.DocumentTemplate?.Card?.HolderName ?? ""),
                        _ => ""
                    },
                    Last4 = (reply0.DocumentTemplate ?? new ApiDocuments.DocumentTemplate()).PaymentCase switch
                    {
                        ApiDocuments.DocumentTemplate.SettlementOneofCase.Card => (reply0.DocumentTemplate?.Card?.Last4 ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _corporateeventsRegistrationApprovalService.RecallRegistrationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLocalization.BatchGetTranslationKeysRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var json2 = JsonFormatter.Default.Format(input2);
        input2 = JsonParser.Default.Parse<ApiLocalization.BatchGetTranslationKeysRequest>(json2);
        using var rpc2 = _localizationTranslationKeyService.BatchGetTranslationKeysAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiCorporateevents.PushVenueRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.PushVenueRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Venue = new ApiCorporateevents.Venue
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.DocumentTemplate?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        using var rpc3 = _corporateeventsVenueSyncService.PushVenueAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiCorporateevents.CancelCorporateEventWorkflowResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply3.Venue?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                CorporateEventId = (request.CorporateEventId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    Recipient = (reply3.Venue?.Address?.Recipient ?? ""),
                },
                Metadata = { (request.CorporateEvent?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Organizer = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.Registration?.Holder?.DisplayName ?? ""),
                },
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = (reply1.Registration?.Contact ?? new ApiCommon.ContactInfo()),
                } },
            },
            Outcome = new ApiCorporateevents.CancelCorporateEventWorkflowResponse.Types.Outcome
            {
                Message = (reply2.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "CancelCorporateEventWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.InspectCorporateEventWorkflowResponse> InspectCorporateEventWorkflow(ApiCorporateevents.InspectCorporateEventWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CorporateEventId, "corporate_event_id");
        _logger.LogInformation("Handling InspectCorporateEventWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event", request.CorporateEventId,
            "InspectCorporateEventWorkflow", "accepted", token);
        var input0 = new ApiSellers.CalculateSellerRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Seller = new ApiSellers.Seller
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
                Segments = { (request.Context?.Actor?.Roles ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        using var rpc0 = _sellersSellerCalculationService.CalculateSellerAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.ShareVenueReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.ShareVenueReportRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Seller?.Contact?.Email ?? ""),
                    Name = (reply0.Seller?.Contact?.Name ?? new ApiCommon.PersonName()),
                    Phone = (reply0.Seller?.Contact?.Phone ?? ""),
                    PreferredChannel = (reply0.Seller?.Contact?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc1 = _corporateeventsVenueReportingService.ShareVenueReportAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.InspectCorporateEventWorkflowResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Seller?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (reply0.Seller?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                CorporateEventId = (request.CorporateEventId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    Locality = (reply1.Venue?.Address?.Locality ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Outcome?.Message ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Pauses a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.PauseCorporateEventWorkflowResponse> PauseCorporateEventWorkflow(ApiCorporateevents.PauseCorporateEventWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PauseCorporateEventWorkflow for {Tenant}", requestContext.TenantId);
        if (request.CorporateEvent is { } record)
        {
            var snapshot = CorporateEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.NotifyDocumentWatchersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.NotifyDocumentWatchersRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                    Phone = (request.Details?.Notify?.Phone ?? ""),
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc0 = _documentsDocumentNotificationService.NotifyDocumentWatchersAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.RecalculateSignaturesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.RecalculateSignaturesRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.CorporateEvent?.Location?.CountryCode ?? ""),
                    Lines = { (request.CorporateEvent?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Locality = (reply0.Document?.BillingAddress?.Locality ?? ""),
                },
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (reply0.Document?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _documentsSignatureCalculationService.RecalculateSignaturesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.PauseCorporateEventWorkflowResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                CorporateEventId = (request.CorporateEventId ?? ""),
                Organizer = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Document?.Counterparty?.DisplayName ?? ""),
                },
                Title = (request.CorporateEvent?.Title ?? ""),
            },
            Outcome = new ApiCorporateevents.PauseCorporateEventWorkflowResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "PauseCorporateEventWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resumes a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.ResumeCorporateEventWorkflowResponse> ResumeCorporateEventWorkflow(ApiCorporateevents.ResumeCorporateEventWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResumeCorporateEventWorkflow for {Tenant}", requestContext.TenantId);
        if (request.CorporateEvent is { } record)
        {
            var snapshot = CorporateEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiHelpdesk.CancelTicketWorkflowResponse();
        foreach (var item in (request.CorporateEvent?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiCorporateevents.CorporateEventParticipant>()))
        {
            var input0 = new ApiHelpdesk.CancelTicketWorkflowRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiHelpdesk.CancelTicketWorkflowRequest.Types.Details
                {
                    Notify = new ApiCommon.ContactInfo
                    {
                        Name = (item.Contact?.Name ?? new ApiCommon.PersonName()),
                    },
                },
                Ticket = new ApiHelpdesk.Ticket
                {
                    Requester = new ApiCommon.ContactInfo
                    {
                        Email = (item.Contact?.Email ?? ""),
                        Phone = (item.Contact?.Phone ?? ""),
                        PreferredChannel = (item.Contact?.PreferredChannel ?? ""),
                    },
                },
            };
            using var rpc0 = _helpdeskTicketWorkflowService.CancelTicketWorkflowAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiItsm.DelegateChangeRequestApprovalRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Channel = new ApiItsm.ChangeRequestChannelOrigin
                {
                    Channel = (reply0.Ticket ?? new ApiHelpdesk.Ticket()).OriginCase switch
                    {
                        ApiHelpdesk.Ticket.OriginOneofCase.Channel => (reply0.Ticket?.Channel?.Channel ?? ""),
                        _ => ""
                    },
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _itsmChangeRequestApprovalService.DelegateChangeRequestApprovalAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDevices.SignalDevicePolicyWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.SignalDevicePolicyWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.CorporateEvent?.Organizer?.Email ?? ""),
                },
            },
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.ChangeRequest?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.ChangeRequest?.Assignee?.DisplayName ?? ""),
                },
            },
        };
        var descriptor2 = new Method<ApiDevices.SignalDevicePolicyWorkflowRequest, ApiDevices.SignalDevicePolicyWorkflowResponse>(
            MethodType.Unary, "acme.devices.v1" + ".DevicePolicyWorkflowService", "SignalDevicePolicyWorkflow",
            Wire.Marshaller<ApiDevices.SignalDevicePolicyWorkflowRequest>(), Wire.Marshaller<ApiDevices.SignalDevicePolicyWorkflowResponse>());
        using var rpc2 = _invoker.AsyncUnaryCall(descriptor2, null,
            new CallOptions(cancellationToken: token), input2);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiCorporateevents.ResumeCorporateEventWorkflowResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                CorporateEventId = (request.CorporateEventId ?? ""),
                Organizer = new ApiIdentity.UserRef
                {
                    UserId = (reply1.ChangeRequest?.Assignee?.UserId ?? ""),
                },
                Window = (request.CorporateEvent?.Window ?? new ApiCommon.TimeWindow()),
            },
            Outcome = new ApiCorporateevents.ResumeCorporateEventWorkflowResponse.Types.Outcome
            {
                Message = (reply2.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "ResumeCorporateEventWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.RetryCorporateEventWorkflowResponse> RetryCorporateEventWorkflow(ApiCorporateevents.RetryCorporateEventWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RetryCorporateEventWorkflow for {Tenant}", requestContext.TenantId);
        if (request.CorporateEvent is { } record)
        {
            var snapshot = CorporateEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.DelegateRegistrationApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.DelegateRegistrationApprovalRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    UserId = (request.CorporateEvent?.Organizer?.UserId ?? ""),
                },
            },
            Registration = new ApiCorporateevents.Registration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.CorporateEvent?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.CorporateEvent?.Organizer?.Email ?? ""),
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiCorporateevents.DelegateRegistrationApprovalRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _corporateeventsRegistrationApprovalService.DelegateRegistrationApprovalAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var reply1 = new ApiDocuments.RecordSignatureVersionResponse();
        foreach (var item in (request.CorporateEvent?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiCorporateevents.CorporateEventParticipant>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input1 = new ApiDocuments.RecordSignatureVersionRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiDocuments.RecordSignatureVersionRequest.Types.Details
                {
                    AuthorEmail = (item.Contact?.Email ?? ""),
                    Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
                },
                Reason = (request.Reason ?? ""),
                Signature = new ApiDocuments.Signature
                {
                    CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                    Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                    Subject = new ApiDocuments.DocumentRef
                    {
                        DisplayName = (request.CorporateEvent?.Organizer?.DisplayName ?? ""),
                    },
                },
            };
            using var rpc1 = _documentsSignatureHistoryService.RecordSignatureVersionAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var batch2 = (request.CorporateEvent?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiCorporateevents.CorporateEventParticipant>()).Select(async item =>
        {
            var input2 = new ApiItsm.RecallChangeRequestRequest
            {
                ChangeRequest = new ApiItsm.ChangeRequest
                {
                    Requester = new ApiCommon.ContactInfo
                    {
                        Phone = (item.Contact?.Phone ?? ""),
                    },
                },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiItsm.RecallChangeRequestRequest.Types.Details
                {
                    Approver = new ApiIdentity.UserRef
                    {
                        Email = (item.Contact?.Email ?? ""),
                    },
                },
            };
            using var rpc2 = _itsmChangeRequestApprovalService.RecallChangeRequestAsync(input2, cancellationToken: token);
            var reply2 = await rpc2.ResponseAsync;
            return reply2;
        }).ToArray();
        var replies2 = await Task.WhenAll(batch2);
        var reply2 = replies2.LastOrDefault() ?? new ApiItsm.RecallChangeRequestResponse();
        var input3 = request.RetryCorporateEventWorkflowToCompareSellerOptions3(reply0, reply1);
        using var rpc3 = _sellersSellerCalculationService.CompareSellerOptionsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiCorporateevents.RetryCorporateEventWorkflowResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                CorporateEventId = (request.CorporateEventId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    Region = (reply3.Seller?.PrimaryAddress?.Region ?? ""),
                },
                Metadata = { (request.CorporateEvent?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Step ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "RetryCorporateEventWorkflow", (request.Details?.Step ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.StartCorporateEventWorkflowResponse> StartCorporateEventWorkflow(ApiCorporateevents.StartCorporateEventWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StartCorporateEventWorkflow for {Tenant}", requestContext.TenantId);
        if (request.CorporateEvent is { } record)
        {
            var snapshot = CorporateEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiItsm.DelegateServiceAssetApprovalResponse();
        foreach (var item in (request.CorporateEvent?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiCorporateevents.CorporateEventParticipant>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input0 = new ApiItsm.DelegateServiceAssetApprovalRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiItsm.DelegateServiceAssetApprovalRequest.Types.Details
                {
                    Approver = new ApiIdentity.UserRef
                    {
                        Email = (item.Contact?.Email ?? ""),
                    },
                },
                ServiceAsset = new ApiItsm.ServiceAsset
                {
                    Address = new ApiCommon.PostalAddress
                    {
                        CountryCode = (request.CorporateEvent?.Location?.CountryCode ?? ""),
                    },
                },
            };
            using var rpc0 = _itsmServiceAssetApprovalService.DelegateServiceAssetApprovalAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiCareplans.WatchCarePlansRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiCareplans.WatchCarePlansRequest.Types.Filter
            {
                Region = (reply0.ServiceAsset?.Address?.Region ?? ""),
            },
            Subscriber = new ApiCommon.ContactInfo
            {
                Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
            },
        };
        using var stream1 = _careplansCarePlanService.WatchCarePlans(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchCarePlans frame {Type}", change1.GetType().Name);
        }
        var response = new ApiCorporateevents.StartCorporateEventWorkflowResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                CorporateEventId = (request.CorporateEventId ?? ""),
                Status = (request.CorporateEvent?.Status ?? (ApiCorporateevents.CorporateEventStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "StartCorporateEventWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiItsm.EscalateServiceAssetResponse> SendAdvanceCorporateEventWorkflow1Async(ApiItsm.EscalateServiceAssetRequest request, CancellationToken token)
    {
        return await _corporateEventWorkflowEndpointSendAdvanceCorporateEventWorkflow1Port.SendAsync(request, token);
    }
}
