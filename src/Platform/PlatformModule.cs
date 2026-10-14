using Acme.Workforce.Platform;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiAdmissions = Acme.Admissions.V1;
using ApiAnalytics = Acme.Analytics.V1;
using ApiAnomalies = Acme.Anomalies.V1;
using ApiAppointments = Acme.Appointments.V1;
using ApiAssets = Acme.Assets.V1;
using ApiAudiences = Acme.Audiences.V1;
using ApiAudit = Acme.Audit.V1;
using ApiAuthorization = Acme.Authorization.V1;
using ApiBanking = Acme.Banking.V1;
using ApiBenefits = Acme.Benefits.V1;
using ApiBoxoffice = Acme.Boxoffice.V1;
using ApiBroadcasts = Acme.Broadcasts.V1;
using ApiBudgeting = Acme.Budgeting.V1;
using ApiCampaigns = Acme.Campaigns.V1;
using ApiCareplans = Acme.Careplans.V1;
using ApiCart = Acme.Cart.V1;
using ApiCatalog = Acme.Catalog.V1;
using ApiCertificates = Acme.Certificates.V1;
using ApiChat = Acme.Chat.V1;
using ApiCheckout = Acme.Checkout.V1;
using ApiCommunity = Acme.Community.V1;
using ApiCompliance = Acme.Compliance.V1;
using ApiConcessions = Acme.Concessions.V1;
using ApiConfiguration = Acme.Configuration.V1;
using ApiContent = Acme.Content.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiCustomers = Acme.Customers.V1;
using ApiDatahub = Acme.Datahub.V1;
using ApiDatasets = Acme.Datasets.V1;
using ApiDeliveries = Acme.Deliveries.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDispatch = Acme.Dispatch.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiDriverapp = Acme.Driverapp.V1;
using ApiDrivers = Acme.Drivers.V1;
using ApiEdgegateway = Acme.Edgegateway.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiExperiments = Acme.Experiments.V1;
using ApiFanportal = Acme.Fanportal.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiFeaturestore = Acme.Featurestore.V1;
using ApiFleet = Acme.Fleet.V1;
using ApiForecasting = Acme.Forecasting.V1;
using ApiForms = Acme.Forms.V1;
using ApiFraud = Acme.Fraud.V1;
using ApiFuel = Acme.Fuel.V1;
using ApiGiftcards = Acme.Giftcards.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiInsurance = Acme.Insurance.V1;
using ApiInvoicing = Acme.Invoicing.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiKeymanagement = Acme.Keymanagement.V1;
using ApiLabeling = Acme.Labeling.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLending = Acme.Lending.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiLineage = Acme.Lineage.V1;
using ApiLocalization = Acme.Localization.V1;
using ApiLoyalty = Acme.Loyalty.V1;
using ApiMaintenance = Acme.Maintenance.V1;
using ApiMedia = Acme.Media.V1;
using ApiMerchandising = Acme.Merchandising.V1;
using ApiMessaging = Acme.Messaging.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiModelregistry = Acme.Modelregistry.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiOrders = Acme.Orders.V1;
using ApiPatientapp = Acme.Patientapp.V1;
using ApiPatients = Acme.Patients.V1;
using ApiPayments = Acme.Payments.V1;
using ApiPayroll = Acme.Payroll.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiPerformers = Acme.Performers.V1;
using ApiPersonalization = Acme.Personalization.V1;
using ApiPipelines = Acme.Pipelines.V1;
using ApiPrescriptions = Acme.Prescriptions.V1;
using ApiPricing = Acme.Pricing.V1;
using ApiProcurement = Acme.Procurement.V1;
using ApiPromotions = Acme.Promotions.V1;
using ApiRatelimiting = Acme.Ratelimiting.V1;
using ApiRecommendations = Acme.Recommendations.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiReturns = Acme.Returns.V1;
using ApiReviews = Acme.Reviews.V1;
using ApiRisk = Acme.Risk.V1;
using ApiRouting = Acme.Routing.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiSearch = Acme.Search.V1;
using ApiSeating = Acme.Seating.V1;
using ApiSecrets = Acme.Secrets.V1;
using ApiSellers = Acme.Sellers.V1;
using ApiShipping = Acme.Shipping.V1;
using ApiShows = Acme.Shows.V1;
using ApiSponsorships = Acme.Sponsorships.V1;
using ApiSubscriptions = Acme.Subscriptions.V1;
using ApiSurveys = Acme.Surveys.V1;
using ApiTelehealth = Acme.Telehealth.V1;
using ApiTelematics = Acme.Telematics.V1;
using ApiTicketing = Acme.Ticketing.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiTours = Acme.Tours.V1;
using ApiTravel = Acme.Travel.V1;
using ApiTreasury = Acme.Treasury.V1;
using ApiVenues = Acme.Venues.V1;
using ApiWarehousing = Acme.Warehousing.V1;
using ApiWebbff = Acme.Webbff.V1;
using ApiWebhooks = Acme.Webhooks.V1;
using ApiWorkplaces = Acme.Workplaces.V1;
using ApiYards = Acme.Yards.V1;

namespace Acme.Workforce.Platform;

