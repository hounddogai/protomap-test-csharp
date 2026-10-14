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
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiLoyalty = Acme.Loyalty.V1;

namespace Acme.Workforce.Corporateevents;

public sealed partial class VenueApprovalEndpoint
{
    /// <summary>
    /// Approves a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.ApproveVenueResponse> ApproveVenue(ApiCorporateevents.ApproveVenueRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ApproveVenue for {Tenant}", requestContext.TenantId);
        if (request.Venue is { } record)
        {
            var snapshot = VenueMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.ListPendingIncidentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.ListPendingIncidentsRequest.Types.Filter
            {
                CreatedAfter = (request.Venue?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (request.Venue?.Owner?.UserId ?? ""),
                Region = (request.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                {
                    ApiCorporateevents.Venue.LocatorOneofCase.Address => (request.Venue?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc0 = _itsmIncidentApprovalService.ListPendingIncidentsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.CheckRegistrationEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Registration = new ApiCorporateevents.Registration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Venue?.Audit?.Etag ?? ""),
                },
                Holder = new ApiCorporateevents.CorporateEventRef
                {
                    DisplayName = (request.Venue?.Owner?.DisplayName ?? ""),
                },
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _corporateeventsRegistrationValidationService.CheckRegistrationEligibilityAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiCorporateevents.ApproveVenueResponse
        {
            Outcome = new ApiCorporateevents.ApproveVenueResponse.Types.Outcome
            {
                EffectiveAt = (reply1.Outcome?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
            Venue = new ApiCorporateevents.Venue
            {
                Owner = (request.Venue?.Owner ?? new ApiIdentity.UserRef()),
                VenueId = (request.VenueId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Venue?.VenueId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "ApproveVenue", (request.Venue?.VenueId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.CommentVenueApprovalResponse> CommentVenueApproval(ApiCorporateevents.CommentVenueApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CommentVenueApproval for {Tenant}", requestContext.TenantId);
        if (request.Venue is { } record)
        {
            var snapshot = VenueMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.FetchVenueReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            VenueId = (request.Venue?.VenueId ?? ""),
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _corporateeventsVenueReportingService.FetchVenueReportAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiCorporateevents.ScheduleCorporateEventExpiryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Venue?.Audit?.Etag ?? ""),
                },
                Location = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                    {
                        ApiCorporateevents.Venue.LocatorOneofCase.Address => (request.Venue?.Address?.CountryCode ?? ""),
                        _ => ""
                    },
                },
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (request.Venue?.Owner?.Email ?? ""),
                },
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Details?.Approver?.Email ?? ""),
                    },
                } },
            },
        };
        using var rpc1 = _corporateeventsCorporateEventLifecycleService.ScheduleCorporateEventExpiryAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.CommentVenueApprovalResponse
        {
            Outcome = new ApiCorporateevents.CommentVenueApprovalResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Venue = new ApiCorporateevents.Venue
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Venue?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (reply1.CorporateEvent?.Audit?.CreatedBy ?? ""),
                },
                Specs = { (request.Venue?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                VenueId = (request.VenueId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Venue?.Owner?.UserId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "CommentVenueApproval", (request.Venue?.Owner?.UserId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.DelegateVenueApprovalResponse> DelegateVenueApproval(ApiCorporateevents.DelegateVenueApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling DelegateVenueApproval for {Tenant}", requestContext.TenantId);
        if (request.Venue is { } record)
        {
            var snapshot = VenueMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.ShareCorporateEventReportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.ShareCorporateEventReportRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _corporateeventsCorporateEventReportingService.ShareCorporateEventReportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLoyalty.CreateLoyaltyMemberRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LoyaltyMember = new ApiLoyalty.LoyaltyMember
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Venue?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.CorporateEvent?.Organizer?.Email ?? ""),
                },
            },
        };
        var named1 = _registry.Get<ApiLoyalty.LoyaltyMemberService.LoyaltyMemberServiceClient>("acme.loyalty.v1.LoyaltyMemberService");
        using var rpc1 = named1.CreateLoyaltyMemberAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.DelegateVenueApprovalResponse
        {
            Venue = new ApiCorporateevents.Venue
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.CorporateEvent?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (reply1.LoyaltyMember?.Audit?.Etag ?? ""),
                },
                Specs = { (request.Venue?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                VenueId = (request.VenueId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Venue?.VenueId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "DelegateVenueApproval", (request.Venue?.VenueId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.EscalateVenueResponse> EscalateVenue(ApiCorporateevents.EscalateVenueRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EscalateVenue for {Tenant}", requestContext.TenantId);
        if (request.Venue is { } record)
        {
            var snapshot = VenueMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.ListManagedDeviceWorkflowsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDevices.ListManagedDeviceWorkflowsRequest.Types.Filter
            {
                Region = (request.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                {
                    ApiCorporateevents.Venue.LocatorOneofCase.Address => (request.Venue?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc0 = _devicesManagedDeviceWorkflowService.ListManagedDeviceWorkflowsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.RecallIncidentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Incident = new ApiItsm.Incident
            {
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Venue?.Owner?.Email ?? ""),
                },
            },
        };
        var command1 = new DispatchCommand<ApiItsm.RecallIncidentRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _itsmIncidentApprovalService.RecallIncidentAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var profile2 = new MappingProfile<ApiCorporateevents.EscalateVenueRequest, ApiLicensing.StartEntitlementWorkflowRequest>(value => new ApiLicensing.StartEntitlementWorkflowRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.StartEntitlementWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (reply1.Incident?.Assignee?.Email ?? ""),
                },
            },
        });
        var input2 = profile2.Map(request);
        using var rpc2 = _licensingEntitlementWorkflowService.StartEntitlementWorkflowAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiCorporateevents.EscalateVenueResponse
        {
            Outcome = new ApiCorporateevents.EscalateVenueResponse.Types.Outcome
            {
                Message = (reply1.Outcome?.Message ?? ""),
                Status = (reply2.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
            },
            Venue = new ApiCorporateevents.Venue
            {
                Capacity = (request.Venue?.Capacity ?? 0L),
                VenueId = (request.VenueId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Venue?.VenueId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "EscalateVenue", (request.Venue?.VenueId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.GetVenueApprovalStatusResponse> GetVenueApprovalStatus(ApiCorporateevents.GetVenueApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.VenueId, "venue_id");
        _logger.LogInformation("Handling GetVenueApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "venue", request.VenueId,
            "GetVenueApprovalStatus", "accepted", token);
        var input0 = new ApiCorporateevents.ResumeCorporateEventWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Organizer = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Context?.Actor?.Email ?? ""),
                    },
                    ParticipantId = (request.VenueId ?? ""),
                } },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _corporateeventsCorporateEventWorkflowService.ResumeCorporateEventWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiCorporateevents.GetVenueApprovalStatusResponse
        {
            Venue = new ApiCorporateevents.Venue
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.CorporateEvent?.Audit?.Etag ?? ""),
                },
                VenueId = (request.VenueId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists venues a page at a time.
    /// </summary>
    public override async Task<ApiCorporateevents.ListPendingVenuesResponse> ListPendingVenues(ApiCorporateevents.ListPendingVenuesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListPendingVenues for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "venue", "batch",
            "ListPendingVenues", "accepted", token);
        var response = new ApiCorporateevents.ListPendingVenuesResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "ListPendingVenues", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.RecallVenueResponse> RecallVenue(ApiCorporateevents.RecallVenueRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecallVenue for {Tenant}", requestContext.TenantId);
        if (request.Venue is { } record)
        {
            var snapshot = VenueMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.PauseLicenseAssignmentWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Venue?.Audit?.Etag ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                    {
                        ApiCorporateevents.Venue.LocatorOneofCase.Address => (request.Venue?.Address?.CountryCode ?? ""),
                        _ => ""
                    },
                },
                Counterparty = new ApiLicensing.EntitlementRef
                {
                    DisplayName = (request.Venue?.Owner?.DisplayName ?? ""),
                },
                Lines = { new ApiLicensing.LicenseAssignmentLine
                {
                    Description = (request.Venue?.Components?.GetValueOrDefault("primary")?.Description ?? ""),
                } },
            },
        };
        using var rpc0 = _licensingLicenseAssignmentWorkflowService.PauseLicenseAssignmentWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.WatchCorporateEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiCorporateevents.WatchCorporateEventsRequest.Types.Filter
            {
                CreatedAfter = (reply0.LicenseAssignment?.DueAt ?? new Timestamp()),
                Region = (reply0.LicenseAssignment?.BillingAddress?.Region ?? ""),
            },
            Since = (reply0.LicenseAssignment?.Audit?.CreatedAt ?? new Timestamp()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (request.Details?.Approver?.Email ?? ""),
            },
        };
        using var stream1 = _corporateeventsCorporateEventService.WatchCorporateEvents(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchCorporateEvents frame {Type}", change1.GetType().Name);
        }
        var response = new ApiCorporateevents.RecallVenueResponse
        {
            Venue = new ApiCorporateevents.Venue
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.LicenseAssignment?.Audit?.CreatedBy ?? ""),
                },
                Status = (request.Venue?.Status ?? (ApiCorporateevents.VenueStatus)0),
                VenueId = (request.VenueId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Venue?.VenueId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "RecallVenue", (request.Venue?.VenueId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.RejectVenueResponse> RejectVenue(ApiCorporateevents.RejectVenueRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RejectVenue for {Tenant}", requestContext.TenantId);
        if (request.Venue is { } record)
        {
            var snapshot = VenueMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLoyalty.SearchLoyaltyMembersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLoyalty.SearchLoyaltyMembersRequest.Types.Filter
            {
                CreatedAfter = (request.Venue?.Audit?.UpdatedAt ?? new Timestamp()),
                OwnerId = (request.Details?.Approver?.UserId ?? ""),
                Region = (request.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                {
                    ApiCorporateevents.Venue.LocatorOneofCase.Address => (request.Venue?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc0 = _loyaltyLoyaltyMemberService.SearchLoyaltyMembersAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiCorporateevents.RejectVenueResponse
        {
            Outcome = new ApiCorporateevents.RejectVenueResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
            Venue = new ApiCorporateevents.Venue
            {
                Capacity = (request.Venue?.Capacity ?? 0L),
                VenueId = (request.VenueId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Venue?.VenueId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "RejectVenue", (request.Venue?.VenueId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.SubmitVenueResponse> SubmitVenue(ApiCorporateevents.SubmitVenueRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitVenue for {Tenant}", requestContext.TenantId);
        if (request.Venue is { } record)
        {
            var snapshot = VenueMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.RetryCorporateEventWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Venue?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Location = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                    {
                        ApiCorporateevents.Venue.LocatorOneofCase.Address => (request.Venue?.Address?.PostalCode ?? ""),
                        _ => ""
                    },
                },
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _corporateeventsCorporateEventWorkflowService.RetryCorporateEventWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiCorporateevents.SubmitVenueResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Venue = new ApiCorporateevents.Venue
            {
                Capacity = (request.Venue?.Capacity ?? 0L),
                VenueId = (request.VenueId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Venue?.VenueId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "SubmitVenue", (request.Venue?.VenueId ?? ""), token);
        return response;
    }
}
