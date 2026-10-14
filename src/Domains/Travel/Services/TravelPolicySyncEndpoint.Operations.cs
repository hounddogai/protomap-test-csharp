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
using ApiAppointments = Acme.Appointments.V1;
using ApiCareplans = Acme.Careplans.V1;
using ApiCommon = Acme.Common.V1;
using ApiCustomers = Acme.Customers.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiReturns = Acme.Returns.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

public sealed partial class TravelPolicySyncEndpoint
{
    /// <summary>
    /// Returns a travel policy.
    /// </summary>
    public override async Task<ApiTravel.GetTravelPolicySyncStateResponse> GetTravelPolicySyncState(ApiTravel.GetTravelPolicySyncStateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TravelPolicyId, "travel_policy_id");
        _logger.LogInformation("Handling GetTravelPolicySyncState for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy", request.TravelPolicyId,
            "GetTravelPolicySyncState", "accepted", token);
        var input0 = new ApiReturns.EstimateReturnRequestRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ReturnRequest = new ApiReturns.ReturnRequest
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Counterparty = new ApiCustomers.CustomerRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _returnsReturnRequestCalculationService.EstimateReturnRequestAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiTravel.GetTravelPolicySyncStateResponse
        {
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.ReturnRequest?.Audit?.CreatedAt ?? new Timestamp()),
                },
                TravelPolicyId = (request.TravelPolicyId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists travel policies a page at a time.
    /// </summary>
    public override async Task<ApiTravel.ListTravelPolicyConflictsResponse> ListTravelPolicyConflicts(ApiTravel.ListTravelPolicyConflictsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListTravelPolicyConflicts for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy", "batch",
            "ListTravelPolicyConflicts", "accepted", token);
        var input0 = new ApiHelpdesk.ResumeSlaPolicyWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _helpdeskSlaPolicyWorkflowService.ResumeSlaPolicyWorkflowAsync);
        var input1 = new ApiCareplans.CompareCarePlanOptionsRequest
        {
            CarePlan = new ApiCareplans.CarePlan
            {
                Counterparty = new ApiAppointments.AppointmentRef
                {
                    DisplayName = (reply0.SlaPolicy?.Owner?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _sendListTravelPolicyConflicts1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.ListTravelPolicyConflictsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
            TravelPolicies = { new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.CarePlan?.Audit?.CreatedAt ?? new Timestamp()),
                },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "ListTravelPolicyConflicts", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pulls a travel policy.
    /// </summary>
    public override async Task<ApiTravel.PullTravelPoliciesResponse> PullTravelPolicies(ApiTravel.PullTravelPoliciesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PullTravelPolicies for {Tenant}", requestContext.TenantId);
        if (request.TravelPolicy is { } record)
        {
            var snapshot = TravelPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.SuggestTravelBookingsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiTravel.SuggestTravelBookingsRequest.Types.Filter
            {
                CreatedAfter = (request.TravelPolicy?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.TravelPolicy?.Owner?.UserId ?? ""),
            },
        };
        var named0 = _registry.Get<ApiTravel.TravelBookingQueryService.TravelBookingQueryServiceClient>("acme.travel.v1.TravelBookingQueryService");
        using var rpc0 = named0.SuggestTravelBookingsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.GenerateLicenseAssignmentReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.TravelPolicy ?? new ApiTravel.TravelPolicy()).ActionCase switch
            {
                ApiTravel.TravelPolicy.ActionOneofCase.Block => (request.TravelPolicy?.Block?.Reason ?? ""),
                _ => ""
            },
        };
        using var rpc1 = _licensingLicenseAssignmentReportingService.GenerateLicenseAssignmentReportAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDocuments.AnnotateSignatureVersionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.AnnotateSignatureVersionRequest.Types.Details
            {
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
                Version = (request.TravelPolicy?.Version ?? 0L),
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (request.TravelPolicy?.Owner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc2 = _documentsSignatureHistoryService.AnnotateSignatureVersionAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiTravel.PullTravelPoliciesResponse
        {
            Outcome = new ApiTravel.PullTravelPoliciesResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
                ReferenceId = (reply2.Outcome?.ReferenceId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply2.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Effective = (request.TravelPolicy?.Effective ?? new ApiCommon.TimeWindow()),
                TravelPolicyId = (request.TravelPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelPolicy?.TravelPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "PullTravelPolicies", (request.TravelPolicy?.TravelPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pushs a travel policy.
    /// </summary>
    public override async Task<ApiTravel.PushTravelPolicyResponse> PushTravelPolicy(ApiTravel.PushTravelPolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PushTravelPolicy for {Tenant}", requestContext.TenantId);
        if (request.TravelPolicy is { } record)
        {
            var snapshot = TravelPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.SimulateTripRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Trip = new ApiTravel.Trip
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.TravelPolicy?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (request.TravelPolicy?.Audit?.Etag ?? ""),
                },
                Lines = { new ApiTravel.TripLine
                {
                    Description = (request.TravelPolicy?.Description ?? ""),
                    Tax = new ApiTravel.TripLineTax
                    {
                        Amount = (request.TravelPolicy ?? new ApiTravel.TravelPolicy()).ActionCase switch
                        {
                            ApiTravel.TravelPolicy.ActionOneofCase.Discount => (request.TravelPolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc0 = _travelTripCalculationService.SimulateTripAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiNotifications.ScoreNotificationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Notification = new ApiNotifications.Notification
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Trip?.Audit?.Etag ?? ""),
                },
            },
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _notificationsNotificationValidationService.ScoreNotificationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = request.PushTravelPolicyToExplainEmploymentContractCalculation2(reply0, reply1);
        using var rpc2 = _employeesEmploymentContractCalculationService.ExplainEmploymentContractCalculationAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiTravel.PushTravelPolicyResponse
        {
            Outcome = new ApiTravel.PushTravelPolicyResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.Notification?.Audit?.UpdatedBy ?? ""),
                },
                Description = (request.TravelPolicy?.Description ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Trip?.Counterparty?.DisplayName ?? ""),
                },
                TravelPolicyId = (request.TravelPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelPolicy?.TravelPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "PushTravelPolicy", (request.TravelPolicy?.TravelPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Reconciles a travel policy.
    /// </summary>
    public override async Task<ApiTravel.ReconcileTravelPoliciesResponse> ReconcileTravelPolicies(ApiTravel.ReconcileTravelPoliciesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ReconcileTravelPolicies for {Tenant}", requestContext.TenantId);
        if (request.TravelPolicy is { } record)
        {
            var snapshot = TravelPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiIdentity.ScoreUserRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            User = new ApiIdentity.User
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.TravelPolicy?.Audit?.CreatedAt ?? new Timestamp()),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    DisplayName = (request.TravelPolicy?.Owner?.DisplayName ?? ""),
                },
            },
        };
        var reply0 = await SendReconcileTravelPolicies0Async(input0, token);
        var input1 = new ApiEmployees.ExportOrgUnitsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.ExportOrgUnitsRequest.Types.Filter
            {
                CreatedAfter = (reply0.User?.Audit?.UpdatedAt ?? new Timestamp()),
                Region = (reply0.User?.PrimaryAddress?.Region ?? ""),
            },
        };
        using var stream1 = _employeesOrgUnitQueryService.ExportOrgUnits(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportOrgUnits frame {Type}", change1.GetType().Name);
        }
        var response = new ApiTravel.ReconcileTravelPoliciesResponse
        {
            Outcome = new ApiTravel.ReconcileTravelPoliciesResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Name = (request.TravelPolicy?.Name ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.User?.RelationshipManager?.DisplayName ?? ""),
                },
                TravelPolicyId = (request.TravelPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelPolicy?.TravelPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "ReconcileTravelPolicies", (request.TravelPolicy?.TravelPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resolves a travel policy.
    /// </summary>
    public override async Task<ApiTravel.ResolveTravelPolicyConflictResponse> ResolveTravelPolicyConflict(ApiTravel.ResolveTravelPolicyConflictRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResolveTravelPolicyConflict for {Tenant}", requestContext.TenantId);
        if (request.TravelPolicy is { } record)
        {
            var snapshot = TravelPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.CalculateItineraryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Itinerary = new ApiTravel.Itinerary
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.TravelPolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiTravel.CalculateItineraryRequest>();
        using var rpc0 = _travelItineraryCalculationService.CalculateItineraryAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.ResolveTravelPolicyConflictResponse
        {
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Itinerary?.Audit?.Etag ?? ""),
                },
                Name = (request.TravelPolicy?.Name ?? ""),
                TravelPolicyId = (request.TravelPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelPolicy?.TravelPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "ResolveTravelPolicyConflict", (request.TravelPolicy?.TravelPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Syncs a travel policy.
    /// </summary>
    public override async Task<ApiTravel.SyncTravelPoliciesResponse> SyncTravelPolicies(ApiTravel.SyncTravelPoliciesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SyncTravelPolicies for {Tenant}", requestContext.TenantId);
        if (request.TravelPolicy is { } record)
        {
            var snapshot = TravelPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.ValidateTripRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Trip = new ApiTravel.Trip
            {
                Attachments = { new ApiTravel.TripAttachment
                {
                    Checksum = (request.Details?.Checksum ?? ""),
                } },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.TravelPolicy?.Audit?.CreatedBy ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.TravelPolicy?.Owner?.DisplayName ?? ""),
                },
                Lines = { new ApiTravel.TripLine
                {
                    Tax = new ApiTravel.TripLineTax
                    {
                        Amount = (request.TravelPolicy ?? new ApiTravel.TravelPolicy()).ActionCase switch
                        {
                            ApiTravel.TravelPolicy.ActionOneofCase.Discount => (request.TravelPolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc0 = _travelTripValidationService.ValidateTripAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.ExplainTripDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Trip = new ApiTravel.Trip
            {
                Lines = { new ApiTravel.TripLine
                {
                    Tax = new ApiTravel.TripLineTax
                    {
                        Amount = (request.TravelPolicy ?? new ApiTravel.TravelPolicy()).ActionCase switch
                        {
                            ApiTravel.TravelPolicy.ActionOneofCase.Discount => (request.TravelPolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc1 = _travelTripValidationService.ExplainTripDecisionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.SyncTravelPoliciesResponse
        {
            Outcome = new ApiTravel.SyncTravelPoliciesResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.Trip?.Audit?.CreatedBy ?? ""),
                },
                TravelPolicyId = (request.TravelPolicyId ?? ""),
                Version = (request.TravelPolicy?.Version ?? 0L),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelPolicy?.TravelPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "SyncTravelPolicies", (request.TravelPolicy?.TravelPolicyId ?? ""), token);
        return response;
    }
    private async Task<ApiIdentity.ScoreUserResponse> SendReconcileTravelPolicies0Async(ApiIdentity.ScoreUserRequest request, CancellationToken token)
    {
        return await _travelPolicySyncEndpointSendReconcileTravelPolicies0Port.SendAsync(request, token);
    }
}
