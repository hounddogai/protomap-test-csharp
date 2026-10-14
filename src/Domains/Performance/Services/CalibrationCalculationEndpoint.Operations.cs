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
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Performance;

public sealed partial class CalibrationCalculationEndpoint
{
    /// <summary>
    /// Calculates a calibration.
    /// </summary>
    public override async Task<ApiPerformance.CalculateCalibrationResponse> CalculateCalibration(ApiPerformance.CalculateCalibrationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CalculateCalibration for {Tenant}", requestContext.TenantId);
        if (request.Calibration is { } record)
        {
            var snapshot = CalibrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.ValidateDocumentFolderRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            Etag = (request.Calibration?.Audit?.Etag ?? ""),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            Recipient = (request.Details?.Destination?.Recipient ?? ""),
                        },
                    } },
                } },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiDocuments.ValidateDocumentFolderRequest>();
        using var rpc0 = _documentsDocumentFolderValidationService.ValidateDocumentFolderAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.ExplainItineraryDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.ExplainItineraryDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Calibration?.Requester?.Phone ?? ""),
                },
            },
            Itinerary = new ApiTravel.Itinerary
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Calibration?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (request.Calibration?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiTravel.ExplainItineraryDecisionRequest>(json1);
        using var rpc1 = _travelItineraryValidationService.ExplainItineraryDecisionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiPerformance.CalculateCalibrationResponse
        {
            Calibration = new ApiPerformance.Calibration
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (reply0.DocumentFolder?.Manager?.UserId ?? ""),
                },
                CalibrationId = (request.CalibrationId ?? ""),
                Sla = new ApiPerformance.CalibrationSla
                {
                    DueAt = (reply1.Itinerary?.DueAt ?? new Timestamp()),
                },
                Status = (request.Calibration?.Status ?? (ApiPerformance.CalibrationStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "calibration",
            "audit", "CalculateCalibration", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares a calibration.
    /// </summary>
    public override async Task<ApiPerformance.CompareCalibrationOptionsResponse> CompareCalibrationOptions(ApiPerformance.CompareCalibrationOptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CompareCalibrationOptions for {Tenant}", requestContext.TenantId);
        if (request.Calibration is { } record)
        {
            var snapshot = CalibrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.ListDocumentTemplateVersionsRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Filter = new ApiDocuments.ListDocumentTemplateVersionsRequest.Types.Filter
        {
            Region = (request.Details?.Destination?.Region ?? ""),
        };
        using var rpc0 = _documentsDocumentTemplateHistoryService.ListDocumentTemplateVersionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiPerformance.SimulateFeedbackRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.SimulateFeedbackRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (request.Details?.Destination?.Locality ?? ""),
                    Recipient = (request.Details?.Destination?.Recipient ?? ""),
                },
            },
            Feedback = new ApiPerformance.Feedback
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (request.Calibration?.Assignee?.DisplayName ?? ""),
                    Email = (request.Calibration?.Assignee?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _performanceFeedbackCalculationService.SimulateFeedbackAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiPerformance.CompareCalibrationOptionsResponse
        {
            Calibration = new ApiPerformance.Calibration
            {
                CalibrationId = (request.CalibrationId ?? ""),
                Status = (request.Calibration?.Status ?? (ApiPerformance.CalibrationStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "calibration",
            "audit", "CompareCalibrationOptions", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates a calibration.
    /// </summary>
    public override async Task<ApiPerformance.EstimateCalibrationResponse> EstimateCalibration(ApiPerformance.EstimateCalibrationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EstimateCalibration for {Tenant}", requestContext.TenantId);
        if (request.Calibration is { } record)
        {
            var snapshot = CalibrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPerformance.NormalizeGoalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Goal = new ApiPerformance.Goal
            {
                Sla = new ApiPerformance.GoalSla
                {
                    PausedFor = (request.Calibration?.Sla?.PausedFor ?? new Duration()),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _performanceGoalValidationService.NormalizeGoalAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiMetrics.PreviewMetricSeriesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiMetrics.PreviewMetricSeriesRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (reply0.Goal?.Requester?.Name ?? new ApiCommon.PersonName()),
                },
            },
            MetricSeries = new ApiMetrics.MetricSeries
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Calibration?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _metricsMetricSeriesValidationService.PreviewMetricSeriesAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiPerformance.EstimateCalibrationResponse
        {
            Calibration = new ApiPerformance.Calibration
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.MetricSeries?.Owner?.DisplayName ?? ""),
                },
                CalibrationId = (request.CalibrationId ?? ""),
                Comments = { new ApiPerformance.CalibrationComment
                {
                    CreatedAt = (reply0.Goal?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply1.MetricSeries?.Owner?.Email ?? ""),
                    PreferredChannel = (reply0.Goal?.Requester?.PreferredChannel ?? ""),
                },
                Status = (request.Calibration?.Status ?? (ApiPerformance.CalibrationStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "calibration",
            "audit", "EstimateCalibration", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a calibration.
    /// </summary>
    public override async Task<ApiPerformance.GetCalibrationRatesResponse> GetCalibrationRates(ApiPerformance.GetCalibrationRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CalibrationId, "calibration_id");
        _logger.LogInformation("Handling GetCalibrationRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "calibration", request.CalibrationId,
            "GetCalibrationRates", "accepted", token);
        var input0 = new ApiIdentity.BatchGetUsersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply0 = await Workflow.SendAsync(input0, token, _identityUserService.BatchGetUsersAsync);
        var input1 = new ApiPerformance.PreviewGoalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.PreviewGoalRequest.Types.Details
            {
                Locale = (request.Context?.Locale ?? ""),
            },
            Goal = new ApiPerformance.Goal
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        using var rpc1 = _sendGetCalibrationRates1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiPerformance.GetCalibrationRatesResponse
        {
            Calibration = new ApiPerformance.Calibration
            {
                CalibrationId = (request.CalibrationId ?? ""),
                Priority = (reply1.Goal?.Priority ?? 0),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Quotes a calibration.
    /// </summary>
    public override async Task<ApiPerformance.QuoteCalibrationResponse> QuoteCalibration(ApiPerformance.QuoteCalibrationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling QuoteCalibration for {Tenant}", requestContext.TenantId);
        if (request.Calibration is { } record)
        {
            var snapshot = CalibrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiVenues.UpdateVenueRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Venue = new ApiVenues.Venue
            {
                Address = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.Calibration?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Calibration?.Requester?.Email ?? ""),
                    UserId = (request.Calibration?.Assignee?.UserId ?? ""),
                },
            },
        };
        var named0 = _registry.Get<ApiVenues.VenueService.VenueServiceClient>("acme.venues.v1.VenueService");
        using var rpc0 = named0.UpdateVenueAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiPerformance.QuoteCalibrationResponse
        {
            Calibration = new ApiPerformance.Calibration
            {
                CalibrationId = (request.CalibrationId ?? ""),
                Comments = { new ApiPerformance.CalibrationComment
                {
                    CreatedAt = (reply0.Venue?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                CustomFields = { (request.Calibration?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "calibration",
            "audit", "QuoteCalibration", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalculates a calibration.
    /// </summary>
    public override async Task<ApiPerformance.RecalculateCalibrationsResponse> RecalculateCalibrations(ApiPerformance.RecalculateCalibrationsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecalculateCalibrations for {Tenant}", requestContext.TenantId);
        if (request.Calibration is { } record)
        {
            var snapshot = CalibrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiFeatureflags.CountFeatureFlagsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiFeatureflags.CountFeatureFlagsRequest.Types.Filter
            {
                CreatedAfter = (request.Calibration?.Audit?.UpdatedAt ?? new Timestamp()),
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        var descriptor0 = new Method<ApiFeatureflags.CountFeatureFlagsRequest, ApiFeatureflags.CountFeatureFlagsResponse>(
            MethodType.Unary, "acme.featureflags.v1" + ".FeatureFlagQueryService", "CountFeatureFlags",
            Wire.Marshaller<ApiFeatureflags.CountFeatureFlagsRequest>(), Wire.Marshaller<ApiFeatureflags.CountFeatureFlagsResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiIdentity.BatchGetUsersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var command1 = new DispatchCommand<ApiIdentity.BatchGetUsersRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _identityUserService.BatchGetUsersAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiPerformance.RecalculateCalibrationsResponse
        {
            Breakdown = new ApiPerformance.RecalculateCalibrationsResponse.Types.Breakdown
            {
                CurrencyCode = (request.Details?.CurrencyCode ?? ""),
            },
            Calibration = new ApiPerformance.Calibration
            {
                CalibrationId = (request.CalibrationId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "calibration",
            "audit", "RecalculateCalibrations", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Simulates a calibration.
    /// </summary>
    public override async Task<ApiPerformance.SimulateCalibrationResponse> SimulateCalibration(ApiPerformance.SimulateCalibrationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SimulateCalibration for {Tenant}", requestContext.TenantId);
        if (request.Calibration is { } record)
        {
            var snapshot = CalibrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiPerformance.SimulateCalibrationRequest, ApiPerformance.ValidateGoalRequest>(value => new ApiPerformance.ValidateGoalRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.ValidateGoalRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (value.Calibration?.Requester?.Phone ?? ""),
                    PreferredChannel = (value.Calibration?.Requester?.PreferredChannel ?? ""),
                },
            },
            Goal = new ApiPerformance.Goal
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (value.Calibration?.Audit?.UpdatedBy ?? ""),
                },
                Sla = new ApiPerformance.GoalSla
                {
                    DueAt = (value.Calibration?.Sla?.DueAt ?? new Timestamp()),
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _performanceGoalValidationService.ValidateGoalAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiPerformance.SimulateCalibrationResponse
        {
            Calibration = new ApiPerformance.Calibration
            {
                CalibrationId = (request.CalibrationId ?? ""),
                CustomFields = { (request.Calibration?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Requester = new ApiCommon.ContactInfo
                {
                    Name = (reply0.Goal?.Requester?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "calibration",
            "audit", "SimulateCalibration", (request.Reason ?? ""), token);
        return response;
    }
}
