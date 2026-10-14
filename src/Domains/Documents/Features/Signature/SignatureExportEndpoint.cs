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
using ApiDocuments = Acme.Documents.V1;
using ApiNotifications = Acme.Notifications.V1;

namespace Acme.Workforce.Documents;

/// <summary>
/// Exports signatures to files and other systems.
/// </summary>
public sealed partial class SignatureExportEndpoint : ApiDocuments.SignatureExportService.SignatureExportServiceBase
{
    private readonly ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient _documentsSignatureCalculationService;
    private readonly ILogger<SignatureExportEndpoint> _logger;
    private readonly ApiNotifications.NotificationValidationService.NotificationValidationServiceClient _notificationsNotificationValidationService;
    private readonly IRecordRepository _repository;

    public SignatureExportEndpoint(
        ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient documentsSignatureCalculationService,
        ILogger<SignatureExportEndpoint> logger,
        ApiNotifications.NotificationValidationService.NotificationValidationServiceClient notificationsNotificationValidationService,
        IRecordRepository repository)
    {
        _documentsSignatureCalculationService = documentsSignatureCalculationService;
        _logger = logger;
        _notificationsNotificationValidationService = notificationsNotificationValidationService;
        _repository = repository;
    }
}
