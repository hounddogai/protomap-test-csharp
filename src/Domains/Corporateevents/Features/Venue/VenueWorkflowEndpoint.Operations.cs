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
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPersonalization = Acme.Personalization.V1;

namespace Acme.Workforce.Corporateevents;

public sealed partial class VenueWorkflowEndpoint
{
    /// <summary>
    /// Advances a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.AdvanceVenueWorkflowResponse> AdvanceVenueWorkflow(ApiCorporateevents.AdvanceVenueWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling AdvanceVenueWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Venue is { } record)
        {
            var snapshot = VenueMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.RejectVenueRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Venue = new ApiCorporateevents.Venue
            {
                Address = new ApiCommon.PostalAddress
                {
                    Region = (request.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                    {
                        ApiCorporateevents.Venue.LocatorOneofCase.Address => (request.Venue?.Address?.Region ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _corporateeventsVenueApprovalService.RejectVenueAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.ApproveRegistrationRequest();
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        input1.Details = new ApiCorporateevents.ApproveRegistrationRequest.Types.Details
        {
            Approver = new ApiIdentity.UserRef
            {
                DisplayName = (reply0.Venue?.Owner?.DisplayName ?? ""),
            },
        };
        input1.Registration = new ApiCorporateevents.Registration
        {
            Audit = new ApiCommon.AuditInfo
            {
                UpdatedAt = (reply0.Venue?.Audit?.UpdatedAt ?? new Timestamp()),
            },
        };
        using var rpc1 = _corporateeventsRegistrationApprovalService.ApproveRegistrationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDocuments.PullSignaturesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.PullSignaturesRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Reason = (request.Reason ?? ""),
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Measurements = { new ApiDocuments.SignatureMeasurement
                {
                    Unit = (reply0.Venue?.Dimensions?.Unit ?? ""),
                } },
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (request.Venue?.Owner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc2 = _documentsSignatureSyncService.PullSignaturesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiCorporateevents.RenewVenueRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.RenewVenueRequest.Types.Details
            {
                NewOwner = new ApiIdentity.UserRef
                {
                    Email = (reply1.Registration?.Contact?.Email ?? ""),
                },
                NotifyContact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                    Phone = (reply1.Registration?.Contact?.Phone ?? ""),
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
            },
            VenueId = (request.VenueId ?? ""),
        };
        var reply3 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _corporateeventsVenueLifecycleService.RenewVenueAsync(input3, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiCorporateevents.AdvanceVenueWorkflowResponse
        {
            Outcome = new ApiCorporateevents.AdvanceVenueWorkflowResponse.Types.Outcome
            {
                Message = (reply2.Outcome?.Message ?? ""),
                NextSteps = { (reply3.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Outcome?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply2.Result?.Status ?? ""),
            },
            Venue = new ApiCorporateevents.Venue
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.Registration?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Name = (reply3.Venue?.Name ?? ""),
                Status = (request.Venue?.Status ?? (ApiCorporateevents.VenueStatus)0),
                VenueId = (request.VenueId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Venue?.VenueId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "AdvanceVenueWorkflow", (request.Venue?.VenueId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.CancelVenueWorkflowResponse> CancelVenueWorkflow(ApiCorporateevents.CancelVenueWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CancelVenueWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Venue is { } record)
        {
            var snapshot = VenueMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.EscalateRegistrationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Registration = new ApiCorporateevents.Registration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Venue?.Audit?.CreatedBy ?? ""),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                },
                Holder = new ApiCorporateevents.CorporateEventRef
                {
                    DisplayName = (request.Venue?.Owner?.DisplayName ?? ""),
                },
                Limits = { new ApiCorporateevents.RegistrationLimit
                {
                    Name = (request.Venue?.Name ?? ""),
                } },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _corporateeventsRegistrationApprovalService.EscalateRegistrationAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiCorporateevents.CancelVenueWorkflowResponse
        {
            Outcome = new ApiCorporateevents.CancelVenueWorkflowResponse.Types.Outcome
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
            Venue = new ApiCorporateevents.Venue
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.Registration?.Audit?.UpdatedBy ?? ""),
                },
                Name = (request.Venue?.Name ?? ""),
                VenueId = (request.VenueId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Venue?.VenueId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "CancelVenueWorkflow", (request.Venue?.VenueId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.InspectVenueWorkflowResponse> InspectVenueWorkflow(ApiCorporateevents.InspectVenueWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.VenueId, "venue_id");
        _logger.LogInformation("Handling InspectVenueWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "venue", request.VenueId,
            "InspectVenueWorkflow", "accepted", token);
        var input0 = new ApiCorporateevents.DeleteVenueReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            VenueId = (request.VenueId ?? ""),
        };
        var reply0 = await Workflow.SendAsync(input0, token, _corporateeventsVenueReportingService.DeleteVenueReportAsync);
        var response = new ApiCorporateevents.InspectVenueWorkflowResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Venue = new ApiCorporateevents.Venue
            {
                VenueId = (request.VenueId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Pauses a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.PauseVenueWorkflowResponse> PauseVenueWorkflow(ApiCorporateevents.PauseVenueWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PauseVenueWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Venue is { } record)
        {
            var snapshot = VenueMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.RejectCorporateEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                    },
                } },
            },
        };
        using var rpc0 = _sendPauseVenueWorkflow0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiPersonalization.ExplainProfileCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Profile = new ApiPersonalization.Profile
            {
                RelationshipManager = new ApiIdentity.UserRef
                {
                    UserId = (request.Venue?.Owner?.UserId ?? ""),
                },
            },
        };
        var named1 = _registry.Get<ApiPersonalization.ProfileCalculationService.ProfileCalculationServiceClient>("acme.personalization.v1.ProfileCalculationService");
        using var rpc1 = named1.ExplainProfileCalculationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.PauseVenueWorkflowResponse
        {
            Outcome = new ApiCorporateevents.PauseVenueWorkflowResponse.Types.Outcome
            {
                NextSteps = { (reply0.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Venue = new ApiCorporateevents.Venue
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.Profile?.RelationshipManager?.DisplayName ?? ""),
                },
                Specs = { (request.Venue?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                VenueId = (request.VenueId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Venue?.VenueId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "PauseVenueWorkflow", (request.Venue?.VenueId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resumes a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.ResumeVenueWorkflowResponse> ResumeVenueWorkflow(ApiCorporateevents.ResumeVenueWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResumeVenueWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Venue is { } record)
        {
            var snapshot = VenueMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.DelegateLicenseAssignmentApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Lines = { new ApiLicensing.LicenseAssignmentLine
                {
                    Description = (request.Venue?.Components?.GetValueOrDefault("primary")?.Description ?? ""),
                } },
            },
        };
        var descriptor0 = new Method<ApiLicensing.DelegateLicenseAssignmentApprovalRequest, ApiLicensing.DelegateLicenseAssignmentApprovalResponse>(
            MethodType.Unary, "acme.licensing.v1" + ".LicenseAssignmentApprovalService", "DelegateLicenseAssignmentApproval",
            Wire.Marshaller<ApiLicensing.DelegateLicenseAssignmentApprovalRequest>(), Wire.Marshaller<ApiLicensing.DelegateLicenseAssignmentApprovalResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.ShareSignatureExportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ShareSignatureExportRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc1 = _documentsSignatureExportService.ShareSignatureExportAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLedger.CloneLedgerAccountRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LedgerAccount = new ApiLedger.LedgerAccount
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.LicenseAssignment?.Audit?.UpdatedBy ?? ""),
                },
                Bank = new ApiLedger.LedgerAccountBankFunding
                {
                    Bic = (reply0.LicenseAssignment ?? new ApiLicensing.LicenseAssignment()).PaymentCase switch
                    {
                        ApiLicensing.LicenseAssignment.SettlementOneofCase.Bank => (reply0.LicenseAssignment?.Bank?.Bic ?? ""),
                        _ => ""
                    },
                },
                Holder = new ApiLedger.PostingBatchRef
                {
                    DisplayName = (reply0.LicenseAssignment?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        using var rpc2 = _ledgerLedgerAccountService.CloneLedgerAccountAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiCorporateevents.ListPendingVenuesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiCorporateevents.ListPendingVenuesRequest.Types.Filter
            {
                Region = (request.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                {
                    ApiCorporateevents.Venue.LocatorOneofCase.Address => (request.Venue?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc3 = _corporateeventsVenueApprovalService.ListPendingVenuesAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiCorporateevents.ResumeVenueWorkflowResponse
        {
            Outcome = new ApiCorporateevents.ResumeVenueWorkflowResponse.Types.Outcome
            {
                EffectiveAt = (reply1.Outcome?.EffectiveAt ?? new Timestamp()),
                Message = (reply2.Result?.Message ?? ""),
                NextSteps = { (reply0.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply3.Result?.Message ?? ""),
            },
            Venue = new ApiCorporateevents.Venue
            {
                Owner = (request.Venue?.Owner ?? new ApiIdentity.UserRef()),
                VenueId = (request.VenueId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Venue?.VenueId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "ResumeVenueWorkflow", (request.Venue?.VenueId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.RetryVenueWorkflowResponse> RetryVenueWorkflow(ApiCorporateevents.RetryVenueWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RetryVenueWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Venue is { } record)
        {
            var snapshot = VenueMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.RetryVenueWorkflowToRejectCorporateEvent0();
        using var rpc0 = _corporateeventsCorporateEventApprovalService.RejectCorporateEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiCorporateevents.RetryVenueWorkflowResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
            Venue = new ApiCorporateevents.Venue
            {
                Name = (request.Venue?.Name ?? ""),
                VenueId = (request.VenueId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Venue?.VenueId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "RetryVenueWorkflow", (request.Venue?.VenueId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.SignalVenueWorkflowResponse> SignalVenueWorkflow(ApiCorporateevents.SignalVenueWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SignalVenueWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Venue is { } record)
        {
            var snapshot = VenueMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.ScheduleSignatureExportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ScheduleSignatureExportRequest.Types.Details
            {
                Filters = { (request.Details?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Reason = (request.Reason ?? ""),
        };
        var reply0 = await SendSignalVenueWorkflow0Async(input0, token);
        var response = new ApiCorporateevents.SignalVenueWorkflowResponse
        {
            Outcome = new ApiCorporateevents.SignalVenueWorkflowResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Venue = new ApiCorporateevents.Venue
            {
                Specs = { (request.Venue?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                VenueId = (request.VenueId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Venue?.VenueId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "SignalVenueWorkflow", (request.Venue?.VenueId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.StartVenueWorkflowResponse> StartVenueWorkflow(ApiCorporateevents.StartVenueWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StartVenueWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Venue is { } record)
        {
            var snapshot = VenueMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.CountCorporateEventTotalsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiCorporateevents.CountCorporateEventTotalsRequest.Types.Filter
            {
                CreatedAfter = (request.Venue?.Audit?.UpdatedAt ?? new Timestamp()),
                OwnerId = (request.Venue?.Owner?.UserId ?? ""),
                Region = (request.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                {
                    ApiCorporateevents.Venue.LocatorOneofCase.Address => (request.Venue?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc0 = _corporateeventsCorporateEventReportingService.CountCorporateEventTotalsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.GetVenueRatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            VenueId = (request.Venue?.VenueId ?? ""),
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiCorporateevents.GetVenueRatesRequest>();
        using var rpc1 = _corporateeventsVenueCalculationService.GetVenueRatesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.StartVenueWorkflowResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Venue = new ApiCorporateevents.Venue
            {
                Components = { { "primary", new ApiCorporateevents.VenueComponent
                {
                    ComponentId = (reply1.Venue?.Components?.GetValueOrDefault("primary")?.ComponentId ?? ""),
                } } },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply1.Venue?.Owner?.Email ?? ""),
                },
                Status = (request.Venue?.Status ?? (ApiCorporateevents.VenueStatus)0),
                VenueId = (request.VenueId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Venue?.Owner?.Email ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "StartVenueWorkflow", (request.Venue?.Owner?.Email ?? ""), token);
        return response;
    }
    private async Task<ApiDocuments.ScheduleSignatureExportResponse> SendSignalVenueWorkflow0Async(ApiDocuments.ScheduleSignatureExportRequest request, CancellationToken token)
    {
        return await _venueWorkflowEndpointSendSignalVenueWorkflow0Port.SendAsync(request, token);
    }
}
