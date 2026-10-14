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
using ApiAudit = Acme.Audit.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiPipelines = Acme.Pipelines.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

public sealed partial class BuildingEndpoint
{
    /// <summary>
    /// Takes a stream of buildings and answers once at the end.
    /// </summary>
    public override async Task<ApiWorkplaces.BatchCreateBuildingsResponse> BatchCreateBuildings(IAsyncStreamReader<ApiWorkplaces.BatchCreateBuildingsRequest> requests, ServerCallContext context)
    {
        var token = context.CancellationToken;
        var result = new ApiWorkplaces.BatchCreateBuildingsResponse();
        await foreach (var request in requests.ReadAllAsync(token))
        {
            RequestGuard.Tenant(request.Context);
            var requestContext = request.Context;
            _logger.LogInformation("Handling BatchCreateBuildings for {Tenant}", requestContext.TenantId);
            if (request.Record is { } record)
            {
                var snapshot = BuildingMapper.ToRecord(requestContext.TenantId, record);
                await _repository.SaveAsync(snapshot, token);
            }
            var input0 = new ApiWorkplaces.ExportBuildingsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiWorkplaces.ExportBuildingsRequest.Types.Filter
                {
                    Region = (request.Record?.Location?.Region ?? ""),
                },
            };
            using var stream0 = _workplacesBuildingQueryService.ExportBuildings(input0, cancellationToken: token);
            await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
            {
                _logger.LogDebug("Received ExportBuildings frame {Type}", change0.GetType().Name);
            }
            var input1 = new ApiWorkplaces.EstimateDeskRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Desk = new ApiWorkplaces.Desk
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        UpdatedBy = request.ChunkCase switch
                        {
                            ApiWorkplaces.BatchCreateBuildingsRequest.ChunkOneofCase.Record => (request.Record?.Audit?.UpdatedBy ?? ""),
                            _ => ""
                        },
                    },
                },
                Details = new ApiWorkplaces.EstimateDeskRequest.Types.Details
                {
                    Destination = new ApiCommon.PostalAddress
                    {
                        Recipient = request.ChunkCase switch
                        {
                            ApiWorkplaces.BatchCreateBuildingsRequest.ChunkOneofCase.Record => (request.Record?.Location?.Recipient ?? ""),
                            _ => ""
                        },
                    },
                },
            };
            using var rpc1 = _workplacesDeskCalculationService.EstimateDeskAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            var input2 = new ApiDocuments.SuggestDocumentFoldersRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiDocuments.SuggestDocumentFoldersRequest.Types.Filter
                {
                    CreatedAfter = request.ChunkCase switch
                    {
                        ApiWorkplaces.BatchCreateBuildingsRequest.ChunkOneofCase.Record => (request.Record?.Audit?.UpdatedAt ?? new Timestamp()),
                        _ => new Timestamp()
                    },
                    OwnerId = (reply1.Result?.CorrelationId ?? ""),
                    Region = (reply1.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                    {
                        ApiWorkplaces.Desk.LocatorOneofCase.Address => (reply1.Desk?.Address?.Region ?? ""),
                        _ => ""
                    },
                },
            };
            using var rpc2 = _documentsDocumentFolderQueryService.SuggestDocumentFoldersAsync(input2, cancellationToken: token);
            var reply2 = await rpc2.ResponseAsync;
            var response = new ApiWorkplaces.BatchCreateBuildingsResponse
            {
                Result = new ApiCommon.OperationResult
                {
                    CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                    Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                },
            };
            _logger.LogDebug("Local audit detail {Detail}", (request.Record?.ParentId ?? ""));
            await _repository.RecordAsync(requestContext.TenantId, "building",
                "audit", "BatchCreateBuildings", (request.Record?.ParentId ?? ""), token);
            result = response;
        }
        return result;
    }
    /// <summary>
    /// Returns several buildings at once.
    /// </summary>
    public override async Task<ApiWorkplaces.BatchGetBuildingsResponse> BatchGetBuildings(ApiWorkplaces.BatchGetBuildingsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling BatchGetBuildings for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "building", "batch",
            "BatchGetBuildings", "accepted", token);
        var input0 = new ApiWorkplaces.PreviewBuildingRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Manager = new ApiIdentity.UserRef
                        {
                            Email = (request.Context?.Actor?.Email ?? ""),
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiWorkplaces.PreviewBuildingRequest>(json0);
        using var rpc0 = _workplacesBuildingValidationService.PreviewBuildingAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiWorkplaces.BatchGetBuildingsResponse
        {
            Buildings = { new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Location = (reply0.Building?.Location ?? new ApiCommon.PostalAddress()),
                    } },
                } },
                Location = (reply0.Building?.Location ?? new ApiCommon.PostalAddress()),
            } },
        };
        return response;
    }
    /// <summary>
    /// Creates a building.
    /// </summary>
    public override async Task<ApiWorkplaces.CloneBuildingResponse> CloneBuilding(ApiWorkplaces.CloneBuildingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CloneBuilding for {Tenant}", requestContext.TenantId);
        if (request.Building is { } record)
        {
            var snapshot = BuildingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.ListDocumentTemplateSubscriptionsRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Filter = new ApiDocuments.ListDocumentTemplateSubscriptionsRequest.Types.Filter
        {
            Region = (request.Building?.Location?.Region ?? ""),
        };
        using var rpc0 = _documentsDocumentTemplateNotificationService.ListDocumentTemplateSubscriptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiWorkplaces.QuoteVisitorPassRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiWorkplaces.QuoteVisitorPassRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Building?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    PostalCode = (request.Building?.Location?.PostalCode ?? ""),
                },
            },
        };
        using var rpc1 = _workplacesVisitorPassCalculationService.QuoteVisitorPassAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.CloneBuildingResponse
        {
            Building = new ApiWorkplaces.Building
            {
                Location = (request.Building?.Location ?? new ApiCommon.PostalAddress()),
                Manager = (request.Building?.Manager ?? new ApiIdentity.UserRef()),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "CloneBuilding", (request.RequestId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Creates a building.
    /// </summary>
    public override async Task<ApiWorkplaces.CreateBuildingResponse> CreateBuilding(ApiWorkplaces.CreateBuildingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CreateBuilding for {Tenant}", requestContext.TenantId);
        if (request.Building is { } record)
        {
            var snapshot = BuildingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAudit.ExplainAuditEventDecisionRequest
        {
            AuditEvent = new ApiAudit.AuditEvent
            {
                Attributes = { (request.Building?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiAudit.AuditTrailRef
                {
                    DisplayName = (request.Building?.Manager?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiAudit.ExplainAuditEventDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Building?.Manager?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _auditAuditEventValidationService.ExplainAuditEventDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.SendDocumentTemplateTestNoticeRequest
        {
            Annotations = { (reply0.AuditEvent?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SendDocumentTemplateTestNoticeRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Building?.Manager?.Email ?? ""),
                },
                Variables = { (request.Building?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        using var rpc1 = _documentsDocumentTemplateNotificationService.SendDocumentTemplateTestNoticeAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.CreateBuildingResponse
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Location = new ApiCommon.PostalAddress
                    {
                        CountryCode = (reply1.DocumentTemplate?.BillingAddress?.CountryCode ?? ""),
                    },
                } },
                Location = (request.Building?.Location ?? new ApiCommon.PostalAddress()),
                ParentId = (request.Building?.ParentId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "CreateBuilding", (request.RequestId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Deletes a building.
    /// </summary>
    public override async Task<ApiWorkplaces.DeleteBuildingResponse> DeleteBuilding(ApiWorkplaces.DeleteBuildingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.BuildingId, "building_id");
        _logger.LogInformation("Handling DeleteBuilding for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "building", request.BuildingId,
            "DeleteBuilding", "accepted", token);
        var input0 = new ApiPipelines.UpdatePipelineRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Pipeline = new ApiPipelines.Pipeline
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Etag ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _pipelinesPipelineService.UpdatePipelineAsync);
        var input1 = new ApiWorkplaces.SimulateDeskRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Desk = new ApiWorkplaces.Desk
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Pipeline?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (reply0.Pipeline?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        using var rpc1 = _sendDeleteBuilding1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiWorkplaces.ScoreVisitorPassRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiWorkplaces.ScoreVisitorPassRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Recipient = (reply1.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                    {
                        ApiWorkplaces.Desk.LocatorOneofCase.Address => (reply1.Desk?.Address?.Recipient ?? ""),
                        _ => ""
                    },
                },
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.Pipeline?.Location?.Locality ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply1.Desk?.Owner?.DisplayName ?? ""),
                },
                Lines = { new ApiWorkplaces.VisitorPassLine
                {
                    Description = (reply1.Desk?.Components?.GetValueOrDefault("primary")?.Description ?? ""),
                } },
            },
        };
        using var rpc2 = _workplacesVisitorPassValidationService.ScoreVisitorPassAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiWorkplaces.DeleteBuildingResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply2.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "DeleteBuilding", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a building.
    /// </summary>
    public override async Task<ApiWorkplaces.GetBuildingResponse> GetBuilding(ApiWorkplaces.GetBuildingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.BuildingId, "building_id");
        _logger.LogInformation("Handling GetBuilding for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "building", request.BuildingId,
            "GetBuilding", "accepted", token);
        var response = new ApiWorkplaces.GetBuildingResponse
        {
            Building = new ApiWorkplaces.Building
            {
                BuildingId = (request.BuildingId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists buildings a page at a time.
    /// </summary>
    public override async Task<ApiWorkplaces.ListBuildingsResponse> ListBuildings(ApiWorkplaces.ListBuildingsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListBuildings for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "building", "batch",
            "ListBuildings", "accepted", token);
        var input0 = new ApiItsm.ValidateServiceAssetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Address = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiItsm.ValidateServiceAssetRequest, ApiItsm.ValidateServiceAssetResponse>(
            MethodType.Unary, "acme.itsm.v1" + ".ServiceAssetValidationService", "ValidateServiceAsset",
            Wire.Marshaller<ApiItsm.ValidateServiceAssetRequest>(), Wire.Marshaller<ApiItsm.ValidateServiceAssetResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiWorkplaces.ListBuildingsResponse
        {
            Buildings = { new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Manager = new ApiIdentity.UserRef
                        {
                            UserId = (reply0.ServiceAsset?.Owner?.UserId ?? ""),
                        },
                    } },
                } },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "ListBuildings", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists buildings a page at a time.
    /// </summary>
    public override async Task<ApiWorkplaces.SearchBuildingsResponse> SearchBuildings(ApiWorkplaces.SearchBuildingsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SearchBuildings for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "building", "batch",
            "SearchBuildings", "accepted", token);
        var input0 = new ApiWorkplaces.FindBuildingsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiWorkplaces.FindBuildingsRequest.Types.Filter
            {
                OwnerId = (request.Filter?.OwnerId ?? ""),
                Region = (request.Filter?.Region ?? ""),
            },
            Page = new ApiCommon.PageRequest
            {
                PageSize = (request.Page?.PageSize ?? 0),
            },
        };
        var command0 = new DispatchCommand<ApiWorkplaces.FindBuildingsRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _workplacesBuildingQueryService.FindBuildingsAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiWorkplaces.SearchBuildingsRequest, ApiItsm.ScoreServiceAssetRequest>(value => new ApiItsm.ScoreServiceAssetRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Address = new ApiCommon.PostalAddress
                {
                    Region = (value.Filter?.Region ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (value.Filter?.CreatedAfter ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (value.Context?.Actor?.Email ?? ""),
                    UserId = (value.Context?.Actor?.UserId ?? ""),
                },
            },
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _itsmServiceAssetValidationService.ScoreServiceAssetAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.SearchBuildingsResponse
        {
            Buildings = { new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Location = new ApiCommon.PostalAddress
                        {
                            Recipient = (reply1.ServiceAsset?.Address?.Recipient ?? ""),
                        },
                    } },
                } },
            } },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "SearchBuildings", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Changes a building.
    /// </summary>
    public override async Task<ApiWorkplaces.UpdateBuildingResponse> UpdateBuilding(ApiWorkplaces.UpdateBuildingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling UpdateBuilding for {Tenant}", requestContext.TenantId);
        if (request.Building is { } record)
        {
            var snapshot = BuildingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWorkplaces.EstimateRoomBookingRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiWorkplaces.EstimateRoomBookingRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Building?.Location?.PostalCode ?? ""),
                },
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Building?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Location = new ApiCommon.PostalAddress
                {
                    Region = (request.Building?.Location?.Region ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _workplacesRoomBookingCalculationService.EstimateRoomBookingAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.UpdateBuildingToCountBuildings1(reply0);
        using var rpc1 = _workplacesBuildingQueryService.CountBuildingsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.UpdateBuildingResponse
        {
            Building = new ApiWorkplaces.Building
            {
                BuildingId = (request.Building?.BuildingId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.RoomBooking?.Location?.CountryCode ?? ""),
                },
                Manager = new ApiIdentity.UserRef
                {
                    Email = (reply0.RoomBooking?.Organizer?.Email ?? ""),
                },
                Name = (request.Building?.Name ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Building?.ParentId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "UpdateBuilding", (request.Building?.ParentId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams the changes of buildings as they happen.
    /// </summary>
    public override async Task WatchBuildings(ApiWorkplaces.WatchBuildingsRequest request, IServerStreamWriter<ApiWorkplaces.WatchBuildingsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling WatchBuildings for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "building", "batch",
            "WatchBuildings", "accepted", token);
        var input0 = new ApiHelpdesk.CompareEscalationOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Escalation = new ApiHelpdesk.Escalation
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Subscriber?.Email ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Subscriber?.PreferredChannel ?? ""),
                },
            },
        };
        var reply0 = await SendWatchBuildings0Async(input0, token);
        var input1 = new ApiIdentity.LintUsersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            User = new ApiIdentity.User
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Escalation?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (reply0.Escalation?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (reply0.Escalation?.Requester?.Name ?? new ApiCommon.PersonName()),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Escalation ?? new ApiHelpdesk.Escalation()).OriginCase switch
                    {
                        ApiHelpdesk.Escalation.OriginOneofCase.Customer => (reply0.Escalation?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _identityUserValidationService.LintUsersAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.WatchBuildingsResponse
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Manager = new ApiIdentity.UserRef
                        {
                            UserId = (reply0.Escalation?.Assignee?.UserId ?? ""),
                        },
                    } },
                } },
                Location = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Manager = new ApiIdentity.UserRef
                {
                    Email = (request.Subscriber?.Email ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Subscriber?.Phone ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "building",
            "audit", "WatchBuildings", (request.Subscriber?.Phone ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    private async Task<ApiHelpdesk.CompareEscalationOptionsResponse> SendWatchBuildings0Async(ApiHelpdesk.CompareEscalationOptionsRequest request, CancellationToken token)
    {
        return await _buildingEndpointSendWatchBuildings0Port.SendAsync(request, token);
    }
}
