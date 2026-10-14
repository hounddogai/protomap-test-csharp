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
using ApiAudiences = Acme.Audiences.V1;
using ApiAudit = Acme.Audit.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiMaintenance = Acme.Maintenance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;
using ApiVenues = Acme.Venues.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

public sealed partial class BuildingCalculationEndpoint
{
    /// <summary>
    /// Calculates a building.
    /// </summary>
    public override async Task<ApiWorkplaces.CalculateBuildingResponse> CalculateBuilding(ApiWorkplaces.CalculateBuildingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CalculateBuilding for {Tenant}", requestContext.TenantId);
        if (request.Building is { } record)
        {
            var snapshot = BuildingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.CountChangeRequestsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.CountChangeRequestsRequest.Types.Filter
            {
                CreatedAfter = (request.Building?.Audit?.CreatedAt ?? new Timestamp()),
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiItsm.CountChangeRequestsRequest>();
        using var rpc0 = _itsmChangeRequestQueryService.CountChangeRequestsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.AnnotateDocumentFolderVersionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.AnnotateDocumentFolderVersionRequest.Types.Details
            {
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Location = new ApiCommon.PostalAddress
                        {
                            CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                            PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                            Region = (request.Building?.Location?.Region ?? ""),
                        },
                    } },
                    Location = new ApiCommon.PostalAddress
                    {
                        Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    },
                } },
            },
        };
        using var rpc1 = _documentsDocumentFolderHistoryService.AnnotateDocumentFolderVersionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.CalculateBuildingResponse
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
                            Path = { (reply1.DocumentFolder?.Path ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                        } },
                    } },
                } },
                Location = new ApiCommon.PostalAddress
                {
                    Region = (reply1.DocumentFolder?.Location?.Region ?? ""),
                },
                ParentId = (request.Building?.ParentId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "CalculateBuilding", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares a building.
    /// </summary>
    public override async Task<ApiWorkplaces.CompareBuildingOptionsResponse> CompareBuildingOptions(ApiWorkplaces.CompareBuildingOptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CompareBuildingOptions for {Tenant}", requestContext.TenantId);
        if (request.Building is { } record)
        {
            var snapshot = BuildingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.PushDocumentTemplateRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.PushDocumentTemplateRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
            },
        };
        using var rpc0 = _documentsDocumentTemplateSyncService.PushDocumentTemplateAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiWorkplaces.CompareBuildingOptionsResponse
        {
            Building = new ApiWorkplaces.Building
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.DocumentTemplate?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                BuildingId = (request.BuildingId ?? ""),
                Name = (request.Building?.Name ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "CompareBuildingOptions", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates a building.
    /// </summary>
    public override async Task<ApiWorkplaces.EstimateBuildingResponse> EstimateBuilding(ApiWorkplaces.EstimateBuildingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EstimateBuilding for {Tenant}", requestContext.TenantId);
        if (request.Building is { } record)
        {
            var snapshot = BuildingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.ExplainItineraryDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Itinerary = new ApiTravel.Itinerary
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Building?.Location?.PostalCode ?? ""),
                },
            },
        };
        using var rpc0 = _travelItineraryValidationService.ExplainItineraryDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDevices.ExplainComplianceScanCalculationRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Attributes = { (request.Building?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (reply0.Itinerary?.Counterparty?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.ExplainComplianceScanCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Building?.Manager?.Email ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc1 = _devicesComplianceScanCalculationService.ExplainComplianceScanCalculationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiHelpdesk.PreviewEscalationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.PreviewEscalationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Building?.Manager?.Email ?? ""),
                },
            },
            Escalation = new ApiHelpdesk.Escalation
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.ComplianceScan?.Subject?.DisplayName ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Itinerary?.Audit?.CreatedBy ?? ""),
                    UpdatedBy = (reply0.Itinerary?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        var reply2 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _helpdeskEscalationValidationService.PreviewEscalationAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiWorkplaces.EstimateBuildingResponse
        {
            Building = new ApiWorkplaces.Building
            {
                BuildingId = (request.BuildingId ?? ""),
                Children = { new ApiWorkplaces.Building
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        UpdatedAt = (reply0.Itinerary?.Audit?.UpdatedAt ?? new Timestamp()),
                    },
                    Children = { new ApiWorkplaces.Building
                    {
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (reply2.Escalation?.Customer?.DisplayName ?? ""),
                        },
                    } },
                    Manager = new ApiIdentity.UserRef
                    {
                        DisplayName = (reply2.Escalation?.Customer?.DisplayName ?? ""),
                        Email = (reply1.ComplianceScan?.Subject?.Email ?? ""),
                    },
                } },
                Depth = (request.Building?.Depth ?? 0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Destination?.Region ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "EstimateBuilding", (request.Details?.Destination?.Region ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a building.
    /// </summary>
    public override async Task<ApiWorkplaces.GetBuildingRatesResponse> GetBuildingRates(ApiWorkplaces.GetBuildingRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.BuildingId, "building_id");
        _logger.LogInformation("Handling GetBuildingRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "building", request.BuildingId,
            "GetBuildingRates", "accepted", token);
        var input0 = new ApiDevices.CalculateComplianceScanRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Measurements = { new ApiDevices.ComplianceScanMeasurement
                {
                    ObservedAt = (request.Context?.Deadline ?? new Timestamp()),
                } },
                Source = (request.BuildingId ?? ""),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _devicesComplianceScanCalculationService.CalculateComplianceScanAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiWorkplaces.GetBuildingRatesResponse
        {
            Building = new ApiWorkplaces.Building
            {
                BuildingId = (request.BuildingId ?? ""),
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Attributes = { (reply0.ComplianceScan?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    } },
                } },
            },
        };
        return response;
    }
    /// <summary>
    /// Quotes a building.
    /// </summary>
    public override async Task<ApiWorkplaces.QuoteBuildingResponse> QuoteBuilding(ApiWorkplaces.QuoteBuildingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling QuoteBuilding for {Tenant}", requestContext.TenantId);
        if (request.Building is { } record)
        {
            var snapshot = BuildingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiMaintenance.FindWorkOrdersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiMaintenance.FindWorkOrdersRequest.Types.Filter
            {
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        using var rpc0 = _sendQuoteBuilding0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiAudit.ScoreAuditEventRequest
        {
            AuditEvent = new ApiAudit.AuditEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiAudit.ScoreAuditEventRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                    Recipient = (request.Building?.Location?.Recipient ?? ""),
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
            },
        };
        using var rpc1 = _auditAuditEventValidationService.ScoreAuditEventAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiVenues.SummarizeVenuesRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiVenues.SummarizeVenuesRequest.Types.Details
            {
                Filters = { (reply1.AuditEvent?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Reason = (request.Reason ?? ""),
        };
        var descriptor2 = new Method<ApiVenues.SummarizeVenuesRequest, ApiVenues.SummarizeVenuesResponse>(
            MethodType.Unary, "acme.venues.v1" + ".VenueQueryService", "SummarizeVenues",
            Wire.Marshaller<ApiVenues.SummarizeVenuesRequest>(), Wire.Marshaller<ApiVenues.SummarizeVenuesResponse>());
        using var rpc2 = _invoker.AsyncUnaryCall(descriptor2, null,
            new CallOptions(cancellationToken: token), input2);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiWorkplaces.QuoteBuildingResponse
        {
            Breakdown = new ApiWorkplaces.QuoteBuildingResponse.Types.Breakdown
            {
                CurrencyCode = (reply1.Breakdown?.CurrencyCode ?? ""),
            },
            Building = new ApiWorkplaces.Building
            {
                BuildingId = (request.BuildingId ?? ""),
                Location = (request.Building?.Location ?? new ApiCommon.PostalAddress()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Destination?.Recipient ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "QuoteBuilding", (request.Details?.Destination?.Recipient ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalculates a building.
    /// </summary>
    public override async Task<ApiWorkplaces.RecalculateBuildingsResponse> RecalculateBuildings(ApiWorkplaces.RecalculateBuildingsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecalculateBuildings for {Tenant}", requestContext.TenantId);
        if (request.Building is { } record)
        {
            var snapshot = BuildingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.CheckItineraryEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.CheckItineraryEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Building?.Manager?.Email ?? ""),
                },
            },
            Itinerary = new ApiTravel.Itinerary
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Building?.Audit?.Etag ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (request.Building?.Location?.Recipient ?? ""),
                },
                Counterparty = new ApiTravel.TravelBookingRef
                {
                    DisplayName = (request.Building?.Manager?.DisplayName ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiTravel.CheckItineraryEligibilityRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _travelItineraryValidationService.CheckItineraryEligibilityAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiWorkplaces.RecalculateBuildingsRequest, ApiItsm.ResolveIncidentsRequest>(value => new ApiItsm.ResolveIncidentsRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _itsmIncidentQueryService.ResolveIncidentsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiTravel.CloneTripRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Trip = new ApiTravel.Trip
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (request.Building?.Location?.Recipient ?? ""),
                },
            },
        };
        input2 = PropertyMap.Copy(input2);
        using var rpc2 = _travelTripService.CloneTripAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiWorkplaces.RecalculateBuildingsResponse
        {
            Building = new ApiWorkplaces.Building
            {
                Attributes = { (request.Building?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                BuildingId = (request.BuildingId ?? ""),
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (reply2.Trip?.Counterparty?.DisplayName ?? ""),
                        },
                    } },
                } },
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "RecalculateBuildings", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Simulates a building.
    /// </summary>
    public override async Task<ApiWorkplaces.SimulateBuildingResponse> SimulateBuilding(ApiWorkplaces.SimulateBuildingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SimulateBuilding for {Tenant}", requestContext.TenantId);
        if (request.Building is { } record)
        {
            var snapshot = BuildingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.SimulateBuildingToSuggestAudiences0();
        using var rpc0 = _audiencesAudienceQueryService.SuggestAudiencesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.LintItinerariesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Itinerary = new ApiTravel.Itinerary
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Building?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        var reply1 = await SendSimulateBuilding1Async(input1, token);
        var input2 = new ApiDocuments.PushDocumentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.PushDocumentRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Document = new ApiDocuments.Document
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Building?.Audit?.Etag ?? ""),
                    UpdatedAt = (reply1.Itinerary?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply1.Itinerary?.BillingAddress?.CountryCode ?? ""),
                },
            },
        };
        using var rpc2 = _documentsDocumentSyncService.PushDocumentAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiWorkplaces.SimulateBuildingResponse
        {
            Building = new ApiWorkplaces.Building
            {
                BuildingId = (request.BuildingId ?? ""),
                Location = (request.Building?.Location ?? new ApiCommon.PostalAddress()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Message = (reply2.Outcome?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "SimulateBuilding", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiTravel.LintItinerariesResponse> SendSimulateBuilding1Async(ApiTravel.LintItinerariesRequest request, CancellationToken token)
    {
        return await _buildingCalculationEndpointSendSimulateBuilding1Port.SendAsync(request, token);
    }
}
