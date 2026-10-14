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
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Scheduling;

/// <summary>
/// Manages shifts: creates, reads, changes, and lists them.
/// </summary>
public sealed partial class ShiftEndpoint : ApiScheduling.ShiftService.ShiftServiceBase
{
    private readonly ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient _documentsDocumentFolderNotificationService;
    private readonly ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient _documentsDocumentTemplateNotificationService;
    private readonly ILogger<ShiftEndpoint> _logger;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiScheduling.AvailabilityWindowValidationService.AvailabilityWindowValidationServiceClient _schedulingAvailabilityWindowValidationService;
    private readonly ApiScheduling.RosterQueryService.RosterQueryServiceClient _schedulingRosterQueryService;
    private readonly ApiScheduling.ShiftValidationService.ShiftValidationServiceClient _schedulingShiftValidationService;
    private readonly ApiScheduling.SwapRequestValidationService.SwapRequestValidationServiceClient _schedulingSwapRequestValidationService;
    private readonly IOptions<MeshOptions> _settings;

    public ShiftEndpoint(
        ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient documentsDocumentFolderNotificationService,
        ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient documentsDocumentTemplateNotificationService,
        ILogger<ShiftEndpoint> logger,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiScheduling.AvailabilityWindowValidationService.AvailabilityWindowValidationServiceClient schedulingAvailabilityWindowValidationService,
        ApiScheduling.RosterQueryService.RosterQueryServiceClient schedulingRosterQueryService,
        ApiScheduling.ShiftValidationService.ShiftValidationServiceClient schedulingShiftValidationService,
        ApiScheduling.SwapRequestValidationService.SwapRequestValidationServiceClient schedulingSwapRequestValidationService,
        IOptions<MeshOptions> settings)
    {
        _documentsDocumentFolderNotificationService = documentsDocumentFolderNotificationService;
        _documentsDocumentTemplateNotificationService = documentsDocumentTemplateNotificationService;
        _logger = logger;
        _registry = registry;
        _repository = repository;
        _schedulingAvailabilityWindowValidationService = schedulingAvailabilityWindowValidationService;
        _schedulingRosterQueryService = schedulingRosterQueryService;
        _schedulingShiftValidationService = schedulingShiftValidationService;
        _schedulingSwapRequestValidationService = schedulingSwapRequestValidationService;
        _settings = settings;
    }
}
