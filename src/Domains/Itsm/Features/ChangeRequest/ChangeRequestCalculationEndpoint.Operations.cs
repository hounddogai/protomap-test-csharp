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
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiFeaturestore = Acme.Featurestore.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Itsm;

public sealed partial class ChangeRequestCalculationEndpoint
{
    /// <summary>
    /// Calculates a change request.
    /// </summary>
    public override async Task<ApiItsm.CalculateChangeRequestResponse> CalculateChangeRequest(ApiItsm.CalculateChangeRequestRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CalculateChangeRequest for {Tenant}", requestContext.TenantId);
        if (request.ChangeRequest is { } record)
        {
            var snapshot = ChangeRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.AnnotateSignatureVersionRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.AnnotateSignatureVersionRequest.Types.Details
            {
                AuthorEmail = (request.ChangeRequest?.Assignee?.Email ?? ""),
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (request.ChangeRequest?.Assignee?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _documentsSignatureHistoryService.AnnotateSignatureVersionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiItsm.CalculateChangeRequestResponse
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                ChangeRequestId = (request.ChangeRequestId ?? ""),
                Status = (request.ChangeRequest?.Status ?? (ApiItsm.ChangeRequestStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "CalculateChangeRequest", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares a change request.
    /// </summary>
    public override async Task<ApiItsm.CompareChangeRequestOptionsResponse> CompareChangeRequestOptions(ApiItsm.CompareChangeRequestOptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CompareChangeRequestOptions for {Tenant}", requestContext.TenantId);
        if (request.ChangeRequest is { } record)
        {
            var snapshot = ChangeRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.ChangeRequest?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiItsm.ChangeRequestComment>()).Select(async item =>
        {
            var input0 = new ApiDevices.EstimateComplianceScanRequest
            {
                ComplianceScan = new ApiDevices.ComplianceScan
                {
                    Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                    OccurredAt = (item.CreatedAt ?? new Timestamp()),
                    Subject = new ApiRecruiting.CandidateRef
                    {
                        Email = (item.AuthorEmail ?? ""),
                    },
                },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiDevices.EstimateComplianceScanRequest.Types.Details
                {
                    Destination = new ApiCommon.PostalAddress
                    {
                        Region = (request.Details?.Destination?.Region ?? ""),
                    },
                },
            };
            using var rpc0 = _devicesComplianceScanCalculationService.EstimateComplianceScanAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiDevices.EstimateComplianceScanResponse();
        var input1 = request.CompareChangeRequestOptionsToBatchGetAccessPolicies1();
        using var rpc1 = _authorizationAccessPolicyService.BatchGetAccessPoliciesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiItsm.SimulateIncidentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.SimulateIncidentRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Recipient = (request.Details?.Destination?.Recipient ?? ""),
                },
            },
            Incident = new ApiItsm.Incident
            {
                Channel = new ApiItsm.IncidentChannelOrigin
                {
                    Channel = (request.ChangeRequest ?? new ApiItsm.ChangeRequest()).OriginCase switch
                    {
                        ApiItsm.ChangeRequest.OriginOneofCase.Channel => (request.ChangeRequest?.Channel?.Channel ?? ""),
                        _ => ""
                    },
                },
                Requester = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.ChangeRequest?.Requester?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc2 = _itsmIncidentCalculationService.SimulateIncidentAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiFeaturestore.BrowseFeatureViewsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiFeaturestore.BrowseFeatureViewsRequest.Types.Filter
            {
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        var labels3 = input3.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input3.Context.Baggage.Clear();
        input3.Context.Baggage.Add(labels3);
        using var rpc3 = _featurestoreFeatureViewQueryService.BrowseFeatureViewsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiItsm.CompareChangeRequestOptionsResponse
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                ChangeRequestId = (request.ChangeRequestId ?? ""),
                CustomFields = { (request.ChangeRequest?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "CompareChangeRequestOptions", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates a change request.
    /// </summary>
    public override async Task<ApiItsm.EstimateChangeRequestResponse> EstimateChangeRequest(ApiItsm.EstimateChangeRequestRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EstimateChangeRequest for {Tenant}", requestContext.TenantId);
        if (request.ChangeRequest is { } record)
        {
            var snapshot = ChangeRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.ValidateEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.ValidateEntitlementRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.ChangeRequest?.Requester?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (request.ChangeRequest?.Requester?.PreferredChannel ?? ""),
                },
            },
            Entitlement = new ApiLicensing.Entitlement
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.ChangeRequest ?? new ApiItsm.ChangeRequest()).OriginCase switch
                    {
                        ApiItsm.ChangeRequest.OriginOneofCase.Customer => (request.ChangeRequest?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                    UserId = (request.ChangeRequest?.Assignee?.UserId ?? ""),
                },
            },
        };
        using var rpc0 = _licensingEntitlementValidationService.ValidateEntitlementAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiItsm.EstimateChangeRequestResponse
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                ChangeRequestId = (request.ChangeRequestId ?? ""),
                Requester = (request.ChangeRequest?.Requester ?? new ApiCommon.ContactInfo()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "EstimateChangeRequest", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a change request.
    /// </summary>
    public override async Task<ApiItsm.GetChangeRequestRatesResponse> GetChangeRequestRates(ApiItsm.GetChangeRequestRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ChangeRequestId, "change_request_id");
        _logger.LogInformation("Handling GetChangeRequestRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "change_request", request.ChangeRequestId,
            "GetChangeRequestRates", "accepted", token);
        var input0 = new ApiItsm.CompareIncidentOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Incident = new ApiItsm.Incident
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiItsm.CompareIncidentOptionsRequest>(json0);
        using var rpc0 = _itsmIncidentCalculationService.CompareIncidentOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiItsm.GetChangeRequestRatesResponse
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                ChangeRequestId = (request.ChangeRequestId ?? ""),
                CustomFields = { (reply0.Incident?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Sla = new ApiItsm.ChangeRequestSla
                {
                    DueAt = (reply0.Incident?.Sla?.DueAt ?? new Timestamp()),
                },
            },
        };
        return response;
    }
    /// <summary>
    /// Quotes a change request.
    /// </summary>
    public override async Task<ApiItsm.QuoteChangeRequestResponse> QuoteChangeRequest(ApiItsm.QuoteChangeRequestRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling QuoteChangeRequest for {Tenant}", requestContext.TenantId);
        if (request.ChangeRequest is { } record)
        {
            var snapshot = ChangeRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.ChangeRequest?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiItsm.ChangeRequestComment>()).Select(async item =>
        {
            var input0 = new ApiDocuments.ResumeDocumentTemplateWorkflowRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                DocumentTemplate = new ApiDocuments.DocumentTemplate
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (item.CreatedAt ?? new Timestamp()),
                    },
                },
            };
            using var rpc0 = _documentsDocumentTemplateWorkflowService.ResumeDocumentTemplateWorkflowAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiDocuments.ResumeDocumentTemplateWorkflowResponse();
        var reply1 = new ApiDocuments.ShareSignatureExportResponse();
        foreach (var item in (request.ChangeRequest?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiItsm.ChangeRequestComment>()))
        {
            var input1 = new ApiDocuments.ShareSignatureExportRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiDocuments.ShareSignatureExportRequest.Types.Details
                {
                    Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                    Recipient = new ApiCommon.ContactInfo
                    {
                        Email = (item.AuthorEmail ?? ""),
                    },
                },
            };
            using var rpc1 = _documentsSignatureExportService.ShareSignatureExportAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var input2 = new ApiItsm.CompareIncidentOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.CompareIncidentOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
        };
        var reply2 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _itsmIncidentCalculationService.CompareIncidentOptionsAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiItsm.QuoteChangeRequestResponse
        {
            Breakdown = new ApiItsm.QuoteChangeRequestResponse.Types.Breakdown
            {
                CurrencyCode = (request.Details?.CurrencyCode ?? ""),
            },
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                ChangeRequestId = (request.ChangeRequestId ?? ""),
                Tags = { (reply2.Incident?.Tags ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "QuoteChangeRequest", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalculates a change request.
    /// </summary>
    public override async Task<ApiItsm.RecalculateChangeRequestsResponse> RecalculateChangeRequests(ApiItsm.RecalculateChangeRequestsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecalculateChangeRequests for {Tenant}", requestContext.TenantId);
        if (request.ChangeRequest is { } record)
        {
            var snapshot = ChangeRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.ExplainComplianceScanCalculationRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.ExplainComplianceScanCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.ChangeRequest?.Requester?.Email ?? ""),
                    Name = (request.ChangeRequest?.Requester?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.ChangeRequest?.Requester?.Phone ?? ""),
                },
            },
        };
        using var rpc0 = _devicesComplianceScanCalculationService.ExplainComplianceScanCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var reply1 = new ApiDevices.CheckComplianceScanEligibilityResponse();
        foreach (var item in (request.ChangeRequest?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiItsm.ChangeRequestComment>()))
        {
            var input1 = new ApiDevices.CheckComplianceScanEligibilityRequest
            {
                ComplianceScan = new ApiDevices.ComplianceScan
                {
                    Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                    Measurements = { new ApiDevices.ComplianceScanMeasurement
                    {
                        ObservedAt = (item.CreatedAt ?? new Timestamp()),
                    } },
                    Subject = new ApiRecruiting.CandidateRef
                    {
                        CandidateId = (reply0.ComplianceScan?.Subject?.CandidateId ?? ""),
                    },
                },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiDevices.CheckComplianceScanEligibilityRequest.Types.Details
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (item.AuthorEmail ?? ""),
                    },
                },
            };
            using var rpc1 = _devicesComplianceScanValidationService.CheckComplianceScanEligibilityAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var input2 = new ApiAudit.ResolveAuditEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc2 = _sendRecalculateChangeRequests2(input2, new CallOptions(cancellationToken: token));
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiLicensing.CreateSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.ChangeRequest?.Requester?.Phone ?? ""),
                },
                Holder = new ApiLicensing.LicenseAssignmentRef
                {
                    DisplayName = (request.ChangeRequest ?? new ApiItsm.ChangeRequest()).OriginCase switch
                    {
                        ApiItsm.ChangeRequest.OriginOneofCase.Customer => (request.ChangeRequest?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc3 = _licensingSoftwareLicenseService.CreateSoftwareLicenseAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiItsm.RecalculateChangeRequestsResponse
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                ChangeRequestId = (request.ChangeRequestId ?? ""),
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply0.ComplianceScan?.Subject?.Email ?? ""),
                    Name = (reply3.SoftwareLicense?.Contact?.Name ?? new ApiCommon.PersonName()),
                    Phone = (reply3.SoftwareLicense?.Contact?.Phone ?? ""),
                },
                Subject = (request.ChangeRequest?.Subject ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.ComplianceScan?.CorrelationId ?? ""),
                Message = (reply2.Result?.Message ?? ""),
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "RecalculateChangeRequests", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Simulates a change request.
    /// </summary>
    public override async Task<ApiItsm.SimulateChangeRequestResponse> SimulateChangeRequest(ApiItsm.SimulateChangeRequestRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SimulateChangeRequest for {Tenant}", requestContext.TenantId);
        if (request.ChangeRequest is { } record)
        {
            var snapshot = ChangeRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.PreviewManagedDeviceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.ChangeRequest?.Assignee?.Email ?? ""),
                    UserId = (request.ChangeRequest?.Assignee?.UserId ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiDevices.PreviewManagedDeviceRequest, ApiDevices.PreviewManagedDeviceResponse>(
            MethodType.Unary, "acme.devices.v1" + ".ManagedDeviceValidationService", "PreviewManagedDevice",
            Wire.Marshaller<ApiDevices.PreviewManagedDeviceRequest>(), Wire.Marshaller<ApiDevices.PreviewManagedDeviceResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiItsm.SimulateChangeRequestResponse
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply0.ManagedDevice?.Owner?.Email ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.ManagedDevice?.Audit?.CreatedAt ?? new Timestamp()),
                },
                ChangeRequestId = (request.ChangeRequestId ?? ""),
                Requester = (request.ChangeRequest?.Requester ?? new ApiCommon.ContactInfo()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "SimulateChangeRequest", (request.Reason ?? ""), token);
        return response;
    }
}
