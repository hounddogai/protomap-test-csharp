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
using ApiAnomalies = Acme.Anomalies.V1;
using ApiAuthorization = Acme.Authorization.V1;
using ApiCareplans = Acme.Careplans.V1;
using ApiCommon = Acme.Common.V1;
using ApiConcessions = Acme.Concessions.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMedia = Acme.Media.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Learning;

public sealed partial class LearningPathCalculationEndpoint
{
    /// <summary>
    /// Calculates a learning path.
    /// </summary>
    public override async Task<ApiLearning.CalculateLearningPathResponse> CalculateLearningPath(ApiLearning.CalculateLearningPathRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CalculateLearningPath for {Tenant}", requestContext.TenantId);
        if (request.LearningPath is { } record)
        {
            var snapshot = LearningPathMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAuthorization.ValidateAccessPolicyRequest
        {
            AccessPolicy = new ApiAuthorization.AccessPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.LearningPath?.Audit?.UpdatedBy ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _authorizationAccessPolicyValidationService.ValidateAccessPolicyAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiLearning.CalculateLearningPathResponse
        {
            LearningPath = new ApiLearning.LearningPath
            {
                Children = { new ApiLearning.LearningPath
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedBy = (reply0.AccessPolicy?.Audit?.CreatedBy ?? ""),
                    },
                } },
                LearningPathId = (request.LearningPathId ?? ""),
                Location = (request.LearningPath?.Location ?? new ApiCommon.PostalAddress()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "learning_path",
            "audit", "CalculateLearningPath", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares a learning path.
    /// </summary>
    public override async Task<ApiLearning.CompareLearningPathOptionsResponse> CompareLearningPathOptions(ApiLearning.CompareLearningPathOptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CompareLearningPathOptions for {Tenant}", requestContext.TenantId);
        if (request.LearningPath is { } record)
        {
            var snapshot = LearningPathMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.NotifyDocumentWatchersRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.NotifyDocumentWatchersRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.LearningPath?.Manager?.Email ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc0 = _documentsDocumentNotificationService.NotifyDocumentWatchersAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiConcessions.ExplainConcessionStandCalculationRequest
        {
            ConcessionStand = new ApiConcessions.ConcessionStand
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Document?.Audit?.CreatedBy ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _sendCompareLearningPathOptions1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLearning.CompareLearningPathOptionsResponse
        {
            LearningPath = new ApiLearning.LearningPath
            {
                Children = { new ApiLearning.LearningPath
                {
                    Children = { new ApiLearning.LearningPath
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            CreatedBy = (reply1.ConcessionStand?.Audit?.CreatedBy ?? ""),
                        },
                    } },
                } },
                LearningPathId = (request.LearningPathId ?? ""),
                Location = (request.LearningPath?.Location ?? new ApiCommon.PostalAddress()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.LearningPath?.ParentId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "learning_path",
            "audit", "CompareLearningPathOptions", (request.LearningPath?.ParentId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates a learning path.
    /// </summary>
    public override async Task<ApiLearning.EstimateLearningPathResponse> EstimateLearningPath(ApiLearning.EstimateLearningPathRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EstimateLearningPath for {Tenant}", requestContext.TenantId);
        if (request.LearningPath is { } record)
        {
            var snapshot = LearningPathMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCareplans.CloneCarePlanRequest
        {
            CarePlan = new ApiCareplans.CarePlan
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Locality = (request.Details?.Destination?.Locality ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var named0 = _registry.Get<ApiCareplans.CarePlanService.CarePlanServiceClient>("acme.careplans.v1.CarePlanService");
        using var rpc0 = named0.CloneCarePlanAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.SimulateLicenseAssignmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.LearningPath?.Audit?.CreatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (request.Details?.Destination?.Recipient ?? ""),
                },
            },
        };
        var descriptor1 = new Method<ApiLicensing.SimulateLicenseAssignmentRequest, ApiLicensing.SimulateLicenseAssignmentResponse>(
            MethodType.Unary, "acme.licensing.v1" + ".LicenseAssignmentCalculationService", "SimulateLicenseAssignment",
            Wire.Marshaller<ApiLicensing.SimulateLicenseAssignmentRequest>(), Wire.Marshaller<ApiLicensing.SimulateLicenseAssignmentResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLearning.EstimateLearningPathResponse
        {
            LearningPath = new ApiLearning.LearningPath
            {
                Children = { new ApiLearning.LearningPath
                {
                    Children = { new ApiLearning.LearningPath
                    {
                        Location = new ApiCommon.PostalAddress
                        {
                            PostalCode = (reply0.CarePlan?.BillingAddress?.PostalCode ?? ""),
                        },
                    } },
                } },
                LearningPathId = (request.LearningPathId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    Region = (reply1.LicenseAssignment?.BillingAddress?.Region ?? ""),
                },
                Status = (request.LearningPath?.Status ?? (ApiLearning.LearningPathStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "learning_path",
            "audit", "EstimateLearningPath", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a learning path.
    /// </summary>
    public override async Task<ApiLearning.ExplainLearningPathCalculationResponse> ExplainLearningPathCalculation(ApiLearning.ExplainLearningPathCalculationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainLearningPathCalculation for {Tenant}", requestContext.TenantId);
        if (request.LearningPath is { } record)
        {
            var snapshot = LearningPathMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.WatchCorporateEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (request.LearningPath?.Manager?.Email ?? ""),
                Phone = (request.Details?.Contact?.Phone ?? ""),
                PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
            },
        };
        using var stream0 = _corporateeventsCorporateEventService.WatchCorporateEvents(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchCorporateEvents frame {Type}", change0.GetType().Name);
        }
        var profile1 = new MappingProfile<ApiLearning.ExplainLearningPathCalculationRequest, ApiMedia.CreateMediaAssetRequest>(value => new ApiMedia.CreateMediaAssetRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            MediaAsset = new ApiMedia.MediaAsset
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (value.LearningPath?.Audit?.CreatedBy ?? ""),
                    Etag = (value.LearningPath?.Audit?.Etag ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (value.LearningPath?.Manager?.Email ?? ""),
                },
            },
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _mediaMediaAssetService.CreateMediaAssetAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiAnomalies.ResolveAnomaliesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        input2 = PropertyMap.Copy(input2);
        using var rpc2 = _anomaliesAnomalyQueryService.ResolveAnomaliesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiItsm.SearchIncidentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.SearchIncidentsRequest.Types.Filter
            {
                CreatedAfter = (request.LearningPath?.Audit?.UpdatedAt ?? new Timestamp()),
                OwnerId = (reply1.MediaAsset?.Owner?.UserId ?? ""),
                Region = (reply1.MediaAsset ?? new ApiMedia.MediaAsset()).LocatorCase switch
                {
                    ApiMedia.MediaAsset.LocatorOneofCase.Address => (reply1.MediaAsset?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc3 = _itsmIncidentService.SearchIncidentsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiLearning.ExplainLearningPathCalculationResponse
        {
            LearningPath = new ApiLearning.LearningPath
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.MediaAsset?.Audit?.CreatedBy ?? ""),
                },
                LearningPathId = (request.LearningPathId ?? ""),
                Manager = (request.LearningPath?.Manager ?? new ApiIdentity.UserRef()),
            },
            Outcome = new ApiLearning.ExplainLearningPathCalculationResponse.Types.Outcome
            {
                Message = (reply2.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply3.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "learning_path",
            "audit", "ExplainLearningPathCalculation", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a learning path.
    /// </summary>
    public override async Task<ApiLearning.GetLearningPathRatesResponse> GetLearningPathRates(ApiLearning.GetLearningPathRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.LearningPathId, "learning_path_id");
        _logger.LogInformation("Handling GetLearningPathRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "learning_path", request.LearningPathId,
            "GetLearningPathRates", "accepted", token);
        var input0 = new ApiWorkplaces.ValidateDeskRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Desk = new ApiWorkplaces.Desk
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
            Details = new ApiWorkplaces.ValidateDeskRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
        };
        var reply0 = await SendGetLearningPathRates0Async(input0, token);
        var response = new ApiLearning.GetLearningPathRatesResponse
        {
            LearningPath = new ApiLearning.LearningPath
            {
                LearningPathId = (request.LearningPathId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Recalculates a learning path.
    /// </summary>
    public override async Task<ApiLearning.RecalculateLearningPathsResponse> RecalculateLearningPaths(ApiLearning.RecalculateLearningPathsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecalculateLearningPaths for {Tenant}", requestContext.TenantId);
        if (request.LearningPath is { } record)
        {
            var snapshot = LearningPathMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.CreateCorporateEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Location = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Locality = (request.LearningPath?.Location?.Locality ?? ""),
                },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _corporateeventsCorporateEventService.CreateCorporateEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.EstimateChangeRequestRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.EstimateChangeRequestRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.CorporateEvent?.Location?.Recipient ?? ""),
                },
            },
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiItsm.EstimateChangeRequestRequest>();
        using var rpc1 = _itsmChangeRequestCalculationService.EstimateChangeRequestAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLearning.RecalculateLearningPathsResponse
        {
            Breakdown = new ApiLearning.RecalculateLearningPathsResponse.Types.Breakdown
            {
                CurrencyCode = (reply1.Breakdown?.CurrencyCode ?? ""),
            },
            LearningPath = new ApiLearning.LearningPath
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.CorporateEvent?.Audit?.CreatedBy ?? ""),
                },
                LearningPathId = (request.LearningPathId ?? ""),
                Status = (request.LearningPath?.Status ?? (ApiLearning.LearningPathStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "learning_path",
            "audit", "RecalculateLearningPaths", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Simulates a learning path.
    /// </summary>
    public override async Task<ApiLearning.SimulateLearningPathResponse> SimulateLearningPath(ApiLearning.SimulateLearningPathRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SimulateLearningPath for {Tenant}", requestContext.TenantId);
        if (request.LearningPath is { } record)
        {
            var snapshot = LearningPathMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAuthorization.WatchAccessPoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiAuthorization.WatchAccessPoliciesRequest.Types.Filter
            {
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        using var stream0 = _authorizationAccessPolicyService.WatchAccessPolicies(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchAccessPolicies frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiWorkplaces.SuggestBuildingsRequest();
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        input1.Filter = new ApiWorkplaces.SuggestBuildingsRequest.Types.Filter
        {
            Region = (request.Details?.Destination?.Region ?? ""),
        };
        using var rpc1 = _workplacesBuildingQueryService.SuggestBuildingsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLearning.SimulateLearningPathResponse
        {
            LearningPath = new ApiLearning.LearningPath
            {
                LearningPathId = (request.LearningPathId ?? ""),
                Manager = (request.LearningPath?.Manager ?? new ApiIdentity.UserRef()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "learning_path",
            "audit", "SimulateLearningPath", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiWorkplaces.ValidateDeskResponse> SendGetLearningPathRates0Async(ApiWorkplaces.ValidateDeskRequest request, CancellationToken token)
    {
        return await _learningPathCalculationEndpointSendGetLearningPathRates0Port.SendAsync(request, token);
    }
}
