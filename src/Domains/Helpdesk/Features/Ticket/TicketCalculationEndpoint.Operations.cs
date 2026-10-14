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
using ApiConcessions = Acme.Concessions.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;

namespace Acme.Workforce.Helpdesk;

public sealed partial class TicketCalculationEndpoint
{
    /// <summary>
    /// Calculates a ticket.
    /// </summary>
    public override async Task<ApiHelpdesk.CalculateTicketResponse> CalculateTicket(ApiHelpdesk.CalculateTicketRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CalculateTicket for {Tenant}", requestContext.TenantId);
        if (request.Ticket is { } record)
        {
            var snapshot = TicketMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiFeatureflags.BatchGetFeatureFlagsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _featureflagsFeatureFlagService.BatchGetFeatureFlagsAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var batch1 = (request.Ticket?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiHelpdesk.TicketComment>()).Select(async item =>
        {
            var input1 = new ApiSearch.CreateSearchIndexRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                SearchIndex = new ApiSearch.SearchIndex
                {
                    Address = new ApiCommon.PostalAddress
                    {
                        PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                    },
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (item.CreatedAt ?? new Timestamp()),
                    },
                    Owner = new ApiIdentity.UserRef
                    {
                        Email = (item.AuthorEmail ?? ""),
                    },
                },
            };
            using var rpc1 = _searchSearchIndexService.CreateSearchIndexAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            return reply1;
        }).ToArray();
        var replies1 = await Task.WhenAll(batch1);
        var reply1 = replies1.LastOrDefault() ?? new ApiSearch.CreateSearchIndexResponse();
        var input2 = new ApiDevices.ExplainDevicePolicyCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Ticket?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (request.Ticket?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        using var rpc2 = _devicesDevicePolicyCalculationService.ExplainDevicePolicyCalculationAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        var input3 = new ApiDevices.SuggestManagedDevicesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDevices.SuggestManagedDevicesRequest.Types.Filter
            {
                Region = (reply1.SearchIndex?.Address?.Region ?? ""),
            },
        };
        using var rpc3 = _devicesManagedDeviceQueryService.SuggestManagedDevicesAsync(input3, cancellationToken: token);
        var pending3 = rpc3.ResponseAsync;
        await Task.WhenAll(pending2, pending3);
        var reply2 = await pending2;
        var reply3 = await pending3;
        var response = new ApiHelpdesk.CalculateTicketResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply3.Result?.Message ?? ""),
                Status = (reply3.Result?.Status ?? ""),
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply2.DevicePolicy?.Owner?.Email ?? ""),
                },
                Status = (request.Ticket?.Status ?? (ApiHelpdesk.TicketStatus)0),
                TicketId = (request.TicketId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Ticket?.TicketId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "ticket",
            "audit", "CalculateTicket", (request.Ticket?.TicketId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares a ticket.
    /// </summary>
    public override async Task<ApiHelpdesk.CompareTicketOptionsResponse> CompareTicketOptions(ApiHelpdesk.CompareTicketOptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CompareTicketOptions for {Tenant}", requestContext.TenantId);
        if (request.Ticket is { } record)
        {
            var snapshot = TicketMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.SimulateChangeRequestRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Ticket ?? new ApiHelpdesk.Ticket()).OriginCase switch
                    {
                        ApiHelpdesk.Ticket.OriginOneofCase.Customer => (request.Ticket?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.Ticket?.Audit?.UpdatedBy ?? ""),
                },
                Customer = new ApiItsm.IncidentRef
                {
                    DisplayName = (request.Ticket?.Assignee?.DisplayName ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Ticket?.Requester?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _itsmChangeRequestCalculationService.SimulateChangeRequestAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiNotifications.ExplainNotificationDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Notification = new ApiNotifications.Notification
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.ChangeRequest?.Assignee?.DisplayName ?? ""),
                    Email = (reply0.ChangeRequest?.Requester?.Email ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Ticket?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        var descriptor1 = new Method<ApiNotifications.ExplainNotificationDecisionRequest, ApiNotifications.ExplainNotificationDecisionResponse>(
            MethodType.Unary, "acme.notifications.v1" + ".NotificationValidationService", "ExplainNotificationDecision",
            Wire.Marshaller<ApiNotifications.ExplainNotificationDecisionRequest>(), Wire.Marshaller<ApiNotifications.ExplainNotificationDecisionResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.CompareTicketOptionsResponse
        {
            Ticket = new ApiHelpdesk.Ticket
            {
                Assignee = (request.Ticket?.Assignee ?? new ApiIdentity.UserRef()),
                Requester = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (reply0.ChangeRequest?.Requester?.PreferredChannel ?? ""),
                },
                Sla = new ApiHelpdesk.TicketSla
                {
                    PausedFor = (reply1.Notification?.Sla?.PausedFor ?? new Duration()),
                },
                Subject = (reply1.Notification?.Subject ?? ""),
                TicketId = (request.TicketId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Ticket?.TicketId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "ticket",
            "audit", "CompareTicketOptions", (request.Ticket?.TicketId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates a ticket.
    /// </summary>
    public override async Task<ApiHelpdesk.EstimateTicketResponse> EstimateTicket(ApiHelpdesk.EstimateTicketRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EstimateTicket for {Tenant}", requestContext.TenantId);
        if (request.Ticket is { } record)
        {
            var snapshot = TicketMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.CalculateManagedDeviceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Address = new ApiCommon.PostalAddress
                {
                    Recipient = (request.Details?.Destination?.Recipient ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiDevices.CalculateManagedDeviceRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _devicesManagedDeviceCalculationService.CalculateManagedDeviceAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiHelpdesk.EstimateTicketRequest, ApiItsm.ValidateServiceAssetRequest>(value => new ApiItsm.ValidateServiceAssetRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Address = new ApiCommon.PostalAddress
                {
                    Locality = (value.Details?.Destination?.Locality ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.ManagedDevice?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _itsmServiceAssetValidationService.ValidateServiceAssetAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.EstimateTicketResponse
        {
            Ticket = new ApiHelpdesk.Ticket
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.ServiceAsset?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (reply0.ManagedDevice?.Audit?.Etag ?? ""),
                },
                Status = (request.Ticket?.Status ?? (ApiHelpdesk.TicketStatus)0),
                TicketId = (request.TicketId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Ticket?.TicketId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "ticket",
            "audit", "EstimateTicket", (request.Ticket?.TicketId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a ticket.
    /// </summary>
    public override async Task<ApiHelpdesk.ExplainTicketCalculationResponse> ExplainTicketCalculation(ApiHelpdesk.ExplainTicketCalculationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainTicketCalculation for {Tenant}", requestContext.TenantId);
        if (request.Ticket is { } record)
        {
            var snapshot = TicketMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.RecalculateComplianceScansRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (request.Ticket ?? new ApiHelpdesk.Ticket()).OriginCase switch
                    {
                        ApiHelpdesk.Ticket.OriginOneofCase.Customer => (request.Ticket?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                    Email = (request.Ticket?.Requester?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        using var rpc0 = _devicesComplianceScanCalculationService.RecalculateComplianceScansAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiHelpdesk.ExplainTicketCalculationResponse
        {
            Ticket = new ApiHelpdesk.Ticket
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.ComplianceScan?.Subject?.DisplayName ?? ""),
                },
                Status = (request.Ticket?.Status ?? (ApiHelpdesk.TicketStatus)0),
                TicketId = (request.TicketId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Ticket?.TicketId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "ticket",
            "audit", "ExplainTicketCalculation", (request.Ticket?.TicketId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a ticket.
    /// </summary>
    public override async Task<ApiHelpdesk.GetTicketRatesResponse> GetTicketRates(ApiHelpdesk.GetTicketRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TicketId, "ticket_id");
        _logger.LogInformation("Handling GetTicketRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "ticket", request.TicketId,
            "GetTicketRates", "accepted", token);
        var input0 = request.GetTicketRatesToValidateServiceAsset0();
        using var rpc0 = _itsmServiceAssetValidationService.ValidateServiceAssetAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.ListDocumentFolderSubscriptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentFolderSubscriptionsRequest.Types.Filter
            {
                Region = (reply0.ServiceAsset ?? new ApiItsm.ServiceAsset()).LocatorCase switch
                {
                    ApiItsm.ServiceAsset.LocatorOneofCase.Address => (reply0.ServiceAsset?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc1 = _documentsDocumentFolderNotificationService.ListDocumentFolderSubscriptionsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.GetTicketRatesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply0.ServiceAsset?.Owner?.Email ?? ""),
                },
                Tags = { (reply0.ServiceAsset?.Tags ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                TicketId = (request.TicketId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Quotes a ticket.
    /// </summary>
    public override async Task<ApiHelpdesk.QuoteTicketResponse> QuoteTicket(ApiHelpdesk.QuoteTicketRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling QuoteTicket for {Tenant}", requestContext.TenantId);
        if (request.Ticket is { } record)
        {
            var snapshot = TicketMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.RecalculateManagedDevicesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Address = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Locality = (request.Details?.Destination?.Locality ?? ""),
                    Recipient = (request.Details?.Destination?.Recipient ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Ticket?.Audit?.Etag ?? ""),
                },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _devicesManagedDeviceCalculationService.RecalculateManagedDevicesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiConcessions.CountConcessionStandsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiConcessions.CountConcessionStandsRequest.Types.Filter
            {
                Region = (reply0.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                {
                    ApiDevices.ManagedDevice.LocatorOneofCase.Address => (reply0.ManagedDevice?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc1 = _concessionsConcessionStandQueryService.CountConcessionStandsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.QuoteTicketResponse
        {
            Breakdown = new ApiHelpdesk.QuoteTicketResponse.Types.Breakdown
            {
                CurrencyCode = (request.Details?.CurrencyCode ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Message = (reply1.Result?.Message ?? ""),
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply0.ManagedDevice?.Owner?.Email ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply0.ManagedDevice?.Owner?.Email ?? ""),
                },
                TicketId = (request.TicketId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Ticket?.TicketId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "ticket",
            "audit", "QuoteTicket", (request.Ticket?.TicketId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Simulates a ticket.
    /// </summary>
    public override async Task<ApiHelpdesk.SimulateTicketResponse> SimulateTicket(ApiHelpdesk.SimulateTicketRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SimulateTicket for {Tenant}", requestContext.TenantId);
        if (request.Ticket is { } record)
        {
            var snapshot = TicketMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiConcessions.FindConcessionStandsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiConcessions.FindConcessionStandsRequest.Types.Filter
            {
                CreatedAfter = (request.Ticket?.Sla?.DueAt ?? new Timestamp()),
                OwnerId = (request.Ticket ?? new ApiHelpdesk.Ticket()).OriginCase switch
                {
                    ApiHelpdesk.Ticket.OriginOneofCase.Customer => (request.Ticket?.Customer?.KnowledgeArticleId ?? ""),
                    _ => ""
                },
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        using var rpc0 = _concessionsConcessionStandQueryService.FindConcessionStandsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiHelpdesk.SimulateTicketResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                Assignee = (request.Ticket?.Assignee ?? new ApiIdentity.UserRef()),
                TicketId = (request.TicketId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Ticket?.TicketId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "ticket",
            "audit", "SimulateTicket", (request.Ticket?.TicketId ?? ""), token);
        return response;
    }
}
