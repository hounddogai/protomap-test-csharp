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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiOnboarding = Acme.Onboarding.V1;

namespace Acme.Workforce.Onboarding;

public sealed partial class EquipmentReportingEndpoint
{
    /// <summary>
    /// Counts equipments by status.
    /// </summary>
    public override async Task<ApiOnboarding.CountEquipmentTotalsResponse> CountEquipmentTotals(ApiOnboarding.CountEquipmentTotalsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CountEquipmentTotals for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "equipment", "batch",
            "CountEquipmentTotals", "accepted", token);
        var response = new ApiOnboarding.CountEquipmentTotalsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "equipment",
            "audit", "CountEquipmentTotals", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Deletes an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.DeleteEquipmentReportResponse> DeleteEquipmentReport(ApiOnboarding.DeleteEquipmentReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EquipmentId, "equipment_id");
        _logger.LogInformation("Handling DeleteEquipmentReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "equipment", request.EquipmentId,
            "DeleteEquipmentReport", "accepted", token);
        var input0 = new ApiOnboarding.FindOnboardingTasksRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiOnboarding.FindOnboardingTasksRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                OwnerId = (request.EquipmentId ?? ""),
            },
        };
        var command0 = new DispatchCommand<ApiOnboarding.FindOnboardingTasksRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _onboardingOnboardingTaskQueryService.FindOnboardingTasksAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiOnboarding.DeleteEquipmentReportResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "equipment",
            "audit", "DeleteEquipmentReport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.FetchEquipmentReportResponse> FetchEquipmentReport(ApiOnboarding.FetchEquipmentReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EquipmentId, "equipment_id");
        _logger.LogInformation("Handling FetchEquipmentReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "equipment", request.EquipmentId,
            "FetchEquipmentReport", "accepted", token);
        var profile0 = new MappingProfile<ApiOnboarding.FetchEquipmentReportRequest, ApiDevices.RejectDevicePolicyRequest>(value => new ApiDevices.RejectDevicePolicyRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (value.Context?.Deadline ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (value.Context?.Actor?.Email ?? ""),
                    UserId = (value.Context?.Actor?.UserId ?? ""),
                },
                Scopes = { (value.Context?.Actor?.Roles ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _devicesDevicePolicyApprovalService.RejectDevicePolicyAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiOnboarding.FetchEquipmentReportResponse
        {
            Equipment = new ApiOnboarding.Equipment
            {
                Components = { { "primary", new ApiOnboarding.EquipmentComponent
                {
                    Description = (reply0.DevicePolicy?.Description ?? ""),
                } } },
                EquipmentId = (request.EquipmentId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.DevicePolicy?.Owner?.Email ?? ""),
                },
            },
        };
        return response;
    }
    /// <summary>
    /// Generates an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.GenerateEquipmentReportResponse> GenerateEquipmentReport(ApiOnboarding.GenerateEquipmentReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling GenerateEquipmentReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "equipment", request.EquipmentId,
            "GenerateEquipmentReport", "accepted", token);
        var input0 = new ApiOnboarding.ExplainEquipmentDecisionRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.ExplainEquipmentDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _onboardingEquipmentValidationService.ExplainEquipmentDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.GenerateEquipmentReportToRecallRegistration1(reply0);
        using var rpc1 = _corporateeventsRegistrationApprovalService.RecallRegistrationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDevices.ApproveDevicePolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.ApproveDevicePolicyRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    UserId = (reply0.Equipment?.Owner?.UserId ?? ""),
                },
            },
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.Registration?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (reply0.Equipment?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.Equipment?.Owner?.Email ?? ""),
                },
            },
        };
        var reply2 = await SendGenerateEquipmentReport2Async(input2, token);
        var response = new ApiOnboarding.GenerateEquipmentReportResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply2.Outcome?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "equipment",
            "audit", "GenerateEquipmentReport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.ScheduleEquipmentReportResponse> ScheduleEquipmentReport(ApiOnboarding.ScheduleEquipmentReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScheduleEquipmentReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "equipment", request.EquipmentId,
            "ScheduleEquipmentReport", "accepted", token);
        var input0 = new ApiAuthorization.SummarizeAccessPoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiAuthorization.SummarizeAccessPoliciesRequest.Types.Details
            {
                Cron = (request.Details?.Cron ?? ""),
                Format = (request.Details?.Format ?? ""),
                Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _authorizationAccessPolicyQueryService.SummarizeAccessPoliciesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.ApproveRegistrationRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.ApproveRegistrationRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiCorporateevents.ApproveRegistrationRequest>();
        using var rpc1 = _corporateeventsRegistrationApprovalService.ApproveRegistrationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.ScheduleEquipmentReportResponse
        {
            DownloadUri = (reply0.DownloadUri ?? ""),
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Summary = { (reply0.Summary ?? new Google.Protobuf.Collections.MapField<string, string>()) },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.EquipmentId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "equipment",
            "audit", "ScheduleEquipmentReport", (request.EquipmentId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Shares an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.ShareEquipmentReportResponse> ShareEquipmentReport(ApiOnboarding.ShareEquipmentReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ShareEquipmentReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "equipment", request.EquipmentId,
            "ShareEquipmentReport", "accepted", token);
        var input0 = new ApiOnboarding.LintOnboardingTasksRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.LintOnboardingTasksRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Recipient?.Phone ?? ""),
                },
            },
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Requester = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Recipient?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (request.Details?.Recipient?.PreferredChannel ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiOnboarding.LintOnboardingTasksRequest>(json0);
        using var rpc0 = _onboardingOnboardingTaskValidationService.LintOnboardingTasksAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiOnboarding.PreviewOnboardingTaskRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.PreviewOnboardingTaskRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Recipient?.Email ?? ""),
                },
            },
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Customer = new ApiOnboarding.EquipmentRef
                {
                    DisplayName = (reply0.OnboardingTask?.Assignee?.DisplayName ?? ""),
                    EquipmentId = (reply0.OnboardingTask ?? new ApiOnboarding.OnboardingTask()).OriginCase switch
                    {
                        ApiOnboarding.OnboardingTask.OriginOneofCase.Customer => (reply0.OnboardingTask?.Customer?.EquipmentId ?? ""),
                        _ => ""
                    },
                },
                Requester = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (reply0.OnboardingTask?.Requester?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc1 = _onboardingOnboardingTaskValidationService.PreviewOnboardingTaskAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiOnboarding.PreviewOnboardingTaskRequest>("acme.onboarding.v1.EquipmentReportingService/ShareEquipmentReport/1", input1.Clone()), token);
        var response = new ApiOnboarding.ShareEquipmentReportResponse
        {
            Equipment = new ApiOnboarding.Equipment
            {
                EquipmentId = (request.EquipmentId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Recipient?.Email ?? ""),
                },
            },
            Outcome = new ApiOnboarding.ShareEquipmentReportResponse.Types.Outcome
            {
                ReferenceId = (reply0.Outcome?.ReferenceId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "equipment",
            "audit", "ShareEquipmentReport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams the changes of equipments as they happen.
    /// </summary>
    public override async Task StreamEquipmentMetrics(ApiOnboarding.StreamEquipmentMetricsRequest request, IServerStreamWriter<ApiOnboarding.StreamEquipmentMetricsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StreamEquipmentMetrics for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "equipment", "batch",
            "StreamEquipmentMetrics", "accepted", token);
        var input0 = new ApiOnboarding.UpdateOnboardingPlanRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                    UpdatedAt = (request.Since ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        using var rpc0 = _onboardingOnboardingPlanService.UpdateOnboardingPlanAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiOnboarding.SearchOnboardingPlansRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiOnboarding.SearchOnboardingPlansRequest.Types.Filter
            {
                Query = (request.Filter?.Query ?? ""),
                Region = (reply0.OnboardingPlan?.BillingAddress?.Region ?? ""),
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _onboardingOnboardingPlanService.SearchOnboardingPlansAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiOnboarding.StreamEquipmentMetricsResponse
        {
            Equipment = new ApiOnboarding.Equipment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.OnboardingPlan?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Subscriber?.Email ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Subscriber?.PreferredChannel ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "equipment",
            "audit", "StreamEquipmentMetrics", (request.Subscriber?.PreferredChannel ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    private async Task<ApiDevices.ApproveDevicePolicyResponse> SendGenerateEquipmentReport2Async(ApiDevices.ApproveDevicePolicyRequest request, CancellationToken token)
    {
        return await _equipmentReportingEndpointSendGenerateEquipmentReport2Port.SendAsync(request, token);
    }
}
