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
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

public sealed partial class BuildingValidationEndpoint
{
    /// <summary>
    /// Checks a building.
    /// </summary>
    public override async Task<ApiWorkplaces.CheckBuildingEligibilityResponse> CheckBuildingEligibility(ApiWorkplaces.CheckBuildingEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CheckBuildingEligibility for {Tenant}", requestContext.TenantId);
        if (request.Building is { } record)
        {
            var snapshot = BuildingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWorkplaces.RecalculateDesksRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Desk = new ApiWorkplaces.Desk
            {
                Address = new ApiCommon.PostalAddress
                {
                    Recipient = (request.Building?.Location?.Recipient ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Building?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Building?.Manager?.UserId ?? ""),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiWorkplaces.RecalculateDesksRequest>();
        using var rpc0 = _workplacesDeskCalculationService.RecalculateDesksAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiWorkplaces.CalculateDeskRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Desk = new ApiWorkplaces.Desk
            {
                Address = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Building?.Location?.CountryCode ?? ""),
                },
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiWorkplaces.CalculateDeskRequest>(json1);
        using var rpc1 = _workplacesDeskCalculationService.CalculateDeskAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.CheckBuildingEligibilityResponse
        {
            Building = new ApiWorkplaces.Building
            {
                Attributes = { (request.Building?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                BuildingId = (request.BuildingId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.Desk?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                Name = (reply1.Desk?.Name ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "CheckBuildingEligibility", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a building.
    /// </summary>
    public override async Task<ApiWorkplaces.ExplainBuildingDecisionResponse> ExplainBuildingDecision(ApiWorkplaces.ExplainBuildingDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainBuildingDecision for {Tenant}", requestContext.TenantId);
        if (request.Building is { } record)
        {
            var snapshot = BuildingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWorkplaces.CalculateBuildingRequest();
        input0.Building = new ApiWorkplaces.Building
        {
            Children = { new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Location = new ApiCommon.PostalAddress
                    {
                        Recipient = (request.Building?.Location?.Recipient ?? ""),
                        Region = (request.Building?.Location?.Region ?? ""),
                    },
                } },
            } },
        };
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        using var rpc0 = _workplacesBuildingCalculationService.CalculateBuildingAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiWorkplaces.ExplainBuildingDecisionResponse
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
                            CountryCode = (reply0.Building?.Location?.CountryCode ?? ""),
                            PostalCode = (reply0.Building?.Location?.PostalCode ?? ""),
                        },
                    } },
                } },
                ParentId = (request.Building?.ParentId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "ExplainBuildingDecision", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a building.
    /// </summary>
    public override async Task<ApiWorkplaces.GetBuildingRulesResponse> GetBuildingRules(ApiWorkplaces.GetBuildingRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.BuildingId, "building_id");
        _logger.LogInformation("Handling GetBuildingRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "building", request.BuildingId,
            "GetBuildingRules", "accepted", token);
        var input0 = new ApiWorkplaces.SimulateDeskRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Desk = new ApiWorkplaces.Desk
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        using var rpc0 = _workplacesDeskCalculationService.SimulateDeskAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiWorkplaces.GetBuildingRulesResponse
        {
            Building = new ApiWorkplaces.Building
            {
                BuildingId = (request.BuildingId ?? ""),
                Children = { new ApiWorkplaces.Building
                {
                    Manager = new ApiIdentity.UserRef
                    {
                        UserId = (reply0.Desk?.Owner?.UserId ?? ""),
                    },
                } },
            },
        };
        return response;
    }
    /// <summary>
    /// Lints a building.
    /// </summary>
    public override async Task<ApiWorkplaces.LintBuildingsResponse> LintBuildings(ApiWorkplaces.LintBuildingsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling LintBuildings for {Tenant}", requestContext.TenantId);
        if (request.Building is { } record)
        {
            var snapshot = BuildingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWorkplaces.EstimateDeskRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Desk = new ApiWorkplaces.Desk
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Building?.Audit?.Etag ?? ""),
                    UpdatedAt = (request.Building?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
            Details = new ApiWorkplaces.EstimateDeskRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (request.Building?.Location?.Locality ?? ""),
                    Region = (request.Building?.Location?.Region ?? ""),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _workplacesDeskCalculationService.EstimateDeskAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiWorkplaces.CalculateDeskRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Desk = new ApiWorkplaces.Desk
            {
                Address = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                    {
                        ApiWorkplaces.Desk.LocatorOneofCase.Address => (reply0.Desk?.Address?.CountryCode ?? ""),
                        _ => ""
                    },
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Building?.Audit?.CreatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Building?.Manager?.DisplayName ?? ""),
                },
            },
            Details = new ApiWorkplaces.CalculateDeskRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Building?.Location?.CountryCode ?? ""),
                },
            },
        };
        using var rpc1 = _workplacesDeskCalculationService.CalculateDeskAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiIdentity.SearchUsersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiIdentity.SearchUsersRequest.Types.Filter
            {
                CreatedAfter = (request.Building?.Audit?.UpdatedAt ?? new Timestamp()),
                Region = (request.Building?.Location?.Region ?? ""),
            },
        };
        var reply2 = await Workflow.SendAsync(input2, token, _identityUserService.SearchUsersAsync);
        var response = new ApiWorkplaces.LintBuildingsResponse
        {
            Building = new ApiWorkplaces.Building
            {
                BuildingId = (request.BuildingId ?? ""),
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Children = { new ApiWorkplaces.Building
                        {
                            Name = (reply0.Desk?.Name ?? ""),
                        } },
                        Location = new ApiCommon.PostalAddress
                        {
                            Locality = (reply1.Desk?.Address?.Locality ?? ""),
                        },
                        Name = (reply1.Desk?.Name ?? ""),
                    } },
                } },
                Depth = (request.Building?.Depth ?? 0),
            },
            Outcome = new ApiWorkplaces.LintBuildingsResponse.Types.Outcome
            {
                Message = (reply2.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "LintBuildings", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Normalizes a building.
    /// </summary>
    public override async Task<ApiWorkplaces.NormalizeBuildingResponse> NormalizeBuilding(ApiWorkplaces.NormalizeBuildingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling NormalizeBuilding for {Tenant}", requestContext.TenantId);
        if (request.Building is { } record)
        {
            var snapshot = BuildingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.SyncSignaturesRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SyncSignaturesRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Reason = (request.Reason ?? ""),
            Signature = new ApiDocuments.Signature
            {
                Attributes = { (request.Building?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (request.Building?.Manager?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _documentsSignatureSyncService.SyncSignaturesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.ScheduleDocumentExportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ScheduleDocumentExportRequest.Types.Details
            {
                Filters = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            DocumentId = (reply0.Signature?.Subject?.DocumentId ?? ""),
            Reason = (request.Reason ?? ""),
        };
        var named1 = _registry.Get<ApiDocuments.DocumentExportService.DocumentExportServiceClient>("acme.documents.v1.DocumentExportService");
        using var rpc1 = named1.ScheduleDocumentExportAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiItsm.RecalculateProblemsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Problem = new ApiItsm.Problem
            {
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
        };
        var descriptor2 = new Method<ApiItsm.RecalculateProblemsRequest, ApiItsm.RecalculateProblemsResponse>(
            MethodType.Unary, "acme.itsm.v1" + ".ProblemCalculationService", "RecalculateProblems",
            Wire.Marshaller<ApiItsm.RecalculateProblemsRequest>(), Wire.Marshaller<ApiItsm.RecalculateProblemsResponse>());
        using var rpc2 = _invoker.AsyncUnaryCall(descriptor2, null,
            new CallOptions(cancellationToken: token), input2);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiWorkplaces.NormalizeBuildingResponse
        {
            Building = new ApiWorkplaces.Building
            {
                BuildingId = (request.BuildingId ?? ""),
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            Etag = (reply2.Problem?.Audit?.Etag ?? ""),
                        },
                    } },
                } },
                Manager = (request.Building?.Manager ?? new ApiIdentity.UserRef()),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Locale ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "NormalizeBuilding", (request.Details?.Locale ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews a building.
    /// </summary>
    public override async Task<ApiWorkplaces.PreviewBuildingResponse> PreviewBuilding(ApiWorkplaces.PreviewBuildingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PreviewBuilding for {Tenant}", requestContext.TenantId);
        if (request.Building is { } record)
        {
            var snapshot = BuildingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAuthorization.CheckAccessPolicyEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiAuthorization.CheckAccessPolicyEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Contact?.Phone ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiAuthorization.CheckAccessPolicyEligibilityRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _authorizationAccessPolicyValidationService.CheckAccessPolicyEligibilityAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.CompareSignatureVersionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.CompareSignatureVersionsRequest.Types.Details
            {
                AuthorEmail = (request.Details?.Contact?.Email ?? ""),
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Reason = (reply0.AccessPolicy ?? new ApiAuthorization.AccessPolicy()).ActionCase switch
            {
                ApiAuthorization.AccessPolicy.ActionOneofCase.Block => (reply0.AccessPolicy?.Block?.Reason ?? ""),
                _ => ""
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (reply0.AccessPolicy?.Owner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _documentsSignatureHistoryService.CompareSignatureVersionsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.PreviewBuildingResponse
        {
            Building = new ApiWorkplaces.Building
            {
                Attributes = { (request.Building?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                BuildingId = (request.BuildingId ?? ""),
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Name = (reply0.AccessPolicy?.Name ?? ""),
                    } },
                } },
                Name = (reply0.AccessPolicy?.Name ?? ""),
            },
            Outcome = new ApiWorkplaces.PreviewBuildingResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "PreviewBuilding", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores a building.
    /// </summary>
    public override async Task<ApiWorkplaces.ScoreBuildingResponse> ScoreBuilding(ApiWorkplaces.ScoreBuildingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScoreBuilding for {Tenant}", requestContext.TenantId);
        if (request.Building is { } record)
        {
            var snapshot = BuildingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.ExplainTravelPolicyDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Building?.Audit?.Etag ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _travelTravelPolicyValidationService.ExplainTravelPolicyDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiTravel.ExplainTravelPolicyDecisionRequest>("acme.workplaces.v1.BuildingValidationService/ScoreBuilding/0", input0.Clone()), token);
        var input1 = request.ScoreBuildingToCalculateDesk1();
        using var rpc1 = _workplacesDeskCalculationService.CalculateDeskAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.ScoreBuildingResponse
        {
            Breakdown = new ApiWorkplaces.ScoreBuildingResponse.Types.Breakdown
            {
                CurrencyCode = (request.Details?.CurrencyCode ?? ""),
            },
            Building = new ApiWorkplaces.Building
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.TravelPolicy?.Audit?.Etag ?? ""),
                },
                BuildingId = (request.BuildingId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply1.Desk?.Address?.PostalCode ?? ""),
                },
                Manager = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.TravelPolicy?.Owner?.DisplayName ?? ""),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "ScoreBuilding", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates a building.
    /// </summary>
    public override async Task<ApiWorkplaces.ValidateBuildingResponse> ValidateBuilding(ApiWorkplaces.ValidateBuildingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ValidateBuilding for {Tenant}", requestContext.TenantId);
        if (request.Building is { } record)
        {
            var snapshot = BuildingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiMetrics.PreviewMetricSeriesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiMetrics.PreviewMetricSeriesRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
            },
            MetricSeries = new ApiMetrics.MetricSeries
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Building?.Audit?.Etag ?? ""),
                },
            },
        };
        var reply0 = await SendValidateBuilding0Async(input0, token);
        var response = new ApiWorkplaces.ValidateBuildingResponse
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
                            Recipient = (reply0.MetricSeries?.Address?.Recipient ?? ""),
                        },
                    } },
                } },
                Location = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.MetricSeries?.Address?.CountryCode ?? ""),
                },
                Status = (request.Building?.Status ?? (ApiWorkplaces.BuildingStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "ValidateBuilding", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiMetrics.PreviewMetricSeriesResponse> SendValidateBuilding0Async(ApiMetrics.PreviewMetricSeriesRequest request, CancellationToken token)
    {
        return await _buildingValidationEndpointSendValidateBuilding0Port.SendAsync(request, token);
    }
}
