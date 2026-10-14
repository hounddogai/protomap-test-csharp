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
using ApiLearning = Acme.Learning.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Scheduling;

public sealed partial class AvailabilityWindowLifecycleEndpoint
{
    /// <summary>
    /// Activates an availability window.
    /// </summary>
    public override async Task<ApiScheduling.ActivateAvailabilityWindowResponse> ActivateAvailabilityWindow(ApiScheduling.ActivateAvailabilityWindowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.AvailabilityWindowId, "availability_window_id");
        _logger.LogInformation("Handling ActivateAvailabilityWindow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "availability_window", request.AvailabilityWindowId,
            "ActivateAvailabilityWindow", "accepted", token);
        var profile0 = new MappingProfile<ApiScheduling.ActivateAvailabilityWindowRequest, ApiScheduling.PreviewRosterRequest>(value => new ApiScheduling.PreviewRosterRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiScheduling.PreviewRosterRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (value.Details?.NotifyContact?.Email ?? ""),
                },
            },
            Roster = new ApiScheduling.Roster
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (value.Details?.NewOwner?.DisplayName ?? ""),
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _schedulingRosterValidationService.PreviewRosterAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiScheduling.FindRostersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiScheduling.FindRostersRequest.Types.Filter
            {
                CreatedAfter = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
                OwnerId = (request.Details?.NewOwner?.UserId ?? ""),
                Region = (reply0.Roster?.BillingAddress?.Region ?? ""),
            },
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _schedulingRosterQueryService.FindRostersAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = request.ActivateAvailabilityWindowToGetAvailabilityWindowRates2();
        using var rpc2 = _schedulingAvailabilityWindowCalculationService.GetAvailabilityWindowRatesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiScheduling.ActivateAvailabilityWindowResponse
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                AvailabilityWindowId = (request.AvailabilityWindowId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.Roster?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Region = (reply0.Roster?.BillingAddress?.Region ?? ""),
                },
                Organizer = new ApiIdentity.UserRef
                {
                    UserId = (reply2.AvailabilityWindow?.Organizer?.UserId ?? ""),
                },
            },
            Outcome = new ApiScheduling.ActivateAvailabilityWindowResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "availability_window",
            "audit", "ActivateAvailabilityWindow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Reinstates an availability window.
    /// </summary>
    public override async Task<ApiScheduling.ReinstateAvailabilityWindowResponse> ReinstateAvailabilityWindow(ApiScheduling.ReinstateAvailabilityWindowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.AvailabilityWindowId, "availability_window_id");
        _logger.LogInformation("Handling ReinstateAvailabilityWindow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "availability_window", request.AvailabilityWindowId,
            "ReinstateAvailabilityWindow", "accepted", token);
        var input0 = new ApiDocuments.PushDocumentFolderRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.PushDocumentFolderRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Manager = new ApiIdentity.UserRef
                    {
                        Email = (request.Details?.NotifyContact?.Email ?? ""),
                    },
                } },
            },
        };
        using var rpc0 = _documentsDocumentFolderSyncService.PushDocumentFolderAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.LintPositionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.DocumentFolder?.Audit?.Etag ?? ""),
                    UpdatedAt = (reply0.DocumentFolder?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (reply0.DocumentFolder?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.DocumentFolder?.Manager?.DisplayName ?? ""),
                },
            },
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _employeesPositionValidationService.LintPositionsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLearning.ListCertificationWorkflowsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLearning.ListCertificationWorkflowsRequest.Types.Filter
            {
                Region = (reply0.DocumentFolder?.Location?.Region ?? ""),
            },
        };
        var packed2 = Any.Pack(input2);
        input2 = packed2.Unpack<ApiLearning.ListCertificationWorkflowsRequest>();
        using var rpc2 = _learningCertificationWorkflowService.ListCertificationWorkflowsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiScheduling.ReinstateAvailabilityWindowResponse
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                AvailabilityWindowId = (request.AvailabilityWindowId ?? ""),
                Location = (reply0.DocumentFolder?.Location ?? new ApiCommon.PostalAddress()),
                Organizer = new ApiIdentity.UserRef
                {
                    UserId = (reply1.Position?.Owner?.UserId ?? ""),
                },
            },
            Outcome = new ApiScheduling.ReinstateAvailabilityWindowResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply2.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "availability_window",
            "audit", "ReinstateAvailabilityWindow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Renews an availability window.
    /// </summary>
    public override async Task<ApiScheduling.RenewAvailabilityWindowResponse> RenewAvailabilityWindow(ApiScheduling.RenewAvailabilityWindowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.AvailabilityWindowId, "availability_window_id");
        _logger.LogInformation("Handling RenewAvailabilityWindow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "availability_window", request.AvailabilityWindowId,
            "RenewAvailabilityWindow", "accepted", token);
        var input0 = new ApiTravel.StartTravelBookingWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.StartTravelBookingWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                },
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiTravel.StartTravelBookingWorkflowRequest>(json0);
        using var rpc0 = _travelTravelBookingWorkflowService.StartTravelBookingWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiScheduling.RenewAvailabilityWindowResponse
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                AvailabilityWindowId = (request.AvailabilityWindowId ?? ""),
            },
            Outcome = new ApiScheduling.RenewAvailabilityWindowResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply0.Outcome?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "availability_window",
            "audit", "RenewAvailabilityWindow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules an availability window.
    /// </summary>
    public override async Task<ApiScheduling.ScheduleAvailabilityWindowExpiryResponse> ScheduleAvailabilityWindowExpiry(ApiScheduling.ScheduleAvailabilityWindowExpiryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScheduleAvailabilityWindowExpiry for {Tenant}", requestContext.TenantId);
        if (request.AvailabilityWindow is { } record)
        {
            var snapshot = AvailabilityWindowMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.LintEmploymentContractsRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.EmploymentContract = new ApiEmployees.EmploymentContract
        {
            Audit = new ApiCommon.AuditInfo
            {
                UpdatedBy = (request.AvailabilityWindow?.Audit?.UpdatedBy ?? ""),
            },
            BillingAddress = new ApiCommon.PostalAddress
            {
                Recipient = (request.AvailabilityWindow?.Location?.Recipient ?? ""),
                Region = (request.AvailabilityWindow?.Location?.Region ?? ""),
            },
            Counterparty = new ApiEmployees.EmployeeRef
            {
                Email = (request.Details?.Notify?.Email ?? ""),
            },
        };
        using var rpc0 = _employeesEmploymentContractValidationService.LintEmploymentContractsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var batch1 = (request.AvailabilityWindow?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.AvailabilityWindowParticipant>()).Select(async item =>
        {
            var input1 = new ApiScheduling.ScoreRosterRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiScheduling.ScoreRosterRequest.Types.Details
                {
                    Destination = new ApiCommon.PostalAddress
                    {
                        CountryCode = (reply0.EmploymentContract?.BillingAddress?.CountryCode ?? ""),
                    },
                },
                Roster = new ApiScheduling.Roster
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        Etag = (request.AvailabilityWindow?.Audit?.Etag ?? ""),
                    },
                    Counterparty = new ApiEmployees.EmployeeRef
                    {
                        DisplayName = (request.AvailabilityWindow?.Organizer?.DisplayName ?? ""),
                        Email = (item.Contact?.Email ?? ""),
                    },
                },
            };
            using var rpc1 = _schedulingRosterValidationService.ScoreRosterAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            return reply1;
        }).ToArray();
        var replies1 = await Task.WhenAll(batch1);
        var reply1 = replies1.LastOrDefault() ?? new ApiScheduling.ScoreRosterResponse();
        var response = new ApiScheduling.ScheduleAvailabilityWindowExpiryResponse
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.EmploymentContract?.Audit?.CreatedBy ?? ""),
                },
                AvailabilityWindowId = (request.AvailabilityWindowId ?? ""),
                Window = (request.AvailabilityWindow?.Window ?? new ApiCommon.TimeWindow()),
            },
            Outcome = new ApiScheduling.ScheduleAvailabilityWindowExpiryResponse.Types.Outcome
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "availability_window",
            "audit", "ScheduleAvailabilityWindowExpiry", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Suspends an availability window.
    /// </summary>
    public override async Task<ApiScheduling.SuspendAvailabilityWindowResponse> SuspendAvailabilityWindow(ApiScheduling.SuspendAvailabilityWindowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.AvailabilityWindowId, "availability_window_id");
        _logger.LogInformation("Handling SuspendAvailabilityWindow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "availability_window", request.AvailabilityWindowId,
            "SuspendAvailabilityWindow", "accepted", token);
        var input0 = new ApiScheduling.CalculateAvailabilityWindowRequest
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                Organizer = new ApiIdentity.UserRef
                {
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
                Participants = { new ApiScheduling.AvailabilityWindowParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Details?.NewOwner?.Email ?? ""),
                    },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _schedulingAvailabilityWindowCalculationService.CalculateAvailabilityWindowAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiScheduling.SuspendAvailabilityWindowResponse
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                AvailabilityWindowId = (request.AvailabilityWindowId ?? ""),
                Title = (reply0.AvailabilityWindow?.Title ?? ""),
            },
            Outcome = new ApiScheduling.SuspendAvailabilityWindowResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "availability_window",
            "audit", "SuspendAvailabilityWindow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Terminates an availability window.
    /// </summary>
    public override async Task<ApiScheduling.TerminateAvailabilityWindowResponse> TerminateAvailabilityWindow(ApiScheduling.TerminateAvailabilityWindowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.AvailabilityWindowId, "availability_window_id");
        _logger.LogInformation("Handling TerminateAvailabilityWindow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "availability_window", request.AvailabilityWindowId,
            "TerminateAvailabilityWindow", "accepted", token);
        var input0 = new ApiDocuments.PreviewSignatureRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.PreviewSignatureRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                },
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
        };
        using var rpc0 = _documentsSignatureValidationService.PreviewSignatureAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiScheduling.TerminateAvailabilityWindowResponse
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                AvailabilityWindowId = (request.AvailabilityWindowId ?? ""),
            },
            Outcome = new ApiScheduling.TerminateAvailabilityWindowResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                ReferenceId = (reply0.Outcome?.ReferenceId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "availability_window",
            "audit", "TerminateAvailabilityWindow", (request.Reason ?? ""), token);
        return response;
    }
}
