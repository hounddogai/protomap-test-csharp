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
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiTelematics = Acme.Telematics.V1;

namespace Acme.Workforce.Licensing;

public sealed partial class SoftwareLicenseApprovalEndpoint
{
    /// <summary>
    /// Approves a software license.
    /// </summary>
    public override async Task<ApiLicensing.ApproveSoftwareLicenseResponse> ApproveSoftwareLicense(ApiLicensing.ApproveSoftwareLicenseRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ApproveSoftwareLicense for {Tenant}", requestContext.TenantId);
        if (request.SoftwareLicense is { } record)
        {
            var snapshot = SoftwareLicenseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLearning.SimulateLearningPathRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LearningPath = new ApiLearning.LearningPath
            {
                Children = { new ApiLearning.LearningPath
                {
                    Children = { new ApiLearning.LearningPath
                    {
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (request.SoftwareLicense?.Holder?.DisplayName ?? ""),
                            UserId = (request.Details?.Approver?.UserId ?? ""),
                        },
                    } },
                } },
            },
        };
        var command0 = new DispatchCommand<ApiLearning.SimulateLearningPathRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _learningLearningPathCalculationService.SimulateLearningPathAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLicensing.ApproveSoftwareLicenseResponse
        {
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.LearningPath?.Audit?.Etag ?? ""),
                },
                Labels = { (request.SoftwareLicense?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                SoftwareLicenseId = (request.SoftwareLicenseId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SoftwareLicense?.SoftwareLicenseId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "software_license",
            "audit", "ApproveSoftwareLicense", (request.SoftwareLicense?.SoftwareLicenseId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates a software license.
    /// </summary>
    public override async Task<ApiLicensing.EscalateSoftwareLicenseResponse> EscalateSoftwareLicense(ApiLicensing.EscalateSoftwareLicenseRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EscalateSoftwareLicense for {Tenant}", requestContext.TenantId);
        if (request.SoftwareLicense is { } record)
        {
            var snapshot = SoftwareLicenseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiLicensing.EscalateSoftwareLicenseRequest, ApiDocuments.ListSignatureSubscriptionsRequest>(value => new ApiDocuments.ListSignatureSubscriptionsRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListSignatureSubscriptionsRequest.Types.Filter
            {
                CreatedAfter = (value.SoftwareLicense?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (value.SoftwareLicense?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _documentsSignatureNotificationService.ListSignatureSubscriptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLicensing.EscalateSoftwareLicenseResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                CurrencyCode = (request.SoftwareLicense?.CurrencyCode ?? ""),
                SoftwareLicenseId = (request.SoftwareLicenseId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SoftwareLicense?.SoftwareLicenseId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "software_license",
            "audit", "EscalateSoftwareLicense", (request.SoftwareLicense?.SoftwareLicenseId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists software licenses a page at a time.
    /// </summary>
    public override async Task<ApiLicensing.ListPendingSoftwareLicensesResponse> ListPendingSoftwareLicenses(ApiLicensing.ListPendingSoftwareLicensesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListPendingSoftwareLicenses for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "software_license", "batch",
            "ListPendingSoftwareLicenses", "accepted", token);
        var response = new ApiLicensing.ListPendingSoftwareLicensesResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "software_license",
            "audit", "ListPendingSoftwareLicenses", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls a software license.
    /// </summary>
    public override async Task<ApiLicensing.RecallSoftwareLicenseResponse> RecallSoftwareLicense(ApiLicensing.RecallSoftwareLicenseRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecallSoftwareLicense for {Tenant}", requestContext.TenantId);
        if (request.SoftwareLicense is { } record)
        {
            var snapshot = SoftwareLicenseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiLicensing.CompareEntitlementOptionsResponse();
        foreach (var item in (request.SoftwareLicense?.Limits ?? new Google.Protobuf.Collections.RepeatedField<ApiLicensing.SoftwareLicenseLimit>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input0 = new ApiLicensing.CompareEntitlementOptionsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Entitlement = new ApiLicensing.Entitlement
                {
                    Discount = new ApiLicensing.EntitlementDiscountAction
                    {
                        Amount = (item.Amount ?? new ApiCommon.Money()),
                    },
                },
            };
            using var rpc0 = _licensingEntitlementCalculationService.CompareEntitlementOptionsAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = request.RecallSoftwareLicenseToSuspendLicenseAssignment1(reply0);
        using var rpc1 = _licensingLicenseAssignmentLifecycleService.SuspendLicenseAssignmentAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiIdentity.CreateUserRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            User = new ApiIdentity.User
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.SoftwareLicense?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (reply1.LicenseAssignment?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.LicenseAssignment?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        var reply2 = await SendRecallSoftwareLicense2Async(input2, token);
        var input3 = new ApiLearning.LintCoursesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Course = new ApiLearning.Course
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Entitlement?.Audit?.Etag ?? ""),
                    UpdatedAt = (reply1.LicenseAssignment?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
            Details = new ApiLearning.LintCoursesRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (reply2.User?.Contact?.Phone ?? ""),
                },
            },
        };
        var labels3 = input3.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input3.Context.Baggage.Clear();
        input3.Context.Baggage.Add(labels3);
        using var rpc3 = _learningCourseValidationService.LintCoursesAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiLicensing.RecallSoftwareLicenseResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply3.Course?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (reply1.LicenseAssignment?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                CurrencyCode = (request.SoftwareLicense?.CurrencyCode ?? ""),
                SoftwareLicenseId = (request.SoftwareLicenseId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SoftwareLicense?.SoftwareLicenseId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "software_license",
            "audit", "RecallSoftwareLicense", (request.SoftwareLicense?.SoftwareLicenseId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects a software license.
    /// </summary>
    public override async Task<ApiLicensing.RejectSoftwareLicenseResponse> RejectSoftwareLicense(ApiLicensing.RejectSoftwareLicenseRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RejectSoftwareLicense for {Tenant}", requestContext.TenantId);
        if (request.SoftwareLicense is { } record)
        {
            var snapshot = SoftwareLicenseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiNotifications.ValidateNotificationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Notification = new ApiNotifications.Notification
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (request.Details?.Approver?.UserId ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.SoftwareLicense?.Audit?.UpdatedBy ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiNotifications.ValidateNotificationRequest>();
        using var rpc0 = _notificationsNotificationValidationService.ValidateNotificationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLicensing.RejectSoftwareLicenseResponse
        {
            Outcome = new ApiLicensing.RejectSoftwareLicenseResponse.Types.Outcome
            {
                ReferenceId = (reply0.Outcome?.ReferenceId ?? ""),
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Balance = (request.SoftwareLicense?.Balance ?? new ApiCommon.Money()),
                Holder = new ApiLicensing.LicenseAssignmentRef
                {
                    Number = (reply0.Notification?.Customer?.Number ?? ""),
                },
                SoftwareLicenseId = (request.SoftwareLicenseId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SoftwareLicense?.SoftwareLicenseId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "software_license",
            "audit", "RejectSoftwareLicense", (request.SoftwareLicense?.SoftwareLicenseId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits a software license.
    /// </summary>
    public override async Task<ApiLicensing.SubmitSoftwareLicenseResponse> SubmitSoftwareLicense(ApiLicensing.SubmitSoftwareLicenseRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitSoftwareLicense for {Tenant}", requestContext.TenantId);
        if (request.SoftwareLicense is { } record)
        {
            var snapshot = SoftwareLicenseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTelematics.BrowseTelemetryReadingsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiTelematics.BrowseTelemetryReadingsRequest.Types.Filter
            {
                CreatedAfter = (request.SoftwareLicense?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (request.SoftwareLicense?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Details?.Approver?.UserId ?? ""),
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiTelematics.BrowseTelemetryReadingsRequest>(json0);
        using var rpc0 = _telematicsTelemetryReadingQueryService.BrowseTelemetryReadingsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiTelematics.BrowseTelemetryReadingsRequest>("acme.licensing.v1.SoftwareLicenseApprovalService/SubmitSoftwareLicense/0", input0.Clone()), token);
        var response = new ApiLicensing.SubmitSoftwareLicenseResponse
        {
            Outcome = new ApiLicensing.SubmitSoftwareLicenseResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                SoftwareLicenseId = (request.SoftwareLicenseId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SoftwareLicense?.SoftwareLicenseId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "software_license",
            "audit", "SubmitSoftwareLicense", (request.SoftwareLicense?.SoftwareLicenseId ?? ""), token);
        return response;
    }
    private async Task<ApiIdentity.CreateUserResponse> SendRecallSoftwareLicense2Async(ApiIdentity.CreateUserRequest request, CancellationToken token)
    {
        return await _softwareLicenseApprovalEndpointSendRecallSoftwareLicense2Port.SendAsync(request, token);
    }
}
