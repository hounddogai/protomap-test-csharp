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
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiForecasting = Acme.Forecasting.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;

namespace Acme.Workforce.Benefits;

/// <summary>
/// Manages benefit plans: creates, reads, changes, and lists them.
/// </summary>
public sealed partial class BenefitPlanEndpoint : ApiBenefits.BenefitPlanService.BenefitPlanServiceBase
{
    private readonly ApiBenefits.DependentValidationService.DependentValidationServiceClient _benefitsDependentValidationService;
    private readonly ApiBenefits.EnrollmentQueryService.EnrollmentQueryServiceClient _benefitsEnrollmentQueryService;
    private readonly ApiForecasting.DemandForecastQueryService.DemandForecastQueryServiceClient _forecastingDemandForecastQueryService;
    private readonly ApiHelpdesk.EscalationCalculationService.EscalationCalculationServiceClient _helpdeskEscalationCalculationService;
    private readonly CallInvoker _invoker;
    private readonly ILogger<BenefitPlanEndpoint> _logger;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;

    public BenefitPlanEndpoint(
        ApiBenefits.DependentValidationService.DependentValidationServiceClient benefitsDependentValidationService,
        ApiBenefits.EnrollmentQueryService.EnrollmentQueryServiceClient benefitsEnrollmentQueryService,
        ApiForecasting.DemandForecastQueryService.DemandForecastQueryServiceClient forecastingDemandForecastQueryService,
        ApiHelpdesk.EscalationCalculationService.EscalationCalculationServiceClient helpdeskEscalationCalculationService,
        CallInvoker invoker,
        ILogger<BenefitPlanEndpoint> logger,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings)
    {
        _benefitsDependentValidationService = benefitsDependentValidationService;
        _benefitsEnrollmentQueryService = benefitsEnrollmentQueryService;
        _forecastingDemandForecastQueryService = forecastingDemandForecastQueryService;
        _helpdeskEscalationCalculationService = helpdeskEscalationCalculationService;
        _invoker = invoker;
        _logger = logger;
        _registry = registry;
        _repository = repository;
        _settings = settings;
    }
}
