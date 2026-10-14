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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

public sealed partial class DeskCalculationEndpoint
{
    /// <summary>
    /// Calculates a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.CalculateDeskResponse> CalculateDesk(ApiWorkplaces.CalculateDeskRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CalculateDesk for {Tenant}", requestContext.TenantId);
        if (request.Desk is { } record)
        {
            var snapshot = DeskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.ExportTicketsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiHelpdesk.ExportTicketsRequest.Types.Filter
            {
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        using var stream0 = _helpdeskTicketQueryService.ExportTickets(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportTickets frame {Type}", change0.GetType().Name);
        }
        var input1 = request.CalculateDeskToScheduleDocumentExport1();
        using var rpc1 = _documentsDocumentExportService.ScheduleDocumentExportAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiHelpdesk.CalculateSlaPolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.CalculateSlaPolicyRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                    Recipient = (request.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                    {
                        ApiWorkplaces.Desk.LocatorOneofCase.Address => (request.Desk?.Address?.Recipient ?? ""),
                        _ => ""
                    },
                },
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Desk?.Audit?.Etag ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Desk?.Owner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc2 = _helpdeskSlaPolicyCalculationService.CalculateSlaPolicyAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiItsm.CheckServiceAssetEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Address = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                    {
                        ApiWorkplaces.Desk.LocatorOneofCase.Address => (request.Desk?.Address?.PostalCode ?? ""),
                        _ => ""
                    },
                    Recipient = (request.Details?.Destination?.Recipient ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply2.SlaPolicy?.Audit?.Etag ?? ""),
                },
            },
        };
        using var rpc3 = _itsmServiceAssetValidationService.CheckServiceAssetEligibilityAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiWorkplaces.CalculateDeskResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                Capacity = (reply3.ServiceAsset?.Capacity ?? 0L),
                Code = (request.Desk?.Code ?? ""),
                DeskId = (request.DeskId ?? ""),
                Name = (reply2.SlaPolicy?.Name ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Destination?.Locality ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "desk",
            "audit", "CalculateDesk", (request.Details?.Destination?.Locality ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.CompareDeskOptionsResponse> CompareDeskOptions(ApiWorkplaces.CompareDeskOptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CompareDeskOptions for {Tenant}", requestContext.TenantId);
        if (request.Desk is { } record)
        {
            var snapshot = DeskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWorkplaces.CompareBuildingOptionsRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            CreatedAt = (request.Desk?.Audit?.CreatedAt ?? new Timestamp()),
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiWorkplaces.CompareBuildingOptionsRequest>();
        using var rpc0 = _workplacesBuildingCalculationService.CompareBuildingOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.ExplainEscalationCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.ExplainEscalationCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Desk?.Owner?.Email ?? ""),
                },
            },
            Escalation = new ApiHelpdesk.Escalation
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (reply0.Building?.Manager?.UserId ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Desk?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiHelpdesk.ExplainEscalationCalculationRequest>(json1);
        using var rpc1 = _helpdeskEscalationCalculationService.ExplainEscalationCalculationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.CompareDeskOptionsResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Building?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (reply1.Escalation?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Code = (request.Desk?.Code ?? ""),
                DeskId = (request.DeskId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "desk",
            "audit", "CompareDeskOptions", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.EstimateDeskResponse> EstimateDesk(ApiWorkplaces.EstimateDeskRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EstimateDesk for {Tenant}", requestContext.TenantId);
        if (request.Desk is { } record)
        {
            var snapshot = DeskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.ScoreServiceAssetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.ScoreServiceAssetRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (request.Details?.Destination?.Locality ?? ""),
                    PostalCode = (request.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                    {
                        ApiWorkplaces.Desk.LocatorOneofCase.Address => (request.Desk?.Address?.PostalCode ?? ""),
                        _ => ""
                    },
                },
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Desk?.Owner?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _itsmServiceAssetValidationService.ScoreServiceAssetAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiFeatureflags.LintFeatureFlagsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            FeatureFlag = new ApiFeatureflags.FeatureFlag
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.ServiceAsset?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (reply0.ServiceAsset?.Audit?.CreatedBy ?? ""),
                    Etag = (reply0.ServiceAsset?.Audit?.Etag ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Desk?.Owner?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _featureflagsFeatureFlagValidationService.LintFeatureFlagsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiWorkplaces.EstimateBuildingRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            Etag = (reply0.ServiceAsset?.Audit?.Etag ?? ""),
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply2 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _workplacesBuildingCalculationService.EstimateBuildingAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiWorkplaces.EstimateDeskResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply2.Building?.Audit?.CreatedBy ?? ""),
                },
                Capacity = (request.Desk?.Capacity ?? 0L),
                DeskId = (request.DeskId ?? ""),
                Specs = { (reply0.ServiceAsset?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "desk",
            "audit", "EstimateDesk", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.ExplainDeskCalculationResponse> ExplainDeskCalculation(ApiWorkplaces.ExplainDeskCalculationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainDeskCalculation for {Tenant}", requestContext.TenantId);
        if (request.Desk is { } record)
        {
            var snapshot = DeskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWorkplaces.EstimateBuildingRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            CreatedBy = (request.Desk?.Audit?.CreatedBy ?? ""),
                            UpdatedBy = (request.Desk?.Audit?.UpdatedBy ?? ""),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            PostalCode = (request.Desk?.Address?.PostalCode ?? ""),
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _workplacesBuildingCalculationService.EstimateBuildingAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiWorkplaces.CompareBuildingOptionsRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            CreatedBy = (request.Desk?.Audit?.CreatedBy ?? ""),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            Locality = (request.Desk?.Address?.Locality ?? ""),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (request.Desk?.Owner?.DisplayName ?? ""),
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _workplacesBuildingCalculationService.CompareBuildingOptionsAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var input2 = new ApiWorkplaces.SimulateBuildingRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Location = new ApiCommon.PostalAddress
                        {
                            PostalCode = (reply1.Building?.Location?.PostalCode ?? ""),
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc2 = _sendExplainDeskCalculation2(input2, new CallOptions(cancellationToken: token));
        var reply2 = await rpc2.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiWorkplaces.SimulateBuildingRequest>("acme.workplaces.v1.DeskCalculationService/ExplainDeskCalculation/2", input2.Clone()), token);
        var input3 = new ApiCorporateevents.ResolveRegistrationsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var named3 = _registry.Get<ApiCorporateevents.RegistrationQueryService.RegistrationQueryServiceClient>("acme.corporateevents.v1.RegistrationQueryService");
        using var rpc3 = named3.ResolveRegistrationsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiWorkplaces.ExplainDeskCalculationResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                DeskId = (request.DeskId ?? ""),
                Name = (reply2.Building?.Name ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.Building?.Manager?.DisplayName ?? ""),
                },
                Status = (request.Desk?.Status ?? (ApiWorkplaces.DeskStatus)0),
            },
            Outcome = new ApiWorkplaces.ExplainDeskCalculationResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply3.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "desk",
            "audit", "ExplainDeskCalculation", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.GetDeskRatesResponse> GetDeskRates(ApiWorkplaces.GetDeskRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DeskId, "desk_id");
        _logger.LogInformation("Handling GetDeskRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "desk", request.DeskId,
            "GetDeskRates", "accepted", token);
        var input0 = new ApiItsm.CountChangeRequestsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.CountChangeRequestsRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                Labels = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.DeskId ?? ""),
            },
        };
        var descriptor0 = new Method<ApiItsm.CountChangeRequestsRequest, ApiItsm.CountChangeRequestsResponse>(
            MethodType.Unary, "acme.itsm.v1" + ".ChangeRequestQueryService", "CountChangeRequests",
            Wire.Marshaller<ApiItsm.CountChangeRequestsRequest>(), Wire.Marshaller<ApiItsm.CountChangeRequestsResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiWorkplaces.GetDeskRatesResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                DeskId = (request.DeskId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Quotes a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.QuoteDeskResponse> QuoteDesk(ApiWorkplaces.QuoteDeskRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling QuoteDesk for {Tenant}", requestContext.TenantId);
        if (request.Desk is { } record)
        {
            var snapshot = DeskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWorkplaces.SimulateBuildingRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            Etag = (request.Desk?.Audit?.Etag ?? ""),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var command0 = new DispatchCommand<ApiWorkplaces.SimulateBuildingRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _workplacesBuildingCalculationService.SimulateBuildingAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiWorkplaces.QuoteDeskResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Building?.Audit?.CreatedBy ?? ""),
                    UpdatedBy = (reply0.Building?.Audit?.UpdatedBy ?? ""),
                },
                Code = (request.Desk?.Code ?? ""),
                DeskId = (request.DeskId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "desk",
            "audit", "QuoteDesk", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalculates a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.RecalculateDesksResponse> RecalculateDesks(ApiWorkplaces.RecalculateDesksRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecalculateDesks for {Tenant}", requestContext.TenantId);
        if (request.Desk is { } record)
        {
            var snapshot = DeskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiWorkplaces.RecalculateDesksRequest, ApiHelpdesk.EstimateEscalationRequest>(value => new ApiHelpdesk.EstimateEscalationRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.EstimateEscalationRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (value.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _helpdeskEscalationCalculationService.EstimateEscalationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.ValidateServiceAssetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Escalation?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _itsmServiceAssetValidationService.ValidateServiceAssetAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.RecalculateDesksResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.ServiceAsset?.Audit?.UpdatedBy ?? ""),
                },
                DeskId = (request.DeskId ?? ""),
                Name = (request.Desk?.Name ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.Escalation?.Requester?.Email ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "desk",
            "audit", "RecalculateDesks", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Simulates a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.SimulateDeskResponse> SimulateDesk(ApiWorkplaces.SimulateDeskRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SimulateDesk for {Tenant}", requestContext.TenantId);
        if (request.Desk is { } record)
        {
            var snapshot = DeskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.SimulateDeskToCompareBuildingOptions0();
        using var rpc0 = _workplacesBuildingCalculationService.CompareBuildingOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiWorkplaces.SimulateDeskResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Building?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                DeskId = (request.DeskId ?? ""),
                Specs = { (request.Desk?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "desk",
            "audit", "SimulateDesk", (request.Reason ?? ""), token);
        return response;
    }
}