public static class PlatformModule
{
    public static IServiceCollection AddWorkforcePlatform(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MeshOptions>(configuration.GetSection("Mesh"));
        services.AddSingleton<WorkloadIdentityInterceptor>();
        services.AddSingleton<WorkforceDatabase>();
        services.AddSingleton<IRecordRepository, DapperRecordRepository>();
        services.AddSingleton<RetryPolicy>();
        services.AddSingleton<NotificationBus>();
        services.AddHostedService<NotificationWorker>();
        services.AddSingleton<CallInvoker>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<MeshOptions>>().Value;
            return GrpcChannel.ForAddress(options.Address).CreateCallInvoker()
                .Intercept(provider.GetRequiredService<WorkloadIdentityInterceptor>());
        });
        services.AddGrpcClient<ApiAdmissions.AdmissionService.AdmissionServiceClient>("acme.admissions.v1.AdmissionService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiAnalytics.AnalyticsEventService.AnalyticsEventServiceClient>("acme.analytics.v1.AnalyticsEventService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiAnomalies.AnomalyQueryService.AnomalyQueryServiceClient>("acme.anomalies.v1.AnomalyQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiAnomalies.AnomalyService.AnomalyServiceClient>("acme.anomalies.v1.AnomalyService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiAppointments.AppointmentQueryService.AppointmentQueryServiceClient>("acme.appointments.v1.AppointmentQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiAppointments.AppointmentService.AppointmentServiceClient>("acme.appointments.v1.AppointmentService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiAssets.FixedAssetCalculationService.FixedAssetCalculationServiceClient>("acme.assets.v1.FixedAssetCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiAudiences.AudienceQueryService.AudienceQueryServiceClient>("acme.audiences.v1.AudienceQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiAudiences.AudienceService.AudienceServiceClient>("acme.audiences.v1.AudienceService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiAudit.AuditEventQueryService.AuditEventQueryServiceClient>("acme.audit.v1.AuditEventQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiAudit.AuditEventService.AuditEventServiceClient>("acme.audit.v1.AuditEventService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient>("acme.audit.v1.AuditEventValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient>("acme.authorization.v1.AccessPolicyQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient>("acme.authorization.v1.AccessPolicyService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient>("acme.authorization.v1.AccessPolicyValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiBanking.BankAccountQueryService.BankAccountQueryServiceClient>("acme.banking.v1.BankAccountQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiBanking.BankAccountService.BankAccountServiceClient>("acme.banking.v1.BankAccountService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiBenefits.BenefitPlanApprovalService.BenefitPlanApprovalServiceClient>("acme.benefits.v1.BenefitPlanApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiBenefits.BenefitPlanQueryService.BenefitPlanQueryServiceClient>("acme.benefits.v1.BenefitPlanQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiBenefits.BenefitPlanService.BenefitPlanServiceClient>("acme.benefits.v1.BenefitPlanService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiBenefits.BenefitPlanWorkflowService.BenefitPlanWorkflowServiceClient>("acme.benefits.v1.BenefitPlanWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiBenefits.DependentApprovalService.DependentApprovalServiceClient>("acme.benefits.v1.DependentApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiBenefits.DependentValidationService.DependentValidationServiceClient>("acme.benefits.v1.DependentValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiBenefits.DependentWorkflowService.DependentWorkflowServiceClient>("acme.benefits.v1.DependentWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiBenefits.EnrollmentApprovalService.EnrollmentApprovalServiceClient>("acme.benefits.v1.EnrollmentApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiBenefits.EnrollmentQueryService.EnrollmentQueryServiceClient>("acme.benefits.v1.EnrollmentQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiBenefits.EnrollmentWorkflowService.EnrollmentWorkflowServiceClient>("acme.benefits.v1.EnrollmentWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiBenefits.LifeEventCalculationService.LifeEventCalculationServiceClient>("acme.benefits.v1.LifeEventCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiBenefits.LifeEventValidationService.LifeEventValidationServiceClient>("acme.benefits.v1.LifeEventValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiBoxoffice.BoxOfficeSaleQueryService.BoxOfficeSaleQueryServiceClient>("acme.boxoffice.v1.BoxOfficeSaleQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiBoxoffice.BoxOfficeSaleService.BoxOfficeSaleServiceClient>("acme.boxoffice.v1.BoxOfficeSaleService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiBroadcasts.BroadcastService.BroadcastServiceClient>("acme.broadcasts.v1.BroadcastService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiBudgeting.BudgetCalculationService.BudgetCalculationServiceClient>("acme.budgeting.v1.BudgetCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiBudgeting.BudgetQueryService.BudgetQueryServiceClient>("acme.budgeting.v1.BudgetQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiBudgeting.BudgetService.BudgetServiceClient>("acme.budgeting.v1.BudgetService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiCampaigns.MarketingCampaignService.MarketingCampaignServiceClient>("acme.campaigns.v1.MarketingCampaignService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiCareplans.CarePlanCalculationService.CarePlanCalculationServiceClient>("acme.careplans.v1.CarePlanCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiCareplans.CarePlanQueryService.CarePlanQueryServiceClient>("acme.careplans.v1.CarePlanQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiCareplans.CarePlanService.CarePlanServiceClient>("acme.careplans.v1.CarePlanService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiCart.CartQueryService.CartQueryServiceClient>("acme.cart.v1.CartQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiCart.CartService.CartServiceClient>("acme.cart.v1.CartService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiCatalog.ProductQueryService.ProductQueryServiceClient>("acme.catalog.v1.ProductQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiCertificates.CertificateCalculationService.CertificateCalculationServiceClient>("acme.certificates.v1.CertificateCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiCertificates.CertificateQueryService.CertificateQueryServiceClient>("acme.certificates.v1.CertificateQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiCertificates.CertificateService.CertificateServiceClient>("acme.certificates.v1.CertificateService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiChat.ChatRoomCalculationService.ChatRoomCalculationServiceClient>("acme.chat.v1.ChatRoomCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiChat.ChatRoomQueryService.ChatRoomQueryServiceClient>("acme.chat.v1.ChatRoomQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiChat.ChatRoomService.ChatRoomServiceClient>("acme.chat.v1.ChatRoomService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiCheckout.CheckoutSessionQueryService.CheckoutSessionQueryServiceClient>("acme.checkout.v1.CheckoutSessionQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiCheckout.CheckoutSessionService.CheckoutSessionServiceClient>("acme.checkout.v1.CheckoutSessionService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiCommunity.ForumService.ForumServiceClient>("acme.community.v1.ForumService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiCompliance.ComplianceCaseQueryService.ComplianceCaseQueryServiceClient>("acme.compliance.v1.ComplianceCaseQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiCompliance.ComplianceCaseService.ComplianceCaseServiceClient>("acme.compliance.v1.ComplianceCaseService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiConcessions.ConcessionStandCalculationService.ConcessionStandCalculationServiceClient>("acme.concessions.v1.ConcessionStandCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiConcessions.ConcessionStandQueryService.ConcessionStandQueryServiceClient>("acme.concessions.v1.ConcessionStandQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiConcessions.ConcessionStandService.ConcessionStandServiceClient>("acme.concessions.v1.ConcessionStandService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiConfiguration.ConfigSetCalculationService.ConfigSetCalculationServiceClient>("acme.configuration.v1.ConfigSetCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiConfiguration.ConfigSetQueryService.ConfigSetQueryServiceClient>("acme.configuration.v1.ConfigSetQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiConfiguration.ConfigSetService.ConfigSetServiceClient>("acme.configuration.v1.ConfigSetService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiContent.ArticleQueryService.ArticleQueryServiceClient>("acme.content.v1.ArticleQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiContent.ArticleService.ArticleServiceClient>("acme.content.v1.ArticleService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiCorporateevents.CorporateEventApprovalService.CorporateEventApprovalServiceClient>("acme.corporateevents.v1.CorporateEventApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient>("acme.corporateevents.v1.CorporateEventCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiCorporateevents.CorporateEventLifecycleService.CorporateEventLifecycleServiceClient>("acme.corporateevents.v1.CorporateEventLifecycleService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiCorporateevents.CorporateEventQueryService.CorporateEventQueryServiceClient>("acme.corporateevents.v1.CorporateEventQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiCorporateevents.CorporateEventReportingService.CorporateEventReportingServiceClient>("acme.corporateevents.v1.CorporateEventReportingService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiCorporateevents.CorporateEventService.CorporateEventServiceClient>("acme.corporateevents.v1.CorporateEventService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiCorporateevents.CorporateEventValidationService.CorporateEventValidationServiceClient>("acme.corporateevents.v1.CorporateEventValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiCorporateevents.CorporateEventWorkflowService.CorporateEventWorkflowServiceClient>("acme.corporateevents.v1.CorporateEventWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiCorporateevents.RegistrationApprovalService.RegistrationApprovalServiceClient>("acme.corporateevents.v1.RegistrationApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiCorporateevents.RegistrationCalculationService.RegistrationCalculationServiceClient>("acme.corporateevents.v1.RegistrationCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiCorporateevents.RegistrationLifecycleService.RegistrationLifecycleServiceClient>("acme.corporateevents.v1.RegistrationLifecycleService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiCorporateevents.RegistrationQueryService.RegistrationQueryServiceClient>("acme.corporateevents.v1.RegistrationQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiCorporateevents.RegistrationReportingService.RegistrationReportingServiceClient>("acme.corporateevents.v1.RegistrationReportingService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiCorporateevents.RegistrationValidationService.RegistrationValidationServiceClient>("acme.corporateevents.v1.RegistrationValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiCorporateevents.RegistrationWorkflowService.RegistrationWorkflowServiceClient>("acme.corporateevents.v1.RegistrationWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiCorporateevents.VenueApprovalService.VenueApprovalServiceClient>("acme.corporateevents.v1.VenueApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiCorporateevents.VenueCalculationService.VenueCalculationServiceClient>("acme.corporateevents.v1.VenueCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiCorporateevents.VenueLifecycleService.VenueLifecycleServiceClient>("acme.corporateevents.v1.VenueLifecycleService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiCorporateevents.VenueReportingService.VenueReportingServiceClient>("acme.corporateevents.v1.VenueReportingService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiCorporateevents.VenueSyncService.VenueSyncServiceClient>("acme.corporateevents.v1.VenueSyncService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiCorporateevents.VenueValidationService.VenueValidationServiceClient>("acme.corporateevents.v1.VenueValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiCorporateevents.VenueWorkflowService.VenueWorkflowServiceClient>("acme.corporateevents.v1.VenueWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiCustomers.CustomerCalculationService.CustomerCalculationServiceClient>("acme.customers.v1.CustomerCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiCustomers.CustomerService.CustomerServiceClient>("acme.customers.v1.CustomerService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDatahub.WorkspaceQueryService.WorkspaceQueryServiceClient>("acme.datahub.v1.WorkspaceQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDatasets.DatasetQueryService.DatasetQueryServiceClient>("acme.datasets.v1.DatasetQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDatasets.DatasetService.DatasetServiceClient>("acme.datasets.v1.DatasetService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDeliveries.DeliveryQueryService.DeliveryQueryServiceClient>("acme.deliveries.v1.DeliveryQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDeliveries.DeliveryService.DeliveryServiceClient>("acme.deliveries.v1.DeliveryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDevices.ComplianceScanApprovalService.ComplianceScanApprovalServiceClient>("acme.devices.v1.ComplianceScanApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient>("acme.devices.v1.ComplianceScanCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDevices.ComplianceScanLifecycleService.ComplianceScanLifecycleServiceClient>("acme.devices.v1.ComplianceScanLifecycleService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDevices.ComplianceScanValidationService.ComplianceScanValidationServiceClient>("acme.devices.v1.ComplianceScanValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDevices.ComplianceScanWorkflowService.ComplianceScanWorkflowServiceClient>("acme.devices.v1.ComplianceScanWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDevices.DevicePolicyApprovalService.DevicePolicyApprovalServiceClient>("acme.devices.v1.DevicePolicyApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDevices.DevicePolicyCalculationService.DevicePolicyCalculationServiceClient>("acme.devices.v1.DevicePolicyCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDevices.DevicePolicyLifecycleService.DevicePolicyLifecycleServiceClient>("acme.devices.v1.DevicePolicyLifecycleService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDevices.DevicePolicyQueryService.DevicePolicyQueryServiceClient>("acme.devices.v1.DevicePolicyQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDevices.DevicePolicyValidationService.DevicePolicyValidationServiceClient>("acme.devices.v1.DevicePolicyValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceClient>("acme.devices.v1.DevicePolicyWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDevices.ManagedDeviceApprovalService.ManagedDeviceApprovalServiceClient>("acme.devices.v1.ManagedDeviceApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDevices.ManagedDeviceCalculationService.ManagedDeviceCalculationServiceClient>("acme.devices.v1.ManagedDeviceCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDevices.ManagedDeviceQueryService.ManagedDeviceQueryServiceClient>("acme.devices.v1.ManagedDeviceQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDevices.ManagedDeviceService.ManagedDeviceServiceClient>("acme.devices.v1.ManagedDeviceService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDevices.ManagedDeviceValidationService.ManagedDeviceValidationServiceClient>("acme.devices.v1.ManagedDeviceValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDevices.ManagedDeviceWorkflowService.ManagedDeviceWorkflowServiceClient>("acme.devices.v1.ManagedDeviceWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDispatch.DispatchOrderQueryService.DispatchOrderQueryServiceClient>("acme.dispatch.v1.DispatchOrderQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDispatch.DispatchOrderService.DispatchOrderServiceClient>("acme.dispatch.v1.DispatchOrderService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDocuments.DocumentAdminService.DocumentAdminServiceClient>("acme.documents.v1.DocumentAdminService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDocuments.DocumentExportService.DocumentExportServiceClient>("acme.documents.v1.DocumentExportService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDocuments.DocumentFolderExportService.DocumentFolderExportServiceClient>("acme.documents.v1.DocumentFolderExportService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDocuments.DocumentFolderHistoryService.DocumentFolderHistoryServiceClient>("acme.documents.v1.DocumentFolderHistoryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient>("acme.documents.v1.DocumentFolderNotificationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient>("acme.documents.v1.DocumentFolderQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient>("acme.documents.v1.DocumentFolderSyncService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient>("acme.documents.v1.DocumentFolderValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDocuments.DocumentHistoryService.DocumentHistoryServiceClient>("acme.documents.v1.DocumentHistoryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient>("acme.documents.v1.DocumentNotificationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDocuments.DocumentQueryService.DocumentQueryServiceClient>("acme.documents.v1.DocumentQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDocuments.DocumentService.DocumentServiceClient>("acme.documents.v1.DocumentService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDocuments.DocumentSyncService.DocumentSyncServiceClient>("acme.documents.v1.DocumentSyncService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDocuments.DocumentTemplateAdminService.DocumentTemplateAdminServiceClient>("acme.documents.v1.DocumentTemplateAdminService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDocuments.DocumentTemplateCalculationService.DocumentTemplateCalculationServiceClient>("acme.documents.v1.DocumentTemplateCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDocuments.DocumentTemplateExportService.DocumentTemplateExportServiceClient>("acme.documents.v1.DocumentTemplateExportService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDocuments.DocumentTemplateHistoryService.DocumentTemplateHistoryServiceClient>("acme.documents.v1.DocumentTemplateHistoryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient>("acme.documents.v1.DocumentTemplateNotificationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient>("acme.documents.v1.DocumentTemplateSyncService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceClient>("acme.documents.v1.DocumentTemplateWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDocuments.DocumentValidationService.DocumentValidationServiceClient>("acme.documents.v1.DocumentValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDocuments.SignatureAdminService.SignatureAdminServiceClient>("acme.documents.v1.SignatureAdminService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient>("acme.documents.v1.SignatureCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDocuments.SignatureExportService.SignatureExportServiceClient>("acme.documents.v1.SignatureExportService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient>("acme.documents.v1.SignatureHistoryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDocuments.SignatureNotificationService.SignatureNotificationServiceClient>("acme.documents.v1.SignatureNotificationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDocuments.SignatureSyncService.SignatureSyncServiceClient>("acme.documents.v1.SignatureSyncService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDocuments.SignatureValidationService.SignatureValidationServiceClient>("acme.documents.v1.SignatureValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDriverapp.DriverHomeService.DriverHomeServiceClient>("acme.driverapp.v1.DriverHomeService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDrivers.DriverCalculationService.DriverCalculationServiceClient>("acme.drivers.v1.DriverCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiDrivers.DriverService.DriverServiceClient>("acme.drivers.v1.DriverService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiEdgegateway.RouteBindingQueryService.RouteBindingQueryServiceClient>("acme.edgegateway.v1.RouteBindingQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiEdgegateway.RouteBindingService.RouteBindingServiceClient>("acme.edgegateway.v1.RouteBindingService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiEmployeeportal.AnnouncementIntegrationService.AnnouncementIntegrationServiceClient>("acme.employeeportal.v1.AnnouncementIntegrationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiEmployeeportal.AnnouncementNotificationService.AnnouncementNotificationServiceClient>("acme.employeeportal.v1.AnnouncementNotificationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiEmployeeportal.AnnouncementWorkflowService.AnnouncementWorkflowServiceClient>("acme.employeeportal.v1.AnnouncementWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiEmployeeportal.PortalHomeIntegrationService.PortalHomeIntegrationServiceClient>("acme.employeeportal.v1.PortalHomeIntegrationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiEmployeeportal.PortalHomeNotificationService.PortalHomeNotificationServiceClient>("acme.employeeportal.v1.PortalHomeNotificationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiEmployeeportal.PortalHomeQueryService.PortalHomeQueryServiceClient>("acme.employeeportal.v1.PortalHomeQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiEmployeeportal.PortalHomeService.PortalHomeServiceClient>("acme.employeeportal.v1.PortalHomeService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiEmployeeportal.SelfServiceRequestIntegrationService.SelfServiceRequestIntegrationServiceClient>("acme.employeeportal.v1.SelfServiceRequestIntegrationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiEmployeeportal.SelfServiceRequestNotificationService.SelfServiceRequestNotificationServiceClient>("acme.employeeportal.v1.SelfServiceRequestNotificationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiEmployeeportal.SelfServiceRequestQueryService.SelfServiceRequestQueryServiceClient>("acme.employeeportal.v1.SelfServiceRequestQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiEmployees.EmployeeApprovalService.EmployeeApprovalServiceClient>("acme.employees.v1.EmployeeApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient>("acme.employees.v1.EmployeeQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiEmployees.EmployeeService.EmployeeServiceClient>("acme.employees.v1.EmployeeService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiEmployees.EmployeeWorkflowService.EmployeeWorkflowServiceClient>("acme.employees.v1.EmployeeWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiEmployees.EmploymentContractApprovalService.EmploymentContractApprovalServiceClient>("acme.employees.v1.EmploymentContractApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient>("acme.employees.v1.EmploymentContractCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient>("acme.employees.v1.EmploymentContractValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiEmployees.OrgUnitApprovalService.OrgUnitApprovalServiceClient>("acme.employees.v1.OrgUnitApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient>("acme.employees.v1.OrgUnitQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiEmployees.OrgUnitWorkflowService.OrgUnitWorkflowServiceClient>("acme.employees.v1.OrgUnitWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiEmployees.PositionApprovalService.PositionApprovalServiceClient>("acme.employees.v1.PositionApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiEmployees.PositionValidationService.PositionValidationServiceClient>("acme.employees.v1.PositionValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiEmployees.PositionWorkflowService.PositionWorkflowServiceClient>("acme.employees.v1.PositionWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiExperiments.ExperimentCalculationService.ExperimentCalculationServiceClient>("acme.experiments.v1.ExperimentCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiExperiments.ExperimentQueryService.ExperimentQueryServiceClient>("acme.experiments.v1.ExperimentQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiExperiments.ExperimentService.ExperimentServiceClient>("acme.experiments.v1.ExperimentService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiFanportal.FanSessionQueryService.FanSessionQueryServiceClient>("acme.fanportal.v1.FanSessionQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiFanportal.FanSessionService.FanSessionServiceClient>("acme.fanportal.v1.FanSessionService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient>("acme.featureflags.v1.FeatureFlagQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiFeatureflags.FeatureFlagService.FeatureFlagServiceClient>("acme.featureflags.v1.FeatureFlagService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient>("acme.featureflags.v1.FeatureFlagValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiFeaturestore.FeatureViewCalculationService.FeatureViewCalculationServiceClient>("acme.featurestore.v1.FeatureViewCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiFeaturestore.FeatureViewQueryService.FeatureViewQueryServiceClient>("acme.featurestore.v1.FeatureViewQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiFleet.VehicleQueryService.VehicleQueryServiceClient>("acme.fleet.v1.VehicleQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiFleet.VehicleService.VehicleServiceClient>("acme.fleet.v1.VehicleService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiForecasting.DemandForecastQueryService.DemandForecastQueryServiceClient>("acme.forecasting.v1.DemandForecastQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiForecasting.DemandForecastService.DemandForecastServiceClient>("acme.forecasting.v1.DemandForecastService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiForms.FormCalculationService.FormCalculationServiceClient>("acme.forms.v1.FormCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiFraud.RiskAssessmentCalculationService.RiskAssessmentCalculationServiceClient>("acme.fraud.v1.RiskAssessmentCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiFraud.RiskAssessmentQueryService.RiskAssessmentQueryServiceClient>("acme.fraud.v1.RiskAssessmentQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiFraud.RiskAssessmentService.RiskAssessmentServiceClient>("acme.fraud.v1.RiskAssessmentService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiFuel.FuelCardCalculationService.FuelCardCalculationServiceClient>("acme.fuel.v1.FuelCardCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiFuel.FuelCardQueryService.FuelCardQueryServiceClient>("acme.fuel.v1.FuelCardQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiFuel.FuelCardService.FuelCardServiceClient>("acme.fuel.v1.FuelCardService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiGiftcards.GiftCardQueryService.GiftCardQueryServiceClient>("acme.giftcards.v1.GiftCardQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiGiftcards.GiftCardService.GiftCardServiceClient>("acme.giftcards.v1.GiftCardService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiHelpdesk.EscalationApprovalService.EscalationApprovalServiceClient>("acme.helpdesk.v1.EscalationApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiHelpdesk.EscalationCalculationService.EscalationCalculationServiceClient>("acme.helpdesk.v1.EscalationCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiHelpdesk.EscalationLifecycleService.EscalationLifecycleServiceClient>("acme.helpdesk.v1.EscalationLifecycleService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiHelpdesk.EscalationReportingService.EscalationReportingServiceClient>("acme.helpdesk.v1.EscalationReportingService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiHelpdesk.EscalationValidationService.EscalationValidationServiceClient>("acme.helpdesk.v1.EscalationValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiHelpdesk.KnowledgeArticleApprovalService.KnowledgeArticleApprovalServiceClient>("acme.helpdesk.v1.KnowledgeArticleApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiHelpdesk.KnowledgeArticleCalculationService.KnowledgeArticleCalculationServiceClient>("acme.helpdesk.v1.KnowledgeArticleCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiHelpdesk.KnowledgeArticleLifecycleService.KnowledgeArticleLifecycleServiceClient>("acme.helpdesk.v1.KnowledgeArticleLifecycleService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiHelpdesk.KnowledgeArticleQueryService.KnowledgeArticleQueryServiceClient>("acme.helpdesk.v1.KnowledgeArticleQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient>("acme.helpdesk.v1.KnowledgeArticleValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiHelpdesk.KnowledgeArticleWorkflowService.KnowledgeArticleWorkflowServiceClient>("acme.helpdesk.v1.KnowledgeArticleWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiHelpdesk.SlaPolicyApprovalService.SlaPolicyApprovalServiceClient>("acme.helpdesk.v1.SlaPolicyApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient>("acme.helpdesk.v1.SlaPolicyCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiHelpdesk.SlaPolicyLifecycleService.SlaPolicyLifecycleServiceClient>("acme.helpdesk.v1.SlaPolicyLifecycleService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiHelpdesk.SlaPolicyValidationService.SlaPolicyValidationServiceClient>("acme.helpdesk.v1.SlaPolicyValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiHelpdesk.SlaPolicyWorkflowService.SlaPolicyWorkflowServiceClient>("acme.helpdesk.v1.SlaPolicyWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiHelpdesk.TicketApprovalService.TicketApprovalServiceClient>("acme.helpdesk.v1.TicketApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiHelpdesk.TicketCalculationService.TicketCalculationServiceClient>("acme.helpdesk.v1.TicketCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiHelpdesk.TicketQueryService.TicketQueryServiceClient>("acme.helpdesk.v1.TicketQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiHelpdesk.TicketService.TicketServiceClient>("acme.helpdesk.v1.TicketService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiHelpdesk.TicketValidationService.TicketValidationServiceClient>("acme.helpdesk.v1.TicketValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiHelpdesk.TicketWorkflowService.TicketWorkflowServiceClient>("acme.helpdesk.v1.TicketWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiIdentity.UserQueryService.UserQueryServiceClient>("acme.identity.v1.UserQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiIdentity.UserService.UserServiceClient>("acme.identity.v1.UserService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiIdentity.UserValidationService.UserValidationServiceClient>("acme.identity.v1.UserValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiInsurance.PolicyCalculationService.PolicyCalculationServiceClient>("acme.insurance.v1.PolicyCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiInsurance.PolicyQueryService.PolicyQueryServiceClient>("acme.insurance.v1.PolicyQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiInsurance.PolicyService.PolicyServiceClient>("acme.insurance.v1.PolicyService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiInvoicing.InvoiceBatchCalculationService.InvoiceBatchCalculationServiceClient>("acme.invoicing.v1.InvoiceBatchCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiInvoicing.InvoiceBatchService.InvoiceBatchServiceClient>("acme.invoicing.v1.InvoiceBatchService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiItsm.ChangeRequestApprovalService.ChangeRequestApprovalServiceClient>("acme.itsm.v1.ChangeRequestApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiItsm.ChangeRequestCalculationService.ChangeRequestCalculationServiceClient>("acme.itsm.v1.ChangeRequestCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiItsm.ChangeRequestLifecycleService.ChangeRequestLifecycleServiceClient>("acme.itsm.v1.ChangeRequestLifecycleService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiItsm.ChangeRequestQueryService.ChangeRequestQueryServiceClient>("acme.itsm.v1.ChangeRequestQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiItsm.ChangeRequestReportingService.ChangeRequestReportingServiceClient>("acme.itsm.v1.ChangeRequestReportingService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient>("acme.itsm.v1.ChangeRequestValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiItsm.ChangeRequestWorkflowService.ChangeRequestWorkflowServiceClient>("acme.itsm.v1.ChangeRequestWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiItsm.IncidentApprovalService.IncidentApprovalServiceClient>("acme.itsm.v1.IncidentApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiItsm.IncidentCalculationService.IncidentCalculationServiceClient>("acme.itsm.v1.IncidentCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiItsm.IncidentLifecycleService.IncidentLifecycleServiceClient>("acme.itsm.v1.IncidentLifecycleService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiItsm.IncidentQueryService.IncidentQueryServiceClient>("acme.itsm.v1.IncidentQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiItsm.IncidentService.IncidentServiceClient>("acme.itsm.v1.IncidentService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiItsm.IncidentValidationService.IncidentValidationServiceClient>("acme.itsm.v1.IncidentValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiItsm.IncidentWorkflowService.IncidentWorkflowServiceClient>("acme.itsm.v1.IncidentWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiItsm.ProblemApprovalService.ProblemApprovalServiceClient>("acme.itsm.v1.ProblemApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiItsm.ProblemCalculationService.ProblemCalculationServiceClient>("acme.itsm.v1.ProblemCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiItsm.ProblemLifecycleService.ProblemLifecycleServiceClient>("acme.itsm.v1.ProblemLifecycleService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiItsm.ProblemReportingService.ProblemReportingServiceClient>("acme.itsm.v1.ProblemReportingService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiItsm.ProblemSyncService.ProblemSyncServiceClient>("acme.itsm.v1.ProblemSyncService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiItsm.ProblemValidationService.ProblemValidationServiceClient>("acme.itsm.v1.ProblemValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiItsm.ServiceAssetApprovalService.ServiceAssetApprovalServiceClient>("acme.itsm.v1.ServiceAssetApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiItsm.ServiceAssetCalculationService.ServiceAssetCalculationServiceClient>("acme.itsm.v1.ServiceAssetCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiItsm.ServiceAssetLifecycleService.ServiceAssetLifecycleServiceClient>("acme.itsm.v1.ServiceAssetLifecycleService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiItsm.ServiceAssetReportingService.ServiceAssetReportingServiceClient>("acme.itsm.v1.ServiceAssetReportingService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiItsm.ServiceAssetValidationService.ServiceAssetValidationServiceClient>("acme.itsm.v1.ServiceAssetValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiItsm.ServiceAssetWorkflowService.ServiceAssetWorkflowServiceClient>("acme.itsm.v1.ServiceAssetWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiKeymanagement.CryptoKeyCalculationService.CryptoKeyCalculationServiceClient>("acme.keymanagement.v1.CryptoKeyCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiKeymanagement.CryptoKeyQueryService.CryptoKeyQueryServiceClient>("acme.keymanagement.v1.CryptoKeyQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLabeling.LabelingTaskCalculationService.LabelingTaskCalculationServiceClient>("acme.labeling.v1.LabelingTaskCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLabeling.LabelingTaskQueryService.LabelingTaskQueryServiceClient>("acme.labeling.v1.LabelingTaskQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLabeling.LabelingTaskService.LabelingTaskServiceClient>("acme.labeling.v1.LabelingTaskService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLearning.CertificationApprovalService.CertificationApprovalServiceClient>("acme.learning.v1.CertificationApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLearning.CertificationCalculationService.CertificationCalculationServiceClient>("acme.learning.v1.CertificationCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLearning.CertificationValidationService.CertificationValidationServiceClient>("acme.learning.v1.CertificationValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLearning.CertificationWorkflowService.CertificationWorkflowServiceClient>("acme.learning.v1.CertificationWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLearning.CourseApprovalService.CourseApprovalServiceClient>("acme.learning.v1.CourseApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLearning.CourseEnrollmentApprovalService.CourseEnrollmentApprovalServiceClient>("acme.learning.v1.CourseEnrollmentApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLearning.CourseEnrollmentCalculationService.CourseEnrollmentCalculationServiceClient>("acme.learning.v1.CourseEnrollmentCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLearning.CourseEnrollmentValidationService.CourseEnrollmentValidationServiceClient>("acme.learning.v1.CourseEnrollmentValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLearning.CourseQueryService.CourseQueryServiceClient>("acme.learning.v1.CourseQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLearning.CourseService.CourseServiceClient>("acme.learning.v1.CourseService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLearning.CourseValidationService.CourseValidationServiceClient>("acme.learning.v1.CourseValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLearning.CourseWorkflowService.CourseWorkflowServiceClient>("acme.learning.v1.CourseWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLearning.LearningPathApprovalService.LearningPathApprovalServiceClient>("acme.learning.v1.LearningPathApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient>("acme.learning.v1.LearningPathCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLearning.LearningPathQueryService.LearningPathQueryServiceClient>("acme.learning.v1.LearningPathQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLearning.LearningPathValidationService.LearningPathValidationServiceClient>("acme.learning.v1.LearningPathValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLearning.LearningPathWorkflowService.LearningPathWorkflowServiceClient>("acme.learning.v1.LearningPathWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient>("acme.ledger.v1.LedgerAccountQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLedger.LedgerAccountService.LedgerAccountServiceClient>("acme.ledger.v1.LedgerAccountService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient>("acme.ledger.v1.LedgerAccountValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLending.LoanApplicationCalculationService.LoanApplicationCalculationServiceClient>("acme.lending.v1.LoanApplicationCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLicensing.EntitlementApprovalService.EntitlementApprovalServiceClient>("acme.licensing.v1.EntitlementApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient>("acme.licensing.v1.EntitlementCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLicensing.EntitlementLifecycleService.EntitlementLifecycleServiceClient>("acme.licensing.v1.EntitlementLifecycleService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLicensing.EntitlementQueryService.EntitlementQueryServiceClient>("acme.licensing.v1.EntitlementQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient>("acme.licensing.v1.EntitlementValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLicensing.EntitlementWorkflowService.EntitlementWorkflowServiceClient>("acme.licensing.v1.EntitlementWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient>("acme.licensing.v1.LicenseAssignmentApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLicensing.LicenseAssignmentCalculationService.LicenseAssignmentCalculationServiceClient>("acme.licensing.v1.LicenseAssignmentCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLicensing.LicenseAssignmentLifecycleService.LicenseAssignmentLifecycleServiceClient>("acme.licensing.v1.LicenseAssignmentLifecycleService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient>("acme.licensing.v1.LicenseAssignmentReportingService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLicensing.LicenseAssignmentValidationService.LicenseAssignmentValidationServiceClient>("acme.licensing.v1.LicenseAssignmentValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLicensing.LicenseAssignmentWorkflowService.LicenseAssignmentWorkflowServiceClient>("acme.licensing.v1.LicenseAssignmentWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLicensing.SoftwareLicenseApprovalService.SoftwareLicenseApprovalServiceClient>("acme.licensing.v1.SoftwareLicenseApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient>("acme.licensing.v1.SoftwareLicenseCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient>("acme.licensing.v1.SoftwareLicenseQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLicensing.SoftwareLicenseService.SoftwareLicenseServiceClient>("acme.licensing.v1.SoftwareLicenseService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient>("acme.licensing.v1.SoftwareLicenseValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLicensing.SoftwareLicenseWorkflowService.SoftwareLicenseWorkflowServiceClient>("acme.licensing.v1.SoftwareLicenseWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLineage.LineageNodeCalculationService.LineageNodeCalculationServiceClient>("acme.lineage.v1.LineageNodeCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLineage.LineageNodeQueryService.LineageNodeQueryServiceClient>("acme.lineage.v1.LineageNodeQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLocalization.TranslationKeyQueryService.TranslationKeyQueryServiceClient>("acme.localization.v1.TranslationKeyQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLocalization.TranslationKeyService.TranslationKeyServiceClient>("acme.localization.v1.TranslationKeyService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLoyalty.LoyaltyMemberCalculationService.LoyaltyMemberCalculationServiceClient>("acme.loyalty.v1.LoyaltyMemberCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLoyalty.LoyaltyMemberQueryService.LoyaltyMemberQueryServiceClient>("acme.loyalty.v1.LoyaltyMemberQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiLoyalty.LoyaltyMemberService.LoyaltyMemberServiceClient>("acme.loyalty.v1.LoyaltyMemberService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiMaintenance.WorkOrderCalculationService.WorkOrderCalculationServiceClient>("acme.maintenance.v1.WorkOrderCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiMaintenance.WorkOrderQueryService.WorkOrderQueryServiceClient>("acme.maintenance.v1.WorkOrderQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiMaintenance.WorkOrderService.WorkOrderServiceClient>("acme.maintenance.v1.WorkOrderService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiMedia.MediaAssetCalculationService.MediaAssetCalculationServiceClient>("acme.media.v1.MediaAssetCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiMedia.MediaAssetQueryService.MediaAssetQueryServiceClient>("acme.media.v1.MediaAssetQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiMedia.MediaAssetService.MediaAssetServiceClient>("acme.media.v1.MediaAssetService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiMerchandising.CollectionService.CollectionServiceClient>("acme.merchandising.v1.CollectionService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiMessaging.ConversationCalculationService.ConversationCalculationServiceClient>("acme.messaging.v1.ConversationCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiMessaging.ConversationQueryService.ConversationQueryServiceClient>("acme.messaging.v1.ConversationQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiMessaging.ConversationService.ConversationServiceClient>("acme.messaging.v1.ConversationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient>("acme.metrics.v1.MetricSeriesQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiMetrics.MetricSeriesService.MetricSeriesServiceClient>("acme.metrics.v1.MetricSeriesService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient>("acme.metrics.v1.MetricSeriesValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiModelregistry.RegisteredModelCalculationService.RegisteredModelCalculationServiceClient>("acme.modelregistry.v1.RegisteredModelCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiModelregistry.RegisteredModelService.RegisteredModelServiceClient>("acme.modelregistry.v1.RegisteredModelService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiNotifications.NotificationQueryService.NotificationQueryServiceClient>("acme.notifications.v1.NotificationQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiNotifications.NotificationService.NotificationServiceClient>("acme.notifications.v1.NotificationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiNotifications.NotificationValidationService.NotificationValidationServiceClient>("acme.notifications.v1.NotificationValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiOnboarding.EquipmentApprovalService.EquipmentApprovalServiceClient>("acme.onboarding.v1.EquipmentApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiOnboarding.EquipmentCalculationService.EquipmentCalculationServiceClient>("acme.onboarding.v1.EquipmentCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiOnboarding.EquipmentLifecycleService.EquipmentLifecycleServiceClient>("acme.onboarding.v1.EquipmentLifecycleService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiOnboarding.EquipmentReportingService.EquipmentReportingServiceClient>("acme.onboarding.v1.EquipmentReportingService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiOnboarding.EquipmentValidationService.EquipmentValidationServiceClient>("acme.onboarding.v1.EquipmentValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiOnboarding.EquipmentWorkflowService.EquipmentWorkflowServiceClient>("acme.onboarding.v1.EquipmentWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiOnboarding.OnboardingPlanApprovalService.OnboardingPlanApprovalServiceClient>("acme.onboarding.v1.OnboardingPlanApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiOnboarding.OnboardingPlanCalculationService.OnboardingPlanCalculationServiceClient>("acme.onboarding.v1.OnboardingPlanCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiOnboarding.OnboardingPlanLifecycleService.OnboardingPlanLifecycleServiceClient>("acme.onboarding.v1.OnboardingPlanLifecycleService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiOnboarding.OnboardingPlanQueryService.OnboardingPlanQueryServiceClient>("acme.onboarding.v1.OnboardingPlanQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiOnboarding.OnboardingPlanService.OnboardingPlanServiceClient>("acme.onboarding.v1.OnboardingPlanService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiOnboarding.OnboardingPlanValidationService.OnboardingPlanValidationServiceClient>("acme.onboarding.v1.OnboardingPlanValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiOnboarding.OnboardingPlanWorkflowService.OnboardingPlanWorkflowServiceClient>("acme.onboarding.v1.OnboardingPlanWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiOnboarding.OnboardingTaskApprovalService.OnboardingTaskApprovalServiceClient>("acme.onboarding.v1.OnboardingTaskApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiOnboarding.OnboardingTaskCalculationService.OnboardingTaskCalculationServiceClient>("acme.onboarding.v1.OnboardingTaskCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiOnboarding.OnboardingTaskQueryService.OnboardingTaskQueryServiceClient>("acme.onboarding.v1.OnboardingTaskQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiOnboarding.OnboardingTaskReportingService.OnboardingTaskReportingServiceClient>("acme.onboarding.v1.OnboardingTaskReportingService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiOnboarding.OnboardingTaskValidationService.OnboardingTaskValidationServiceClient>("acme.onboarding.v1.OnboardingTaskValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiOrders.OrderCalculationService.OrderCalculationServiceClient>("acme.orders.v1.OrderCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiPatientapp.PatientHomeQueryService.PatientHomeQueryServiceClient>("acme.patientapp.v1.PatientHomeQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiPatientapp.PatientHomeService.PatientHomeServiceClient>("acme.patientapp.v1.PatientHomeService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiPatients.PatientCalculationService.PatientCalculationServiceClient>("acme.patients.v1.PatientCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiPatients.PatientQueryService.PatientQueryServiceClient>("acme.patients.v1.PatientQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiPatients.PatientService.PatientServiceClient>("acme.patients.v1.PatientService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiPayments.PaymentCalculationService.PaymentCalculationServiceClient>("acme.payments.v1.PaymentCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiPayroll.PayRunCalculationService.PayRunCalculationServiceClient>("acme.payroll.v1.PayRunCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiPayroll.PayRunService.PayRunServiceClient>("acme.payroll.v1.PayRunService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiPerformance.CalibrationApprovalService.CalibrationApprovalServiceClient>("acme.performance.v1.CalibrationApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiPerformance.CalibrationCalculationService.CalibrationCalculationServiceClient>("acme.performance.v1.CalibrationCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiPerformance.CalibrationLifecycleService.CalibrationLifecycleServiceClient>("acme.performance.v1.CalibrationLifecycleService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiPerformance.CalibrationValidationService.CalibrationValidationServiceClient>("acme.performance.v1.CalibrationValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiPerformance.FeedbackApprovalService.FeedbackApprovalServiceClient>("acme.performance.v1.FeedbackApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiPerformance.FeedbackCalculationService.FeedbackCalculationServiceClient>("acme.performance.v1.FeedbackCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiPerformance.FeedbackValidationService.FeedbackValidationServiceClient>("acme.performance.v1.FeedbackValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiPerformance.FeedbackWorkflowService.FeedbackWorkflowServiceClient>("acme.performance.v1.FeedbackWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiPerformance.GoalApprovalService.GoalApprovalServiceClient>("acme.performance.v1.GoalApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiPerformance.GoalQueryService.GoalQueryServiceClient>("acme.performance.v1.GoalQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiPerformance.GoalValidationService.GoalValidationServiceClient>("acme.performance.v1.GoalValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiPerformance.GoalWorkflowService.GoalWorkflowServiceClient>("acme.performance.v1.GoalWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiPerformance.ReviewApprovalService.ReviewApprovalServiceClient>("acme.performance.v1.ReviewApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiPerformance.ReviewQueryService.ReviewQueryServiceClient>("acme.performance.v1.ReviewQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiPerformance.ReviewService.ReviewServiceClient>("acme.performance.v1.ReviewService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiPerformance.ReviewWorkflowService.ReviewWorkflowServiceClient>("acme.performance.v1.ReviewWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiPerformers.PerformerQueryService.PerformerQueryServiceClient>("acme.performers.v1.PerformerQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiPerformers.PerformerService.PerformerServiceClient>("acme.performers.v1.PerformerService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiPersonalization.ProfileCalculationService.ProfileCalculationServiceClient>("acme.personalization.v1.ProfileCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiPersonalization.ProfileQueryService.ProfileQueryServiceClient>("acme.personalization.v1.ProfileQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiPersonalization.ProfileService.ProfileServiceClient>("acme.personalization.v1.ProfileService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiPipelines.PipelineCalculationService.PipelineCalculationServiceClient>("acme.pipelines.v1.PipelineCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiPipelines.PipelineService.PipelineServiceClient>("acme.pipelines.v1.PipelineService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiPrescriptions.PrescriptionCalculationService.PrescriptionCalculationServiceClient>("acme.prescriptions.v1.PrescriptionCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiPrescriptions.PrescriptionQueryService.PrescriptionQueryServiceClient>("acme.prescriptions.v1.PrescriptionQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiPrescriptions.PrescriptionService.PrescriptionServiceClient>("acme.prescriptions.v1.PrescriptionService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiPricing.PriceListQueryService.PriceListQueryServiceClient>("acme.pricing.v1.PriceListQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiProcurement.PurchaseRequisitionCalculationService.PurchaseRequisitionCalculationServiceClient>("acme.procurement.v1.PurchaseRequisitionCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiPromotions.PromotionCalculationService.PromotionCalculationServiceClient>("acme.promotions.v1.PromotionCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiPromotions.PromotionService.PromotionServiceClient>("acme.promotions.v1.PromotionService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiRatelimiting.RateLimitCalculationService.RateLimitCalculationServiceClient>("acme.ratelimiting.v1.RateLimitCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiRatelimiting.RateLimitQueryService.RateLimitQueryServiceClient>("acme.ratelimiting.v1.RateLimitQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiRatelimiting.RateLimitService.RateLimitServiceClient>("acme.ratelimiting.v1.RateLimitService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiRecommendations.RecommendationSetQueryService.RecommendationSetQueryServiceClient>("acme.recommendations.v1.RecommendationSetQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiRecommendations.RecommendationSetService.RecommendationSetServiceClient>("acme.recommendations.v1.RecommendationSetService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiRecruiting.CandidateApprovalService.CandidateApprovalServiceClient>("acme.recruiting.v1.CandidateApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient>("acme.recruiting.v1.CandidateCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiRecruiting.CandidateLifecycleService.CandidateLifecycleServiceClient>("acme.recruiting.v1.CandidateLifecycleService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiRecruiting.CandidateQueryService.CandidateQueryServiceClient>("acme.recruiting.v1.CandidateQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiRecruiting.CandidateReportingService.CandidateReportingServiceClient>("acme.recruiting.v1.CandidateReportingService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient>("acme.recruiting.v1.CandidateValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiRecruiting.CandidateWorkflowService.CandidateWorkflowServiceClient>("acme.recruiting.v1.CandidateWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient>("acme.recruiting.v1.InterviewCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiRecruiting.InterviewLifecycleService.InterviewLifecycleServiceClient>("acme.recruiting.v1.InterviewLifecycleService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiRecruiting.InterviewReportingService.InterviewReportingServiceClient>("acme.recruiting.v1.InterviewReportingService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiRecruiting.InterviewSyncService.InterviewSyncServiceClient>("acme.recruiting.v1.InterviewSyncService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient>("acme.recruiting.v1.InterviewValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiRecruiting.JobOpeningApprovalService.JobOpeningApprovalServiceClient>("acme.recruiting.v1.JobOpeningApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient>("acme.recruiting.v1.JobOpeningCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiRecruiting.JobOpeningLifecycleService.JobOpeningLifecycleServiceClient>("acme.recruiting.v1.JobOpeningLifecycleService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiRecruiting.JobOpeningQueryService.JobOpeningQueryServiceClient>("acme.recruiting.v1.JobOpeningQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiRecruiting.JobOpeningService.JobOpeningServiceClient>("acme.recruiting.v1.JobOpeningService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient>("acme.recruiting.v1.JobOpeningValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiRecruiting.JobOpeningWorkflowService.JobOpeningWorkflowServiceClient>("acme.recruiting.v1.JobOpeningWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiRecruiting.OfferApprovalService.OfferApprovalServiceClient>("acme.recruiting.v1.OfferApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiRecruiting.OfferCalculationService.OfferCalculationServiceClient>("acme.recruiting.v1.OfferCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiRecruiting.OfferLifecycleService.OfferLifecycleServiceClient>("acme.recruiting.v1.OfferLifecycleService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiRecruiting.OfferReportingService.OfferReportingServiceClient>("acme.recruiting.v1.OfferReportingService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiRecruiting.OfferSyncService.OfferSyncServiceClient>("acme.recruiting.v1.OfferSyncService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiRecruiting.OfferValidationService.OfferValidationServiceClient>("acme.recruiting.v1.OfferValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiReturns.ReturnRequestCalculationService.ReturnRequestCalculationServiceClient>("acme.returns.v1.ReturnRequestCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiReturns.ReturnRequestQueryService.ReturnRequestQueryServiceClient>("acme.returns.v1.ReturnRequestQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiReturns.ReturnRequestService.ReturnRequestServiceClient>("acme.returns.v1.ReturnRequestService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiReviews.ProductReviewQueryService.ProductReviewQueryServiceClient>("acme.reviews.v1.ProductReviewQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiRisk.RiskRegisterQueryService.RiskRegisterQueryServiceClient>("acme.risk.v1.RiskRegisterQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiRisk.RiskRegisterService.RiskRegisterServiceClient>("acme.risk.v1.RiskRegisterService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiRouting.RouteCalculationService.RouteCalculationServiceClient>("acme.routing.v1.RouteCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiRouting.RouteQueryService.RouteQueryServiceClient>("acme.routing.v1.RouteQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiScheduling.AvailabilityWindowApprovalService.AvailabilityWindowApprovalServiceClient>("acme.scheduling.v1.AvailabilityWindowApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiScheduling.AvailabilityWindowCalculationService.AvailabilityWindowCalculationServiceClient>("acme.scheduling.v1.AvailabilityWindowCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiScheduling.AvailabilityWindowLifecycleService.AvailabilityWindowLifecycleServiceClient>("acme.scheduling.v1.AvailabilityWindowLifecycleService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiScheduling.AvailabilityWindowValidationService.AvailabilityWindowValidationServiceClient>("acme.scheduling.v1.AvailabilityWindowValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiScheduling.AvailabilityWindowWorkflowService.AvailabilityWindowWorkflowServiceClient>("acme.scheduling.v1.AvailabilityWindowWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiScheduling.RosterApprovalService.RosterApprovalServiceClient>("acme.scheduling.v1.RosterApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiScheduling.RosterCalculationService.RosterCalculationServiceClient>("acme.scheduling.v1.RosterCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiScheduling.RosterQueryService.RosterQueryServiceClient>("acme.scheduling.v1.RosterQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiScheduling.RosterValidationService.RosterValidationServiceClient>("acme.scheduling.v1.RosterValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiScheduling.RosterWorkflowService.RosterWorkflowServiceClient>("acme.scheduling.v1.RosterWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiScheduling.ShiftApprovalService.ShiftApprovalServiceClient>("acme.scheduling.v1.ShiftApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiScheduling.ShiftQueryService.ShiftQueryServiceClient>("acme.scheduling.v1.ShiftQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiScheduling.ShiftService.ShiftServiceClient>("acme.scheduling.v1.ShiftService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiScheduling.ShiftValidationService.ShiftValidationServiceClient>("acme.scheduling.v1.ShiftValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiScheduling.ShiftWorkflowService.ShiftWorkflowServiceClient>("acme.scheduling.v1.ShiftWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiScheduling.SwapRequestApprovalService.SwapRequestApprovalServiceClient>("acme.scheduling.v1.SwapRequestApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiScheduling.SwapRequestCalculationService.SwapRequestCalculationServiceClient>("acme.scheduling.v1.SwapRequestCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiScheduling.SwapRequestValidationService.SwapRequestValidationServiceClient>("acme.scheduling.v1.SwapRequestValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient>("acme.search.v1.SearchIndexQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiSearch.SearchIndexService.SearchIndexServiceClient>("acme.search.v1.SearchIndexService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiSearch.SearchIndexValidationService.SearchIndexValidationServiceClient>("acme.search.v1.SearchIndexValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiSeating.SeatMapQueryService.SeatMapQueryServiceClient>("acme.seating.v1.SeatMapQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiSecrets.SecretQueryService.SecretQueryServiceClient>("acme.secrets.v1.SecretQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiSecrets.SecretService.SecretServiceClient>("acme.secrets.v1.SecretService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiSellers.SellerCalculationService.SellerCalculationServiceClient>("acme.sellers.v1.SellerCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiSellers.SellerQueryService.SellerQueryServiceClient>("acme.sellers.v1.SellerQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiSellers.SellerService.SellerServiceClient>("acme.sellers.v1.SellerService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiShipping.ShipmentQueryService.ShipmentQueryServiceClient>("acme.shipping.v1.ShipmentQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiShipping.ShipmentService.ShipmentServiceClient>("acme.shipping.v1.ShipmentService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiShows.ShowQueryService.ShowQueryServiceClient>("acme.shows.v1.ShowQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiShows.ShowService.ShowServiceClient>("acme.shows.v1.ShowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiSponsorships.SponsorCalculationService.SponsorCalculationServiceClient>("acme.sponsorships.v1.SponsorCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiSubscriptions.SubscriptionService.SubscriptionServiceClient>("acme.subscriptions.v1.SubscriptionService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiSurveys.SurveyQueryService.SurveyQueryServiceClient>("acme.surveys.v1.SurveyQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiSurveys.SurveyService.SurveyServiceClient>("acme.surveys.v1.SurveyService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTelehealth.VideoVisitCalculationService.VideoVisitCalculationServiceClient>("acme.telehealth.v1.VideoVisitCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTelehealth.VideoVisitQueryService.VideoVisitQueryServiceClient>("acme.telehealth.v1.VideoVisitQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTelematics.TelemetryReadingCalculationService.TelemetryReadingCalculationServiceClient>("acme.telematics.v1.TelemetryReadingCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTelematics.TelemetryReadingQueryService.TelemetryReadingQueryServiceClient>("acme.telematics.v1.TelemetryReadingQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTicketing.TicketService.TicketServiceClient>("acme.ticketing.v1.TicketService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTimekeeping.ClockEventApprovalService.ClockEventApprovalServiceClient>("acme.timekeeping.v1.ClockEventApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTimekeeping.ClockEventCalculationService.ClockEventCalculationServiceClient>("acme.timekeeping.v1.ClockEventCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTimekeeping.ClockEventQueryService.ClockEventQueryServiceClient>("acme.timekeeping.v1.ClockEventQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTimekeeping.ClockEventValidationService.ClockEventValidationServiceClient>("acme.timekeeping.v1.ClockEventValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTimekeeping.ClockEventWorkflowService.ClockEventWorkflowServiceClient>("acme.timekeeping.v1.ClockEventWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTimekeeping.OvertimeApprovalService.OvertimeApprovalServiceClient>("acme.timekeeping.v1.OvertimeApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTimekeeping.OvertimeCalculationService.OvertimeCalculationServiceClient>("acme.timekeeping.v1.OvertimeCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTimekeeping.OvertimeLifecycleService.OvertimeLifecycleServiceClient>("acme.timekeeping.v1.OvertimeLifecycleService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTimekeeping.OvertimeValidationService.OvertimeValidationServiceClient>("acme.timekeeping.v1.OvertimeValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTimekeeping.OvertimeWorkflowService.OvertimeWorkflowServiceClient>("acme.timekeeping.v1.OvertimeWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTimekeeping.TimesheetApprovalService.TimesheetApprovalServiceClient>("acme.timekeeping.v1.TimesheetApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTimekeeping.TimesheetQueryService.TimesheetQueryServiceClient>("acme.timekeeping.v1.TimesheetQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTimekeeping.TimesheetService.TimesheetServiceClient>("acme.timekeeping.v1.TimesheetService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTimekeeping.TimesheetValidationService.TimesheetValidationServiceClient>("acme.timekeeping.v1.TimesheetValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTimekeeping.TimesheetWorkflowService.TimesheetWorkflowServiceClient>("acme.timekeeping.v1.TimesheetWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTours.TourQueryService.TourQueryServiceClient>("acme.tours.v1.TourQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTravel.ItineraryApprovalService.ItineraryApprovalServiceClient>("acme.travel.v1.ItineraryApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTravel.ItineraryCalculationService.ItineraryCalculationServiceClient>("acme.travel.v1.ItineraryCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTravel.ItineraryLifecycleService.ItineraryLifecycleServiceClient>("acme.travel.v1.ItineraryLifecycleService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTravel.ItineraryReportingService.ItineraryReportingServiceClient>("acme.travel.v1.ItineraryReportingService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTravel.ItinerarySyncService.ItinerarySyncServiceClient>("acme.travel.v1.ItinerarySyncService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTravel.ItineraryValidationService.ItineraryValidationServiceClient>("acme.travel.v1.ItineraryValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTravel.TravelBookingApprovalService.TravelBookingApprovalServiceClient>("acme.travel.v1.TravelBookingApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTravel.TravelBookingCalculationService.TravelBookingCalculationServiceClient>("acme.travel.v1.TravelBookingCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTravel.TravelBookingLifecycleService.TravelBookingLifecycleServiceClient>("acme.travel.v1.TravelBookingLifecycleService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTravel.TravelBookingQueryService.TravelBookingQueryServiceClient>("acme.travel.v1.TravelBookingQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTravel.TravelBookingReportingService.TravelBookingReportingServiceClient>("acme.travel.v1.TravelBookingReportingService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTravel.TravelBookingValidationService.TravelBookingValidationServiceClient>("acme.travel.v1.TravelBookingValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTravel.TravelBookingWorkflowService.TravelBookingWorkflowServiceClient>("acme.travel.v1.TravelBookingWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTravel.TravelPolicyApprovalService.TravelPolicyApprovalServiceClient>("acme.travel.v1.TravelPolicyApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTravel.TravelPolicyCalculationService.TravelPolicyCalculationServiceClient>("acme.travel.v1.TravelPolicyCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTravel.TravelPolicyLifecycleService.TravelPolicyLifecycleServiceClient>("acme.travel.v1.TravelPolicyLifecycleService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTravel.TravelPolicyReportingService.TravelPolicyReportingServiceClient>("acme.travel.v1.TravelPolicyReportingService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTravel.TravelPolicySyncService.TravelPolicySyncServiceClient>("acme.travel.v1.TravelPolicySyncService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTravel.TravelPolicyValidationService.TravelPolicyValidationServiceClient>("acme.travel.v1.TravelPolicyValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTravel.TravelPolicyWorkflowService.TravelPolicyWorkflowServiceClient>("acme.travel.v1.TravelPolicyWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTravel.TripApprovalService.TripApprovalServiceClient>("acme.travel.v1.TripApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTravel.TripCalculationService.TripCalculationServiceClient>("acme.travel.v1.TripCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTravel.TripLifecycleService.TripLifecycleServiceClient>("acme.travel.v1.TripLifecycleService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTravel.TripQueryService.TripQueryServiceClient>("acme.travel.v1.TripQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTravel.TripService.TripServiceClient>("acme.travel.v1.TripService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTravel.TripValidationService.TripValidationServiceClient>("acme.travel.v1.TripValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTravel.TripWorkflowService.TripWorkflowServiceClient>("acme.travel.v1.TripWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTreasury.CashPositionQueryService.CashPositionQueryServiceClient>("acme.treasury.v1.CashPositionQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiTreasury.CashPositionService.CashPositionServiceClient>("acme.treasury.v1.CashPositionService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiVenues.VenueQueryService.VenueQueryServiceClient>("acme.venues.v1.VenueQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiVenues.VenueService.VenueServiceClient>("acme.venues.v1.VenueService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiVenues.VenueValidationService.VenueValidationServiceClient>("acme.venues.v1.VenueValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiWarehousing.WarehouseQueryService.WarehouseQueryServiceClient>("acme.warehousing.v1.WarehouseQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiWarehousing.WarehouseService.WarehouseServiceClient>("acme.warehousing.v1.WarehouseService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiWebbff.PageCompositionQueryService.PageCompositionQueryServiceClient>("acme.webbff.v1.PageCompositionQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiWebbff.PageCompositionService.PageCompositionServiceClient>("acme.webbff.v1.PageCompositionService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiWebhooks.WebhookEndpointQueryService.WebhookEndpointQueryServiceClient>("acme.webhooks.v1.WebhookEndpointQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiWorkplaces.BuildingApprovalService.BuildingApprovalServiceClient>("acme.workplaces.v1.BuildingApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiWorkplaces.BuildingCalculationService.BuildingCalculationServiceClient>("acme.workplaces.v1.BuildingCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiWorkplaces.BuildingQueryService.BuildingQueryServiceClient>("acme.workplaces.v1.BuildingQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiWorkplaces.BuildingService.BuildingServiceClient>("acme.workplaces.v1.BuildingService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiWorkplaces.BuildingValidationService.BuildingValidationServiceClient>("acme.workplaces.v1.BuildingValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiWorkplaces.BuildingWorkflowService.BuildingWorkflowServiceClient>("acme.workplaces.v1.BuildingWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiWorkplaces.DeskApprovalService.DeskApprovalServiceClient>("acme.workplaces.v1.DeskApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient>("acme.workplaces.v1.DeskCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiWorkplaces.DeskLifecycleService.DeskLifecycleServiceClient>("acme.workplaces.v1.DeskLifecycleService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiWorkplaces.DeskQueryService.DeskQueryServiceClient>("acme.workplaces.v1.DeskQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiWorkplaces.DeskValidationService.DeskValidationServiceClient>("acme.workplaces.v1.DeskValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiWorkplaces.DeskWorkflowService.DeskWorkflowServiceClient>("acme.workplaces.v1.DeskWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiWorkplaces.RoomBookingApprovalService.RoomBookingApprovalServiceClient>("acme.workplaces.v1.RoomBookingApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiWorkplaces.RoomBookingCalculationService.RoomBookingCalculationServiceClient>("acme.workplaces.v1.RoomBookingCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiWorkplaces.RoomBookingLifecycleService.RoomBookingLifecycleServiceClient>("acme.workplaces.v1.RoomBookingLifecycleService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiWorkplaces.RoomBookingReportingService.RoomBookingReportingServiceClient>("acme.workplaces.v1.RoomBookingReportingService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiWorkplaces.RoomBookingValidationService.RoomBookingValidationServiceClient>("acme.workplaces.v1.RoomBookingValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiWorkplaces.RoomBookingWorkflowService.RoomBookingWorkflowServiceClient>("acme.workplaces.v1.RoomBookingWorkflowService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiWorkplaces.VisitorPassApprovalService.VisitorPassApprovalServiceClient>("acme.workplaces.v1.VisitorPassApprovalService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiWorkplaces.VisitorPassCalculationService.VisitorPassCalculationServiceClient>("acme.workplaces.v1.VisitorPassCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiWorkplaces.VisitorPassLifecycleService.VisitorPassLifecycleServiceClient>("acme.workplaces.v1.VisitorPassLifecycleService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiWorkplaces.VisitorPassReportingService.VisitorPassReportingServiceClient>("acme.workplaces.v1.VisitorPassReportingService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiWorkplaces.VisitorPassSyncService.VisitorPassSyncServiceClient>("acme.workplaces.v1.VisitorPassSyncService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiWorkplaces.VisitorPassValidationService.VisitorPassValidationServiceClient>("acme.workplaces.v1.VisitorPassValidationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiYards.YardCalculationService.YardCalculationServiceClient>("acme.yards.v1.YardCalculationService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiYards.YardQueryService.YardQueryServiceClient>("acme.yards.v1.YardQueryService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddGrpcClient<ApiYards.YardService.YardServiceClient>("acme.yards.v1.YardService", (provider, options) =>
            options.Address = new Uri(provider.GetRequiredService<IOptions<MeshOptions>>().Value.Address))
            .AddInterceptor<WorkloadIdentityInterceptor>();
        services.AddSingleton(provider => new ClientRegistry(
            new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["acme.admissions.v1.AdmissionService"] = provider.GetRequiredService<ApiAdmissions.AdmissionService.AdmissionServiceClient>(),
                ["acme.analytics.v1.AnalyticsEventService"] = provider.GetRequiredService<ApiAnalytics.AnalyticsEventService.AnalyticsEventServiceClient>(),
                ["acme.anomalies.v1.AnomalyQueryService"] = provider.GetRequiredService<ApiAnomalies.AnomalyQueryService.AnomalyQueryServiceClient>(),
                ["acme.anomalies.v1.AnomalyService"] = provider.GetRequiredService<ApiAnomalies.AnomalyService.AnomalyServiceClient>(),
                ["acme.appointments.v1.AppointmentQueryService"] = provider.GetRequiredService<ApiAppointments.AppointmentQueryService.AppointmentQueryServiceClient>(),
                ["acme.appointments.v1.AppointmentService"] = provider.GetRequiredService<ApiAppointments.AppointmentService.AppointmentServiceClient>(),
                ["acme.assets.v1.FixedAssetCalculationService"] = provider.GetRequiredService<ApiAssets.FixedAssetCalculationService.FixedAssetCalculationServiceClient>(),
                ["acme.audiences.v1.AudienceQueryService"] = provider.GetRequiredService<ApiAudiences.AudienceQueryService.AudienceQueryServiceClient>(),
                ["acme.audiences.v1.AudienceService"] = provider.GetRequiredService<ApiAudiences.AudienceService.AudienceServiceClient>(),
                ["acme.audit.v1.AuditEventQueryService"] = provider.GetRequiredService<ApiAudit.AuditEventQueryService.AuditEventQueryServiceClient>(),
                ["acme.audit.v1.AuditEventService"] = provider.GetRequiredService<ApiAudit.AuditEventService.AuditEventServiceClient>(),
                ["acme.audit.v1.AuditEventValidationService"] = provider.GetRequiredService<ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient>(),
                ["acme.authorization.v1.AccessPolicyQueryService"] = provider.GetRequiredService<ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient>(),
                ["acme.authorization.v1.AccessPolicyService"] = provider.GetRequiredService<ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient>(),
                ["acme.authorization.v1.AccessPolicyValidationService"] = provider.GetRequiredService<ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient>(),
                ["acme.banking.v1.BankAccountQueryService"] = provider.GetRequiredService<ApiBanking.BankAccountQueryService.BankAccountQueryServiceClient>(),
                ["acme.banking.v1.BankAccountService"] = provider.GetRequiredService<ApiBanking.BankAccountService.BankAccountServiceClient>(),
                ["acme.benefits.v1.BenefitPlanApprovalService"] = provider.GetRequiredService<ApiBenefits.BenefitPlanApprovalService.BenefitPlanApprovalServiceClient>(),
                ["acme.benefits.v1.BenefitPlanQueryService"] = provider.GetRequiredService<ApiBenefits.BenefitPlanQueryService.BenefitPlanQueryServiceClient>(),
                ["acme.benefits.v1.BenefitPlanService"] = provider.GetRequiredService<ApiBenefits.BenefitPlanService.BenefitPlanServiceClient>(),
                ["acme.benefits.v1.BenefitPlanWorkflowService"] = provider.GetRequiredService<ApiBenefits.BenefitPlanWorkflowService.BenefitPlanWorkflowServiceClient>(),
                ["acme.benefits.v1.DependentApprovalService"] = provider.GetRequiredService<ApiBenefits.DependentApprovalService.DependentApprovalServiceClient>(),
                ["acme.benefits.v1.DependentValidationService"] = provider.GetRequiredService<ApiBenefits.DependentValidationService.DependentValidationServiceClient>(),
                ["acme.benefits.v1.DependentWorkflowService"] = provider.GetRequiredService<ApiBenefits.DependentWorkflowService.DependentWorkflowServiceClient>(),
                ["acme.benefits.v1.EnrollmentApprovalService"] = provider.GetRequiredService<ApiBenefits.EnrollmentApprovalService.EnrollmentApprovalServiceClient>(),
                ["acme.benefits.v1.EnrollmentQueryService"] = provider.GetRequiredService<ApiBenefits.EnrollmentQueryService.EnrollmentQueryServiceClient>(),
                ["acme.benefits.v1.EnrollmentWorkflowService"] = provider.GetRequiredService<ApiBenefits.EnrollmentWorkflowService.EnrollmentWorkflowServiceClient>(),
                ["acme.benefits.v1.LifeEventCalculationService"] = provider.GetRequiredService<ApiBenefits.LifeEventCalculationService.LifeEventCalculationServiceClient>(),
                ["acme.benefits.v1.LifeEventValidationService"] = provider.GetRequiredService<ApiBenefits.LifeEventValidationService.LifeEventValidationServiceClient>(),
                ["acme.boxoffice.v1.BoxOfficeSaleQueryService"] = provider.GetRequiredService<ApiBoxoffice.BoxOfficeSaleQueryService.BoxOfficeSaleQueryServiceClient>(),
                ["acme.boxoffice.v1.BoxOfficeSaleService"] = provider.GetRequiredService<ApiBoxoffice.BoxOfficeSaleService.BoxOfficeSaleServiceClient>(),
                ["acme.broadcasts.v1.BroadcastService"] = provider.GetRequiredService<ApiBroadcasts.BroadcastService.BroadcastServiceClient>(),
                ["acme.budgeting.v1.BudgetCalculationService"] = provider.GetRequiredService<ApiBudgeting.BudgetCalculationService.BudgetCalculationServiceClient>(),
                ["acme.budgeting.v1.BudgetQueryService"] = provider.GetRequiredService<ApiBudgeting.BudgetQueryService.BudgetQueryServiceClient>(),
                ["acme.budgeting.v1.BudgetService"] = provider.GetRequiredService<ApiBudgeting.BudgetService.BudgetServiceClient>(),
                ["acme.campaigns.v1.MarketingCampaignService"] = provider.GetRequiredService<ApiCampaigns.MarketingCampaignService.MarketingCampaignServiceClient>(),
                ["acme.careplans.v1.CarePlanCalculationService"] = provider.GetRequiredService<ApiCareplans.CarePlanCalculationService.CarePlanCalculationServiceClient>(),
                ["acme.careplans.v1.CarePlanQueryService"] = provider.GetRequiredService<ApiCareplans.CarePlanQueryService.CarePlanQueryServiceClient>(),
                ["acme.careplans.v1.CarePlanService"] = provider.GetRequiredService<ApiCareplans.CarePlanService.CarePlanServiceClient>(),
                ["acme.cart.v1.CartQueryService"] = provider.GetRequiredService<ApiCart.CartQueryService.CartQueryServiceClient>(),
                ["acme.cart.v1.CartService"] = provider.GetRequiredService<ApiCart.CartService.CartServiceClient>(),
                ["acme.catalog.v1.ProductQueryService"] = provider.GetRequiredService<ApiCatalog.ProductQueryService.ProductQueryServiceClient>(),
                ["acme.certificates.v1.CertificateCalculationService"] = provider.GetRequiredService<ApiCertificates.CertificateCalculationService.CertificateCalculationServiceClient>(),
                ["acme.certificates.v1.CertificateQueryService"] = provider.GetRequiredService<ApiCertificates.CertificateQueryService.CertificateQueryServiceClient>(),
                ["acme.certificates.v1.CertificateService"] = provider.GetRequiredService<ApiCertificates.CertificateService.CertificateServiceClient>(),
                ["acme.chat.v1.ChatRoomCalculationService"] = provider.GetRequiredService<ApiChat.ChatRoomCalculationService.ChatRoomCalculationServiceClient>(),
                ["acme.chat.v1.ChatRoomQueryService"] = provider.GetRequiredService<ApiChat.ChatRoomQueryService.ChatRoomQueryServiceClient>(),
                ["acme.chat.v1.ChatRoomService"] = provider.GetRequiredService<ApiChat.ChatRoomService.ChatRoomServiceClient>(),
                ["acme.checkout.v1.CheckoutSessionQueryService"] = provider.GetRequiredService<ApiCheckout.CheckoutSessionQueryService.CheckoutSessionQueryServiceClient>(),
                ["acme.checkout.v1.CheckoutSessionService"] = provider.GetRequiredService<ApiCheckout.CheckoutSessionService.CheckoutSessionServiceClient>(),
                ["acme.community.v1.ForumService"] = provider.GetRequiredService<ApiCommunity.ForumService.ForumServiceClient>(),
                ["acme.compliance.v1.ComplianceCaseQueryService"] = provider.GetRequiredService<ApiCompliance.ComplianceCaseQueryService.ComplianceCaseQueryServiceClient>(),
                ["acme.compliance.v1.ComplianceCaseService"] = provider.GetRequiredService<ApiCompliance.ComplianceCaseService.ComplianceCaseServiceClient>(),
                ["acme.concessions.v1.ConcessionStandCalculationService"] = provider.GetRequiredService<ApiConcessions.ConcessionStandCalculationService.ConcessionStandCalculationServiceClient>(),
                ["acme.concessions.v1.ConcessionStandQueryService"] = provider.GetRequiredService<ApiConcessions.ConcessionStandQueryService.ConcessionStandQueryServiceClient>(),
                ["acme.concessions.v1.ConcessionStandService"] = provider.GetRequiredService<ApiConcessions.ConcessionStandService.ConcessionStandServiceClient>(),
                ["acme.configuration.v1.ConfigSetCalculationService"] = provider.GetRequiredService<ApiConfiguration.ConfigSetCalculationService.ConfigSetCalculationServiceClient>(),
                ["acme.configuration.v1.ConfigSetQueryService"] = provider.GetRequiredService<ApiConfiguration.ConfigSetQueryService.ConfigSetQueryServiceClient>(),
                ["acme.configuration.v1.ConfigSetService"] = provider.GetRequiredService<ApiConfiguration.ConfigSetService.ConfigSetServiceClient>(),
                ["acme.content.v1.ArticleQueryService"] = provider.GetRequiredService<ApiContent.ArticleQueryService.ArticleQueryServiceClient>(),
                ["acme.content.v1.ArticleService"] = provider.GetRequiredService<ApiContent.ArticleService.ArticleServiceClient>(),
                ["acme.corporateevents.v1.CorporateEventApprovalService"] = provider.GetRequiredService<ApiCorporateevents.CorporateEventApprovalService.CorporateEventApprovalServiceClient>(),
                ["acme.corporateevents.v1.CorporateEventCalculationService"] = provider.GetRequiredService<ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient>(),
                ["acme.corporateevents.v1.CorporateEventLifecycleService"] = provider.GetRequiredService<ApiCorporateevents.CorporateEventLifecycleService.CorporateEventLifecycleServiceClient>(),
                ["acme.corporateevents.v1.CorporateEventQueryService"] = provider.GetRequiredService<ApiCorporateevents.CorporateEventQueryService.CorporateEventQueryServiceClient>(),
                ["acme.corporateevents.v1.CorporateEventReportingService"] = provider.GetRequiredService<ApiCorporateevents.CorporateEventReportingService.CorporateEventReportingServiceClient>(),
                ["acme.corporateevents.v1.CorporateEventService"] = provider.GetRequiredService<ApiCorporateevents.CorporateEventService.CorporateEventServiceClient>(),
                ["acme.corporateevents.v1.CorporateEventValidationService"] = provider.GetRequiredService<ApiCorporateevents.CorporateEventValidationService.CorporateEventValidationServiceClient>(),
                ["acme.corporateevents.v1.CorporateEventWorkflowService"] = provider.GetRequiredService<ApiCorporateevents.CorporateEventWorkflowService.CorporateEventWorkflowServiceClient>(),
                ["acme.corporateevents.v1.RegistrationApprovalService"] = provider.GetRequiredService<ApiCorporateevents.RegistrationApprovalService.RegistrationApprovalServiceClient>(),
                ["acme.corporateevents.v1.RegistrationCalculationService"] = provider.GetRequiredService<ApiCorporateevents.RegistrationCalculationService.RegistrationCalculationServiceClient>(),
                ["acme.corporateevents.v1.RegistrationLifecycleService"] = provider.GetRequiredService<ApiCorporateevents.RegistrationLifecycleService.RegistrationLifecycleServiceClient>(),
                ["acme.corporateevents.v1.RegistrationQueryService"] = provider.GetRequiredService<ApiCorporateevents.RegistrationQueryService.RegistrationQueryServiceClient>(),
                ["acme.corporateevents.v1.RegistrationReportingService"] = provider.GetRequiredService<ApiCorporateevents.RegistrationReportingService.RegistrationReportingServiceClient>(),
                ["acme.corporateevents.v1.RegistrationValidationService"] = provider.GetRequiredService<ApiCorporateevents.RegistrationValidationService.RegistrationValidationServiceClient>(),
                ["acme.corporateevents.v1.RegistrationWorkflowService"] = provider.GetRequiredService<ApiCorporateevents.RegistrationWorkflowService.RegistrationWorkflowServiceClient>(),
                ["acme.corporateevents.v1.VenueApprovalService"] = provider.GetRequiredService<ApiCorporateevents.VenueApprovalService.VenueApprovalServiceClient>(),
                ["acme.corporateevents.v1.VenueCalculationService"] = provider.GetRequiredService<ApiCorporateevents.VenueCalculationService.VenueCalculationServiceClient>(),
                ["acme.corporateevents.v1.VenueLifecycleService"] = provider.GetRequiredService<ApiCorporateevents.VenueLifecycleService.VenueLifecycleServiceClient>(),
                ["acme.corporateevents.v1.VenueReportingService"] = provider.GetRequiredService<ApiCorporateevents.VenueReportingService.VenueReportingServiceClient>(),
                ["acme.corporateevents.v1.VenueSyncService"] = provider.GetRequiredService<ApiCorporateevents.VenueSyncService.VenueSyncServiceClient>(),
                ["acme.corporateevents.v1.VenueValidationService"] = provider.GetRequiredService<ApiCorporateevents.VenueValidationService.VenueValidationServiceClient>(),
                ["acme.corporateevents.v1.VenueWorkflowService"] = provider.GetRequiredService<ApiCorporateevents.VenueWorkflowService.VenueWorkflowServiceClient>(),
                ["acme.customers.v1.CustomerCalculationService"] = provider.GetRequiredService<ApiCustomers.CustomerCalculationService.CustomerCalculationServiceClient>(),
                ["acme.customers.v1.CustomerService"] = provider.GetRequiredService<ApiCustomers.CustomerService.CustomerServiceClient>(),
                ["acme.datahub.v1.WorkspaceQueryService"] = provider.GetRequiredService<ApiDatahub.WorkspaceQueryService.WorkspaceQueryServiceClient>(),
                ["acme.datasets.v1.DatasetQueryService"] = provider.GetRequiredService<ApiDatasets.DatasetQueryService.DatasetQueryServiceClient>(),
                ["acme.datasets.v1.DatasetService"] = provider.GetRequiredService<ApiDatasets.DatasetService.DatasetServiceClient>(),
                ["acme.deliveries.v1.DeliveryQueryService"] = provider.GetRequiredService<ApiDeliveries.DeliveryQueryService.DeliveryQueryServiceClient>(),
                ["acme.deliveries.v1.DeliveryService"] = provider.GetRequiredService<ApiDeliveries.DeliveryService.DeliveryServiceClient>(),
                ["acme.devices.v1.ComplianceScanApprovalService"] = provider.GetRequiredService<ApiDevices.ComplianceScanApprovalService.ComplianceScanApprovalServiceClient>(),
                ["acme.devices.v1.ComplianceScanCalculationService"] = provider.GetRequiredService<ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient>(),
                ["acme.devices.v1.ComplianceScanLifecycleService"] = provider.GetRequiredService<ApiDevices.ComplianceScanLifecycleService.ComplianceScanLifecycleServiceClient>(),
                ["acme.devices.v1.ComplianceScanValidationService"] = provider.GetRequiredService<ApiDevices.ComplianceScanValidationService.ComplianceScanValidationServiceClient>(),
                ["acme.devices.v1.ComplianceScanWorkflowService"] = provider.GetRequiredService<ApiDevices.ComplianceScanWorkflowService.ComplianceScanWorkflowServiceClient>(),
                ["acme.devices.v1.DevicePolicyApprovalService"] = provider.GetRequiredService<ApiDevices.DevicePolicyApprovalService.DevicePolicyApprovalServiceClient>(),
                ["acme.devices.v1.DevicePolicyCalculationService"] = provider.GetRequiredService<ApiDevices.DevicePolicyCalculationService.DevicePolicyCalculationServiceClient>(),
                ["acme.devices.v1.DevicePolicyLifecycleService"] = provider.GetRequiredService<ApiDevices.DevicePolicyLifecycleService.DevicePolicyLifecycleServiceClient>(),
                ["acme.devices.v1.DevicePolicyQueryService"] = provider.GetRequiredService<ApiDevices.DevicePolicyQueryService.DevicePolicyQueryServiceClient>(),
                ["acme.devices.v1.DevicePolicyValidationService"] = provider.GetRequiredService<ApiDevices.DevicePolicyValidationService.DevicePolicyValidationServiceClient>(),
                ["acme.devices.v1.DevicePolicyWorkflowService"] = provider.GetRequiredService<ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceClient>(),
                ["acme.devices.v1.ManagedDeviceApprovalService"] = provider.GetRequiredService<ApiDevices.ManagedDeviceApprovalService.ManagedDeviceApprovalServiceClient>(),
                ["acme.devices.v1.ManagedDeviceCalculationService"] = provider.GetRequiredService<ApiDevices.ManagedDeviceCalculationService.ManagedDeviceCalculationServiceClient>(),
                ["acme.devices.v1.ManagedDeviceQueryService"] = provider.GetRequiredService<ApiDevices.ManagedDeviceQueryService.ManagedDeviceQueryServiceClient>(),
                ["acme.devices.v1.ManagedDeviceService"] = provider.GetRequiredService<ApiDevices.ManagedDeviceService.ManagedDeviceServiceClient>(),
                ["acme.devices.v1.ManagedDeviceValidationService"] = provider.GetRequiredService<ApiDevices.ManagedDeviceValidationService.ManagedDeviceValidationServiceClient>(),
                ["acme.devices.v1.ManagedDeviceWorkflowService"] = provider.GetRequiredService<ApiDevices.ManagedDeviceWorkflowService.ManagedDeviceWorkflowServiceClient>(),
                ["acme.dispatch.v1.DispatchOrderQueryService"] = provider.GetRequiredService<ApiDispatch.DispatchOrderQueryService.DispatchOrderQueryServiceClient>(),
                ["acme.dispatch.v1.DispatchOrderService"] = provider.GetRequiredService<ApiDispatch.DispatchOrderService.DispatchOrderServiceClient>(),
                ["acme.documents.v1.DocumentAdminService"] = provider.GetRequiredService<ApiDocuments.DocumentAdminService.DocumentAdminServiceClient>(),
                ["acme.documents.v1.DocumentExportService"] = provider.GetRequiredService<ApiDocuments.DocumentExportService.DocumentExportServiceClient>(),
                ["acme.documents.v1.DocumentFolderExportService"] = provider.GetRequiredService<ApiDocuments.DocumentFolderExportService.DocumentFolderExportServiceClient>(),
                ["acme.documents.v1.DocumentFolderHistoryService"] = provider.GetRequiredService<ApiDocuments.DocumentFolderHistoryService.DocumentFolderHistoryServiceClient>(),
                ["acme.documents.v1.DocumentFolderNotificationService"] = provider.GetRequiredService<ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient>(),
                ["acme.documents.v1.DocumentFolderQueryService"] = provider.GetRequiredService<ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient>(),
                ["acme.documents.v1.DocumentFolderSyncService"] = provider.GetRequiredService<ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient>(),
                ["acme.documents.v1.DocumentFolderValidationService"] = provider.GetRequiredService<ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient>(),
                ["acme.documents.v1.DocumentHistoryService"] = provider.GetRequiredService<ApiDocuments.DocumentHistoryService.DocumentHistoryServiceClient>(),
                ["acme.documents.v1.DocumentNotificationService"] = provider.GetRequiredService<ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient>(),
                ["acme.documents.v1.DocumentQueryService"] = provider.GetRequiredService<ApiDocuments.DocumentQueryService.DocumentQueryServiceClient>(),
                ["acme.documents.v1.DocumentService"] = provider.GetRequiredService<ApiDocuments.DocumentService.DocumentServiceClient>(),
                ["acme.documents.v1.DocumentSyncService"] = provider.GetRequiredService<ApiDocuments.DocumentSyncService.DocumentSyncServiceClient>(),
                ["acme.documents.v1.DocumentTemplateAdminService"] = provider.GetRequiredService<ApiDocuments.DocumentTemplateAdminService.DocumentTemplateAdminServiceClient>(),
                ["acme.documents.v1.DocumentTemplateCalculationService"] = provider.GetRequiredService<ApiDocuments.DocumentTemplateCalculationService.DocumentTemplateCalculationServiceClient>(),
                ["acme.documents.v1.DocumentTemplateExportService"] = provider.GetRequiredService<ApiDocuments.DocumentTemplateExportService.DocumentTemplateExportServiceClient>(),
                ["acme.documents.v1.DocumentTemplateHistoryService"] = provider.GetRequiredService<ApiDocuments.DocumentTemplateHistoryService.DocumentTemplateHistoryServiceClient>(),
                ["acme.documents.v1.DocumentTemplateNotificationService"] = provider.GetRequiredService<ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient>(),
                ["acme.documents.v1.DocumentTemplateSyncService"] = provider.GetRequiredService<ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient>(),
                ["acme.documents.v1.DocumentTemplateWorkflowService"] = provider.GetRequiredService<ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceClient>(),
                ["acme.documents.v1.DocumentValidationService"] = provider.GetRequiredService<ApiDocuments.DocumentValidationService.DocumentValidationServiceClient>(),
                ["acme.documents.v1.SignatureAdminService"] = provider.GetRequiredService<ApiDocuments.SignatureAdminService.SignatureAdminServiceClient>(),
                ["acme.documents.v1.SignatureCalculationService"] = provider.GetRequiredService<ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient>(),
                ["acme.documents.v1.SignatureExportService"] = provider.GetRequiredService<ApiDocuments.SignatureExportService.SignatureExportServiceClient>(),
                ["acme.documents.v1.SignatureHistoryService"] = provider.GetRequiredService<ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient>(),
                ["acme.documents.v1.SignatureNotificationService"] = provider.GetRequiredService<ApiDocuments.SignatureNotificationService.SignatureNotificationServiceClient>(),
                ["acme.documents.v1.SignatureSyncService"] = provider.GetRequiredService<ApiDocuments.SignatureSyncService.SignatureSyncServiceClient>(),
                ["acme.documents.v1.SignatureValidationService"] = provider.GetRequiredService<ApiDocuments.SignatureValidationService.SignatureValidationServiceClient>(),
                ["acme.driverapp.v1.DriverHomeService"] = provider.GetRequiredService<ApiDriverapp.DriverHomeService.DriverHomeServiceClient>(),
                ["acme.drivers.v1.DriverCalculationService"] = provider.GetRequiredService<ApiDrivers.DriverCalculationService.DriverCalculationServiceClient>(),
                ["acme.drivers.v1.DriverService"] = provider.GetRequiredService<ApiDrivers.DriverService.DriverServiceClient>(),
                ["acme.edgegateway.v1.RouteBindingQueryService"] = provider.GetRequiredService<ApiEdgegateway.RouteBindingQueryService.RouteBindingQueryServiceClient>(),
                ["acme.edgegateway.v1.RouteBindingService"] = provider.GetRequiredService<ApiEdgegateway.RouteBindingService.RouteBindingServiceClient>(),
                ["acme.employeeportal.v1.AnnouncementIntegrationService"] = provider.GetRequiredService<ApiEmployeeportal.AnnouncementIntegrationService.AnnouncementIntegrationServiceClient>(),
                ["acme.employeeportal.v1.AnnouncementNotificationService"] = provider.GetRequiredService<ApiEmployeeportal.AnnouncementNotificationService.AnnouncementNotificationServiceClient>(),
                ["acme.employeeportal.v1.AnnouncementWorkflowService"] = provider.GetRequiredService<ApiEmployeeportal.AnnouncementWorkflowService.AnnouncementWorkflowServiceClient>(),
                ["acme.employeeportal.v1.PortalHomeIntegrationService"] = provider.GetRequiredService<ApiEmployeeportal.PortalHomeIntegrationService.PortalHomeIntegrationServiceClient>(),
                ["acme.employeeportal.v1.PortalHomeNotificationService"] = provider.GetRequiredService<ApiEmployeeportal.PortalHomeNotificationService.PortalHomeNotificationServiceClient>(),
                ["acme.employeeportal.v1.PortalHomeQueryService"] = provider.GetRequiredService<ApiEmployeeportal.PortalHomeQueryService.PortalHomeQueryServiceClient>(),
                ["acme.employeeportal.v1.PortalHomeService"] = provider.GetRequiredService<ApiEmployeeportal.PortalHomeService.PortalHomeServiceClient>(),
                ["acme.employeeportal.v1.SelfServiceRequestIntegrationService"] = provider.GetRequiredService<ApiEmployeeportal.SelfServiceRequestIntegrationService.SelfServiceRequestIntegrationServiceClient>(),
                ["acme.employeeportal.v1.SelfServiceRequestNotificationService"] = provider.GetRequiredService<ApiEmployeeportal.SelfServiceRequestNotificationService.SelfServiceRequestNotificationServiceClient>(),
                ["acme.employeeportal.v1.SelfServiceRequestQueryService"] = provider.GetRequiredService<ApiEmployeeportal.SelfServiceRequestQueryService.SelfServiceRequestQueryServiceClient>(),
                ["acme.employees.v1.EmployeeApprovalService"] = provider.GetRequiredService<ApiEmployees.EmployeeApprovalService.EmployeeApprovalServiceClient>(),
                ["acme.employees.v1.EmployeeQueryService"] = provider.GetRequiredService<ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient>(),
                ["acme.employees.v1.EmployeeService"] = provider.GetRequiredService<ApiEmployees.EmployeeService.EmployeeServiceClient>(),
                ["acme.employees.v1.EmployeeWorkflowService"] = provider.GetRequiredService<ApiEmployees.EmployeeWorkflowService.EmployeeWorkflowServiceClient>(),
                ["acme.employees.v1.EmploymentContractApprovalService"] = provider.GetRequiredService<ApiEmployees.EmploymentContractApprovalService.EmploymentContractApprovalServiceClient>(),
                ["acme.employees.v1.EmploymentContractCalculationService"] = provider.GetRequiredService<ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient>(),
                ["acme.employees.v1.EmploymentContractValidationService"] = provider.GetRequiredService<ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient>(),
                ["acme.employees.v1.OrgUnitApprovalService"] = provider.GetRequiredService<ApiEmployees.OrgUnitApprovalService.OrgUnitApprovalServiceClient>(),
                ["acme.employees.v1.OrgUnitQueryService"] = provider.GetRequiredService<ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient>(),
                ["acme.employees.v1.OrgUnitWorkflowService"] = provider.GetRequiredService<ApiEmployees.OrgUnitWorkflowService.OrgUnitWorkflowServiceClient>(),
                ["acme.employees.v1.PositionApprovalService"] = provider.GetRequiredService<ApiEmployees.PositionApprovalService.PositionApprovalServiceClient>(),
                ["acme.employees.v1.PositionValidationService"] = provider.GetRequiredService<ApiEmployees.PositionValidationService.PositionValidationServiceClient>(),
                ["acme.employees.v1.PositionWorkflowService"] = provider.GetRequiredService<ApiEmployees.PositionWorkflowService.PositionWorkflowServiceClient>(),
                ["acme.experiments.v1.ExperimentCalculationService"] = provider.GetRequiredService<ApiExperiments.ExperimentCalculationService.ExperimentCalculationServiceClient>(),
                ["acme.experiments.v1.ExperimentQueryService"] = provider.GetRequiredService<ApiExperiments.ExperimentQueryService.ExperimentQueryServiceClient>(),
                ["acme.experiments.v1.ExperimentService"] = provider.GetRequiredService<ApiExperiments.ExperimentService.ExperimentServiceClient>(),
                ["acme.fanportal.v1.FanSessionQueryService"] = provider.GetRequiredService<ApiFanportal.FanSessionQueryService.FanSessionQueryServiceClient>(),
                ["acme.fanportal.v1.FanSessionService"] = provider.GetRequiredService<ApiFanportal.FanSessionService.FanSessionServiceClient>(),
                ["acme.featureflags.v1.FeatureFlagQueryService"] = provider.GetRequiredService<ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient>(),
                ["acme.featureflags.v1.FeatureFlagService"] = provider.GetRequiredService<ApiFeatureflags.FeatureFlagService.FeatureFlagServiceClient>(),
                ["acme.featureflags.v1.FeatureFlagValidationService"] = provider.GetRequiredService<ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient>(),
                ["acme.featurestore.v1.FeatureViewCalculationService"] = provider.GetRequiredService<ApiFeaturestore.FeatureViewCalculationService.FeatureViewCalculationServiceClient>(),
                ["acme.featurestore.v1.FeatureViewQueryService"] = provider.GetRequiredService<ApiFeaturestore.FeatureViewQueryService.FeatureViewQueryServiceClient>(),
                ["acme.fleet.v1.VehicleQueryService"] = provider.GetRequiredService<ApiFleet.VehicleQueryService.VehicleQueryServiceClient>(),
                ["acme.fleet.v1.VehicleService"] = provider.GetRequiredService<ApiFleet.VehicleService.VehicleServiceClient>(),
                ["acme.forecasting.v1.DemandForecastQueryService"] = provider.GetRequiredService<ApiForecasting.DemandForecastQueryService.DemandForecastQueryServiceClient>(),
                ["acme.forecasting.v1.DemandForecastService"] = provider.GetRequiredService<ApiForecasting.DemandForecastService.DemandForecastServiceClient>(),
                ["acme.forms.v1.FormCalculationService"] = provider.GetRequiredService<ApiForms.FormCalculationService.FormCalculationServiceClient>(),
                ["acme.fraud.v1.RiskAssessmentCalculationService"] = provider.GetRequiredService<ApiFraud.RiskAssessmentCalculationService.RiskAssessmentCalculationServiceClient>(),
                ["acme.fraud.v1.RiskAssessmentQueryService"] = provider.GetRequiredService<ApiFraud.RiskAssessmentQueryService.RiskAssessmentQueryServiceClient>(),
                ["acme.fraud.v1.RiskAssessmentService"] = provider.GetRequiredService<ApiFraud.RiskAssessmentService.RiskAssessmentServiceClient>(),
                ["acme.fuel.v1.FuelCardCalculationService"] = provider.GetRequiredService<ApiFuel.FuelCardCalculationService.FuelCardCalculationServiceClient>(),
                ["acme.fuel.v1.FuelCardQueryService"] = provider.GetRequiredService<ApiFuel.FuelCardQueryService.FuelCardQueryServiceClient>(),
                ["acme.fuel.v1.FuelCardService"] = provider.GetRequiredService<ApiFuel.FuelCardService.FuelCardServiceClient>(),
                ["acme.giftcards.v1.GiftCardQueryService"] = provider.GetRequiredService<ApiGiftcards.GiftCardQueryService.GiftCardQueryServiceClient>(),
                ["acme.giftcards.v1.GiftCardService"] = provider.GetRequiredService<ApiGiftcards.GiftCardService.GiftCardServiceClient>(),
                ["acme.helpdesk.v1.EscalationApprovalService"] = provider.GetRequiredService<ApiHelpdesk.EscalationApprovalService.EscalationApprovalServiceClient>(),
                ["acme.helpdesk.v1.EscalationCalculationService"] = provider.GetRequiredService<ApiHelpdesk.EscalationCalculationService.EscalationCalculationServiceClient>(),
                ["acme.helpdesk.v1.EscalationLifecycleService"] = provider.GetRequiredService<ApiHelpdesk.EscalationLifecycleService.EscalationLifecycleServiceClient>(),
                ["acme.helpdesk.v1.EscalationReportingService"] = provider.GetRequiredService<ApiHelpdesk.EscalationReportingService.EscalationReportingServiceClient>(),
                ["acme.helpdesk.v1.EscalationValidationService"] = provider.GetRequiredService<ApiHelpdesk.EscalationValidationService.EscalationValidationServiceClient>(),
                ["acme.helpdesk.v1.KnowledgeArticleApprovalService"] = provider.GetRequiredService<ApiHelpdesk.KnowledgeArticleApprovalService.KnowledgeArticleApprovalServiceClient>(),
                ["acme.helpdesk.v1.KnowledgeArticleCalculationService"] = provider.GetRequiredService<ApiHelpdesk.KnowledgeArticleCalculationService.KnowledgeArticleCalculationServiceClient>(),
                ["acme.helpdesk.v1.KnowledgeArticleLifecycleService"] = provider.GetRequiredService<ApiHelpdesk.KnowledgeArticleLifecycleService.KnowledgeArticleLifecycleServiceClient>(),
                ["acme.helpdesk.v1.KnowledgeArticleQueryService"] = provider.GetRequiredService<ApiHelpdesk.KnowledgeArticleQueryService.KnowledgeArticleQueryServiceClient>(),
                ["acme.helpdesk.v1.KnowledgeArticleValidationService"] = provider.GetRequiredService<ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient>(),
                ["acme.helpdesk.v1.KnowledgeArticleWorkflowService"] = provider.GetRequiredService<ApiHelpdesk.KnowledgeArticleWorkflowService.KnowledgeArticleWorkflowServiceClient>(),
                ["acme.helpdesk.v1.SlaPolicyApprovalService"] = provider.GetRequiredService<ApiHelpdesk.SlaPolicyApprovalService.SlaPolicyApprovalServiceClient>(),
                ["acme.helpdesk.v1.SlaPolicyCalculationService"] = provider.GetRequiredService<ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient>(),
                ["acme.helpdesk.v1.SlaPolicyLifecycleService"] = provider.GetRequiredService<ApiHelpdesk.SlaPolicyLifecycleService.SlaPolicyLifecycleServiceClient>(),
                ["acme.helpdesk.v1.SlaPolicyValidationService"] = provider.GetRequiredService<ApiHelpdesk.SlaPolicyValidationService.SlaPolicyValidationServiceClient>(),
                ["acme.helpdesk.v1.SlaPolicyWorkflowService"] = provider.GetRequiredService<ApiHelpdesk.SlaPolicyWorkflowService.SlaPolicyWorkflowServiceClient>(),
                ["acme.helpdesk.v1.TicketApprovalService"] = provider.GetRequiredService<ApiHelpdesk.TicketApprovalService.TicketApprovalServiceClient>(),
                ["acme.helpdesk.v1.TicketCalculationService"] = provider.GetRequiredService<ApiHelpdesk.TicketCalculationService.TicketCalculationServiceClient>(),
                ["acme.helpdesk.v1.TicketQueryService"] = provider.GetRequiredService<ApiHelpdesk.TicketQueryService.TicketQueryServiceClient>(),
                ["acme.helpdesk.v1.TicketService"] = provider.GetRequiredService<ApiHelpdesk.TicketService.TicketServiceClient>(),
                ["acme.helpdesk.v1.TicketValidationService"] = provider.GetRequiredService<ApiHelpdesk.TicketValidationService.TicketValidationServiceClient>(),
                ["acme.helpdesk.v1.TicketWorkflowService"] = provider.GetRequiredService<ApiHelpdesk.TicketWorkflowService.TicketWorkflowServiceClient>(),
                ["acme.identity.v1.UserQueryService"] = provider.GetRequiredService<ApiIdentity.UserQueryService.UserQueryServiceClient>(),
                ["acme.identity.v1.UserService"] = provider.GetRequiredService<ApiIdentity.UserService.UserServiceClient>(),
                ["acme.identity.v1.UserValidationService"] = provider.GetRequiredService<ApiIdentity.UserValidationService.UserValidationServiceClient>(),
                ["acme.insurance.v1.PolicyCalculationService"] = provider.GetRequiredService<ApiInsurance.PolicyCalculationService.PolicyCalculationServiceClient>(),
                ["acme.insurance.v1.PolicyQueryService"] = provider.GetRequiredService<ApiInsurance.PolicyQueryService.PolicyQueryServiceClient>(),
                ["acme.insurance.v1.PolicyService"] = provider.GetRequiredService<ApiInsurance.PolicyService.PolicyServiceClient>(),
                ["acme.invoicing.v1.InvoiceBatchCalculationService"] = provider.GetRequiredService<ApiInvoicing.InvoiceBatchCalculationService.InvoiceBatchCalculationServiceClient>(),
                ["acme.invoicing.v1.InvoiceBatchService"] = provider.GetRequiredService<ApiInvoicing.InvoiceBatchService.InvoiceBatchServiceClient>(),
                ["acme.itsm.v1.ChangeRequestApprovalService"] = provider.GetRequiredService<ApiItsm.ChangeRequestApprovalService.ChangeRequestApprovalServiceClient>(),
                ["acme.itsm.v1.ChangeRequestCalculationService"] = provider.GetRequiredService<ApiItsm.ChangeRequestCalculationService.ChangeRequestCalculationServiceClient>(),
                ["acme.itsm.v1.ChangeRequestLifecycleService"] = provider.GetRequiredService<ApiItsm.ChangeRequestLifecycleService.ChangeRequestLifecycleServiceClient>(),
                ["acme.itsm.v1.ChangeRequestQueryService"] = provider.GetRequiredService<ApiItsm.ChangeRequestQueryService.ChangeRequestQueryServiceClient>(),
                ["acme.itsm.v1.ChangeRequestReportingService"] = provider.GetRequiredService<ApiItsm.ChangeRequestReportingService.ChangeRequestReportingServiceClient>(),
                ["acme.itsm.v1.ChangeRequestValidationService"] = provider.GetRequiredService<ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient>(),
                ["acme.itsm.v1.ChangeRequestWorkflowService"] = provider.GetRequiredService<ApiItsm.ChangeRequestWorkflowService.ChangeRequestWorkflowServiceClient>(),
                ["acme.itsm.v1.IncidentApprovalService"] = provider.GetRequiredService<ApiItsm.IncidentApprovalService.IncidentApprovalServiceClient>(),
                ["acme.itsm.v1.IncidentCalculationService"] = provider.GetRequiredService<ApiItsm.IncidentCalculationService.IncidentCalculationServiceClient>(),
                ["acme.itsm.v1.IncidentLifecycleService"] = provider.GetRequiredService<ApiItsm.IncidentLifecycleService.IncidentLifecycleServiceClient>(),
                ["acme.itsm.v1.IncidentQueryService"] = provider.GetRequiredService<ApiItsm.IncidentQueryService.IncidentQueryServiceClient>(),
                ["acme.itsm.v1.IncidentService"] = provider.GetRequiredService<ApiItsm.IncidentService.IncidentServiceClient>(),
                ["acme.itsm.v1.IncidentValidationService"] = provider.GetRequiredService<ApiItsm.IncidentValidationService.IncidentValidationServiceClient>(),
                ["acme.itsm.v1.IncidentWorkflowService"] = provider.GetRequiredService<ApiItsm.IncidentWorkflowService.IncidentWorkflowServiceClient>(),
                ["acme.itsm.v1.ProblemApprovalService"] = provider.GetRequiredService<ApiItsm.ProblemApprovalService.ProblemApprovalServiceClient>(),
                ["acme.itsm.v1.ProblemCalculationService"] = provider.GetRequiredService<ApiItsm.ProblemCalculationService.ProblemCalculationServiceClient>(),
                ["acme.itsm.v1.ProblemLifecycleService"] = provider.GetRequiredService<ApiItsm.ProblemLifecycleService.ProblemLifecycleServiceClient>(),
                ["acme.itsm.v1.ProblemReportingService"] = provider.GetRequiredService<ApiItsm.ProblemReportingService.ProblemReportingServiceClient>(),
                ["acme.itsm.v1.ProblemSyncService"] = provider.GetRequiredService<ApiItsm.ProblemSyncService.ProblemSyncServiceClient>(),
                ["acme.itsm.v1.ProblemValidationService"] = provider.GetRequiredService<ApiItsm.ProblemValidationService.ProblemValidationServiceClient>(),
                ["acme.itsm.v1.ServiceAssetApprovalService"] = provider.GetRequiredService<ApiItsm.ServiceAssetApprovalService.ServiceAssetApprovalServiceClient>(),
                ["acme.itsm.v1.ServiceAssetCalculationService"] = provider.GetRequiredService<ApiItsm.ServiceAssetCalculationService.ServiceAssetCalculationServiceClient>(),
                ["acme.itsm.v1.ServiceAssetLifecycleService"] = provider.GetRequiredService<ApiItsm.ServiceAssetLifecycleService.ServiceAssetLifecycleServiceClient>(),
                ["acme.itsm.v1.ServiceAssetReportingService"] = provider.GetRequiredService<ApiItsm.ServiceAssetReportingService.ServiceAssetReportingServiceClient>(),
                ["acme.itsm.v1.ServiceAssetValidationService"] = provider.GetRequiredService<ApiItsm.ServiceAssetValidationService.ServiceAssetValidationServiceClient>(),
                ["acme.itsm.v1.ServiceAssetWorkflowService"] = provider.GetRequiredService<ApiItsm.ServiceAssetWorkflowService.ServiceAssetWorkflowServiceClient>(),
                ["acme.keymanagement.v1.CryptoKeyCalculationService"] = provider.GetRequiredService<ApiKeymanagement.CryptoKeyCalculationService.CryptoKeyCalculationServiceClient>(),
                ["acme.keymanagement.v1.CryptoKeyQueryService"] = provider.GetRequiredService<ApiKeymanagement.CryptoKeyQueryService.CryptoKeyQueryServiceClient>(),
                ["acme.labeling.v1.LabelingTaskCalculationService"] = provider.GetRequiredService<ApiLabeling.LabelingTaskCalculationService.LabelingTaskCalculationServiceClient>(),
                ["acme.labeling.v1.LabelingTaskQueryService"] = provider.GetRequiredService<ApiLabeling.LabelingTaskQueryService.LabelingTaskQueryServiceClient>(),
                ["acme.labeling.v1.LabelingTaskService"] = provider.GetRequiredService<ApiLabeling.LabelingTaskService.LabelingTaskServiceClient>(),
                ["acme.learning.v1.CertificationApprovalService"] = provider.GetRequiredService<ApiLearning.CertificationApprovalService.CertificationApprovalServiceClient>(),
                ["acme.learning.v1.CertificationCalculationService"] = provider.GetRequiredService<ApiLearning.CertificationCalculationService.CertificationCalculationServiceClient>(),
                ["acme.learning.v1.CertificationValidationService"] = provider.GetRequiredService<ApiLearning.CertificationValidationService.CertificationValidationServiceClient>(),
                ["acme.learning.v1.CertificationWorkflowService"] = provider.GetRequiredService<ApiLearning.CertificationWorkflowService.CertificationWorkflowServiceClient>(),
                ["acme.learning.v1.CourseApprovalService"] = provider.GetRequiredService<ApiLearning.CourseApprovalService.CourseApprovalServiceClient>(),
                ["acme.learning.v1.CourseEnrollmentApprovalService"] = provider.GetRequiredService<ApiLearning.CourseEnrollmentApprovalService.CourseEnrollmentApprovalServiceClient>(),
                ["acme.learning.v1.CourseEnrollmentCalculationService"] = provider.GetRequiredService<ApiLearning.CourseEnrollmentCalculationService.CourseEnrollmentCalculationServiceClient>(),
                ["acme.learning.v1.CourseEnrollmentValidationService"] = provider.GetRequiredService<ApiLearning.CourseEnrollmentValidationService.CourseEnrollmentValidationServiceClient>(),
                ["acme.learning.v1.CourseQueryService"] = provider.GetRequiredService<ApiLearning.CourseQueryService.CourseQueryServiceClient>(),
                ["acme.learning.v1.CourseService"] = provider.GetRequiredService<ApiLearning.CourseService.CourseServiceClient>(),
                ["acme.learning.v1.CourseValidationService"] = provider.GetRequiredService<ApiLearning.CourseValidationService.CourseValidationServiceClient>(),
                ["acme.learning.v1.CourseWorkflowService"] = provider.GetRequiredService<ApiLearning.CourseWorkflowService.CourseWorkflowServiceClient>(),
                ["acme.learning.v1.LearningPathApprovalService"] = provider.GetRequiredService<ApiLearning.LearningPathApprovalService.LearningPathApprovalServiceClient>(),
                ["acme.learning.v1.LearningPathCalculationService"] = provider.GetRequiredService<ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient>(),
                ["acme.learning.v1.LearningPathQueryService"] = provider.GetRequiredService<ApiLearning.LearningPathQueryService.LearningPathQueryServiceClient>(),
                ["acme.learning.v1.LearningPathValidationService"] = provider.GetRequiredService<ApiLearning.LearningPathValidationService.LearningPathValidationServiceClient>(),
                ["acme.learning.v1.LearningPathWorkflowService"] = provider.GetRequiredService<ApiLearning.LearningPathWorkflowService.LearningPathWorkflowServiceClient>(),
                ["acme.ledger.v1.LedgerAccountQueryService"] = provider.GetRequiredService<ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient>(),
                ["acme.ledger.v1.LedgerAccountService"] = provider.GetRequiredService<ApiLedger.LedgerAccountService.LedgerAccountServiceClient>(),
                ["acme.ledger.v1.LedgerAccountValidationService"] = provider.GetRequiredService<ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient>(),
                ["acme.lending.v1.LoanApplicationCalculationService"] = provider.GetRequiredService<ApiLending.LoanApplicationCalculationService.LoanApplicationCalculationServiceClient>(),
                ["acme.licensing.v1.EntitlementApprovalService"] = provider.GetRequiredService<ApiLicensing.EntitlementApprovalService.EntitlementApprovalServiceClient>(),
                ["acme.licensing.v1.EntitlementCalculationService"] = provider.GetRequiredService<ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient>(),
                ["acme.licensing.v1.EntitlementLifecycleService"] = provider.GetRequiredService<ApiLicensing.EntitlementLifecycleService.EntitlementLifecycleServiceClient>(),
                ["acme.licensing.v1.EntitlementQueryService"] = provider.GetRequiredService<ApiLicensing.EntitlementQueryService.EntitlementQueryServiceClient>(),
                ["acme.licensing.v1.EntitlementValidationService"] = provider.GetRequiredService<ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient>(),
                ["acme.licensing.v1.EntitlementWorkflowService"] = provider.GetRequiredService<ApiLicensing.EntitlementWorkflowService.EntitlementWorkflowServiceClient>(),
                ["acme.licensing.v1.LicenseAssignmentApprovalService"] = provider.GetRequiredService<ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient>(),
                ["acme.licensing.v1.LicenseAssignmentCalculationService"] = provider.GetRequiredService<ApiLicensing.LicenseAssignmentCalculationService.LicenseAssignmentCalculationServiceClient>(),
                ["acme.licensing.v1.LicenseAssignmentLifecycleService"] = provider.GetRequiredService<ApiLicensing.LicenseAssignmentLifecycleService.LicenseAssignmentLifecycleServiceClient>(),
                ["acme.licensing.v1.LicenseAssignmentReportingService"] = provider.GetRequiredService<ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient>(),
                ["acme.licensing.v1.LicenseAssignmentValidationService"] = provider.GetRequiredService<ApiLicensing.LicenseAssignmentValidationService.LicenseAssignmentValidationServiceClient>(),
                ["acme.licensing.v1.LicenseAssignmentWorkflowService"] = provider.GetRequiredService<ApiLicensing.LicenseAssignmentWorkflowService.LicenseAssignmentWorkflowServiceClient>(),
                ["acme.licensing.v1.SoftwareLicenseApprovalService"] = provider.GetRequiredService<ApiLicensing.SoftwareLicenseApprovalService.SoftwareLicenseApprovalServiceClient>(),
                ["acme.licensing.v1.SoftwareLicenseCalculationService"] = provider.GetRequiredService<ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient>(),
                ["acme.licensing.v1.SoftwareLicenseQueryService"] = provider.GetRequiredService<ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient>(),
                ["acme.licensing.v1.SoftwareLicenseService"] = provider.GetRequiredService<ApiLicensing.SoftwareLicenseService.SoftwareLicenseServiceClient>(),
                ["acme.licensing.v1.SoftwareLicenseValidationService"] = provider.GetRequiredService<ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient>(),
                ["acme.licensing.v1.SoftwareLicenseWorkflowService"] = provider.GetRequiredService<ApiLicensing.SoftwareLicenseWorkflowService.SoftwareLicenseWorkflowServiceClient>(),
                ["acme.lineage.v1.LineageNodeCalculationService"] = provider.GetRequiredService<ApiLineage.LineageNodeCalculationService.LineageNodeCalculationServiceClient>(),
                ["acme.lineage.v1.LineageNodeQueryService"] = provider.GetRequiredService<ApiLineage.LineageNodeQueryService.LineageNodeQueryServiceClient>(),
                ["acme.localization.v1.TranslationKeyQueryService"] = provider.GetRequiredService<ApiLocalization.TranslationKeyQueryService.TranslationKeyQueryServiceClient>(),
                ["acme.localization.v1.TranslationKeyService"] = provider.GetRequiredService<ApiLocalization.TranslationKeyService.TranslationKeyServiceClient>(),
                ["acme.loyalty.v1.LoyaltyMemberCalculationService"] = provider.GetRequiredService<ApiLoyalty.LoyaltyMemberCalculationService.LoyaltyMemberCalculationServiceClient>(),
                ["acme.loyalty.v1.LoyaltyMemberQueryService"] = provider.GetRequiredService<ApiLoyalty.LoyaltyMemberQueryService.LoyaltyMemberQueryServiceClient>(),
                ["acme.loyalty.v1.LoyaltyMemberService"] = provider.GetRequiredService<ApiLoyalty.LoyaltyMemberService.LoyaltyMemberServiceClient>(),
                ["acme.maintenance.v1.WorkOrderCalculationService"] = provider.GetRequiredService<ApiMaintenance.WorkOrderCalculationService.WorkOrderCalculationServiceClient>(),
                ["acme.maintenance.v1.WorkOrderQueryService"] = provider.GetRequiredService<ApiMaintenance.WorkOrderQueryService.WorkOrderQueryServiceClient>(),
                ["acme.maintenance.v1.WorkOrderService"] = provider.GetRequiredService<ApiMaintenance.WorkOrderService.WorkOrderServiceClient>(),
                ["acme.media.v1.MediaAssetCalculationService"] = provider.GetRequiredService<ApiMedia.MediaAssetCalculationService.MediaAssetCalculationServiceClient>(),
                ["acme.media.v1.MediaAssetQueryService"] = provider.GetRequiredService<ApiMedia.MediaAssetQueryService.MediaAssetQueryServiceClient>(),
                ["acme.media.v1.MediaAssetService"] = provider.GetRequiredService<ApiMedia.MediaAssetService.MediaAssetServiceClient>(),
                ["acme.merchandising.v1.CollectionService"] = provider.GetRequiredService<ApiMerchandising.CollectionService.CollectionServiceClient>(),
                ["acme.messaging.v1.ConversationCalculationService"] = provider.GetRequiredService<ApiMessaging.ConversationCalculationService.ConversationCalculationServiceClient>(),
                ["acme.messaging.v1.ConversationQueryService"] = provider.GetRequiredService<ApiMessaging.ConversationQueryService.ConversationQueryServiceClient>(),
                ["acme.messaging.v1.ConversationService"] = provider.GetRequiredService<ApiMessaging.ConversationService.ConversationServiceClient>(),
                ["acme.metrics.v1.MetricSeriesQueryService"] = provider.GetRequiredService<ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient>(),
                ["acme.metrics.v1.MetricSeriesService"] = provider.GetRequiredService<ApiMetrics.MetricSeriesService.MetricSeriesServiceClient>(),
                ["acme.metrics.v1.MetricSeriesValidationService"] = provider.GetRequiredService<ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient>(),
                ["acme.modelregistry.v1.RegisteredModelCalculationService"] = provider.GetRequiredService<ApiModelregistry.RegisteredModelCalculationService.RegisteredModelCalculationServiceClient>(),
                ["acme.modelregistry.v1.RegisteredModelService"] = provider.GetRequiredService<ApiModelregistry.RegisteredModelService.RegisteredModelServiceClient>(),
                ["acme.notifications.v1.NotificationQueryService"] = provider.GetRequiredService<ApiNotifications.NotificationQueryService.NotificationQueryServiceClient>(),
                ["acme.notifications.v1.NotificationService"] = provider.GetRequiredService<ApiNotifications.NotificationService.NotificationServiceClient>(),
                ["acme.notifications.v1.NotificationValidationService"] = provider.GetRequiredService<ApiNotifications.NotificationValidationService.NotificationValidationServiceClient>(),
                ["acme.onboarding.v1.EquipmentApprovalService"] = provider.GetRequiredService<ApiOnboarding.EquipmentApprovalService.EquipmentApprovalServiceClient>(),
                ["acme.onboarding.v1.EquipmentCalculationService"] = provider.GetRequiredService<ApiOnboarding.EquipmentCalculationService.EquipmentCalculationServiceClient>(),
                ["acme.onboarding.v1.EquipmentLifecycleService"] = provider.GetRequiredService<ApiOnboarding.EquipmentLifecycleService.EquipmentLifecycleServiceClient>(),
                ["acme.onboarding.v1.EquipmentReportingService"] = provider.GetRequiredService<ApiOnboarding.EquipmentReportingService.EquipmentReportingServiceClient>(),
                ["acme.onboarding.v1.EquipmentValidationService"] = provider.GetRequiredService<ApiOnboarding.EquipmentValidationService.EquipmentValidationServiceClient>(),
                ["acme.onboarding.v1.EquipmentWorkflowService"] = provider.GetRequiredService<ApiOnboarding.EquipmentWorkflowService.EquipmentWorkflowServiceClient>(),
                ["acme.onboarding.v1.OnboardingPlanApprovalService"] = provider.GetRequiredService<ApiOnboarding.OnboardingPlanApprovalService.OnboardingPlanApprovalServiceClient>(),
                ["acme.onboarding.v1.OnboardingPlanCalculationService"] = provider.GetRequiredService<ApiOnboarding.OnboardingPlanCalculationService.OnboardingPlanCalculationServiceClient>(),
                ["acme.onboarding.v1.OnboardingPlanLifecycleService"] = provider.GetRequiredService<ApiOnboarding.OnboardingPlanLifecycleService.OnboardingPlanLifecycleServiceClient>(),
                ["acme.onboarding.v1.OnboardingPlanQueryService"] = provider.GetRequiredService<ApiOnboarding.OnboardingPlanQueryService.OnboardingPlanQueryServiceClient>(),
                ["acme.onboarding.v1.OnboardingPlanService"] = provider.GetRequiredService<ApiOnboarding.OnboardingPlanService.OnboardingPlanServiceClient>(),
                ["acme.onboarding.v1.OnboardingPlanValidationService"] = provider.GetRequiredService<ApiOnboarding.OnboardingPlanValidationService.OnboardingPlanValidationServiceClient>(),
                ["acme.onboarding.v1.OnboardingPlanWorkflowService"] = provider.GetRequiredService<ApiOnboarding.OnboardingPlanWorkflowService.OnboardingPlanWorkflowServiceClient>(),
                ["acme.onboarding.v1.OnboardingTaskApprovalService"] = provider.GetRequiredService<ApiOnboarding.OnboardingTaskApprovalService.OnboardingTaskApprovalServiceClient>(),
                ["acme.onboarding.v1.OnboardingTaskCalculationService"] = provider.GetRequiredService<ApiOnboarding.OnboardingTaskCalculationService.OnboardingTaskCalculationServiceClient>(),
                ["acme.onboarding.v1.OnboardingTaskQueryService"] = provider.GetRequiredService<ApiOnboarding.OnboardingTaskQueryService.OnboardingTaskQueryServiceClient>(),
                ["acme.onboarding.v1.OnboardingTaskReportingService"] = provider.GetRequiredService<ApiOnboarding.OnboardingTaskReportingService.OnboardingTaskReportingServiceClient>(),
                ["acme.onboarding.v1.OnboardingTaskValidationService"] = provider.GetRequiredService<ApiOnboarding.OnboardingTaskValidationService.OnboardingTaskValidationServiceClient>(),
                ["acme.orders.v1.OrderCalculationService"] = provider.GetRequiredService<ApiOrders.OrderCalculationService.OrderCalculationServiceClient>(),
                ["acme.patientapp.v1.PatientHomeQueryService"] = provider.GetRequiredService<ApiPatientapp.PatientHomeQueryService.PatientHomeQueryServiceClient>(),
                ["acme.patientapp.v1.PatientHomeService"] = provider.GetRequiredService<ApiPatientapp.PatientHomeService.PatientHomeServiceClient>(),
                ["acme.patients.v1.PatientCalculationService"] = provider.GetRequiredService<ApiPatients.PatientCalculationService.PatientCalculationServiceClient>(),
                ["acme.patients.v1.PatientQueryService"] = provider.GetRequiredService<ApiPatients.PatientQueryService.PatientQueryServiceClient>(),
                ["acme.patients.v1.PatientService"] = provider.GetRequiredService<ApiPatients.PatientService.PatientServiceClient>(),
                ["acme.payments.v1.PaymentCalculationService"] = provider.GetRequiredService<ApiPayments.PaymentCalculationService.PaymentCalculationServiceClient>(),
                ["acme.payroll.v1.PayRunCalculationService"] = provider.GetRequiredService<ApiPayroll.PayRunCalculationService.PayRunCalculationServiceClient>(),
                ["acme.payroll.v1.PayRunService"] = provider.GetRequiredService<ApiPayroll.PayRunService.PayRunServiceClient>(),
                ["acme.performance.v1.CalibrationApprovalService"] = provider.GetRequiredService<ApiPerformance.CalibrationApprovalService.CalibrationApprovalServiceClient>(),
                ["acme.performance.v1.CalibrationCalculationService"] = provider.GetRequiredService<ApiPerformance.CalibrationCalculationService.CalibrationCalculationServiceClient>(),
                ["acme.performance.v1.CalibrationLifecycleService"] = provider.GetRequiredService<ApiPerformance.CalibrationLifecycleService.CalibrationLifecycleServiceClient>(),
                ["acme.performance.v1.CalibrationValidationService"] = provider.GetRequiredService<ApiPerformance.CalibrationValidationService.CalibrationValidationServiceClient>(),
                ["acme.performance.v1.FeedbackApprovalService"] = provider.GetRequiredService<ApiPerformance.FeedbackApprovalService.FeedbackApprovalServiceClient>(),
                ["acme.performance.v1.FeedbackCalculationService"] = provider.GetRequiredService<ApiPerformance.FeedbackCalculationService.FeedbackCalculationServiceClient>(),
                ["acme.performance.v1.FeedbackValidationService"] = provider.GetRequiredService<ApiPerformance.FeedbackValidationService.FeedbackValidationServiceClient>(),
                ["acme.performance.v1.FeedbackWorkflowService"] = provider.GetRequiredService<ApiPerformance.FeedbackWorkflowService.FeedbackWorkflowServiceClient>(),
                ["acme.performance.v1.GoalApprovalService"] = provider.GetRequiredService<ApiPerformance.GoalApprovalService.GoalApprovalServiceClient>(),
                ["acme.performance.v1.GoalQueryService"] = provider.GetRequiredService<ApiPerformance.GoalQueryService.GoalQueryServiceClient>(),
                ["acme.performance.v1.GoalValidationService"] = provider.GetRequiredService<ApiPerformance.GoalValidationService.GoalValidationServiceClient>(),
                ["acme.performance.v1.GoalWorkflowService"] = provider.GetRequiredService<ApiPerformance.GoalWorkflowService.GoalWorkflowServiceClient>(),
                ["acme.performance.v1.ReviewApprovalService"] = provider.GetRequiredService<ApiPerformance.ReviewApprovalService.ReviewApprovalServiceClient>(),
                ["acme.performance.v1.ReviewQueryService"] = provider.GetRequiredService<ApiPerformance.ReviewQueryService.ReviewQueryServiceClient>(),
                ["acme.performance.v1.ReviewService"] = provider.GetRequiredService<ApiPerformance.ReviewService.ReviewServiceClient>(),
                ["acme.performance.v1.ReviewWorkflowService"] = provider.GetRequiredService<ApiPerformance.ReviewWorkflowService.ReviewWorkflowServiceClient>(),
                ["acme.performers.v1.PerformerQueryService"] = provider.GetRequiredService<ApiPerformers.PerformerQueryService.PerformerQueryServiceClient>(),
                ["acme.performers.v1.PerformerService"] = provider.GetRequiredService<ApiPerformers.PerformerService.PerformerServiceClient>(),
                ["acme.personalization.v1.ProfileCalculationService"] = provider.GetRequiredService<ApiPersonalization.ProfileCalculationService.ProfileCalculationServiceClient>(),
                ["acme.personalization.v1.ProfileQueryService"] = provider.GetRequiredService<ApiPersonalization.ProfileQueryService.ProfileQueryServiceClient>(),
                ["acme.personalization.v1.ProfileService"] = provider.GetRequiredService<ApiPersonalization.ProfileService.ProfileServiceClient>(),
                ["acme.pipelines.v1.PipelineCalculationService"] = provider.GetRequiredService<ApiPipelines.PipelineCalculationService.PipelineCalculationServiceClient>(),
                ["acme.pipelines.v1.PipelineService"] = provider.GetRequiredService<ApiPipelines.PipelineService.PipelineServiceClient>(),
                ["acme.prescriptions.v1.PrescriptionCalculationService"] = provider.GetRequiredService<ApiPrescriptions.PrescriptionCalculationService.PrescriptionCalculationServiceClient>(),
                ["acme.prescriptions.v1.PrescriptionQueryService"] = provider.GetRequiredService<ApiPrescriptions.PrescriptionQueryService.PrescriptionQueryServiceClient>(),
                ["acme.prescriptions.v1.PrescriptionService"] = provider.GetRequiredService<ApiPrescriptions.PrescriptionService.PrescriptionServiceClient>(),
                ["acme.pricing.v1.PriceListQueryService"] = provider.GetRequiredService<ApiPricing.PriceListQueryService.PriceListQueryServiceClient>(),
                ["acme.procurement.v1.PurchaseRequisitionCalculationService"] = provider.GetRequiredService<ApiProcurement.PurchaseRequisitionCalculationService.PurchaseRequisitionCalculationServiceClient>(),
                ["acme.promotions.v1.PromotionCalculationService"] = provider.GetRequiredService<ApiPromotions.PromotionCalculationService.PromotionCalculationServiceClient>(),
                ["acme.promotions.v1.PromotionService"] = provider.GetRequiredService<ApiPromotions.PromotionService.PromotionServiceClient>(),
                ["acme.ratelimiting.v1.RateLimitCalculationService"] = provider.GetRequiredService<ApiRatelimiting.RateLimitCalculationService.RateLimitCalculationServiceClient>(),
                ["acme.ratelimiting.v1.RateLimitQueryService"] = provider.GetRequiredService<ApiRatelimiting.RateLimitQueryService.RateLimitQueryServiceClient>(),
                ["acme.ratelimiting.v1.RateLimitService"] = provider.GetRequiredService<ApiRatelimiting.RateLimitService.RateLimitServiceClient>(),
                ["acme.recommendations.v1.RecommendationSetQueryService"] = provider.GetRequiredService<ApiRecommendations.RecommendationSetQueryService.RecommendationSetQueryServiceClient>(),
                ["acme.recommendations.v1.RecommendationSetService"] = provider.GetRequiredService<ApiRecommendations.RecommendationSetService.RecommendationSetServiceClient>(),
                ["acme.recruiting.v1.CandidateApprovalService"] = provider.GetRequiredService<ApiRecruiting.CandidateApprovalService.CandidateApprovalServiceClient>(),
                ["acme.recruiting.v1.CandidateCalculationService"] = provider.GetRequiredService<ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient>(),
                ["acme.recruiting.v1.CandidateLifecycleService"] = provider.GetRequiredService<ApiRecruiting.CandidateLifecycleService.CandidateLifecycleServiceClient>(),
                ["acme.recruiting.v1.CandidateQueryService"] = provider.GetRequiredService<ApiRecruiting.CandidateQueryService.CandidateQueryServiceClient>(),
                ["acme.recruiting.v1.CandidateReportingService"] = provider.GetRequiredService<ApiRecruiting.CandidateReportingService.CandidateReportingServiceClient>(),
                ["acme.recruiting.v1.CandidateValidationService"] = provider.GetRequiredService<ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient>(),
                ["acme.recruiting.v1.CandidateWorkflowService"] = provider.GetRequiredService<ApiRecruiting.CandidateWorkflowService.CandidateWorkflowServiceClient>(),
                ["acme.recruiting.v1.InterviewCalculationService"] = provider.GetRequiredService<ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient>(),
                ["acme.recruiting.v1.InterviewLifecycleService"] = provider.GetRequiredService<ApiRecruiting.InterviewLifecycleService.InterviewLifecycleServiceClient>(),
                ["acme.recruiting.v1.InterviewReportingService"] = provider.GetRequiredService<ApiRecruiting.InterviewReportingService.InterviewReportingServiceClient>(),
                ["acme.recruiting.v1.InterviewSyncService"] = provider.GetRequiredService<ApiRecruiting.InterviewSyncService.InterviewSyncServiceClient>(),
                ["acme.recruiting.v1.InterviewValidationService"] = provider.GetRequiredService<ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient>(),
                ["acme.recruiting.v1.JobOpeningApprovalService"] = provider.GetRequiredService<ApiRecruiting.JobOpeningApprovalService.JobOpeningApprovalServiceClient>(),
                ["acme.recruiting.v1.JobOpeningCalculationService"] = provider.GetRequiredService<ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient>(),
                ["acme.recruiting.v1.JobOpeningLifecycleService"] = provider.GetRequiredService<ApiRecruiting.JobOpeningLifecycleService.JobOpeningLifecycleServiceClient>(),
                ["acme.recruiting.v1.JobOpeningQueryService"] = provider.GetRequiredService<ApiRecruiting.JobOpeningQueryService.JobOpeningQueryServiceClient>(),
                ["acme.recruiting.v1.JobOpeningService"] = provider.GetRequiredService<ApiRecruiting.JobOpeningService.JobOpeningServiceClient>(),
                ["acme.recruiting.v1.JobOpeningValidationService"] = provider.GetRequiredService<ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient>(),
                ["acme.recruiting.v1.JobOpeningWorkflowService"] = provider.GetRequiredService<ApiRecruiting.JobOpeningWorkflowService.JobOpeningWorkflowServiceClient>(),
                ["acme.recruiting.v1.OfferApprovalService"] = provider.GetRequiredService<ApiRecruiting.OfferApprovalService.OfferApprovalServiceClient>(),
                ["acme.recruiting.v1.OfferCalculationService"] = provider.GetRequiredService<ApiRecruiting.OfferCalculationService.OfferCalculationServiceClient>(),
                ["acme.recruiting.v1.OfferLifecycleService"] = provider.GetRequiredService<ApiRecruiting.OfferLifecycleService.OfferLifecycleServiceClient>(),
                ["acme.recruiting.v1.OfferReportingService"] = provider.GetRequiredService<ApiRecruiting.OfferReportingService.OfferReportingServiceClient>(),
                ["acme.recruiting.v1.OfferSyncService"] = provider.GetRequiredService<ApiRecruiting.OfferSyncService.OfferSyncServiceClient>(),
                ["acme.recruiting.v1.OfferValidationService"] = provider.GetRequiredService<ApiRecruiting.OfferValidationService.OfferValidationServiceClient>(),
                ["acme.returns.v1.ReturnRequestCalculationService"] = provider.GetRequiredService<ApiReturns.ReturnRequestCalculationService.ReturnRequestCalculationServiceClient>(),
                ["acme.returns.v1.ReturnRequestQueryService"] = provider.GetRequiredService<ApiReturns.ReturnRequestQueryService.ReturnRequestQueryServiceClient>(),
                ["acme.returns.v1.ReturnRequestService"] = provider.GetRequiredService<ApiReturns.ReturnRequestService.ReturnRequestServiceClient>(),
                ["acme.reviews.v1.ProductReviewQueryService"] = provider.GetRequiredService<ApiReviews.ProductReviewQueryService.ProductReviewQueryServiceClient>(),
                ["acme.risk.v1.RiskRegisterQueryService"] = provider.GetRequiredService<ApiRisk.RiskRegisterQueryService.RiskRegisterQueryServiceClient>(),
                ["acme.risk.v1.RiskRegisterService"] = provider.GetRequiredService<ApiRisk.RiskRegisterService.RiskRegisterServiceClient>(),
                ["acme.routing.v1.RouteCalculationService"] = provider.GetRequiredService<ApiRouting.RouteCalculationService.RouteCalculationServiceClient>(),
                ["acme.routing.v1.RouteQueryService"] = provider.GetRequiredService<ApiRouting.RouteQueryService.RouteQueryServiceClient>(),
                ["acme.scheduling.v1.AvailabilityWindowApprovalService"] = provider.GetRequiredService<ApiScheduling.AvailabilityWindowApprovalService.AvailabilityWindowApprovalServiceClient>(),
                ["acme.scheduling.v1.AvailabilityWindowCalculationService"] = provider.GetRequiredService<ApiScheduling.AvailabilityWindowCalculationService.AvailabilityWindowCalculationServiceClient>(),
                ["acme.scheduling.v1.AvailabilityWindowLifecycleService"] = provider.GetRequiredService<ApiScheduling.AvailabilityWindowLifecycleService.AvailabilityWindowLifecycleServiceClient>(),
                ["acme.scheduling.v1.AvailabilityWindowValidationService"] = provider.GetRequiredService<ApiScheduling.AvailabilityWindowValidationService.AvailabilityWindowValidationServiceClient>(),
                ["acme.scheduling.v1.AvailabilityWindowWorkflowService"] = provider.GetRequiredService<ApiScheduling.AvailabilityWindowWorkflowService.AvailabilityWindowWorkflowServiceClient>(),
                ["acme.scheduling.v1.RosterApprovalService"] = provider.GetRequiredService<ApiScheduling.RosterApprovalService.RosterApprovalServiceClient>(),
                ["acme.scheduling.v1.RosterCalculationService"] = provider.GetRequiredService<ApiScheduling.RosterCalculationService.RosterCalculationServiceClient>(),
                ["acme.scheduling.v1.RosterQueryService"] = provider.GetRequiredService<ApiScheduling.RosterQueryService.RosterQueryServiceClient>(),
                ["acme.scheduling.v1.RosterValidationService"] = provider.GetRequiredService<ApiScheduling.RosterValidationService.RosterValidationServiceClient>(),
                ["acme.scheduling.v1.RosterWorkflowService"] = provider.GetRequiredService<ApiScheduling.RosterWorkflowService.RosterWorkflowServiceClient>(),
                ["acme.scheduling.v1.ShiftApprovalService"] = provider.GetRequiredService<ApiScheduling.ShiftApprovalService.ShiftApprovalServiceClient>(),
                ["acme.scheduling.v1.ShiftQueryService"] = provider.GetRequiredService<ApiScheduling.ShiftQueryService.ShiftQueryServiceClient>(),
                ["acme.scheduling.v1.ShiftService"] = provider.GetRequiredService<ApiScheduling.ShiftService.ShiftServiceClient>(),
                ["acme.scheduling.v1.ShiftValidationService"] = provider.GetRequiredService<ApiScheduling.ShiftValidationService.ShiftValidationServiceClient>(),
                ["acme.scheduling.v1.ShiftWorkflowService"] = provider.GetRequiredService<ApiScheduling.ShiftWorkflowService.ShiftWorkflowServiceClient>(),
                ["acme.scheduling.v1.SwapRequestApprovalService"] = provider.GetRequiredService<ApiScheduling.SwapRequestApprovalService.SwapRequestApprovalServiceClient>(),
                ["acme.scheduling.v1.SwapRequestCalculationService"] = provider.GetRequiredService<ApiScheduling.SwapRequestCalculationService.SwapRequestCalculationServiceClient>(),
                ["acme.scheduling.v1.SwapRequestValidationService"] = provider.GetRequiredService<ApiScheduling.SwapRequestValidationService.SwapRequestValidationServiceClient>(),
                ["acme.search.v1.SearchIndexQueryService"] = provider.GetRequiredService<ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient>(),
                ["acme.search.v1.SearchIndexService"] = provider.GetRequiredService<ApiSearch.SearchIndexService.SearchIndexServiceClient>(),
                ["acme.search.v1.SearchIndexValidationService"] = provider.GetRequiredService<ApiSearch.SearchIndexValidationService.SearchIndexValidationServiceClient>(),
                ["acme.seating.v1.SeatMapQueryService"] = provider.GetRequiredService<ApiSeating.SeatMapQueryService.SeatMapQueryServiceClient>(),
                ["acme.secrets.v1.SecretQueryService"] = provider.GetRequiredService<ApiSecrets.SecretQueryService.SecretQueryServiceClient>(),
                ["acme.secrets.v1.SecretService"] = provider.GetRequiredService<ApiSecrets.SecretService.SecretServiceClient>(),
                ["acme.sellers.v1.SellerCalculationService"] = provider.GetRequiredService<ApiSellers.SellerCalculationService.SellerCalculationServiceClient>(),
                ["acme.sellers.v1.SellerQueryService"] = provider.GetRequiredService<ApiSellers.SellerQueryService.SellerQueryServiceClient>(),
                ["acme.sellers.v1.SellerService"] = provider.GetRequiredService<ApiSellers.SellerService.SellerServiceClient>(),
                ["acme.shipping.v1.ShipmentQueryService"] = provider.GetRequiredService<ApiShipping.ShipmentQueryService.ShipmentQueryServiceClient>(),
                ["acme.shipping.v1.ShipmentService"] = provider.GetRequiredService<ApiShipping.ShipmentService.ShipmentServiceClient>(),
                ["acme.shows.v1.ShowQueryService"] = provider.GetRequiredService<ApiShows.ShowQueryService.ShowQueryServiceClient>(),
                ["acme.shows.v1.ShowService"] = provider.GetRequiredService<ApiShows.ShowService.ShowServiceClient>(),
                ["acme.sponsorships.v1.SponsorCalculationService"] = provider.GetRequiredService<ApiSponsorships.SponsorCalculationService.SponsorCalculationServiceClient>(),
                ["acme.subscriptions.v1.SubscriptionService"] = provider.GetRequiredService<ApiSubscriptions.SubscriptionService.SubscriptionServiceClient>(),
                ["acme.surveys.v1.SurveyQueryService"] = provider.GetRequiredService<ApiSurveys.SurveyQueryService.SurveyQueryServiceClient>(),
                ["acme.surveys.v1.SurveyService"] = provider.GetRequiredService<ApiSurveys.SurveyService.SurveyServiceClient>(),
                ["acme.telehealth.v1.VideoVisitCalculationService"] = provider.GetRequiredService<ApiTelehealth.VideoVisitCalculationService.VideoVisitCalculationServiceClient>(),
                ["acme.telehealth.v1.VideoVisitQueryService"] = provider.GetRequiredService<ApiTelehealth.VideoVisitQueryService.VideoVisitQueryServiceClient>(),
                ["acme.telematics.v1.TelemetryReadingCalculationService"] = provider.GetRequiredService<ApiTelematics.TelemetryReadingCalculationService.TelemetryReadingCalculationServiceClient>(),
                ["acme.telematics.v1.TelemetryReadingQueryService"] = provider.GetRequiredService<ApiTelematics.TelemetryReadingQueryService.TelemetryReadingQueryServiceClient>(),
                ["acme.ticketing.v1.TicketService"] = provider.GetRequiredService<ApiTicketing.TicketService.TicketServiceClient>(),
                ["acme.timekeeping.v1.ClockEventApprovalService"] = provider.GetRequiredService<ApiTimekeeping.ClockEventApprovalService.ClockEventApprovalServiceClient>(),
                ["acme.timekeeping.v1.ClockEventCalculationService"] = provider.GetRequiredService<ApiTimekeeping.ClockEventCalculationService.ClockEventCalculationServiceClient>(),
                ["acme.timekeeping.v1.ClockEventQueryService"] = provider.GetRequiredService<ApiTimekeeping.ClockEventQueryService.ClockEventQueryServiceClient>(),
                ["acme.timekeeping.v1.ClockEventValidationService"] = provider.GetRequiredService<ApiTimekeeping.ClockEventValidationService.ClockEventValidationServiceClient>(),
                ["acme.timekeeping.v1.ClockEventWorkflowService"] = provider.GetRequiredService<ApiTimekeeping.ClockEventWorkflowService.ClockEventWorkflowServiceClient>(),
                ["acme.timekeeping.v1.OvertimeApprovalService"] = provider.GetRequiredService<ApiTimekeeping.OvertimeApprovalService.OvertimeApprovalServiceClient>(),
                ["acme.timekeeping.v1.OvertimeCalculationService"] = provider.GetRequiredService<ApiTimekeeping.OvertimeCalculationService.OvertimeCalculationServiceClient>(),
                ["acme.timekeeping.v1.OvertimeLifecycleService"] = provider.GetRequiredService<ApiTimekeeping.OvertimeLifecycleService.OvertimeLifecycleServiceClient>(),
                ["acme.timekeeping.v1.OvertimeValidationService"] = provider.GetRequiredService<ApiTimekeeping.OvertimeValidationService.OvertimeValidationServiceClient>(),
                ["acme.timekeeping.v1.OvertimeWorkflowService"] = provider.GetRequiredService<ApiTimekeeping.OvertimeWorkflowService.OvertimeWorkflowServiceClient>(),
                ["acme.timekeeping.v1.TimesheetApprovalService"] = provider.GetRequiredService<ApiTimekeeping.TimesheetApprovalService.TimesheetApprovalServiceClient>(),
                ["acme.timekeeping.v1.TimesheetQueryService"] = provider.GetRequiredService<ApiTimekeeping.TimesheetQueryService.TimesheetQueryServiceClient>(),
                ["acme.timekeeping.v1.TimesheetService"] = provider.GetRequiredService<ApiTimekeeping.TimesheetService.TimesheetServiceClient>(),
                ["acme.timekeeping.v1.TimesheetValidationService"] = provider.GetRequiredService<ApiTimekeeping.TimesheetValidationService.TimesheetValidationServiceClient>(),
                ["acme.timekeeping.v1.TimesheetWorkflowService"] = provider.GetRequiredService<ApiTimekeeping.TimesheetWorkflowService.TimesheetWorkflowServiceClient>(),
                ["acme.tours.v1.TourQueryService"] = provider.GetRequiredService<ApiTours.TourQueryService.TourQueryServiceClient>(),
                ["acme.travel.v1.ItineraryApprovalService"] = provider.GetRequiredService<ApiTravel.ItineraryApprovalService.ItineraryApprovalServiceClient>(),
                ["acme.travel.v1.ItineraryCalculationService"] = provider.GetRequiredService<ApiTravel.ItineraryCalculationService.ItineraryCalculationServiceClient>(),
                ["acme.travel.v1.ItineraryLifecycleService"] = provider.GetRequiredService<ApiTravel.ItineraryLifecycleService.ItineraryLifecycleServiceClient>(),
                ["acme.travel.v1.ItineraryReportingService"] = provider.GetRequiredService<ApiTravel.ItineraryReportingService.ItineraryReportingServiceClient>(),
                ["acme.travel.v1.ItinerarySyncService"] = provider.GetRequiredService<ApiTravel.ItinerarySyncService.ItinerarySyncServiceClient>(),
                ["acme.travel.v1.ItineraryValidationService"] = provider.GetRequiredService<ApiTravel.ItineraryValidationService.ItineraryValidationServiceClient>(),
                ["acme.travel.v1.TravelBookingApprovalService"] = provider.GetRequiredService<ApiTravel.TravelBookingApprovalService.TravelBookingApprovalServiceClient>(),
                ["acme.travel.v1.TravelBookingCalculationService"] = provider.GetRequiredService<ApiTravel.TravelBookingCalculationService.TravelBookingCalculationServiceClient>(),
                ["acme.travel.v1.TravelBookingLifecycleService"] = provider.GetRequiredService<ApiTravel.TravelBookingLifecycleService.TravelBookingLifecycleServiceClient>(),
                ["acme.travel.v1.TravelBookingQueryService"] = provider.GetRequiredService<ApiTravel.TravelBookingQueryService.TravelBookingQueryServiceClient>(),
                ["acme.travel.v1.TravelBookingReportingService"] = provider.GetRequiredService<ApiTravel.TravelBookingReportingService.TravelBookingReportingServiceClient>(),
                ["acme.travel.v1.TravelBookingValidationService"] = provider.GetRequiredService<ApiTravel.TravelBookingValidationService.TravelBookingValidationServiceClient>(),
                ["acme.travel.v1.TravelBookingWorkflowService"] = provider.GetRequiredService<ApiTravel.TravelBookingWorkflowService.TravelBookingWorkflowServiceClient>(),
                ["acme.travel.v1.TravelPolicyApprovalService"] = provider.GetRequiredService<ApiTravel.TravelPolicyApprovalService.TravelPolicyApprovalServiceClient>(),
                ["acme.travel.v1.TravelPolicyCalculationService"] = provider.GetRequiredService<ApiTravel.TravelPolicyCalculationService.TravelPolicyCalculationServiceClient>(),
                ["acme.travel.v1.TravelPolicyLifecycleService"] = provider.GetRequiredService<ApiTravel.TravelPolicyLifecycleService.TravelPolicyLifecycleServiceClient>(),
                ["acme.travel.v1.TravelPolicyReportingService"] = provider.GetRequiredService<ApiTravel.TravelPolicyReportingService.TravelPolicyReportingServiceClient>(),
                ["acme.travel.v1.TravelPolicySyncService"] = provider.GetRequiredService<ApiTravel.TravelPolicySyncService.TravelPolicySyncServiceClient>(),
                ["acme.travel.v1.TravelPolicyValidationService"] = provider.GetRequiredService<ApiTravel.TravelPolicyValidationService.TravelPolicyValidationServiceClient>(),
                ["acme.travel.v1.TravelPolicyWorkflowService"] = provider.GetRequiredService<ApiTravel.TravelPolicyWorkflowService.TravelPolicyWorkflowServiceClient>(),
                ["acme.travel.v1.TripApprovalService"] = provider.GetRequiredService<ApiTravel.TripApprovalService.TripApprovalServiceClient>(),
                ["acme.travel.v1.TripCalculationService"] = provider.GetRequiredService<ApiTravel.TripCalculationService.TripCalculationServiceClient>(),
                ["acme.travel.v1.TripLifecycleService"] = provider.GetRequiredService<ApiTravel.TripLifecycleService.TripLifecycleServiceClient>(),
                ["acme.travel.v1.TripQueryService"] = provider.GetRequiredService<ApiTravel.TripQueryService.TripQueryServiceClient>(),
                ["acme.travel.v1.TripService"] = provider.GetRequiredService<ApiTravel.TripService.TripServiceClient>(),
                ["acme.travel.v1.TripValidationService"] = provider.GetRequiredService<ApiTravel.TripValidationService.TripValidationServiceClient>(),
                ["acme.travel.v1.TripWorkflowService"] = provider.GetRequiredService<ApiTravel.TripWorkflowService.TripWorkflowServiceClient>(),
                ["acme.treasury.v1.CashPositionQueryService"] = provider.GetRequiredService<ApiTreasury.CashPositionQueryService.CashPositionQueryServiceClient>(),
                ["acme.treasury.v1.CashPositionService"] = provider.GetRequiredService<ApiTreasury.CashPositionService.CashPositionServiceClient>(),
                ["acme.venues.v1.VenueQueryService"] = provider.GetRequiredService<ApiVenues.VenueQueryService.VenueQueryServiceClient>(),
                ["acme.venues.v1.VenueService"] = provider.GetRequiredService<ApiVenues.VenueService.VenueServiceClient>(),
                ["acme.venues.v1.VenueValidationService"] = provider.GetRequiredService<ApiVenues.VenueValidationService.VenueValidationServiceClient>(),
                ["acme.warehousing.v1.WarehouseQueryService"] = provider.GetRequiredService<ApiWarehousing.WarehouseQueryService.WarehouseQueryServiceClient>(),
                ["acme.warehousing.v1.WarehouseService"] = provider.GetRequiredService<ApiWarehousing.WarehouseService.WarehouseServiceClient>(),
                ["acme.webbff.v1.PageCompositionQueryService"] = provider.GetRequiredService<ApiWebbff.PageCompositionQueryService.PageCompositionQueryServiceClient>(),
                ["acme.webbff.v1.PageCompositionService"] = provider.GetRequiredService<ApiWebbff.PageCompositionService.PageCompositionServiceClient>(),
                ["acme.webhooks.v1.WebhookEndpointQueryService"] = provider.GetRequiredService<ApiWebhooks.WebhookEndpointQueryService.WebhookEndpointQueryServiceClient>(),
                ["acme.workplaces.v1.BuildingApprovalService"] = provider.GetRequiredService<ApiWorkplaces.BuildingApprovalService.BuildingApprovalServiceClient>(),
                ["acme.workplaces.v1.BuildingCalculationService"] = provider.GetRequiredService<ApiWorkplaces.BuildingCalculationService.BuildingCalculationServiceClient>(),
                ["acme.workplaces.v1.BuildingQueryService"] = provider.GetRequiredService<ApiWorkplaces.BuildingQueryService.BuildingQueryServiceClient>(),
                ["acme.workplaces.v1.BuildingService"] = provider.GetRequiredService<ApiWorkplaces.BuildingService.BuildingServiceClient>(),
                ["acme.workplaces.v1.BuildingValidationService"] = provider.GetRequiredService<ApiWorkplaces.BuildingValidationService.BuildingValidationServiceClient>(),
                ["acme.workplaces.v1.BuildingWorkflowService"] = provider.GetRequiredService<ApiWorkplaces.BuildingWorkflowService.BuildingWorkflowServiceClient>(),
                ["acme.workplaces.v1.DeskApprovalService"] = provider.GetRequiredService<ApiWorkplaces.DeskApprovalService.DeskApprovalServiceClient>(),
                ["acme.workplaces.v1.DeskCalculationService"] = provider.GetRequiredService<ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient>(),
                ["acme.workplaces.v1.DeskLifecycleService"] = provider.GetRequiredService<ApiWorkplaces.DeskLifecycleService.DeskLifecycleServiceClient>(),
                ["acme.workplaces.v1.DeskQueryService"] = provider.GetRequiredService<ApiWorkplaces.DeskQueryService.DeskQueryServiceClient>(),
                ["acme.workplaces.v1.DeskValidationService"] = provider.GetRequiredService<ApiWorkplaces.DeskValidationService.DeskValidationServiceClient>(),
                ["acme.workplaces.v1.DeskWorkflowService"] = provider.GetRequiredService<ApiWorkplaces.DeskWorkflowService.DeskWorkflowServiceClient>(),
                ["acme.workplaces.v1.RoomBookingApprovalService"] = provider.GetRequiredService<ApiWorkplaces.RoomBookingApprovalService.RoomBookingApprovalServiceClient>(),
                ["acme.workplaces.v1.RoomBookingCalculationService"] = provider.GetRequiredService<ApiWorkplaces.RoomBookingCalculationService.RoomBookingCalculationServiceClient>(),
                ["acme.workplaces.v1.RoomBookingLifecycleService"] = provider.GetRequiredService<ApiWorkplaces.RoomBookingLifecycleService.RoomBookingLifecycleServiceClient>(),
                ["acme.workplaces.v1.RoomBookingReportingService"] = provider.GetRequiredService<ApiWorkplaces.RoomBookingReportingService.RoomBookingReportingServiceClient>(),
                ["acme.workplaces.v1.RoomBookingValidationService"] = provider.GetRequiredService<ApiWorkplaces.RoomBookingValidationService.RoomBookingValidationServiceClient>(),
                ["acme.workplaces.v1.RoomBookingWorkflowService"] = provider.GetRequiredService<ApiWorkplaces.RoomBookingWorkflowService.RoomBookingWorkflowServiceClient>(),
                ["acme.workplaces.v1.VisitorPassApprovalService"] = provider.GetRequiredService<ApiWorkplaces.VisitorPassApprovalService.VisitorPassApprovalServiceClient>(),
                ["acme.workplaces.v1.VisitorPassCalculationService"] = provider.GetRequiredService<ApiWorkplaces.VisitorPassCalculationService.VisitorPassCalculationServiceClient>(),
                ["acme.workplaces.v1.VisitorPassLifecycleService"] = provider.GetRequiredService<ApiWorkplaces.VisitorPassLifecycleService.VisitorPassLifecycleServiceClient>(),
                ["acme.workplaces.v1.VisitorPassReportingService"] = provider.GetRequiredService<ApiWorkplaces.VisitorPassReportingService.VisitorPassReportingServiceClient>(),
                ["acme.workplaces.v1.VisitorPassSyncService"] = provider.GetRequiredService<ApiWorkplaces.VisitorPassSyncService.VisitorPassSyncServiceClient>(),
                ["acme.workplaces.v1.VisitorPassValidationService"] = provider.GetRequiredService<ApiWorkplaces.VisitorPassValidationService.VisitorPassValidationServiceClient>(),
                ["acme.yards.v1.YardCalculationService"] = provider.GetRequiredService<ApiYards.YardCalculationService.YardCalculationServiceClient>(),
                ["acme.yards.v1.YardQueryService"] = provider.GetRequiredService<ApiYards.YardQueryService.YardQueryServiceClient>(),
                ["acme.yards.v1.YardService"] = provider.GetRequiredService<ApiYards.YardService.YardServiceClient>(),
            }));
        return services;
    }
}
