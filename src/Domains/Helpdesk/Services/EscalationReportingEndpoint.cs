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
using ApiEmployees = Acme.Employees.V1;
using ApiFuel = Acme.Fuel.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiSurveys = Acme.Surveys.V1;

namespace Acme.Workforce.Helpdesk;

/// <summary>
/// Builds and schedules reports about escalations.
/// </summary>
public sealed partial class EscalationReportingEndpoint : ApiHelpdesk.EscalationReportingService.EscalationReportingServiceBase
{
    private readonly ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient _documentsDocumentTemplateNotificationService;
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _employeesEmployeeService;
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient _employeesEmploymentContractValidationService;
    private readonly IEscalationReportingEndpointSendShareEscalationReport2Port _escalationReportingEndpointSendShareEscalationReport2Port;
    private readonly ApiFuel.FuelCardService.FuelCardServiceClient _fuelFuelCardService;
    private readonly ApiHelpdesk.EscalationValidationService.EscalationValidationServiceClient _helpdeskEscalationValidationService;
    private readonly ApiHelpdesk.KnowledgeArticleQueryService.KnowledgeArticleQueryServiceClient _helpdeskKnowledgeArticleQueryService;
    private readonly ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient _helpdeskKnowledgeArticleValidationService;
    private readonly ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient _helpdeskSlaPolicyCalculationService;
    private readonly ApiHelpdesk.TicketService.TicketServiceClient _helpdeskTicketService;
    private readonly ApiHelpdesk.TicketValidationService.TicketValidationServiceClient _helpdeskTicketValidationService;
    private readonly CallInvoker _invoker;
    private readonly ILogger<EscalationReportingEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient _metricsMetricSeriesQueryService;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiSurveys.SurveyService.SurveyServiceClient _surveysSurveyService;

    public EscalationReportingEndpoint(
        ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient documentsDocumentTemplateNotificationService,
        ApiEmployees.EmployeeService.EmployeeServiceClient employeesEmployeeService,
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient employeesEmploymentContractValidationService,
        IEscalationReportingEndpointSendShareEscalationReport2Port escalationReportingEndpointSendShareEscalationReport2PortAdapter,
        ApiFuel.FuelCardService.FuelCardServiceClient fuelFuelCardService,
        ApiHelpdesk.EscalationValidationService.EscalationValidationServiceClient helpdeskEscalationValidationService,
        ApiHelpdesk.KnowledgeArticleQueryService.KnowledgeArticleQueryServiceClient helpdeskKnowledgeArticleQueryService,
        ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient helpdeskKnowledgeArticleValidationService,
        ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient helpdeskSlaPolicyCalculationService,
        ApiHelpdesk.TicketService.TicketServiceClient helpdeskTicketService,
        ApiHelpdesk.TicketValidationService.TicketValidationServiceClient helpdeskTicketValidationService,
        CallInvoker invoker,
        ILogger<EscalationReportingEndpoint> logger,
        ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient metricsMetricSeriesQueryService,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiSurveys.SurveyService.SurveyServiceClient surveysSurveyService)
    {
        _documentsDocumentTemplateNotificationService = documentsDocumentTemplateNotificationService;
        _employeesEmployeeService = employeesEmployeeService;
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _employeesEmploymentContractValidationService = employeesEmploymentContractValidationService;
        _escalationReportingEndpointSendShareEscalationReport2Port = escalationReportingEndpointSendShareEscalationReport2PortAdapter;
        _fuelFuelCardService = fuelFuelCardService;
        _helpdeskEscalationValidationService = helpdeskEscalationValidationService;
        _helpdeskKnowledgeArticleQueryService = helpdeskKnowledgeArticleQueryService;
        _helpdeskKnowledgeArticleValidationService = helpdeskKnowledgeArticleValidationService;
        _helpdeskSlaPolicyCalculationService = helpdeskSlaPolicyCalculationService;
        _helpdeskTicketService = helpdeskTicketService;
        _helpdeskTicketValidationService = helpdeskTicketValidationService;
        _invoker = invoker;
        _logger = logger;
        _metricsMetricSeriesQueryService = metricsMetricSeriesQueryService;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _surveysSurveyService = surveysSurveyService;
    }
}
