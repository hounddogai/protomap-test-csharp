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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Itsm;

public sealed partial class IncidentCalculationEndpoint
{
    /// <summary>
    /// Calculates an incident.
    /// </summary>
    public override async Task<ApiItsm.CalculateIncidentResponse> CalculateIncident(ApiItsm.CalculateIncidentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CalculateIncident for {Tenant}", requestContext.TenantId);
        if (request.Incident is { } record)
        {
            var snapshot = IncidentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiNotifications.CreateNotificationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Notification = new ApiNotifications.Notification
            {
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Incident ?? new ApiItsm.Incident()).OriginCase switch
                    {
                        ApiItsm.Incident.OriginOneofCase.Customer => (request.Incident?.Customer?.Email ?? ""),
                        _ => ""
                    },
                },
                Sla = new ApiNotifications.NotificationSla
                {
                    PolicyId = (request.Incident?.Sla?.PolicyId ?? ""),
                },
            },
        };
        using var rpc0 = _notificationsNotificationService.CreateNotificationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.CloneSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.Notification?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _licensingSoftwareLicenseService.CloneSoftwareLicenseAsync);
        var response = new ApiItsm.CalculateIncidentResponse
        {
            Incident = new ApiItsm.Incident
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply0.Notification?.Assignee?.Email ?? ""),
                },
                Comments = { new ApiItsm.IncidentComment
                {
                    CreatedAt = (reply1.SoftwareLicense?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                IncidentId = (request.IncidentId ?? ""),
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Notification?.Assignee?.Email ?? ""),
                },
                Status = (request.Incident?.Status ?? (ApiItsm.IncidentStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "CalculateIncident", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares an incident.
    /// </summary>
    public override async Task<ApiItsm.CompareIncidentOptionsResponse> CompareIncidentOptions(ApiItsm.CompareIncidentOptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CompareIncidentOptions for {Tenant}", requestContext.TenantId);
        if (request.Incident is { } record)
        {
            var snapshot = IncidentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.LintComplianceScansRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (request.Incident ?? new ApiItsm.Incident()).OriginCase switch
                    {
                        ApiItsm.Incident.OriginOneofCase.Customer => (request.Incident?.Customer?.Email ?? ""),
                        _ => ""
                    },
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.LintComplianceScansRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Incident?.Requester?.Email ?? ""),
                    Phone = (request.Incident?.Requester?.Phone ?? ""),
                    PreferredChannel = (request.Incident?.Requester?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc0 = _devicesComplianceScanValidationService.LintComplianceScansAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiAudit.CheckAuditEventEligibilityRequest
        {
            AuditEvent = new ApiAudit.AuditEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiAudit.CheckAuditEventEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Incident?.Assignee?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _auditAuditEventValidationService.CheckAuditEventEligibilityAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var reply2 = new ApiDevices.ListManagedDevicesResponse();
        foreach (var item in (request.Incident?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiItsm.IncidentComment>()))
        {
            var input2 = new ApiDevices.ListManagedDevicesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiDevices.ListManagedDevicesRequest.Types.Filter
                {
                    CreatedAfter = (item.CreatedAt ?? new Timestamp()),
                    Labels = { (request.Incident?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    OwnerId = (item.CommentId ?? ""),
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
            };
            using var rpc2 = _devicesManagedDeviceService.ListManagedDevicesAsync(input2, cancellationToken: token);
            reply2 = await rpc2.ResponseAsync;
        }
        var response = new ApiItsm.CompareIncidentOptionsResponse
        {
            Incident = new ApiItsm.Incident
            {
                IncidentId = (request.IncidentId ?? ""),
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply0.ComplianceScan?.Subject?.Email ?? ""),
                },
                Subject = (request.Incident?.Subject ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "CompareIncidentOptions", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates an incident.
    /// </summary>
    public override async Task<ApiItsm.EstimateIncidentResponse> EstimateIncident(ApiItsm.EstimateIncidentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EstimateIncident for {Tenant}", requestContext.TenantId);
        if (request.Incident is { } record)
        {
            var snapshot = IncidentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiDocuments.ListDocumentsResponse();
        foreach (var item in (request.Incident?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiItsm.IncidentComment>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input0 = new ApiDocuments.ListDocumentsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiDocuments.ListDocumentsRequest.Types.Filter
                {
                    CreatedAfter = (item.CreatedAt ?? new Timestamp()),
                    Labels = { (request.Incident?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    OwnerId = (item.CommentId ?? ""),
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
            };
            using var rpc0 = _documentsDocumentService.ListDocumentsAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var profile1 = new MappingProfile<ApiItsm.EstimateIncidentRequest, ApiDocuments.PreviewDocumentRequest>(value => new ApiDocuments.PreviewDocumentRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Document = new ApiDocuments.Document
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (value.Incident?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (value.Incident?.Audit?.CreatedBy ?? ""),
                    UpdatedBy = (value.Incident?.Audit?.UpdatedBy ?? ""),
                },
            },
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _documentsDocumentValidationService.PreviewDocumentAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var reply2 = new ApiLicensing.CountSoftwareLicensesResponse();
        foreach (var item in (request.Incident?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiItsm.IncidentComment>()))
        {
            var input2 = new ApiLicensing.CountSoftwareLicensesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiLicensing.CountSoftwareLicensesRequest.Types.Filter
                {
                    CreatedAfter = (item.CreatedAt ?? new Timestamp()),
                },
            };
            using var rpc2 = _licensingSoftwareLicenseQueryService.CountSoftwareLicensesAsync(input2, cancellationToken: token);
            reply2 = await rpc2.ResponseAsync;
        }
        var input3 = new ApiDocuments.SimulateSignatureRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SimulateSignatureRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply1.Document?.BillingAddress?.CountryCode ?? ""),
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                    Recipient = (request.Details?.Destination?.Recipient ?? ""),
                },
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (request.Incident ?? new ApiItsm.Incident()).OriginCase switch
                    {
                        ApiItsm.Incident.OriginOneofCase.Customer => (request.Incident?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc3 = _documentsSignatureCalculationService.SimulateSignatureAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiItsm.EstimateIncidentResponse
        {
            Breakdown = new ApiItsm.EstimateIncidentResponse.Types.Breakdown
            {
                CurrencyCode = (reply3.Breakdown?.CurrencyCode ?? ""),
            },
            Incident = new ApiItsm.Incident
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply3.Signature?.Subject?.DisplayName ?? ""),
                },
                CustomFields = { (request.Incident?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                IncidentId = (request.IncidentId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "EstimateIncident", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains an incident.
    /// </summary>
    public override async Task<ApiItsm.ExplainIncidentCalculationResponse> ExplainIncidentCalculation(ApiItsm.ExplainIncidentCalculationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainIncidentCalculation for {Tenant}", requestContext.TenantId);
        if (request.Incident is { } record)
        {
            var snapshot = IncidentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.Incident?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiItsm.IncidentComment>()).Select(async item =>
        {
            var input0 = new ApiLicensing.BrowseSoftwareLicensesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiLicensing.BrowseSoftwareLicensesRequest.Types.Filter
                {
                    CreatedAfter = (item.CreatedAt ?? new Timestamp()),
                    Labels = { (request.Incident?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    OwnerId = (item.CommentId ?? ""),
                },
            };
            using var rpc0 = _licensingSoftwareLicenseQueryService.BrowseSoftwareLicensesAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiLicensing.BrowseSoftwareLicensesResponse();
        var reply1 = new ApiLicensing.CountEntitlementsResponse();
        foreach (var item in (request.Incident?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiItsm.IncidentComment>()))
        {
            var input1 = new ApiLicensing.CountEntitlementsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiLicensing.CountEntitlementsRequest.Types.Filter
                {
                    CreatedAfter = (item.CreatedAt ?? new Timestamp()),
                    OwnerId = (item.CommentId ?? ""),
                },
            };
            using var rpc1 = _licensingEntitlementQueryService.CountEntitlementsAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var reply2 = new ApiLicensing.ExplainSoftwareLicenseDecisionResponse();
        foreach (var item in (request.Incident?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiItsm.IncidentComment>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input2 = new ApiLicensing.ExplainSoftwareLicenseDecisionRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                SoftwareLicense = new ApiLicensing.SoftwareLicense
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (item.CreatedAt ?? new Timestamp()),
                        CreatedBy = (request.Incident?.Audit?.CreatedBy ?? ""),
                    },
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (item.AuthorEmail ?? ""),
                        Name = (request.Incident?.Requester?.Name ?? new ApiCommon.PersonName()),
                    },
                },
            };
            using var rpc2 = _licensingSoftwareLicenseValidationService.ExplainSoftwareLicenseDecisionAsync(input2, cancellationToken: token);
            reply2 = await rpc2.ResponseAsync;
        }
        foreach (var item in (request.Incident?.History ?? new Google.Protobuf.Collections.RepeatedField<ApiItsm.IncidentStatusChange>()))
        {
            var input3 = new ApiDevices.ExportManagedDevicesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiDevices.ExportManagedDevicesRequest.Types.Filter
                {
                    CreatedAfter = (item.ChangedAt ?? new Timestamp()),
                },
            };
            using var stream3 = _devicesManagedDeviceQueryService.ExportManagedDevices(input3, cancellationToken: token);
            await foreach (var change3 in stream3.ResponseStream.ReadAllAsync(token))
            {
                _logger.LogDebug("Received ExportManagedDevices frame {Type}", change3.GetType().Name);
            }
        }
        var response = new ApiItsm.ExplainIncidentCalculationResponse
        {
            Incident = new ApiItsm.Incident
            {
                IncidentId = (request.IncidentId ?? ""),
                Requester = (request.Incident?.Requester ?? new ApiCommon.ContactInfo()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "ExplainIncidentCalculation", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an incident.
    /// </summary>
    public override async Task<ApiItsm.GetIncidentRatesResponse> GetIncidentRates(ApiItsm.GetIncidentRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.IncidentId, "incident_id");
        _logger.LogInformation("Handling GetIncidentRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "incident", request.IncidentId,
            "GetIncidentRates", "accepted", token);
        var input0 = new ApiDocuments.BatchGetDocumentsRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        using var rpc0 = _documentsDocumentService.BatchGetDocumentsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiItsm.GetIncidentRatesResponse
        {
            Incident = new ApiItsm.Incident
            {
                IncidentId = (request.IncidentId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Recalculates an incident.
    /// </summary>
    public override async Task<ApiItsm.RecalculateIncidentsResponse> RecalculateIncidents(ApiItsm.RecalculateIncidentsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecalculateIncidents for {Tenant}", requestContext.TenantId);
        if (request.Incident is { } record)
        {
            var snapshot = IncidentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.SuggestSoftwareLicensesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLicensing.SuggestSoftwareLicensesRequest.Types.Filter
            {
                CreatedAfter = (request.Incident?.Audit?.UpdatedAt ?? new Timestamp()),
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        using var rpc0 = _licensingSoftwareLicenseQueryService.SuggestSoftwareLicensesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiItsm.RecalculateIncidentsResponse
        {
            Incident = new ApiItsm.Incident
            {
                CustomFields = { (request.Incident?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                IncidentId = (request.IncidentId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "RecalculateIncidents", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Simulates an incident.
    /// </summary>
    public override async Task<ApiItsm.SimulateIncidentResponse> SimulateIncident(ApiItsm.SimulateIncidentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SimulateIncident for {Tenant}", requestContext.TenantId);
        if (request.Incident is { } record)
        {
            var snapshot = IncidentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.StreamDocumentTemplateExportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.StreamDocumentTemplateExportRequest.Types.Filter
            {
                CreatedAfter = (request.Incident?.Sla?.DueAt ?? new Timestamp()),
                Labels = { (request.Incident?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Incident?.Customer?.EmployeeId ?? ""),
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        using var stream0 = _documentsDocumentTemplateExportService.StreamDocumentTemplateExport(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamDocumentTemplateExport frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiLicensing.ValidateSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.ValidateSoftwareLicenseRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Incident?.Requester?.Phone ?? ""),
                },
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Holder = new ApiLicensing.LicenseAssignmentRef
                {
                    DisplayName = (request.Incident?.Assignee?.DisplayName ?? ""),
                },
            },
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _licensingSoftwareLicenseValidationService.ValidateSoftwareLicenseAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiLicensing.PreviewLicenseAssignmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Incident?.Audit?.CreatedBy ?? ""),
                },
                Card = new ApiLicensing.LicenseAssignmentCardSettlement
                {
                    HolderName = (reply1.SoftwareLicense ?? new ApiLicensing.SoftwareLicense()).PaymentCase switch
                    {
                        ApiLicensing.SoftwareLicense.FundingOneofCase.Card => (reply1.SoftwareLicense?.Card?.HolderName ?? ""),
                        _ => ""
                    },
                    Network = (reply1.SoftwareLicense ?? new ApiLicensing.SoftwareLicense()).PaymentCase switch
                    {
                        ApiLicensing.SoftwareLicense.FundingOneofCase.Card => (reply1.SoftwareLicense?.Card?.Network ?? ""),
                        _ => ""
                    },
                    Token = (reply1.SoftwareLicense ?? new ApiLicensing.SoftwareLicense()).PaymentCase switch
                    {
                        ApiLicensing.SoftwareLicense.FundingOneofCase.Card => (reply1.SoftwareLicense?.Card?.Token ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc2 = _licensingLicenseAssignmentValidationService.PreviewLicenseAssignmentAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiLicensing.PreviewLicenseAssignmentRequest>("acme.itsm.v1.IncidentCalculationService/SimulateIncident/2", input2.Clone()), token);
        var input3 = new ApiDocuments.RetryDocumentTemplateExportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.RetryDocumentTemplateExportRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Phone = (request.Incident?.Requester?.Phone ?? ""),
                },
            },
        };
        using var rpc3 = _sendSimulateIncident3(input3, new CallOptions(cancellationToken: token));
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiItsm.SimulateIncidentResponse
        {
            Breakdown = new ApiItsm.SimulateIncidentResponse.Types.Breakdown
            {
                CurrencyCode = (request.Details?.CurrencyCode ?? ""),
            },
            Incident = new ApiItsm.Incident
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.SoftwareLicense?.Holder?.DisplayName ?? ""),
                },
                Comments = { new ApiItsm.IncidentComment
                {
                    CreatedAt = (reply3.DocumentTemplate?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                IncidentId = (request.IncidentId ?? ""),
                Requester = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (reply1.SoftwareLicense?.Contact?.PreferredChannel ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply3.Result?.CorrelationId ?? ""),
                Message = (reply2.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "SimulateIncident", (request.Reason ?? ""), token);
        return response;
    }
}
