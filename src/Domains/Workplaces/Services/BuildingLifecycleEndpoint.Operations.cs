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
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiCustomers = Acme.Customers.V1;
using ApiDevices = Acme.Devices.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFraud = Acme.Fraud.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiKeymanagement = Acme.Keymanagement.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

public sealed partial class BuildingLifecycleEndpoint
{
    /// <summary>
    /// Activates a building.
    /// </summary>
    public override async Task<ApiWorkplaces.ActivateBuildingResponse> ActivateBuilding(ApiWorkplaces.ActivateBuildingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.BuildingId, "building_id");
        _logger.LogInformation("Handling ActivateBuilding for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "building", request.BuildingId,
            "ActivateBuilding", "accepted", token);
        var input0 = new ApiEmployees.NormalizePositionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiEmployees.NormalizePositionRequest>();
        using var rpc0 = _employeesPositionValidationService.NormalizePositionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiFraud.RecalculateRiskAssessmentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            RiskAssessment = new ApiFraud.RiskAssessment
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                },
                Customer = new ApiCustomers.CustomerRef
                {
                    Email = (reply0.Position?.Owner?.Email ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiFraud.RecalculateRiskAssessmentsRequest>(json1);
        using var rpc1 = _fraudRiskAssessmentCalculationService.RecalculateRiskAssessmentsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.ActivateBuildingResponse
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
                            DisplayName = (reply0.Position?.Owner?.DisplayName ?? ""),
                            Email = (reply1.RiskAssessment?.Requester?.Email ?? ""),
                        },
                    } },
                } },
            },
            Outcome = new ApiWorkplaces.ActivateBuildingResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "ActivateBuilding", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Expires a building.
    /// </summary>
    public override async Task<ApiWorkplaces.ExpireBuildingResponse> ExpireBuilding(ApiWorkplaces.ExpireBuildingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.BuildingId, "building_id");
        _logger.LogInformation("Handling ExpireBuilding for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "building", request.BuildingId,
            "ExpireBuilding", "accepted", token);
        var input0 = new ApiWorkplaces.CompareVisitorPassOptionsRequest();
        input0.Annotations.Add((request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()));
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.VisitorPass = new ApiWorkplaces.VisitorPass
        {
            Counterparty = new ApiEmployees.EmployeeRef
            {
                DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                Email = (request.Details?.NewOwner?.Email ?? ""),
            },
        };
        using var rpc0 = _workplacesVisitorPassCalculationService.CompareVisitorPassOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiWorkplaces.RecalculateVisitorPassesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiWorkplaces.RecalculateVisitorPassesRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.VisitorPass?.BillingAddress?.Recipient ?? ""),
                    Region = (reply0.VisitorPass?.BillingAddress?.Region ?? ""),
                },
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                Bank = new ApiWorkplaces.VisitorPassBankSettlement
                {
                    AccountHolder = (reply0.VisitorPass?.Bank?.AccountHolder ?? ""),
                    Bic = (reply0.VisitorPass?.Bank?.Bic ?? ""),
                },
            },
        };
        using var rpc1 = _workplacesVisitorPassCalculationService.RecalculateVisitorPassesAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        var input2 = new ApiWorkplaces.FindBuildingsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiWorkplaces.FindBuildingsRequest.Types.Filter
            {
                Region = (reply0.VisitorPass?.BillingAddress?.Region ?? ""),
            },
        };
        using var rpc2 = _workplacesBuildingQueryService.FindBuildingsAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        await Task.WhenAll(pending1, pending2);
        var reply1 = await pending1;
        var reply2 = await pending2;
        var response = new ApiWorkplaces.ExpireBuildingResponse
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
                            CountryCode = (reply0.VisitorPass?.BillingAddress?.CountryCode ?? ""),
                            Region = (reply1.VisitorPass?.BillingAddress?.Region ?? ""),
                        },
                    } },
                    Location = new ApiCommon.PostalAddress
                    {
                        Locality = (reply0.VisitorPass?.BillingAddress?.Locality ?? ""),
                    },
                } },
                Location = new ApiCommon.PostalAddress
                {
                    Locality = (reply1.VisitorPass?.BillingAddress?.Locality ?? ""),
                },
            },
            Outcome = new ApiWorkplaces.ExpireBuildingResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "ExpireBuilding", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a building.
    /// </summary>
    public override async Task<ApiWorkplaces.GetBuildingLifecycleResponse> GetBuildingLifecycle(ApiWorkplaces.GetBuildingLifecycleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.BuildingId, "building_id");
        _logger.LogInformation("Handling GetBuildingLifecycle for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "building", request.BuildingId,
            "GetBuildingLifecycle", "accepted", token);
        var input0 = new ApiWorkplaces.ExplainVisitorPassCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiWorkplaces.ExplainVisitorPassCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _workplacesVisitorPassCalculationService.ExplainVisitorPassCalculationAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiWorkplaces.GetBuildingLifecycleResponse
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
                            Email = (reply0.VisitorPass?.Counterparty?.Email ?? ""),
                        },
                    } },
                } },
            },
        };
        return response;
    }
    /// <summary>
    /// Reinstates a building.
    /// </summary>
    public override async Task<ApiWorkplaces.ReinstateBuildingResponse> ReinstateBuilding(ApiWorkplaces.ReinstateBuildingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.BuildingId, "building_id");
        _logger.LogInformation("Handling ReinstateBuilding for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "building", request.BuildingId,
            "ReinstateBuilding", "accepted", token);
        var input0 = new ApiWorkplaces.RecalculateBuildingsRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Manager = new ApiIdentity.UserRef
                        {
                            Email = (request.Details?.NewOwner?.Email ?? ""),
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply0 = await Workflow.SendAsync(input0, token, _workplacesBuildingCalculationService.RecalculateBuildingsAsync);
        var input1 = new ApiEmployees.CalculateEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.CalculateEmploymentContractRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.Building?.Location?.Locality ?? ""),
                },
            },
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Building?.Audit?.CreatedBy ?? ""),
                    UpdatedBy = (reply0.Building?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        using var rpc1 = _sendReinstateBuilding1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.ReinstateBuildingResponse
        {
            Building = new ApiWorkplaces.Building
            {
                BuildingId = (request.BuildingId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply1.EmploymentContract?.BillingAddress?.PostalCode ?? ""),
                },
            },
            Outcome = new ApiWorkplaces.ReinstateBuildingResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "ReinstateBuilding", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Renews a building.
    /// </summary>
    public override async Task<ApiWorkplaces.RenewBuildingResponse> RenewBuilding(ApiWorkplaces.RenewBuildingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.BuildingId, "building_id");
        _logger.LogInformation("Handling RenewBuilding for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "building", request.BuildingId,
            "RenewBuilding", "accepted", token);
        var input0 = new ApiTravel.RetryTravelBookingWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.RetryTravelBookingWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
        };
        var named0 = _registry.Get<ApiTravel.TravelBookingWorkflowService.TravelBookingWorkflowServiceClient>("acme.travel.v1.TravelBookingWorkflowService");
        using var rpc0 = named0.RetryTravelBookingWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiWorkplaces.PeekBuildingRequest
        {
            BuildingId = (request.BuildingId ?? ""),
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var descriptor1 = new Method<ApiWorkplaces.PeekBuildingRequest, ApiWorkplaces.PeekBuildingResponse>(
            MethodType.Unary, "acme.workplaces.v1" + ".BuildingQueryService", "PeekBuilding",
            Wire.Marshaller<ApiWorkplaces.PeekBuildingRequest>(), Wire.Marshaller<ApiWorkplaces.PeekBuildingResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.RenewBuildingResponse
        {
            Building = new ApiWorkplaces.Building
            {
                BuildingId = (request.BuildingId ?? ""),
                Children = { new ApiWorkplaces.Building
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        UpdatedAt = (reply1.Building?.Audit?.UpdatedAt ?? new Timestamp()),
                    },
                    Children = { new ApiWorkplaces.Building
                    {
                        Children = { new ApiWorkplaces.Building
                        {
                            Location = (reply1.Building?.Location ?? new ApiCommon.PostalAddress()),
                        } },
                    } },
                } },
                Location = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.TravelBooking?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    PostalCode = (reply0.TravelBooking?.BillingAddress?.PostalCode ?? ""),
                },
            },
            Outcome = new ApiWorkplaces.RenewBuildingResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "RenewBuilding", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules a building.
    /// </summary>
    public override async Task<ApiWorkplaces.ScheduleBuildingExpiryResponse> ScheduleBuildingExpiry(ApiWorkplaces.ScheduleBuildingExpiryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScheduleBuildingExpiry for {Tenant}", requestContext.TenantId);
        if (request.Building is { } record)
        {
            var snapshot = BuildingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiKeymanagement.ResolveCryptoKeysRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _keymanagementCryptoKeyQueryService.ResolveCryptoKeysAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiAuthorization.UpdateAccessPolicyRequest
        {
            AccessPolicy = new ApiAuthorization.AccessPolicy
            {
                Block = new ApiAuthorization.AccessPolicyBlockAction
                {
                    Reason = (request.Reason ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Building?.Manager?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _authorizationAccessPolicyService.UpdateAccessPolicyAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var input2 = new ApiDevices.CancelDevicePolicyWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Discount = new ApiDevices.DevicePolicyDiscountAction
                {
                    Amount = (reply1.AccessPolicy ?? new ApiAuthorization.AccessPolicy()).ActionCase switch
                    {
                        ApiAuthorization.AccessPolicy.ActionOneofCase.Discount => (reply1.AccessPolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                        _ => new ApiCommon.Money()
                    },
                    Percent = (reply1.AccessPolicy ?? new ApiAuthorization.AccessPolicy()).ActionCase switch
                    {
                        ApiAuthorization.AccessPolicy.ActionOneofCase.Discount => (reply1.AccessPolicy?.Discount?.Percent ?? 0d),
                        _ => 0d
                    },
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (reply1.AccessPolicy?.Owner?.UserId ?? ""),
                },
            },
        };
        using var rpc2 = _devicesDevicePolicyWorkflowService.CancelDevicePolicyWorkflowAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiWorkplaces.ScheduleBuildingExpiryResponse
        {
            Building = new ApiWorkplaces.Building
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.AccessPolicy?.Audit?.Etag ?? ""),
                },
                BuildingId = (request.BuildingId ?? ""),
                Children = { new ApiWorkplaces.Building
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (reply2.DevicePolicy?.Audit?.CreatedAt ?? new Timestamp()),
                    },
                } },
                Manager = new ApiIdentity.UserRef
                {
                    UserId = (reply1.AccessPolicy?.Owner?.UserId ?? ""),
                },
                ParentId = (request.Building?.ParentId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Step ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "ScheduleBuildingExpiry", (request.Details?.Step ?? ""), token);
        return response;
    }
    /// <summary>
    /// Suspends a building.
    /// </summary>
    public override async Task<ApiWorkplaces.SuspendBuildingResponse> SuspendBuilding(ApiWorkplaces.SuspendBuildingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.BuildingId, "building_id");
        _logger.LogInformation("Handling SuspendBuilding for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "building", request.BuildingId,
            "SuspendBuilding", "accepted", token);
        var input0 = request.SuspendBuildingToRejectTravelBooking0();
        using var rpc0 = _travelTravelBookingApprovalService.RejectTravelBookingAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiWorkplaces.LintBuildingsRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            Etag = (reply0.TravelBooking?.Audit?.Etag ?? ""),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            Locality = (reply0.TravelBooking?.BillingAddress?.Locality ?? ""),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply1 = await SendSuspendBuilding1Async(input1, token);
        var response = new ApiWorkplaces.SuspendBuildingResponse
        {
            Building = new ApiWorkplaces.Building
            {
                BuildingId = (request.BuildingId ?? ""),
                Children = { new ApiWorkplaces.Building
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedBy = (reply0.TravelBooking?.Audit?.CreatedBy ?? ""),
                    },
                    Location = (reply1.Building?.Location ?? new ApiCommon.PostalAddress()),
                    Status = (reply1.Building?.Status ?? (ApiWorkplaces.BuildingStatus)0),
                } },
            },
            Outcome = new ApiWorkplaces.SuspendBuildingResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "SuspendBuilding", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Terminates a building.
    /// </summary>
    public override async Task<ApiWorkplaces.TerminateBuildingResponse> TerminateBuilding(ApiWorkplaces.TerminateBuildingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.BuildingId, "building_id");
        _logger.LogInformation("Handling TerminateBuilding for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "building", request.BuildingId,
            "TerminateBuilding", "accepted", token);
        var input0 = new ApiMetrics.ValidateMetricSeriesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiMetrics.ValidateMetricSeriesRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _metricsMetricSeriesValidationService.ValidateMetricSeriesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiWorkplaces.SuggestDesksRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiWorkplaces.SuggestDesksRequest.Types.Filter
            {
                CreatedAfter = (reply0.MetricSeries?.Audit?.CreatedAt ?? new Timestamp()),
                Region = (reply0.MetricSeries ?? new ApiMetrics.MetricSeries()).LocatorCase switch
                {
                    ApiMetrics.MetricSeries.LocatorOneofCase.Address => (reply0.MetricSeries?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc1 = _workplacesDeskQueryService.SuggestDesksAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.TerminateBuildingResponse
        {
            Building = new ApiWorkplaces.Building
            {
                BuildingId = (request.BuildingId ?? ""),
                Children = { new ApiWorkplaces.Building
                {
                    Location = new ApiCommon.PostalAddress
                    {
                        PostalCode = (reply0.MetricSeries?.Address?.PostalCode ?? ""),
                    },
                } },
                Manager = new ApiIdentity.UserRef
                {
                    UserId = (reply0.MetricSeries?.Owner?.UserId ?? ""),
                },
            },
            Outcome = new ApiWorkplaces.TerminateBuildingResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply1.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "TerminateBuilding", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiWorkplaces.LintBuildingsResponse> SendSuspendBuilding1Async(ApiWorkplaces.LintBuildingsRequest request, CancellationToken token)
    {
        return await _buildingLifecycleEndpointSendSuspendBuilding1Port.SendAsync(request, token);
    }
}
