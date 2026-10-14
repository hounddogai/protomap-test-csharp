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

/// <summary>
/// Computes the figures of a change request, such as totals, taxes, and estimates.
/// </summary>
public sealed partial class ChangeRequestCalculationEndpoint : ApiItsm.ChangeRequestCalculationService.ChangeRequestCalculationServiceBase
{
    private readonly ApiAudit.AuditEventQueryService.AuditEventQueryServiceClient _auditAuditEventQueryService;
    private readonly ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient _authorizationAccessPolicyService;
    private readonly ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient _devicesComplianceScanCalculationService;
    private readonly ApiDevices.ComplianceScanValidationService.ComplianceScanValidationServiceClient _devicesComplianceScanValidationService;
    private readonly ApiDevices.ManagedDeviceValidationService.ManagedDeviceValidationServiceClient _devicesManagedDeviceValidationService;
    private readonly ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceClient _documentsDocumentTemplateWorkflowService;
    private readonly ApiDocuments.SignatureExportService.SignatureExportServiceClient _documentsSignatureExportService;
    private readonly ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient _documentsSignatureHistoryService;
    private readonly ApiFeaturestore.FeatureViewQueryService.FeatureViewQueryServiceClient _featurestoreFeatureViewQueryService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.IncidentCalculationService.IncidentCalculationServiceClient _itsmIncidentCalculationService;
    private readonly ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient _licensingEntitlementValidationService;
    private readonly ApiLicensing.SoftwareLicenseService.SoftwareLicenseServiceClient _licensingSoftwareLicenseService;
    private readonly ILogger<ChangeRequestCalculationEndpoint> _logger;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiAudit.ResolveAuditEventsRequest, CallOptions, AsyncUnaryCall<ApiAudit.ResolveAuditEventsResponse>> _sendRecalculateChangeRequests2;

    public ChangeRequestCalculationEndpoint(
        ApiAudit.AuditEventQueryService.AuditEventQueryServiceClient auditAuditEventQueryService,
        ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient authorizationAccessPolicyService,
        ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient devicesComplianceScanCalculationService,
        ApiDevices.ComplianceScanValidationService.ComplianceScanValidationServiceClient devicesComplianceScanValidationService,
        ApiDevices.ManagedDeviceValidationService.ManagedDeviceValidationServiceClient devicesManagedDeviceValidationService,
        ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceClient documentsDocumentTemplateWorkflowService,
        ApiDocuments.SignatureExportService.SignatureExportServiceClient documentsSignatureExportService,
        ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient documentsSignatureHistoryService,
        ApiFeaturestore.FeatureViewQueryService.FeatureViewQueryServiceClient featurestoreFeatureViewQueryService,
        CallInvoker invoker,
        ApiItsm.IncidentCalculationService.IncidentCalculationServiceClient itsmIncidentCalculationService,
        ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient licensingEntitlementValidationService,
        ApiLicensing.SoftwareLicenseService.SoftwareLicenseServiceClient licensingSoftwareLicenseService,
        ILogger<ChangeRequestCalculationEndpoint> logger,
        IRecordRepository repository)
    {
        _auditAuditEventQueryService = auditAuditEventQueryService;
        _authorizationAccessPolicyService = authorizationAccessPolicyService;
        _devicesComplianceScanCalculationService = devicesComplianceScanCalculationService;
        _devicesComplianceScanValidationService = devicesComplianceScanValidationService;
        _devicesManagedDeviceValidationService = devicesManagedDeviceValidationService;
        _documentsDocumentTemplateWorkflowService = documentsDocumentTemplateWorkflowService;
        _documentsSignatureExportService = documentsSignatureExportService;
        _documentsSignatureHistoryService = documentsSignatureHistoryService;
        _featurestoreFeatureViewQueryService = featurestoreFeatureViewQueryService;
        _invoker = invoker;
        _itsmIncidentCalculationService = itsmIncidentCalculationService;
        _licensingEntitlementValidationService = licensingEntitlementValidationService;
        _licensingSoftwareLicenseService = licensingSoftwareLicenseService;
        _logger = logger;
        _repository = repository;
        _sendRecalculateChangeRequests2 = _auditAuditEventQueryService.ResolveAuditEventsAsync;
    }
}
