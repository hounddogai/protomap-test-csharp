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
using ApiEmployees = Acme.Employees.V1;
using ApiFuel = Acme.Fuel.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Itsm;

public sealed partial class IncidentEndpoint
{
    /// <summary>
    /// Creates an incident.
    /// </summary>
    public override async Task<ApiItsm.CloneIncidentResponse> CloneIncident(ApiItsm.CloneIncidentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CloneIncident for {Tenant}", requestContext.TenantId);
        if (request.Incident is { } record)
        {
            var snapshot = IncidentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.CompareIncidentOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Incident = new ApiItsm.Incident
            {
                Customer = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Incident?.Assignee?.DisplayName ?? ""),
                    Email = (request.Incident?.Requester?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _itsmIncidentCalculationService.CompareIncidentOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.SimulateProblemRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Problem = new ApiItsm.Problem
            {
                Customer = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply0.Incident ?? new ApiItsm.Incident()).OriginCase switch
                    {
                        ApiItsm.Incident.OriginOneofCase.Customer => (reply0.Incident?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Incident ?? new ApiItsm.Incident()).OriginCase switch
                    {
                        ApiItsm.Incident.OriginOneofCase.Customer => (reply0.Incident?.Customer?.Email ?? ""),
                        _ => ""
                    },
                },
                Sla = new ApiItsm.ProblemSla
                {
                    PausedFor = (reply0.Incident?.Sla?.PausedFor ?? new Duration()),
                },
            },
        };
        using var rpc1 = _itsmProblemCalculationService.SimulateProblemAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.CloneIncidentResponse
        {
            Incident = new ApiItsm.Incident
            {
                Assignee = (request.Incident?.Assignee ?? new ApiIdentity.UserRef()),
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Incident?.Audit?.CreatedBy ?? ""),
                },
                Comments = { new ApiItsm.IncidentComment
                {
                    CreatedAt = (reply0.Incident?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                CustomFields = { (reply1.Problem?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Priority = (request.Incident?.Priority ?? 0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "CloneIncident", (request.RequestId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Creates an incident.
    /// </summary>
    public override async Task<ApiItsm.CreateIncidentResponse> CreateIncident(ApiItsm.CreateIncidentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CreateIncident for {Tenant}", requestContext.TenantId);
        if (request.Incident is { } record)
        {
            var snapshot = IncidentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.SimulateIncidentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Incident = new ApiItsm.Incident
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Incident ?? new ApiItsm.Incident()).OriginCase switch
                    {
                        ApiItsm.Incident.OriginOneofCase.Customer => (request.Incident?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Incident?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        using var rpc0 = _itsmIncidentCalculationService.SimulateIncidentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.CheckProblemEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.CheckProblemEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Incident ?? new ApiItsm.Incident()).OriginCase switch
                    {
                        ApiItsm.Incident.OriginOneofCase.Customer => (reply0.Incident?.Customer?.Email ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _itsmProblemValidationService.CheckProblemEligibilityAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDevices.ValidateComplianceScanRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (request.Incident?.Assignee?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.ValidateComplianceScanRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply1.Problem?.Requester?.Email ?? ""),
                    Name = (reply1.Problem?.Requester?.Name ?? new ApiCommon.PersonName()),
                    Phone = (reply1.Problem?.Requester?.Phone ?? ""),
                },
            },
        };
        using var rpc2 = _devicesComplianceScanValidationService.ValidateComplianceScanAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiLicensing.CalculateSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply2.ComplianceScan?.Subject?.Email ?? ""),
                },
                Holder = new ApiLicensing.LicenseAssignmentRef
                {
                    DisplayName = (reply1.Problem?.Assignee?.DisplayName ?? ""),
                },
            },
        };
        var named3 = _registry.Get<ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient>("acme.licensing.v1.SoftwareLicenseCalculationService");
        using var rpc3 = named3.CalculateSoftwareLicenseAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiItsm.CreateIncidentResponse
        {
            Incident = new ApiItsm.Incident
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (reply1.Problem?.Assignee?.UserId ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.Problem?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (reply3.SoftwareLicense?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                CustomFields = { (request.Incident?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Priority = (reply0.Incident?.Priority ?? 0),
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply2.ComplianceScan?.Subject?.Email ?? ""),
                    Name = (reply0.Incident?.Requester?.Name ?? new ApiCommon.PersonName()),
                },
                Subject = (request.Incident?.Subject ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "CreateIncident", (request.RequestId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Deletes an incident.
    /// </summary>
    public override async Task<ApiItsm.DeleteIncidentResponse> DeleteIncident(ApiItsm.DeleteIncidentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.IncidentId, "incident_id");
        _logger.LogInformation("Handling DeleteIncident for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "incident", request.IncidentId,
            "DeleteIncident", "accepted", token);
        var input0 = new ApiItsm.BrowseChangeRequestsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.BrowseChangeRequestsRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                OwnerId = (request.IncidentId ?? ""),
            },
        };
        var descriptor0 = new Method<ApiItsm.BrowseChangeRequestsRequest, ApiItsm.BrowseChangeRequestsResponse>(
            MethodType.Unary, "acme.itsm.v1" + ".ChangeRequestQueryService", "BrowseChangeRequests",
            Wire.Marshaller<ApiItsm.BrowseChangeRequestsRequest>(), Wire.Marshaller<ApiItsm.BrowseChangeRequestsResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.ExplainSoftwareLicenseDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.ExplainSoftwareLicenseDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Etag ?? ""),
                },
            },
        };
        var command1 = new DispatchCommand<ApiLicensing.ExplainSoftwareLicenseDecisionRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _licensingSoftwareLicenseValidationService.ExplainSoftwareLicenseDecisionAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLicensing.EstimateSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.SoftwareLicense?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        using var rpc2 = _licensingSoftwareLicenseCalculationService.EstimateSoftwareLicenseAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        var input3 = new ApiLicensing.ExplainLicenseAssignmentDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.SoftwareLicense?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        using var rpc3 = _licensingLicenseAssignmentValidationService.ExplainLicenseAssignmentDecisionAsync(input3, cancellationToken: token);
        var pending3 = rpc3.ResponseAsync;
        await Task.WhenAll(pending2, pending3);
        var reply2 = await pending2;
        var reply3 = await pending3;
        var response = new ApiItsm.DeleteIncidentResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply3.Outcome?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns an incident.
    /// </summary>
    public override async Task<ApiItsm.GetIncidentResponse> GetIncident(ApiItsm.GetIncidentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.IncidentId, "incident_id");
        _logger.LogInformation("Handling GetIncident for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "incident", request.IncidentId,
            "GetIncident", "accepted", token);
        var input0 = request.GetIncidentToCalculateFuelCard0();
        using var rpc0 = _fuelFuelCardCalculationService.CalculateFuelCardAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiItsm.GetIncidentResponse
        {
            Incident = new ApiItsm.Incident
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply0.FuelCard?.Contact?.Email ?? ""),
                },
                IncidentId = (request.IncidentId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists incidents a page at a time.
    /// </summary>
    public override async Task<ApiItsm.ListIncidentsResponse> ListIncidents(ApiItsm.ListIncidentsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListIncidents for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "incident", "batch",
            "ListIncidents", "accepted", token);
        var input0 = new ApiDocuments.FindDocumentFoldersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.FindDocumentFoldersRequest.Types.Filter
            {
                OwnerId = (request.Filter?.OwnerId ?? ""),
                Query = (request.Filter?.Query ?? ""),
            },
            Page = new ApiCommon.PageRequest
            {
                PageSize = (request.Page?.PageSize ?? 0),
                PageToken = (request.Page?.PageToken ?? ""),
            },
        };
        var reply0 = await SendListIncidents0Async(input0, token);
        var response = new ApiItsm.ListIncidentsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.OrderBy ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "ListIncidents", (request.OrderBy ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists incidents a page at a time.
    /// </summary>
    public override async Task<ApiItsm.SearchIncidentsResponse> SearchIncidents(ApiItsm.SearchIncidentsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SearchIncidents for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "incident", "batch",
            "SearchIncidents", "accepted", token);
        var input0 = new ApiItsm.CheckChangeRequestEligibilityRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.CheckChangeRequestEligibilityRequest.Types.Details
            {
                Locale = (request.Context?.Locale ?? ""),
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _itsmChangeRequestValidationService.CheckChangeRequestEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiItsm.SearchIncidentsResponse
        {
            Incidents = { new ApiItsm.Incident
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply0.ChangeRequest?.Assignee?.Email ?? ""),
                },
            } },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "SearchIncidents", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    private async Task<ApiDocuments.FindDocumentFoldersResponse> SendListIncidents0Async(ApiDocuments.FindDocumentFoldersRequest request, CancellationToken token)
    {
        return await _incidentEndpointSendListIncidents0Port.SendAsync(request, token);
    }
}
