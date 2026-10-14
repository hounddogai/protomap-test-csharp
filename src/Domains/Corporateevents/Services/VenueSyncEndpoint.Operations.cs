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
using ApiAudit = Acme.Audit.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Corporateevents;

public sealed partial class VenueSyncEndpoint
{
    /// <summary>
    /// Returns a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.GetVenueSyncStateResponse> GetVenueSyncState(ApiCorporateevents.GetVenueSyncStateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.VenueId, "venue_id");
        _logger.LogInformation("Handling GetVenueSyncState for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "venue", request.VenueId,
            "GetVenueSyncState", "accepted", token);
        var response = new ApiCorporateevents.GetVenueSyncStateResponse
        {
            Venue = new ApiCorporateevents.Venue
            {
                VenueId = (request.VenueId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists venues a page at a time.
    /// </summary>
    public override async Task<ApiCorporateevents.ListVenueConflictsResponse> ListVenueConflicts(ApiCorporateevents.ListVenueConflictsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListVenueConflicts for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "venue", "batch",
            "ListVenueConflicts", "accepted", token);
        var input0 = new ApiCorporateevents.QuoteCorporateEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
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
                } },
            },
            Details = new ApiCorporateevents.QuoteCorporateEventRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiCorporateevents.QuoteCorporateEventRequest>(json0);
        using var rpc0 = _corporateeventsCorporateEventCalculationService.QuoteCorporateEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiCorporateevents.ListVenueConflictsResponse
        {
            Venues = { new ApiCorporateevents.Venue
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.CorporateEvent?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.CorporateEvent?.Organizer?.Email ?? ""),
                },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "ListVenueConflicts", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pulls a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.PullVenuesResponse> PullVenues(ApiCorporateevents.PullVenuesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PullVenues for {Tenant}", requestContext.TenantId);
        if (request.Venue is { } record)
        {
            var snapshot = VenueMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.SyncDocumentTemplatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SyncDocumentTemplatesRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (request.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                    {
                        ApiCorporateevents.Venue.LocatorOneofCase.Address => (request.Venue?.Address?.Locality ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _documentsDocumentTemplateSyncService.SyncDocumentTemplatesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.ScheduleDocumentFolderExportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DocumentFolderId = (reply0.DocumentTemplate?.Counterparty?.DocumentFolderId ?? ""),
        };
        using var rpc1 = _documentsDocumentFolderExportService.ScheduleDocumentFolderExportAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.PullVenuesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
                Status = (reply0.Outcome?.Status ?? ""),
            },
            Venue = new ApiCorporateevents.Venue
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.DocumentTemplate?.Audit?.Etag ?? ""),
                },
                Name = (request.Venue?.Name ?? ""),
                VenueId = (request.VenueId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Venue?.VenueId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "PullVenues", (request.Venue?.VenueId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pushs a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.PushVenueResponse> PushVenue(ApiCorporateevents.PushVenueRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PushVenue for {Tenant}", requestContext.TenantId);
        if (request.Venue is { } record)
        {
            var snapshot = VenueMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.NormalizeRegistrationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Registration = new ApiCorporateevents.Registration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Venue?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _corporateeventsRegistrationValidationService.NormalizeRegistrationAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiCorporateevents.SummarizeRegistrationsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
            RegistrationId = (reply0.Registration?.RegistrationId ?? ""),
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _corporateeventsRegistrationQueryService.SummarizeRegistrationsAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiLicensing.RejectSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (reply0.Registration?.Contact?.Phone ?? ""),
                },
            },
        };
        var reply2 = await Workflow.SendAsync(input2, token, _licensingSoftwareLicenseApprovalService.RejectSoftwareLicenseAsync);
        var input3 = new ApiFeatureflags.SuggestFeatureFlagsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiFeatureflags.SuggestFeatureFlagsRequest.Types.Filter
            {
                Region = (request.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                {
                    ApiCorporateevents.Venue.LocatorOneofCase.Address => (request.Venue?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc3 = _featureflagsFeatureFlagQueryService.SuggestFeatureFlagsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiFeatureflags.SuggestFeatureFlagsRequest>("acme.corporateevents.v1.VenueSyncService/PushVenue/3", input3.Clone()), token);
        var response = new ApiCorporateevents.PushVenueResponse
        {
            Outcome = new ApiCorporateevents.PushVenueResponse.Types.Outcome
            {
                EffectiveAt = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
                Message = (reply3.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
            Venue = new ApiCorporateevents.Venue
            {
                Specs = { (request.Venue?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                VenueId = (request.VenueId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Venue?.VenueId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "PushVenue", (request.Venue?.VenueId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Reconciles a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.ReconcileVenuesResponse> ReconcileVenues(ApiCorporateevents.ReconcileVenuesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ReconcileVenues for {Tenant}", requestContext.TenantId);
        if (request.Venue is { } record)
        {
            var snapshot = VenueMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.QuoteVenueRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Venue = new ApiCorporateevents.Venue
            {
                Address = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                    {
                        ApiCorporateevents.Venue.LocatorOneofCase.Address => (request.Venue?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()),
                        _ => new Google.Protobuf.Collections.RepeatedField<string>()
                    } },
                    Recipient = (request.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                    {
                        ApiCorporateevents.Venue.LocatorOneofCase.Address => (request.Venue?.Address?.Recipient ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _corporateeventsVenueCalculationService.QuoteVenueAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.LintRegistrationsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Registration = new ApiCorporateevents.Registration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Venue?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Venue?.Owner?.Email ?? ""),
                },
            },
        };
        var descriptor1 = new Method<ApiCorporateevents.LintRegistrationsRequest, ApiCorporateevents.LintRegistrationsResponse>(
            MethodType.Unary, "acme.corporateevents.v1" + ".RegistrationValidationService", "LintRegistrations",
            Wire.Marshaller<ApiCorporateevents.LintRegistrationsRequest>(), Wire.Marshaller<ApiCorporateevents.LintRegistrationsResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.ReconcileVenuesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Result?.Status ?? ""),
            },
            Venue = new ApiCorporateevents.Venue
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.Registration?.Audit?.Etag ?? ""),
                },
                Owner = (request.Venue?.Owner ?? new ApiIdentity.UserRef()),
                Status = (reply0.Venue?.Status ?? (ApiCorporateevents.VenueStatus)0),
                VenueId = (request.VenueId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Venue?.VenueId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "ReconcileVenues", (request.Venue?.VenueId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resolves a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.ResolveVenueConflictResponse> ResolveVenueConflict(ApiCorporateevents.ResolveVenueConflictRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResolveVenueConflict for {Tenant}", requestContext.TenantId);
        if (request.Venue is { } record)
        {
            var snapshot = VenueMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.ExplainRegistrationCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Registration = new ApiCorporateevents.Registration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Venue?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Holder = new ApiCorporateevents.CorporateEventRef
                {
                    DisplayName = (request.Venue?.Owner?.DisplayName ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiCorporateevents.ExplainRegistrationCalculationRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _corporateeventsRegistrationCalculationService.ExplainRegistrationCalculationAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiCorporateevents.ResolveVenueConflictResponse
        {
            Outcome = new ApiCorporateevents.ResolveVenueConflictResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Venue = new ApiCorporateevents.Venue
            {
                Specs = { (request.Venue?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                VenueId = (request.VenueId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Venue?.VenueId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "ResolveVenueConflict", (request.Venue?.VenueId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Exchanges changes of venues with a peer.
    /// </summary>
    public override async Task StreamVenueChanges(IAsyncStreamReader<ApiCorporateevents.StreamVenueChangesRequest> requests, IServerStreamWriter<ApiCorporateevents.StreamVenueChangesResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        await foreach (var request in requests.ReadAllAsync(token))
        {
            RequestGuard.Tenant(request.Context);
            var requestContext = request.Context;
            _logger.LogInformation("Handling StreamVenueChanges for {Tenant}", requestContext.TenantId);
            if (request.Change is { } record)
            {
                var snapshot = VenueMapper.ToRecord(requestContext.TenantId, record);
                await _repository.SaveAsync(snapshot, token);
            }
            var input0 = new ApiDocuments.CheckDocumentEligibilityRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Document = new ApiDocuments.Document
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = request.FrameCase switch
                        {
                            ApiCorporateevents.StreamVenueChangesRequest.FrameOneofCase.Change => (request.Change?.Audit?.CreatedAt ?? new Timestamp()),
                            _ => new Timestamp()
                        },
                    },
                },
            };
            using var rpc0 = _documentsDocumentValidationService.CheckDocumentEligibilityAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            var input1 = new ApiCorporateevents.CompareRegistrationOptionsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiCorporateevents.CompareRegistrationOptionsRequest.Types.Details
                {
                    Destination = new ApiCommon.PostalAddress
                    {
                        CountryCode = request.FrameCase switch
                        {
                            ApiCorporateevents.StreamVenueChangesRequest.FrameOneofCase.Change => (request.Change?.Address?.CountryCode ?? ""),
                            _ => ""
                        },
                    },
                },
                Registration = new ApiCorporateevents.Registration
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        Etag = (reply0.Document?.Audit?.Etag ?? ""),
                        UpdatedBy = (reply0.Document?.Audit?.UpdatedBy ?? ""),
                    },
                },
            };
            using var rpc1 = _corporateeventsRegistrationCalculationService.CompareRegistrationOptionsAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            var input2 = request.StreamVenueChangesToRetryDevicePolicyWorkflow2(reply0, reply1);
            using var rpc2 = _devicesDevicePolicyWorkflowService.RetryDevicePolicyWorkflowAsync(input2, cancellationToken: token);
            var reply2 = await rpc2.ResponseAsync;
            var response = new ApiCorporateevents.StreamVenueChangesResponse
            {
                Result = new ApiCommon.OperationResult
                {
                    Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    Message = (reply2.Outcome?.Message ?? ""),
                    Status = (reply1.Result?.Status ?? ""),
                },
                Venue = new ApiCorporateevents.Venue
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedBy = (reply1.Registration?.Audit?.CreatedBy ?? ""),
                    },
                    Owner = (request.Change?.Owner ?? new ApiIdentity.UserRef()),
                    Specs = { (request.Change?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                },
            };
            _logger.LogDebug("Local audit detail {Detail}", (request.Cursor ?? ""));
            await _repository.RecordAsync(requestContext.TenantId, "venue",
                "audit", "StreamVenueChanges", (request.Cursor ?? ""), token);
            await responses.WriteAsync(response, token);
        }
    }
    /// <summary>
    /// Syncs a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.SyncVenuesResponse> SyncVenues(ApiCorporateevents.SyncVenuesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SyncVenues for {Tenant}", requestContext.TenantId);
        if (request.Venue is { } record)
        {
            var snapshot = VenueMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAudit.CountAuditEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiAudit.CountAuditEventsRequest.Types.Filter
            {
                Region = (request.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                {
                    ApiCorporateevents.Venue.LocatorOneofCase.Address => (request.Venue?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc0 = _auditAuditEventQueryService.CountAuditEventsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.SyncDocumentTemplatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SyncDocumentTemplatesRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                    {
                        ApiCorporateevents.Venue.LocatorOneofCase.Address => (request.Venue?.Address?.PostalCode ?? ""),
                        _ => ""
                    },
                    Recipient = (request.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                    {
                        ApiCorporateevents.Venue.LocatorOneofCase.Address => (request.Venue?.Address?.Recipient ?? ""),
                        _ => ""
                    },
                },
                Lines = { new ApiDocuments.DocumentTemplateLine
                {
                    Description = (request.Venue?.Components?.GetValueOrDefault("primary")?.Description ?? ""),
                } },
            },
        };
        using var rpc1 = _documentsDocumentTemplateSyncService.SyncDocumentTemplatesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.SyncVenuesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Venue = new ApiCorporateevents.Venue
            {
                Code = (request.Venue?.Code ?? ""),
                VenueId = (request.VenueId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Venue?.VenueId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "SyncVenues", (request.Venue?.VenueId ?? ""), token);
        return response;
    }
}
