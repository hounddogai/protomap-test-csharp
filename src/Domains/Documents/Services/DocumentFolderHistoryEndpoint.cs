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
using ApiIdentity = Acme.Identity.V1;
using ApiNotifications = Acme.Notifications.V1;

namespace Acme.Workforce.Documents;

/// <summary>
/// Keeps every version of a document folder and restores earlier ones.
/// </summary>
public sealed partial class DocumentFolderHistoryEndpoint : ApiDocuments.DocumentFolderHistoryService.DocumentFolderHistoryServiceBase
{
    private readonly ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient _documentsDocumentFolderNotificationService;
    private readonly ILogger<DocumentFolderHistoryEndpoint> _logger;
    private readonly ApiNotifications.NotificationQueryService.NotificationQueryServiceClient _notificationsNotificationQueryService;
    private readonly IRecordRepository _repository;

    public DocumentFolderHistoryEndpoint(
        ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient documentsDocumentFolderNotificationService,
        ILogger<DocumentFolderHistoryEndpoint> logger,
        ApiNotifications.NotificationQueryService.NotificationQueryServiceClient notificationsNotificationQueryService,
        IRecordRepository repository)
    {
        _documentsDocumentFolderNotificationService = documentsDocumentFolderNotificationService;
        _logger = logger;
        _notificationsNotificationQueryService = notificationsNotificationQueryService;
        _repository = repository;
    }
}
