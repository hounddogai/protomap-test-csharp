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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFraud = Acme.Fraud.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

public sealed partial class ItinerarySyncEndpoint
{
    /// <summary>
    /// Returns an itinerary.
    /// </summary>
    public override async Task<ApiTravel.GetItinerarySyncStateResponse> GetItinerarySyncState(ApiTravel.GetItinerarySyncStateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ItineraryId, "itinerary_id");
        _logger.LogInformation("Handling GetItinerarySyncState for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "itinerary", request.ItineraryId,
            "GetItinerarySyncState", "accepted", token);
        var input0 = new ApiTravel.CountTravelBookingTotalsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiTravel.CountTravelBookingTotalsRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiTravel.CountTravelBookingTotalsRequest>(json0);
        using var rpc0 = _travelTravelBookingReportingService.CountTravelBookingTotalsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.GetItinerarySyncStateResponse
        {
            Itinerary = new ApiTravel.Itinerary
            {
                ItineraryId = (request.ItineraryId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Lists itineraries a page at a time.
    /// </summary>
    public override async Task<ApiTravel.ListItineraryConflictsResponse> ListItineraryConflicts(ApiTravel.ListItineraryConflictsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListItineraryConflicts for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "itinerary", "batch",
            "ListItineraryConflicts", "accepted", token);
        var response = new ApiTravel.ListItineraryConflictsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "itinerary",
            "audit", "ListItineraryConflicts", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pulls an itinerary.
    /// </summary>
    public override async Task<ApiTravel.PullItinerariesResponse> PullItineraries(ApiTravel.PullItinerariesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PullItineraries for {Tenant}", requestContext.TenantId);
        if (request.Itinerary is { } record)
        {
            var snapshot = ItineraryMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAudit.CheckAuditEventEligibilityRequest
        {
            AuditEvent = new ApiAudit.AuditEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiAudit.AuditTrailRef
                {
                    DisplayName = (request.Itinerary?.Counterparty?.DisplayName ?? ""),
                    Number = (request.Itinerary?.Number ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        using var rpc0 = _auditAuditEventValidationService.CheckAuditEventEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiFraud.UpdateRiskAssessmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            RiskAssessment = new ApiFraud.RiskAssessment
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.AuditEvent?.Subject?.DisplayName ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Itinerary?.Audit?.CreatedBy ?? ""),
                    Etag = (request.Itinerary?.Audit?.Etag ?? ""),
                    UpdatedBy = (request.Itinerary?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        using var rpc1 = _fraudRiskAssessmentService.UpdateRiskAssessmentAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.PullItinerariesResponse
        {
            Itinerary = new ApiTravel.Itinerary
            {
                BillingAddress = (request.Itinerary?.BillingAddress ?? new ApiCommon.PostalAddress()),
                Counterparty = new ApiTravel.TravelBookingRef
                {
                    Number = (reply0.AuditEvent?.Subject?.Number ?? ""),
                },
                ItineraryId = (request.ItineraryId ?? ""),
            },
            Outcome = new ApiTravel.PullItinerariesResponse.Types.Outcome
            {
                EffectiveAt = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
                Status = (reply1.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Itinerary?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "itinerary",
            "audit", "PullItineraries", (request.Itinerary?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pushs an itinerary.
    /// </summary>
    public override async Task<ApiTravel.PushItineraryResponse> PushItinerary(ApiTravel.PushItineraryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PushItinerary for {Tenant}", requestContext.TenantId);
        if (request.Itinerary is { } record)
        {
            var snapshot = ItineraryMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.CountDocumentFoldersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.CountDocumentFoldersRequest.Types.Filter
            {
                CreatedAfter = (request.Itinerary?.IssuedAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Region = (request.Itinerary?.BillingAddress?.Region ?? ""),
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _documentsDocumentFolderQueryService.CountDocumentFoldersAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiDevices.ApproveDevicePolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Itinerary?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Itinerary?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _devicesDevicePolicyApprovalService.ApproveDevicePolicyAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiTravel.PushItineraryResponse
        {
            Itinerary = new ApiTravel.Itinerary
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.DevicePolicy?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiTravel.TravelBookingRef
                {
                    Number = (request.Itinerary?.Number ?? ""),
                },
                ItineraryId = (request.ItineraryId ?? ""),
            },
            Outcome = new ApiTravel.PushItineraryResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "itinerary",
            "audit", "PushItinerary", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Reconciles an itinerary.
    /// </summary>
    public override async Task<ApiTravel.ReconcileItinerariesResponse> ReconcileItineraries(ApiTravel.ReconcileItinerariesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ReconcileItineraries for {Tenant}", requestContext.TenantId);
        if (request.Itinerary is { } record)
        {
            var snapshot = ItineraryMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.SimulateEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.SimulateEmploymentContractRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Itinerary?.BillingAddress?.CountryCode ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _employeesEmploymentContractCalculationService.SimulateEmploymentContractAsync);
        var input1 = new ApiTravel.ScoreTravelPolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.ScoreTravelPolicyRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.EmploymentContract?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
        };
        using var rpc1 = _sendReconcileItineraries1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiTravel.SyncTravelPoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.SyncTravelPoliciesRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.EmploymentContract?.Counterparty?.DisplayName ?? ""),
                    Email = (reply1.TravelPolicy?.Owner?.Email ?? ""),
                },
                Route = new ApiTravel.TravelPolicyRouteAction
                {
                    Target = (reply1.TravelPolicy ?? new ApiTravel.TravelPolicy()).ActionCase switch
                    {
                        ApiTravel.TravelPolicy.ActionOneofCase.Route => (reply1.TravelPolicy?.Route?.Target ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc2 = _travelTravelPolicySyncService.SyncTravelPoliciesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiDevices.PauseManagedDeviceWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Address = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Itinerary?.BillingAddress?.PostalCode ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Itinerary?.Counterparty?.DisplayName ?? ""),
                    Email = (reply0.EmploymentContract?.Counterparty?.Email ?? ""),
                },
            },
        };
        var descriptor3 = new Method<ApiDevices.PauseManagedDeviceWorkflowRequest, ApiDevices.PauseManagedDeviceWorkflowResponse>(
            MethodType.Unary, "acme.devices.v1" + ".ManagedDeviceWorkflowService", "PauseManagedDeviceWorkflow",
            Wire.Marshaller<ApiDevices.PauseManagedDeviceWorkflowRequest>(), Wire.Marshaller<ApiDevices.PauseManagedDeviceWorkflowResponse>());
        using var rpc3 = _invoker.AsyncUnaryCall(descriptor3, null,
            new CallOptions(cancellationToken: token), input3);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiTravel.ReconcileItinerariesResponse
        {
            Itinerary = new ApiTravel.Itinerary
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.EmploymentContract?.Audit?.UpdatedBy ?? ""),
                },
                IssuedAt = (request.Itinerary?.IssuedAt ?? new Timestamp()),
                ItineraryId = (request.ItineraryId ?? ""),
                Lines = { new ApiTravel.ItineraryLine
                {
                    Amount = (reply1.TravelPolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                } },
            },
            Outcome = new ApiTravel.ReconcileItinerariesResponse.Types.Outcome
            {
                Message = (reply3.Result?.Message ?? ""),
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "itinerary",
            "audit", "ReconcileItineraries", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resolves an itinerary.
    /// </summary>
    public override async Task<ApiTravel.ResolveItineraryConflictResponse> ResolveItineraryConflict(ApiTravel.ResolveItineraryConflictRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResolveItineraryConflict for {Tenant}", requestContext.TenantId);
        if (request.Itinerary is { } record)
        {
            var snapshot = ItineraryMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.CheckEmploymentContractEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (request.Itinerary?.BillingAddress?.Locality ?? ""),
                },
                Card = new ApiEmployees.EmploymentContractCardSettlement
                {
                    HolderName = (request.Itinerary ?? new ApiTravel.Itinerary()).PaymentCase switch
                    {
                        ApiTravel.Itinerary.SettlementOneofCase.Card => (request.Itinerary?.Card?.HolderName ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _employeesEmploymentContractValidationService.CheckEmploymentContractEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiTravel.ResolveItineraryConflictRequest, ApiTravel.FindTripsRequest>(value => new ApiTravel.FindTripsRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiTravel.FindTripsRequest.Types.Filter
            {
                Region = (value.Itinerary?.BillingAddress?.Region ?? ""),
            },
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _travelTripQueryService.FindTripsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.ResolveItineraryConflictResponse
        {
            Itinerary = new ApiTravel.Itinerary
            {
                ItineraryId = (request.ItineraryId ?? ""),
                Status = (request.Itinerary?.Status ?? (ApiTravel.ItineraryStatus)0),
            },
            Outcome = new ApiTravel.ResolveItineraryConflictResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "itinerary",
            "audit", "ResolveItineraryConflict", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Exchanges changes of itineraries with a peer.
    /// </summary>
    public override async Task StreamItineraryChanges(IAsyncStreamReader<ApiTravel.StreamItineraryChangesRequest> requests, IServerStreamWriter<ApiTravel.StreamItineraryChangesResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        await foreach (var request in requests.ReadAllAsync(token))
        {
            RequestGuard.Tenant(request.Context);
            var requestContext = request.Context;
            _logger.LogInformation("Handling StreamItineraryChanges for {Tenant}", requestContext.TenantId);
            if (request.Change is { } record)
            {
                var snapshot = ItineraryMapper.ToRecord(requestContext.TenantId, record);
                await _repository.SaveAsync(snapshot, token);
            }
            var input0 = new ApiDocuments.EstimateSignatureRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiDocuments.EstimateSignatureRequest.Types.Details
                {
                    Destination = new ApiCommon.PostalAddress
                    {
                        CountryCode = request.FrameCase switch
                        {
                            ApiTravel.StreamItineraryChangesRequest.FrameOneofCase.Change => (request.Change?.BillingAddress?.CountryCode ?? ""),
                            _ => ""
                        },
                        Locality = request.FrameCase switch
                        {
                            ApiTravel.StreamItineraryChangesRequest.FrameOneofCase.Change => (request.Change?.BillingAddress?.Locality ?? ""),
                            _ => ""
                        },
                    },
                },
                Signature = new ApiDocuments.Signature
                {
                    Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                    Subject = new ApiDocuments.DocumentRef
                    {
                        Number = request.FrameCase switch
                        {
                            ApiTravel.StreamItineraryChangesRequest.FrameOneofCase.Change => (request.Change?.Counterparty?.Number ?? ""),
                            _ => ""
                        },
                    },
                },
            };
            using var rpc0 = _documentsSignatureCalculationService.EstimateSignatureAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            var input1 = request.StreamItineraryChangesToResolveOrgUnits1();
            using var rpc1 = _employeesOrgUnitQueryService.ResolveOrgUnitsAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            var response = new ApiTravel.StreamItineraryChangesResponse
            {
                Itinerary = new ApiTravel.Itinerary
                {
                    IssuedAt = (request.Change?.IssuedAt ?? new Timestamp()),
                    Total = (request.Change?.Total ?? new ApiCommon.Money()),
                },
                Result = new ApiCommon.OperationResult
                {
                    Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    Status = (reply0.Result?.Status ?? ""),
                },
                Sequence = (reply0.Signature?.Sequence ?? 0L),
            };
            _logger.LogDebug("Local audit detail {Detail}", (request.Cursor ?? ""));
            await _repository.RecordAsync(requestContext.TenantId, "itinerary",
                "audit", "StreamItineraryChanges", (request.Cursor ?? ""), token);
            await responses.WriteAsync(response, token);
        }
    }
    /// <summary>
    /// Syncs an itinerary.
    /// </summary>
    public override async Task<ApiTravel.SyncItinerariesResponse> SyncItineraries(ApiTravel.SyncItinerariesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SyncItineraries for {Tenant}", requestContext.TenantId);
        if (request.Itinerary is { } record)
        {
            var snapshot = ItineraryMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.GenerateTravelBookingReportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
            TravelBookingId = (request.Itinerary?.Counterparty?.TravelBookingId ?? ""),
        };
        var reply0 = await SendSyncItineraries0Async(input0, token);
        await _bus.PublishAsync(new TransportNotification<ApiTravel.GenerateTravelBookingReportRequest>("acme.travel.v1.ItinerarySyncService/SyncItineraries/0", input0.Clone()), token);
        var response = new ApiTravel.SyncItinerariesResponse
        {
            Itinerary = new ApiTravel.Itinerary
            {
                Counterparty = (request.Itinerary?.Counterparty ?? new ApiTravel.TravelBookingRef()),
                ItineraryId = (request.ItineraryId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Itinerary?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "itinerary",
            "audit", "SyncItineraries", (request.Itinerary?.VoucherCode ?? ""), token);
        return response;
    }
    private async Task<ApiTravel.GenerateTravelBookingReportResponse> SendSyncItineraries0Async(ApiTravel.GenerateTravelBookingReportRequest request, CancellationToken token)
    {
        return await _itinerarySyncEndpointSendSyncItineraries0Port.SendAsync(request, token);
    }
}
