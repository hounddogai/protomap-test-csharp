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

namespace Acme.Workforce.Documents;

/// <summary>
/// Tells the people who follow a document what changed.
/// </summary>
public sealed partial class DocumentNotificationEndpoint : ApiDocuments.DocumentNotificationService.DocumentNotificationServiceBase
{
    private readonly ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient _authorizationAccessPolicyValidationService;
    private readonly NotificationBus _bus;
    private readonly ApiDocuments.DocumentExportService.DocumentExportServiceClient _documentsDocumentExportService;
    private readonly ApiDocuments.DocumentTemplateCalculationService.DocumentTemplateCalculationServiceClient _documentsDocumentTemplateCalculationService;
    private readonly ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceClient _documentsDocumentTemplateWorkflowService;
    private readonly ILogger<DocumentNotificationEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;

    public DocumentNotificationEndpoint(
        ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient authorizationAccessPolicyValidationService,
        NotificationBus bus,
        ApiDocuments.DocumentExportService.DocumentExportServiceClient documentsDocumentExportService,
        ApiDocuments.DocumentTemplateCalculationService.DocumentTemplateCalculationServiceClient documentsDocumentTemplateCalculationService,
        ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceClient documentsDocumentTemplateWorkflowService,
        ILogger<DocumentNotificationEndpoint> logger,
        RetryPolicy policy,
        IRecordRepository repository)
    {
        _authorizationAccessPolicyValidationService = authorizationAccessPolicyValidationService;
        _bus = bus;
        _documentsDocumentExportService = documentsDocumentExportService;
        _documentsDocumentTemplateCalculationService = documentsDocumentTemplateCalculationService;
        _documentsDocumentTemplateWorkflowService = documentsDocumentTemplateWorkflowService;
        _logger = logger;
        _policy = policy;
        _repository = repository;
    }
}
