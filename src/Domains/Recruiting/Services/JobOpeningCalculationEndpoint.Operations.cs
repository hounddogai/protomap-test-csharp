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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiPersonalization = Acme.Personalization.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiSearch = Acme.Search.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Recruiting;

public sealed partial class JobOpeningCalculationEndpoint
{
    /// <summary>
    /// Calculates a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.CalculateJobOpeningResponse> CalculateJobOpening(ApiRecruiting.CalculateJobOpeningRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CalculateJobOpening for {Tenant}", requestContext.TenantId);
        if (request.JobOpening is { } record)
        {
            var snapshot = JobOpeningMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiDevices.LintDevicePoliciesResponse();
        foreach (var item in (request.JobOpening?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiRecruiting.JobOpeningLine>()))
        {
            var input0 = new ApiDevices.LintDevicePoliciesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                DevicePolicy = new ApiDevices.DevicePolicy
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (request.JobOpening?.Audit?.CreatedAt ?? new Timestamp()),
                        UpdatedBy = (request.JobOpening?.Audit?.UpdatedBy ?? ""),
                    },
                    Description = (item.Description ?? ""),
                    Discount = new ApiDevices.DevicePolicyDiscountAction
                    {
                        Amount = (item.Tax?.Amount ?? new ApiCommon.Money()),
                    },
                },
            };
            using var rpc0 = _devicesDevicePolicyValidationService.LintDevicePoliciesAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiPersonalization.BrowseProfilesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiPersonalization.BrowseProfilesRequest.Types.Filter
            {
                CreatedAfter = (reply0.DevicePolicy?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (request.JobOpening?.Counterparty?.InterviewId ?? ""),
                Region = (request.JobOpening?.BillingAddress?.Region ?? ""),
            },
        };
        using var rpc1 = _sendCalculateJobOpening1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.CalculateJobOpeningResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                IssuedAt = (request.JobOpening?.IssuedAt ?? new Timestamp()),
                JobOpeningId = (request.JobOpeningId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "CalculateJobOpening", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.CompareJobOpeningOptionsResponse> CompareJobOpeningOptions(ApiRecruiting.CompareJobOpeningOptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CompareJobOpeningOptions for {Tenant}", requestContext.TenantId);
        if (request.JobOpening is { } record)
        {
            var snapshot = JobOpeningMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiNotifications.SearchNotificationsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiNotifications.SearchNotificationsRequest.Types.Filter
            {
                CreatedAfter = (request.JobOpening?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (request.JobOpening?.Counterparty?.InterviewId ?? ""),
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        var named0 = _registry.Get<ApiNotifications.NotificationService.NotificationServiceClient>("acme.notifications.v1.NotificationService");
        using var rpc0 = named0.SearchNotificationsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var reply1 = new ApiHelpdesk.ValidateSlaPolicyResponse();
        foreach (var item in (request.JobOpening?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiRecruiting.JobOpeningLine>()))
        {
            var input1 = new ApiHelpdesk.ValidateSlaPolicyRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                SlaPolicy = new ApiHelpdesk.SlaPolicy
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        UpdatedAt = (request.JobOpening?.Audit?.UpdatedAt ?? new Timestamp()),
                        UpdatedBy = (request.JobOpening?.Audit?.UpdatedBy ?? ""),
                    },
                    Description = (item.Description ?? ""),
                    Discount = new ApiHelpdesk.SlaPolicyDiscountAction
                    {
                        Amount = (item.Tax?.Amount ?? new ApiCommon.Money()),
                    },
                },
            };
            using var rpc1 = _helpdeskSlaPolicyValidationService.ValidateSlaPolicyAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var response = new ApiRecruiting.CompareJobOpeningOptionsResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                BillingAddress = (request.JobOpening?.BillingAddress ?? new ApiCommon.PostalAddress()),
                JobOpeningId = (request.JobOpeningId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "CompareJobOpeningOptions", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.EstimateJobOpeningResponse> EstimateJobOpening(ApiRecruiting.EstimateJobOpeningRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EstimateJobOpening for {Tenant}", requestContext.TenantId);
        if (request.JobOpening is { } record)
        {
            var snapshot = JobOpeningMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiFeatureflags.CountFeatureFlagsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiFeatureflags.CountFeatureFlagsRequest.Types.Filter
            {
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        var command0 = new DispatchCommand<ApiFeatureflags.CountFeatureFlagsRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _featureflagsFeatureFlagQueryService.CountFeatureFlagsAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiRecruiting.EstimateJobOpeningRequest, ApiDevices.ValidateManagedDeviceRequest>(value => new ApiDevices.ValidateManagedDeviceRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Address = new ApiCommon.PostalAddress
                {
                    Locality = (value.JobOpening?.BillingAddress?.Locality ?? ""),
                    Region = (value.Details?.Destination?.Region ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (value.JobOpening?.Audit?.Etag ?? ""),
                },
            },
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _devicesManagedDeviceValidationService.ValidateManagedDeviceAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDocuments.NotifyDocumentTemplateWatchersRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.NotifyDocumentTemplateWatchersRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (reply1.ManagedDevice?.Owner?.Email ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc2 = _documentsDocumentTemplateNotificationService.NotifyDocumentTemplateWatchersAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiRecruiting.EstimateJobOpeningResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply2.DocumentTemplate?.Audit?.Etag ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply2.DocumentTemplate?.BillingAddress?.PostalCode ?? ""),
                },
                JobOpeningId = (request.JobOpeningId ?? ""),
                Metadata = { (request.JobOpening?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.JobOpening?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "EstimateJobOpening", (request.JobOpening?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.ExplainJobOpeningCalculationResponse> ExplainJobOpeningCalculation(ApiRecruiting.ExplainJobOpeningCalculationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainJobOpeningCalculation for {Tenant}", requestContext.TenantId);
        if (request.JobOpening is { } record)
        {
            var snapshot = JobOpeningMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.ExplainJobOpeningCalculationToCompareBuildingOptions0();
        using var rpc0 = _workplacesBuildingCalculationService.CompareBuildingOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiRecruiting.ExplainJobOpeningCalculationResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.Building?.Audit?.UpdatedBy ?? ""),
                },
                BillingAddress = (request.JobOpening?.BillingAddress ?? new ApiCommon.PostalAddress()),
                JobOpeningId = (request.JobOpeningId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "ExplainJobOpeningCalculation", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.GetJobOpeningRatesResponse> GetJobOpeningRates(ApiRecruiting.GetJobOpeningRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.JobOpeningId, "job_opening_id");
        _logger.LogInformation("Handling GetJobOpeningRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "job_opening", request.JobOpeningId,
            "GetJobOpeningRates", "accepted", token);
        var input0 = new ApiDocuments.RepairDocumentTemplateRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.RepairDocumentTemplateRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Metadata = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        using var rpc0 = _documentsDocumentTemplateAdminService.RepairDocumentTemplateAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiRecruiting.GetJobOpeningRatesResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (reply0.DocumentTemplate?.BillingAddress?.Region ?? ""),
                },
                JobOpeningId = (request.JobOpeningId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Quotes a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.QuoteJobOpeningResponse> QuoteJobOpening(ApiRecruiting.QuoteJobOpeningRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling QuoteJobOpening for {Tenant}", requestContext.TenantId);
        if (request.JobOpening is { } record)
        {
            var snapshot = JobOpeningMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiScheduling.SuggestRostersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiScheduling.SuggestRostersRequest.Types.Filter
            {
                CreatedAfter = (request.JobOpening?.Audit?.UpdatedAt ?? new Timestamp()),
                OwnerId = (request.JobOpening?.Counterparty?.InterviewId ?? ""),
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _schedulingRosterQueryService.SuggestRostersAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTimekeeping.CountTimesheetsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiTimekeeping.CountTimesheetsRequest.Types.Filter
            {
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiTimekeeping.CountTimesheetsRequest>();
        using var rpc1 = _timekeepingTimesheetQueryService.CountTimesheetsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiSearch.SuggestSearchIndexesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiSearch.SuggestSearchIndexesRequest.Types.Filter
            {
                CreatedAfter = (request.JobOpening?.Counterparty?.StartsAt ?? new Timestamp()),
                Labels = { (request.JobOpening?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply1.Result?.CorrelationId ?? ""),
                Region = (request.JobOpening?.BillingAddress?.Region ?? ""),
            },
        };
        var json2 = JsonFormatter.Default.Format(input2);
        input2 = JsonParser.Default.Parse<ApiSearch.SuggestSearchIndexesRequest>(json2);
        using var rpc2 = _searchSearchIndexQueryService.SuggestSearchIndexesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiRecruiting.QuoteJobOpeningResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                BillingAddress = (request.JobOpening?.BillingAddress ?? new ApiCommon.PostalAddress()),
                JobOpeningId = (request.JobOpeningId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "QuoteJobOpening", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalculates a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.RecalculateJobOpeningsResponse> RecalculateJobOpenings(ApiRecruiting.RecalculateJobOpeningsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecalculateJobOpenings for {Tenant}", requestContext.TenantId);
        if (request.JobOpening is { } record)
        {
            var snapshot = JobOpeningMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.ResolveTicketsRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        using var rpc0 = _helpdeskTicketQueryService.ResolveTicketsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiFeatureflags.CountFeatureFlagsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiFeatureflags.CountFeatureFlagsRequest.Types.Filter
            {
                CreatedAfter = (request.JobOpening?.Counterparty?.StartsAt ?? new Timestamp()),
                OwnerId = (request.JobOpening?.Counterparty?.InterviewId ?? ""),
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        using var rpc1 = _featureflagsFeatureFlagQueryService.CountFeatureFlagsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.RecalculateJobOpeningsResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                JobOpeningId = (request.JobOpeningId ?? ""),
                Metadata = { (request.JobOpening?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "RecalculateJobOpenings", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Simulates a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.SimulateJobOpeningResponse> SimulateJobOpening(ApiRecruiting.SimulateJobOpeningRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SimulateJobOpening for {Tenant}", requestContext.TenantId);
        if (request.JobOpening is { } record)
        {
            var snapshot = JobOpeningMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.ExplainManagedDeviceDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Address = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                    Recipient = (request.Details?.Destination?.Recipient ?? ""),
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.JobOpening?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _devicesManagedDeviceValidationService.ExplainManagedDeviceDecisionAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiRecruiting.SimulateJobOpeningResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                DueAt = (request.JobOpening?.DueAt ?? new Timestamp()),
                JobOpeningId = (request.JobOpeningId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "SimulateJobOpening", (request.Reason ?? ""), token);
        return response;
    }
}
