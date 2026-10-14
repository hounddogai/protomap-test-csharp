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
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiSellers = Acme.Sellers.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

public sealed partial class TravelPolicyReportingEndpoint
{
    /// <summary>
    /// Counts travel policies by status.
    /// </summary>
    public override async Task<ApiTravel.CountTravelPolicyTotalsResponse> CountTravelPolicyTotals(ApiTravel.CountTravelPolicyTotalsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CountTravelPolicyTotals for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy", "batch",
            "CountTravelPolicyTotals", "accepted", token);
        var input0 = new ApiLicensing.SignalEntitlementWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.SignalEntitlementWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
                Parameters = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        var named0 = _registry.Get<ApiLicensing.EntitlementWorkflowService.EntitlementWorkflowServiceClient>("acme.licensing.v1.EntitlementWorkflowService");
        using var rpc0 = named0.SignalEntitlementWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.CountTravelPolicyTotalsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "CountTravelPolicyTotals", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Deletes a travel policy.
    /// </summary>
    public override async Task<ApiTravel.DeleteTravelPolicyReportResponse> DeleteTravelPolicyReport(ApiTravel.DeleteTravelPolicyReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TravelPolicyId, "travel_policy_id");
        _logger.LogInformation("Handling DeleteTravelPolicyReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy", request.TravelPolicyId,
            "DeleteTravelPolicyReport", "accepted", token);
        var input0 = new ApiEmployees.ScoreEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Etag ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
        };
        var descriptor0 = new Method<ApiEmployees.ScoreEmploymentContractRequest, ApiEmployees.ScoreEmploymentContractResponse>(
            MethodType.Unary, "acme.employees.v1" + ".EmploymentContractValidationService", "ScoreEmploymentContract",
            Wire.Marshaller<ApiEmployees.ScoreEmploymentContractRequest>(), Wire.Marshaller<ApiEmployees.ScoreEmploymentContractResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.DeleteTravelPolicyReportResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "DeleteTravelPolicyReport", (request.TravelPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a travel policy.
    /// </summary>
    public override async Task<ApiTravel.FetchTravelPolicyReportResponse> FetchTravelPolicyReport(ApiTravel.FetchTravelPolicyReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TravelPolicyId, "travel_policy_id");
        _logger.LogInformation("Handling FetchTravelPolicyReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy", request.TravelPolicyId,
            "FetchTravelPolicyReport", "accepted", token);
        var input0 = new ApiEmployees.ValidatePositionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.ValidatePositionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiEmployees.ValidatePositionRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _employeesPositionValidationService.ValidatePositionAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiEmployees.ValidatePositionRequest>("acme.travel.v1.TravelPolicyReportingService/FetchTravelPolicyReport/0", routed0.Payload.Clone()), token);
        var profile1 = new MappingProfile<ApiTravel.FetchTravelPolicyReportRequest, ApiEmployees.PreviewEmploymentContractRequest>(value => new ApiEmployees.PreviewEmploymentContractRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Position?.Audit?.CreatedBy ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (reply0.Position?.Owner?.Email ?? ""),
                },
            },
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _employeesEmploymentContractValidationService.PreviewEmploymentContractAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.FetchTravelPolicyReportResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Outcome?.Message ?? ""),
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Position?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (reply1.EmploymentContract?.Audit?.UpdatedBy ?? ""),
                },
                TravelPolicyId = (request.TravelPolicyId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Generates a travel policy.
    /// </summary>
    public override async Task<ApiTravel.GenerateTravelPolicyReportResponse> GenerateTravelPolicyReport(ApiTravel.GenerateTravelPolicyReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling GenerateTravelPolicyReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy", request.TravelPolicyId,
            "GenerateTravelPolicyReport", "accepted", token);
        var input0 = new ApiTravel.GetTravelPolicyRatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TravelPolicyId = (request.TravelPolicyId ?? ""),
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _travelTravelPolicyCalculationService.GetTravelPolicyRatesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.SyncTravelPoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.SyncTravelPoliciesRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.TravelPolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Block = new ApiTravel.TravelPolicyBlockAction
                {
                    NotifyEmail = (reply0.TravelPolicy ?? new ApiTravel.TravelPolicy()).ActionCase switch
                    {
                        ApiTravel.TravelPolicy.ActionOneofCase.Block => (reply0.TravelPolicy?.Block?.NotifyEmail ?? ""),
                        _ => ""
                    },
                    Reason = (reply0.TravelPolicy ?? new ApiTravel.TravelPolicy()).ActionCase switch
                    {
                        ApiTravel.TravelPolicy.ActionOneofCase.Block => (reply0.TravelPolicy?.Block?.Reason ?? ""),
                        _ => ""
                    },
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (reply0.TravelPolicy?.Owner?.UserId ?? ""),
                },
            },
        };
        using var rpc1 = _travelTravelPolicySyncService.SyncTravelPoliciesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiTravel.PushTravelPolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.PushTravelPolicyRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.TravelPolicy?.Audit?.Etag ?? ""),
                    UpdatedBy = (reply1.TravelPolicy?.Audit?.UpdatedBy ?? ""),
                },
                Route = new ApiTravel.TravelPolicyRouteAction
                {
                    Target = (reply1.TravelPolicy ?? new ApiTravel.TravelPolicy()).ActionCase switch
                    {
                        ApiTravel.TravelPolicy.ActionOneofCase.Route => (reply1.TravelPolicy?.Route?.Target ?? ""),
                        _ => ""
                    },
                    Weight = (reply1.TravelPolicy ?? new ApiTravel.TravelPolicy()).ActionCase switch
                    {
                        ApiTravel.TravelPolicy.ActionOneofCase.Route => (reply1.TravelPolicy?.Route?.Weight ?? 0),
                        _ => 0
                    },
                },
            },
        };
        using var rpc2 = _travelTravelPolicySyncService.PushTravelPolicyAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiTravel.GenerateTravelPolicyReportResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "GenerateTravelPolicyReport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists travel policies a page at a time.
    /// </summary>
    public override async Task<ApiTravel.ListTravelPolicyReportsResponse> ListTravelPolicyReports(ApiTravel.ListTravelPolicyReportsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListTravelPolicyReports for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy", "batch",
            "ListTravelPolicyReports", "accepted", token);
        var response = new ApiTravel.ListTravelPolicyReportsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "ListTravelPolicyReports", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules a travel policy.
    /// </summary>
    public override async Task<ApiTravel.ScheduleTravelPolicyReportResponse> ScheduleTravelPolicyReport(ApiTravel.ScheduleTravelPolicyReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScheduleTravelPolicyReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy", request.TravelPolicyId,
            "ScheduleTravelPolicyReport", "accepted", token);
        var input0 = new ApiSellers.CompareSellerOptionsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _sellersSellerCalculationService.CompareSellerOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.PauseDocumentTemplateWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Seller?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiDocuments.PauseDocumentTemplateWorkflowRequest>();
        using var rpc1 = _documentsDocumentTemplateWorkflowService.PauseDocumentTemplateWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.ScheduleTravelPolicyReportResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "ScheduleTravelPolicyReport", (request.TravelPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Shares a travel policy.
    /// </summary>
    public override async Task<ApiTravel.ShareTravelPolicyReportResponse> ShareTravelPolicyReport(ApiTravel.ShareTravelPolicyReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ShareTravelPolicyReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy", request.TravelPolicyId,
            "ShareTravelPolicyReport", "accepted", token);
        var input0 = new ApiTravel.SimulateTripRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Trip = new ApiTravel.Trip
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Details?.Recipient?.Email ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiTravel.SimulateTripRequest>(json0);
        using var rpc0 = _travelTripCalculationService.SimulateTripAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.CheckEmploymentContractEligibilityRequest();
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        input1.Details = new ApiEmployees.CheckEmploymentContractEligibilityRequest.Types.Details
        {
            Contact = new ApiCommon.ContactInfo
            {
                Name = (request.Details?.Recipient?.Name ?? new ApiCommon.PersonName()),
            },
        };
        input1.EmploymentContract = new ApiEmployees.EmploymentContract
        {
            Audit = new ApiCommon.AuditInfo
            {
                CreatedBy = (reply0.Trip?.Audit?.CreatedBy ?? ""),
            },
            BillingAddress = new ApiCommon.PostalAddress
            {
                CountryCode = (reply0.Trip?.BillingAddress?.CountryCode ?? ""),
            },
        };
        using var rpc1 = _employeesEmploymentContractValidationService.CheckEmploymentContractEligibilityAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.ShareTravelPolicyReportResponse
        {
            Outcome = new ApiTravel.ShareTravelPolicyReportResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.EmploymentContract?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (reply0.Trip?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (reply1.EmploymentContract?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Recipient?.Email ?? ""),
                },
                TravelPolicyId = (request.TravelPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "ShareTravelPolicyReport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams the changes of travel policies as they happen.
    /// </summary>
    public override async Task StreamTravelPolicyMetrics(ApiTravel.StreamTravelPolicyMetricsRequest request, IServerStreamWriter<ApiTravel.StreamTravelPolicyMetricsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StreamTravelPolicyMetrics for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy", "batch",
            "StreamTravelPolicyMetrics", "accepted", token);
        var input0 = new ApiSellers.CalculateSellerRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Seller = new ApiSellers.Seller
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Subscriber?.Email ?? ""),
                    Phone = (request.Subscriber?.Phone ?? ""),
                    PreferredChannel = (request.Subscriber?.PreferredChannel ?? ""),
                },
                Name = (request.Subscriber?.Name ?? new ApiCommon.PersonName()),
            },
        };
        using var rpc0 = _sellersSellerCalculationService.CalculateSellerAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.NormalizeItineraryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.NormalizeItineraryRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (reply0.Seller?.Contact?.Phone ?? ""),
                },
            },
            Itinerary = new ApiTravel.Itinerary
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.Seller?.PrimaryAddress?.PostalCode ?? ""),
                    Recipient = (reply0.Seller?.PrimaryAddress?.Recipient ?? ""),
                },
                Counterparty = new ApiTravel.TravelBookingRef
                {
                    DisplayName = (reply0.Seller?.RelationshipManager?.DisplayName ?? ""),
                },
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _travelItineraryValidationService.NormalizeItineraryAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiTravel.StreamTravelPolicyMetricsResponse
        {
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.Itinerary?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Subscriber?.Email ?? ""),
                    UserId = (reply0.Seller?.RelationshipManager?.UserId ?? ""),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Filter?.Region ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "StreamTravelPolicyMetrics", (request.Filter?.Region ?? ""), token);
        await responses.WriteAsync(response, token);
    }
}
