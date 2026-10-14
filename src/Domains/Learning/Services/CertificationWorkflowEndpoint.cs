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
using ApiInvoicing = Acme.Invoicing.V1;
using ApiLearning = Acme.Learning.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Learning;

/// <summary>
/// Runs the long-lived workflow that takes a certification from start to finish.
/// </summary>
public sealed partial class CertificationWorkflowEndpoint : ApiLearning.CertificationWorkflowService.CertificationWorkflowServiceBase
{
    private readonly NotificationBus _bus;
    private readonly ICertificationWorkflowEndpointSendSignalCertificationWorkflow0Port _certificationWorkflowEndpointSendSignalCertificationWorkflow0Port;
    private readonly ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient _documentsDocumentFolderQueryService;
    private readonly ApiDocuments.DocumentService.DocumentServiceClient _documentsDocumentService;
    private readonly ApiInvoicing.InvoiceBatchService.InvoiceBatchServiceClient _invoicingInvoiceBatchService;
    private readonly ApiLearning.CertificationApprovalService.CertificationApprovalServiceClient _learningCertificationApprovalService;
    private readonly ApiLearning.CertificationCalculationService.CertificationCalculationServiceClient _learningCertificationCalculationService;
    private readonly ApiLearning.CourseApprovalService.CourseApprovalServiceClient _learningCourseApprovalService;
    private readonly ApiLearning.CourseEnrollmentValidationService.CourseEnrollmentValidationServiceClient _learningCourseEnrollmentValidationService;
    private readonly ApiLearning.CourseService.CourseServiceClient _learningCourseService;
    private readonly ApiLearning.CourseWorkflowService.CourseWorkflowServiceClient _learningCourseWorkflowService;
    private readonly ILogger<CertificationWorkflowEndpoint> _logger;
    private readonly ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient _recruitingCandidateValidationService;
    private readonly ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient _recruitingJobOpeningCalculationService;
    private readonly ApiRecruiting.OfferCalculationService.OfferCalculationServiceClient _recruitingOfferCalculationService;
    private readonly ApiRecruiting.OfferValidationService.OfferValidationServiceClient _recruitingOfferValidationService;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiLearning.DelegateCertificationApprovalRequest, CallOptions, AsyncUnaryCall<ApiLearning.DelegateCertificationApprovalResponse>> _sendListCertificationWorkflows0;
    private readonly ApiTravel.TripApprovalService.TripApprovalServiceClient _travelTripApprovalService;

    public CertificationWorkflowEndpoint(
        NotificationBus bus,
        ICertificationWorkflowEndpointSendSignalCertificationWorkflow0Port certificationWorkflowEndpointSendSignalCertificationWorkflow0PortAdapter,
        ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient documentsDocumentFolderQueryService,
        ApiDocuments.DocumentService.DocumentServiceClient documentsDocumentService,
        ApiInvoicing.InvoiceBatchService.InvoiceBatchServiceClient invoicingInvoiceBatchService,
        ApiLearning.CertificationApprovalService.CertificationApprovalServiceClient learningCertificationApprovalService,
        ApiLearning.CertificationCalculationService.CertificationCalculationServiceClient learningCertificationCalculationService,
        ApiLearning.CourseApprovalService.CourseApprovalServiceClient learningCourseApprovalService,
        ApiLearning.CourseEnrollmentValidationService.CourseEnrollmentValidationServiceClient learningCourseEnrollmentValidationService,
        ApiLearning.CourseService.CourseServiceClient learningCourseService,
        ApiLearning.CourseWorkflowService.CourseWorkflowServiceClient learningCourseWorkflowService,
        ILogger<CertificationWorkflowEndpoint> logger,
        ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient recruitingCandidateValidationService,
        ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient recruitingJobOpeningCalculationService,
        ApiRecruiting.OfferCalculationService.OfferCalculationServiceClient recruitingOfferCalculationService,
        ApiRecruiting.OfferValidationService.OfferValidationServiceClient recruitingOfferValidationService,
        IRecordRepository repository,
        ApiTravel.TripApprovalService.TripApprovalServiceClient travelTripApprovalService)
    {
        _bus = bus;
        _certificationWorkflowEndpointSendSignalCertificationWorkflow0Port = certificationWorkflowEndpointSendSignalCertificationWorkflow0PortAdapter;
        _documentsDocumentFolderQueryService = documentsDocumentFolderQueryService;
        _documentsDocumentService = documentsDocumentService;
        _invoicingInvoiceBatchService = invoicingInvoiceBatchService;
        _learningCertificationApprovalService = learningCertificationApprovalService;
        _learningCertificationCalculationService = learningCertificationCalculationService;
        _learningCourseApprovalService = learningCourseApprovalService;
        _learningCourseEnrollmentValidationService = learningCourseEnrollmentValidationService;
        _learningCourseService = learningCourseService;
        _learningCourseWorkflowService = learningCourseWorkflowService;
        _logger = logger;
        _recruitingCandidateValidationService = recruitingCandidateValidationService;
        _recruitingJobOpeningCalculationService = recruitingJobOpeningCalculationService;
        _recruitingOfferCalculationService = recruitingOfferCalculationService;
        _recruitingOfferValidationService = recruitingOfferValidationService;
        _repository = repository;
        _travelTripApprovalService = travelTripApprovalService;
        _sendListCertificationWorkflows0 = _learningCertificationApprovalService.DelegateCertificationApprovalAsync;
    }
}
