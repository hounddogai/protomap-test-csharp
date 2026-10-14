using Acme.Workforce.Platform;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
using ApiCommon = Acme.Common.V1;
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

namespace Acme.Workforce.Checks;

public static class Program
{
    public static async Task Main()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var configuration = new ConfigurationBuilder().Build();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddWorkforcePlatform(configuration);
        services.AddSingleton<ProbeInvoker>();
        services.AddSingleton<CallInvoker>(provider => provider.GetRequiredService<ProbeInvoker>());
        services.AddSingleton<IRecordRepository, NullRepository>();
        Acme.Workforce.Benefits.DomainModule.AddBenefits(services);
        Acme.Workforce.Corporateevents.DomainModule.AddCorporateevents(services);
        Acme.Workforce.Devices.DomainModule.AddDevices(services);
        Acme.Workforce.Documents.DomainModule.AddDocuments(services);
        Acme.Workforce.Employeeportal.DomainModule.AddEmployeeportal(services);
        Acme.Workforce.Employees.DomainModule.AddEmployees(services);
        Acme.Workforce.Helpdesk.DomainModule.AddHelpdesk(services);
        Acme.Workforce.Itsm.DomainModule.AddItsm(services);
        Acme.Workforce.Learning.DomainModule.AddLearning(services);
        Acme.Workforce.Licensing.DomainModule.AddLicensing(services);
        Acme.Workforce.Onboarding.DomainModule.AddOnboarding(services);
        Acme.Workforce.Performance.DomainModule.AddPerformance(services);
        Acme.Workforce.Recruiting.DomainModule.AddRecruiting(services);
        Acme.Workforce.Scheduling.DomainModule.AddScheduling(services);
        Acme.Workforce.Timekeeping.DomainModule.AddTimekeeping(services);
        Acme.Workforce.Travel.DomainModule.AddTravel(services);
        Acme.Workforce.Workplaces.DomainModule.AddWorkplaces(services);
        services.AddSingleton(provider => new ApiAdmissions.AdmissionService.AdmissionServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiAnalytics.AnalyticsEventService.AnalyticsEventServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiAnomalies.AnomalyQueryService.AnomalyQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiAnomalies.AnomalyService.AnomalyServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiAppointments.AppointmentQueryService.AppointmentQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiAppointments.AppointmentService.AppointmentServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiAssets.FixedAssetCalculationService.FixedAssetCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiAudiences.AudienceQueryService.AudienceQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiAudiences.AudienceService.AudienceServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiAudit.AuditEventQueryService.AuditEventQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiAudit.AuditEventService.AuditEventServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiBanking.BankAccountQueryService.BankAccountQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiBanking.BankAccountService.BankAccountServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiBenefits.BenefitPlanApprovalService.BenefitPlanApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiBenefits.BenefitPlanQueryService.BenefitPlanQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiBenefits.BenefitPlanService.BenefitPlanServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiBenefits.BenefitPlanWorkflowService.BenefitPlanWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiBenefits.DependentApprovalService.DependentApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiBenefits.DependentValidationService.DependentValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiBenefits.DependentWorkflowService.DependentWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiBenefits.EnrollmentApprovalService.EnrollmentApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiBenefits.EnrollmentQueryService.EnrollmentQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiBenefits.EnrollmentWorkflowService.EnrollmentWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiBenefits.LifeEventCalculationService.LifeEventCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiBenefits.LifeEventValidationService.LifeEventValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiBoxoffice.BoxOfficeSaleQueryService.BoxOfficeSaleQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiBoxoffice.BoxOfficeSaleService.BoxOfficeSaleServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiBroadcasts.BroadcastService.BroadcastServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiBudgeting.BudgetCalculationService.BudgetCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiBudgeting.BudgetQueryService.BudgetQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiBudgeting.BudgetService.BudgetServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiCampaigns.MarketingCampaignService.MarketingCampaignServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiCareplans.CarePlanCalculationService.CarePlanCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiCareplans.CarePlanQueryService.CarePlanQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiCareplans.CarePlanService.CarePlanServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiCart.CartQueryService.CartQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiCart.CartService.CartServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiCatalog.ProductQueryService.ProductQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiCertificates.CertificateCalculationService.CertificateCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiCertificates.CertificateQueryService.CertificateQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiCertificates.CertificateService.CertificateServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiChat.ChatRoomCalculationService.ChatRoomCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiChat.ChatRoomQueryService.ChatRoomQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiChat.ChatRoomService.ChatRoomServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiCheckout.CheckoutSessionQueryService.CheckoutSessionQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiCheckout.CheckoutSessionService.CheckoutSessionServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiCommunity.ForumService.ForumServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiCompliance.ComplianceCaseQueryService.ComplianceCaseQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiCompliance.ComplianceCaseService.ComplianceCaseServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiConcessions.ConcessionStandCalculationService.ConcessionStandCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiConcessions.ConcessionStandQueryService.ConcessionStandQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiConcessions.ConcessionStandService.ConcessionStandServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiConfiguration.ConfigSetCalculationService.ConfigSetCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiConfiguration.ConfigSetQueryService.ConfigSetQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiConfiguration.ConfigSetService.ConfigSetServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiContent.ArticleQueryService.ArticleQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiContent.ArticleService.ArticleServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiCorporateevents.CorporateEventApprovalService.CorporateEventApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiCorporateevents.CorporateEventLifecycleService.CorporateEventLifecycleServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiCorporateevents.CorporateEventQueryService.CorporateEventQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiCorporateevents.CorporateEventReportingService.CorporateEventReportingServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiCorporateevents.CorporateEventService.CorporateEventServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiCorporateevents.CorporateEventValidationService.CorporateEventValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiCorporateevents.CorporateEventWorkflowService.CorporateEventWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiCorporateevents.RegistrationApprovalService.RegistrationApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiCorporateevents.RegistrationCalculationService.RegistrationCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiCorporateevents.RegistrationLifecycleService.RegistrationLifecycleServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiCorporateevents.RegistrationQueryService.RegistrationQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiCorporateevents.RegistrationReportingService.RegistrationReportingServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiCorporateevents.RegistrationValidationService.RegistrationValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiCorporateevents.RegistrationWorkflowService.RegistrationWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiCorporateevents.VenueApprovalService.VenueApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiCorporateevents.VenueCalculationService.VenueCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiCorporateevents.VenueLifecycleService.VenueLifecycleServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiCorporateevents.VenueReportingService.VenueReportingServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiCorporateevents.VenueSyncService.VenueSyncServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiCorporateevents.VenueValidationService.VenueValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiCorporateevents.VenueWorkflowService.VenueWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiCustomers.CustomerCalculationService.CustomerCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiCustomers.CustomerService.CustomerServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDatahub.WorkspaceQueryService.WorkspaceQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDatasets.DatasetQueryService.DatasetQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDatasets.DatasetService.DatasetServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDeliveries.DeliveryQueryService.DeliveryQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDeliveries.DeliveryService.DeliveryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDevices.ComplianceScanApprovalService.ComplianceScanApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDevices.ComplianceScanLifecycleService.ComplianceScanLifecycleServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDevices.ComplianceScanValidationService.ComplianceScanValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDevices.ComplianceScanWorkflowService.ComplianceScanWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDevices.DevicePolicyApprovalService.DevicePolicyApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDevices.DevicePolicyCalculationService.DevicePolicyCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDevices.DevicePolicyLifecycleService.DevicePolicyLifecycleServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDevices.DevicePolicyQueryService.DevicePolicyQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDevices.DevicePolicyValidationService.DevicePolicyValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDevices.ManagedDeviceApprovalService.ManagedDeviceApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDevices.ManagedDeviceCalculationService.ManagedDeviceCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDevices.ManagedDeviceQueryService.ManagedDeviceQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDevices.ManagedDeviceService.ManagedDeviceServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDevices.ManagedDeviceValidationService.ManagedDeviceValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDevices.ManagedDeviceWorkflowService.ManagedDeviceWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDispatch.DispatchOrderQueryService.DispatchOrderQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDispatch.DispatchOrderService.DispatchOrderServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDocuments.DocumentAdminService.DocumentAdminServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDocuments.DocumentExportService.DocumentExportServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDocuments.DocumentFolderExportService.DocumentFolderExportServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDocuments.DocumentFolderHistoryService.DocumentFolderHistoryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDocuments.DocumentHistoryService.DocumentHistoryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDocuments.DocumentQueryService.DocumentQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDocuments.DocumentService.DocumentServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDocuments.DocumentSyncService.DocumentSyncServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDocuments.DocumentTemplateAdminService.DocumentTemplateAdminServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDocuments.DocumentTemplateCalculationService.DocumentTemplateCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDocuments.DocumentTemplateExportService.DocumentTemplateExportServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDocuments.DocumentTemplateHistoryService.DocumentTemplateHistoryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDocuments.DocumentValidationService.DocumentValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDocuments.SignatureAdminService.SignatureAdminServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDocuments.SignatureExportService.SignatureExportServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDocuments.SignatureNotificationService.SignatureNotificationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDocuments.SignatureSyncService.SignatureSyncServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDocuments.SignatureValidationService.SignatureValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDriverapp.DriverHomeService.DriverHomeServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDrivers.DriverCalculationService.DriverCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiDrivers.DriverService.DriverServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiEdgegateway.RouteBindingQueryService.RouteBindingQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiEdgegateway.RouteBindingService.RouteBindingServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiEmployeeportal.AnnouncementIntegrationService.AnnouncementIntegrationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiEmployeeportal.AnnouncementNotificationService.AnnouncementNotificationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiEmployeeportal.AnnouncementWorkflowService.AnnouncementWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiEmployeeportal.PortalHomeIntegrationService.PortalHomeIntegrationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiEmployeeportal.PortalHomeNotificationService.PortalHomeNotificationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiEmployeeportal.PortalHomeQueryService.PortalHomeQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiEmployeeportal.PortalHomeService.PortalHomeServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiEmployeeportal.SelfServiceRequestIntegrationService.SelfServiceRequestIntegrationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiEmployeeportal.SelfServiceRequestNotificationService.SelfServiceRequestNotificationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiEmployeeportal.SelfServiceRequestQueryService.SelfServiceRequestQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiEmployees.EmployeeApprovalService.EmployeeApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiEmployees.EmployeeService.EmployeeServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiEmployees.EmployeeWorkflowService.EmployeeWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiEmployees.EmploymentContractApprovalService.EmploymentContractApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiEmployees.OrgUnitApprovalService.OrgUnitApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiEmployees.OrgUnitWorkflowService.OrgUnitWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiEmployees.PositionApprovalService.PositionApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiEmployees.PositionValidationService.PositionValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiEmployees.PositionWorkflowService.PositionWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiExperiments.ExperimentCalculationService.ExperimentCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiExperiments.ExperimentQueryService.ExperimentQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiExperiments.ExperimentService.ExperimentServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiFanportal.FanSessionQueryService.FanSessionQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiFanportal.FanSessionService.FanSessionServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiFeatureflags.FeatureFlagService.FeatureFlagServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiFeaturestore.FeatureViewCalculationService.FeatureViewCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiFeaturestore.FeatureViewQueryService.FeatureViewQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiFleet.VehicleQueryService.VehicleQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiFleet.VehicleService.VehicleServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiForecasting.DemandForecastQueryService.DemandForecastQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiForecasting.DemandForecastService.DemandForecastServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiForms.FormCalculationService.FormCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiFraud.RiskAssessmentCalculationService.RiskAssessmentCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiFraud.RiskAssessmentQueryService.RiskAssessmentQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiFraud.RiskAssessmentService.RiskAssessmentServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiFuel.FuelCardCalculationService.FuelCardCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiFuel.FuelCardQueryService.FuelCardQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiFuel.FuelCardService.FuelCardServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiGiftcards.GiftCardQueryService.GiftCardQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiGiftcards.GiftCardService.GiftCardServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiHelpdesk.EscalationApprovalService.EscalationApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiHelpdesk.EscalationCalculationService.EscalationCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiHelpdesk.EscalationLifecycleService.EscalationLifecycleServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiHelpdesk.EscalationReportingService.EscalationReportingServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiHelpdesk.EscalationValidationService.EscalationValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiHelpdesk.KnowledgeArticleApprovalService.KnowledgeArticleApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiHelpdesk.KnowledgeArticleCalculationService.KnowledgeArticleCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiHelpdesk.KnowledgeArticleLifecycleService.KnowledgeArticleLifecycleServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiHelpdesk.KnowledgeArticleQueryService.KnowledgeArticleQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiHelpdesk.KnowledgeArticleWorkflowService.KnowledgeArticleWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiHelpdesk.SlaPolicyApprovalService.SlaPolicyApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiHelpdesk.SlaPolicyLifecycleService.SlaPolicyLifecycleServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiHelpdesk.SlaPolicyValidationService.SlaPolicyValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiHelpdesk.SlaPolicyWorkflowService.SlaPolicyWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiHelpdesk.TicketApprovalService.TicketApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiHelpdesk.TicketCalculationService.TicketCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiHelpdesk.TicketQueryService.TicketQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiHelpdesk.TicketService.TicketServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiHelpdesk.TicketValidationService.TicketValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiHelpdesk.TicketWorkflowService.TicketWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiIdentity.UserQueryService.UserQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiIdentity.UserService.UserServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiIdentity.UserValidationService.UserValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiInsurance.PolicyCalculationService.PolicyCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiInsurance.PolicyQueryService.PolicyQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiInsurance.PolicyService.PolicyServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiInvoicing.InvoiceBatchCalculationService.InvoiceBatchCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiInvoicing.InvoiceBatchService.InvoiceBatchServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiItsm.ChangeRequestApprovalService.ChangeRequestApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiItsm.ChangeRequestCalculationService.ChangeRequestCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiItsm.ChangeRequestLifecycleService.ChangeRequestLifecycleServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiItsm.ChangeRequestQueryService.ChangeRequestQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiItsm.ChangeRequestReportingService.ChangeRequestReportingServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiItsm.ChangeRequestWorkflowService.ChangeRequestWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiItsm.IncidentApprovalService.IncidentApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiItsm.IncidentCalculationService.IncidentCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiItsm.IncidentLifecycleService.IncidentLifecycleServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiItsm.IncidentQueryService.IncidentQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiItsm.IncidentService.IncidentServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiItsm.IncidentValidationService.IncidentValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiItsm.IncidentWorkflowService.IncidentWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiItsm.ProblemApprovalService.ProblemApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiItsm.ProblemCalculationService.ProblemCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiItsm.ProblemLifecycleService.ProblemLifecycleServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiItsm.ProblemReportingService.ProblemReportingServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiItsm.ProblemSyncService.ProblemSyncServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiItsm.ProblemValidationService.ProblemValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiItsm.ServiceAssetApprovalService.ServiceAssetApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiItsm.ServiceAssetCalculationService.ServiceAssetCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiItsm.ServiceAssetLifecycleService.ServiceAssetLifecycleServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiItsm.ServiceAssetReportingService.ServiceAssetReportingServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiItsm.ServiceAssetValidationService.ServiceAssetValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiItsm.ServiceAssetWorkflowService.ServiceAssetWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiKeymanagement.CryptoKeyCalculationService.CryptoKeyCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiKeymanagement.CryptoKeyQueryService.CryptoKeyQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLabeling.LabelingTaskCalculationService.LabelingTaskCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLabeling.LabelingTaskQueryService.LabelingTaskQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLabeling.LabelingTaskService.LabelingTaskServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLearning.CertificationApprovalService.CertificationApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLearning.CertificationCalculationService.CertificationCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLearning.CertificationValidationService.CertificationValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLearning.CertificationWorkflowService.CertificationWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLearning.CourseApprovalService.CourseApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLearning.CourseEnrollmentApprovalService.CourseEnrollmentApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLearning.CourseEnrollmentCalculationService.CourseEnrollmentCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLearning.CourseEnrollmentValidationService.CourseEnrollmentValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLearning.CourseQueryService.CourseQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLearning.CourseService.CourseServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLearning.CourseValidationService.CourseValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLearning.CourseWorkflowService.CourseWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLearning.LearningPathApprovalService.LearningPathApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLearning.LearningPathQueryService.LearningPathQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLearning.LearningPathValidationService.LearningPathValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLearning.LearningPathWorkflowService.LearningPathWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLedger.LedgerAccountService.LedgerAccountServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLending.LoanApplicationCalculationService.LoanApplicationCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLicensing.EntitlementApprovalService.EntitlementApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLicensing.EntitlementLifecycleService.EntitlementLifecycleServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLicensing.EntitlementQueryService.EntitlementQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLicensing.EntitlementWorkflowService.EntitlementWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLicensing.LicenseAssignmentCalculationService.LicenseAssignmentCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLicensing.LicenseAssignmentLifecycleService.LicenseAssignmentLifecycleServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLicensing.LicenseAssignmentValidationService.LicenseAssignmentValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLicensing.LicenseAssignmentWorkflowService.LicenseAssignmentWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLicensing.SoftwareLicenseApprovalService.SoftwareLicenseApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLicensing.SoftwareLicenseService.SoftwareLicenseServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLicensing.SoftwareLicenseWorkflowService.SoftwareLicenseWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLineage.LineageNodeCalculationService.LineageNodeCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLineage.LineageNodeQueryService.LineageNodeQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLocalization.TranslationKeyQueryService.TranslationKeyQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLocalization.TranslationKeyService.TranslationKeyServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLoyalty.LoyaltyMemberCalculationService.LoyaltyMemberCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLoyalty.LoyaltyMemberQueryService.LoyaltyMemberQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiLoyalty.LoyaltyMemberService.LoyaltyMemberServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiMaintenance.WorkOrderCalculationService.WorkOrderCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiMaintenance.WorkOrderQueryService.WorkOrderQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiMaintenance.WorkOrderService.WorkOrderServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiMedia.MediaAssetCalculationService.MediaAssetCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiMedia.MediaAssetQueryService.MediaAssetQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiMedia.MediaAssetService.MediaAssetServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiMerchandising.CollectionService.CollectionServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiMessaging.ConversationCalculationService.ConversationCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiMessaging.ConversationQueryService.ConversationQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiMessaging.ConversationService.ConversationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiMetrics.MetricSeriesService.MetricSeriesServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiModelregistry.RegisteredModelCalculationService.RegisteredModelCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiModelregistry.RegisteredModelService.RegisteredModelServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiNotifications.NotificationQueryService.NotificationQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiNotifications.NotificationService.NotificationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiNotifications.NotificationValidationService.NotificationValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiOnboarding.EquipmentApprovalService.EquipmentApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiOnboarding.EquipmentCalculationService.EquipmentCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiOnboarding.EquipmentLifecycleService.EquipmentLifecycleServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiOnboarding.EquipmentReportingService.EquipmentReportingServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiOnboarding.EquipmentValidationService.EquipmentValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiOnboarding.EquipmentWorkflowService.EquipmentWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiOnboarding.OnboardingPlanApprovalService.OnboardingPlanApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiOnboarding.OnboardingPlanCalculationService.OnboardingPlanCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiOnboarding.OnboardingPlanLifecycleService.OnboardingPlanLifecycleServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiOnboarding.OnboardingPlanQueryService.OnboardingPlanQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiOnboarding.OnboardingPlanService.OnboardingPlanServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiOnboarding.OnboardingPlanValidationService.OnboardingPlanValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiOnboarding.OnboardingPlanWorkflowService.OnboardingPlanWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiOnboarding.OnboardingTaskApprovalService.OnboardingTaskApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiOnboarding.OnboardingTaskCalculationService.OnboardingTaskCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiOnboarding.OnboardingTaskQueryService.OnboardingTaskQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiOnboarding.OnboardingTaskReportingService.OnboardingTaskReportingServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiOnboarding.OnboardingTaskValidationService.OnboardingTaskValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiOrders.OrderCalculationService.OrderCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiPatientapp.PatientHomeQueryService.PatientHomeQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiPatientapp.PatientHomeService.PatientHomeServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiPatients.PatientCalculationService.PatientCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiPatients.PatientQueryService.PatientQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiPatients.PatientService.PatientServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiPayments.PaymentCalculationService.PaymentCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiPayroll.PayRunCalculationService.PayRunCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiPayroll.PayRunService.PayRunServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiPerformance.CalibrationApprovalService.CalibrationApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiPerformance.CalibrationCalculationService.CalibrationCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiPerformance.CalibrationLifecycleService.CalibrationLifecycleServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiPerformance.CalibrationValidationService.CalibrationValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiPerformance.FeedbackApprovalService.FeedbackApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiPerformance.FeedbackCalculationService.FeedbackCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiPerformance.FeedbackValidationService.FeedbackValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiPerformance.FeedbackWorkflowService.FeedbackWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiPerformance.GoalApprovalService.GoalApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiPerformance.GoalQueryService.GoalQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiPerformance.GoalValidationService.GoalValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiPerformance.GoalWorkflowService.GoalWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiPerformance.ReviewApprovalService.ReviewApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiPerformance.ReviewQueryService.ReviewQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiPerformance.ReviewService.ReviewServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiPerformance.ReviewWorkflowService.ReviewWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiPerformers.PerformerQueryService.PerformerQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiPerformers.PerformerService.PerformerServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiPersonalization.ProfileCalculationService.ProfileCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiPersonalization.ProfileQueryService.ProfileQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiPersonalization.ProfileService.ProfileServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiPipelines.PipelineCalculationService.PipelineCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiPipelines.PipelineService.PipelineServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiPrescriptions.PrescriptionCalculationService.PrescriptionCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiPrescriptions.PrescriptionQueryService.PrescriptionQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiPrescriptions.PrescriptionService.PrescriptionServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiPricing.PriceListQueryService.PriceListQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiProcurement.PurchaseRequisitionCalculationService.PurchaseRequisitionCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiPromotions.PromotionCalculationService.PromotionCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiPromotions.PromotionService.PromotionServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiRatelimiting.RateLimitCalculationService.RateLimitCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiRatelimiting.RateLimitQueryService.RateLimitQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiRatelimiting.RateLimitService.RateLimitServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiRecommendations.RecommendationSetQueryService.RecommendationSetQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiRecommendations.RecommendationSetService.RecommendationSetServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiRecruiting.CandidateApprovalService.CandidateApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiRecruiting.CandidateLifecycleService.CandidateLifecycleServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiRecruiting.CandidateQueryService.CandidateQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiRecruiting.CandidateReportingService.CandidateReportingServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiRecruiting.CandidateWorkflowService.CandidateWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiRecruiting.InterviewLifecycleService.InterviewLifecycleServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiRecruiting.InterviewReportingService.InterviewReportingServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiRecruiting.InterviewSyncService.InterviewSyncServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiRecruiting.JobOpeningApprovalService.JobOpeningApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiRecruiting.JobOpeningLifecycleService.JobOpeningLifecycleServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiRecruiting.JobOpeningQueryService.JobOpeningQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiRecruiting.JobOpeningService.JobOpeningServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiRecruiting.JobOpeningWorkflowService.JobOpeningWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiRecruiting.OfferApprovalService.OfferApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiRecruiting.OfferCalculationService.OfferCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiRecruiting.OfferLifecycleService.OfferLifecycleServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiRecruiting.OfferReportingService.OfferReportingServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiRecruiting.OfferSyncService.OfferSyncServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiRecruiting.OfferValidationService.OfferValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiReturns.ReturnRequestCalculationService.ReturnRequestCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiReturns.ReturnRequestQueryService.ReturnRequestQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiReturns.ReturnRequestService.ReturnRequestServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiReviews.ProductReviewQueryService.ProductReviewQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiRisk.RiskRegisterQueryService.RiskRegisterQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiRisk.RiskRegisterService.RiskRegisterServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiRouting.RouteCalculationService.RouteCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiRouting.RouteQueryService.RouteQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiScheduling.AvailabilityWindowApprovalService.AvailabilityWindowApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiScheduling.AvailabilityWindowCalculationService.AvailabilityWindowCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiScheduling.AvailabilityWindowLifecycleService.AvailabilityWindowLifecycleServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiScheduling.AvailabilityWindowValidationService.AvailabilityWindowValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiScheduling.AvailabilityWindowWorkflowService.AvailabilityWindowWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiScheduling.RosterApprovalService.RosterApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiScheduling.RosterCalculationService.RosterCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiScheduling.RosterQueryService.RosterQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiScheduling.RosterValidationService.RosterValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiScheduling.RosterWorkflowService.RosterWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiScheduling.ShiftApprovalService.ShiftApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiScheduling.ShiftQueryService.ShiftQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiScheduling.ShiftService.ShiftServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiScheduling.ShiftValidationService.ShiftValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiScheduling.ShiftWorkflowService.ShiftWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiScheduling.SwapRequestApprovalService.SwapRequestApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiScheduling.SwapRequestCalculationService.SwapRequestCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiScheduling.SwapRequestValidationService.SwapRequestValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiSearch.SearchIndexService.SearchIndexServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiSearch.SearchIndexValidationService.SearchIndexValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiSeating.SeatMapQueryService.SeatMapQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiSecrets.SecretQueryService.SecretQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiSecrets.SecretService.SecretServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiSellers.SellerCalculationService.SellerCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiSellers.SellerQueryService.SellerQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiSellers.SellerService.SellerServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiShipping.ShipmentQueryService.ShipmentQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiShipping.ShipmentService.ShipmentServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiShows.ShowQueryService.ShowQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiShows.ShowService.ShowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiSponsorships.SponsorCalculationService.SponsorCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiSubscriptions.SubscriptionService.SubscriptionServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiSurveys.SurveyQueryService.SurveyQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiSurveys.SurveyService.SurveyServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTelehealth.VideoVisitCalculationService.VideoVisitCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTelehealth.VideoVisitQueryService.VideoVisitQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTelematics.TelemetryReadingCalculationService.TelemetryReadingCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTelematics.TelemetryReadingQueryService.TelemetryReadingQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTicketing.TicketService.TicketServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTimekeeping.ClockEventApprovalService.ClockEventApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTimekeeping.ClockEventCalculationService.ClockEventCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTimekeeping.ClockEventQueryService.ClockEventQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTimekeeping.ClockEventValidationService.ClockEventValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTimekeeping.ClockEventWorkflowService.ClockEventWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTimekeeping.OvertimeApprovalService.OvertimeApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTimekeeping.OvertimeCalculationService.OvertimeCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTimekeeping.OvertimeLifecycleService.OvertimeLifecycleServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTimekeeping.OvertimeValidationService.OvertimeValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTimekeeping.OvertimeWorkflowService.OvertimeWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTimekeeping.TimesheetApprovalService.TimesheetApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTimekeeping.TimesheetQueryService.TimesheetQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTimekeeping.TimesheetService.TimesheetServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTimekeeping.TimesheetValidationService.TimesheetValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTimekeeping.TimesheetWorkflowService.TimesheetWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTours.TourQueryService.TourQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTravel.ItineraryApprovalService.ItineraryApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTravel.ItineraryCalculationService.ItineraryCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTravel.ItineraryLifecycleService.ItineraryLifecycleServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTravel.ItineraryReportingService.ItineraryReportingServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTravel.ItinerarySyncService.ItinerarySyncServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTravel.ItineraryValidationService.ItineraryValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTravel.TravelBookingApprovalService.TravelBookingApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTravel.TravelBookingCalculationService.TravelBookingCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTravel.TravelBookingLifecycleService.TravelBookingLifecycleServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTravel.TravelBookingQueryService.TravelBookingQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTravel.TravelBookingReportingService.TravelBookingReportingServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTravel.TravelBookingValidationService.TravelBookingValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTravel.TravelBookingWorkflowService.TravelBookingWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTravel.TravelPolicyApprovalService.TravelPolicyApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTravel.TravelPolicyCalculationService.TravelPolicyCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTravel.TravelPolicyLifecycleService.TravelPolicyLifecycleServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTravel.TravelPolicyReportingService.TravelPolicyReportingServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTravel.TravelPolicySyncService.TravelPolicySyncServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTravel.TravelPolicyValidationService.TravelPolicyValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTravel.TravelPolicyWorkflowService.TravelPolicyWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTravel.TripApprovalService.TripApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTravel.TripCalculationService.TripCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTravel.TripLifecycleService.TripLifecycleServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTravel.TripQueryService.TripQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTravel.TripService.TripServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTravel.TripValidationService.TripValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTravel.TripWorkflowService.TripWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTreasury.CashPositionQueryService.CashPositionQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiTreasury.CashPositionService.CashPositionServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiVenues.VenueQueryService.VenueQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiVenues.VenueService.VenueServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiVenues.VenueValidationService.VenueValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiWarehousing.WarehouseQueryService.WarehouseQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiWarehousing.WarehouseService.WarehouseServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiWebbff.PageCompositionQueryService.PageCompositionQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiWebbff.PageCompositionService.PageCompositionServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiWebhooks.WebhookEndpointQueryService.WebhookEndpointQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiWorkplaces.BuildingApprovalService.BuildingApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiWorkplaces.BuildingCalculationService.BuildingCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiWorkplaces.BuildingQueryService.BuildingQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiWorkplaces.BuildingService.BuildingServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiWorkplaces.BuildingValidationService.BuildingValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiWorkplaces.BuildingWorkflowService.BuildingWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiWorkplaces.DeskApprovalService.DeskApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiWorkplaces.DeskLifecycleService.DeskLifecycleServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiWorkplaces.DeskQueryService.DeskQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiWorkplaces.DeskValidationService.DeskValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiWorkplaces.DeskWorkflowService.DeskWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiWorkplaces.RoomBookingApprovalService.RoomBookingApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiWorkplaces.RoomBookingCalculationService.RoomBookingCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiWorkplaces.RoomBookingLifecycleService.RoomBookingLifecycleServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiWorkplaces.RoomBookingReportingService.RoomBookingReportingServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiWorkplaces.RoomBookingValidationService.RoomBookingValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiWorkplaces.RoomBookingWorkflowService.RoomBookingWorkflowServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiWorkplaces.VisitorPassApprovalService.VisitorPassApprovalServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiWorkplaces.VisitorPassCalculationService.VisitorPassCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiWorkplaces.VisitorPassLifecycleService.VisitorPassLifecycleServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiWorkplaces.VisitorPassReportingService.VisitorPassReportingServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiWorkplaces.VisitorPassSyncService.VisitorPassSyncServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiWorkplaces.VisitorPassValidationService.VisitorPassValidationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiYards.YardCalculationService.YardCalculationServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiYards.YardQueryService.YardQueryServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        services.AddSingleton(provider => new ApiYards.YardService.YardServiceClient(provider.GetRequiredService<ProbeInvoker>()));
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });
        var invoker = provider.GetRequiredService<ProbeInvoker>();
        using var oracle = System.Text.Json.JsonDocument.Parse(await System.IO.File.ReadAllTextAsync("expected.json"));
        var verified = 0;
        for (var variant = 0; variant < 3; variant++)
            verified += await Check0Async(provider, oracle.RootElement, invoker, variant);
        for (var variant = 0; variant < 3; variant++)
            verified += await Check1Async(provider, oracle.RootElement, invoker, variant);
        for (var variant = 0; variant < 3; variant++)
            verified += await Check2Async(provider, oracle.RootElement, invoker, variant);
        for (var variant = 0; variant < 3; variant++)
            verified += await Check3Async(provider, oracle.RootElement, invoker, variant);
        for (var variant = 0; variant < 3; variant++)
            verified += await Check4Async(provider, oracle.RootElement, invoker, variant);
        for (var variant = 0; variant < 3; variant++)
            verified += await Check5Async(provider, oracle.RootElement, invoker, variant);
        for (var variant = 0; variant < 3; variant++)
            verified += await Check6Async(provider, oracle.RootElement, invoker, variant);
        for (var variant = 0; variant < 3; variant++)
            verified += await Check7Async(provider, oracle.RootElement, invoker, variant);
        for (var variant = 0; variant < 3; variant++)
            verified += await Check8Async(provider, oracle.RootElement, invoker, variant);
        for (var variant = 0; variant < 3; variant++)
            verified += await Check9Async(provider, oracle.RootElement, invoker, variant);
        for (var variant = 0; variant < 3; variant++)
            verified += await Check10Async(provider, oracle.RootElement, invoker, variant);
        for (var variant = 0; variant < 3; variant++)
            verified += await Check11Async(provider, oracle.RootElement, invoker, variant);
        for (var variant = 0; variant < 3; variant++)
            verified += await Check12Async(provider, oracle.RootElement, invoker, variant);
        for (var variant = 0; variant < 3; variant++)
            verified += await Check13Async(provider, oracle.RootElement, invoker, variant);
        for (var variant = 0; variant < 3; variant++)
            verified += await Check14Async(provider, oracle.RootElement, invoker, variant);
        for (var variant = 0; variant < 3; variant++)
            verified += await Check15Async(provider, oracle.RootElement, invoker, variant);
        for (var variant = 0; variant < 3; variant++)
            verified += await Check16Async(provider, oracle.RootElement, invoker, variant);
        for (var variant = 0; variant < 3; variant++)
            verified += await Check17Async(provider, oracle.RootElement, invoker, variant);
        for (var variant = 0; variant < 3; variant++)
            verified += await Check18Async(provider, oracle.RootElement, invoker, variant);
        for (var variant = 0; variant < 3; variant++)
            verified += await Check19Async(provider, oracle.RootElement, invoker, variant);
        for (var variant = 0; variant < 3; variant++)
            verified += await Check20Async(provider, oracle.RootElement, invoker, variant);
        for (var variant = 0; variant < 3; variant++)
            verified += await Check21Async(provider, oracle.RootElement, invoker, variant);
        for (var variant = 0; variant < 3; variant++)
            verified += await Check22Async(provider, oracle.RootElement, invoker, variant);
        await ProbeData.CheckDatabaseAsync();
        // Identity answers lookups as the Go monolith does: the record of the id asked about, unless revoked.
        invoker.Answer = (method, request) => method == "acme.identity.v1.UserQueryService/LookupUser" && request is ApiIdentity.LookupUserRequest asked
            ? new ApiIdentity.LookupUserResponse
            {
                Result = new ApiCommon.OperationResult { Status = asked.UserId == "revoked-token" ? "revoked" : "ok" },
                User = new ApiIdentity.User { UserId = asked.UserId },
            }
            : null;
        var workload = Microsoft.Extensions.Options.Options.Create(new MeshOptions { ServiceToken = "workload-token" });
        var identity = new ApiIdentity.UserQueryService.UserQueryServiceClient(invoker.Intercept(new WorkloadIdentityInterceptor(workload)));
        var interceptor = new IdentityInterceptor(identity,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<IdentityInterceptor>.Instance);
        ProbeContext Caller(string? token, string? tenant)
        {
            var context = new ProbeContext();
            if (token is not null) context.RequestHeaders.Add("authorization", "Bearer " + token);
            if (tenant is not null) context.RequestHeaders.Add("x-tenant-id", tenant);
            return context;
        }
        invoker.Clear();
        string? served = null;
        await interceptor.UnaryServerHandler(new Acme.Common.V1.RequestContext(), Caller("probe-session", "probe-tenant"),
            (request, _) => { served = CallerTenant.Current; return Task.FromResult(request); });
        if (invoker.Calls.Count(call => call.Method == "acme.identity.v1.UserQueryService/LookupUser") != 1)
            throw new InvalidOperationException("The global interceptor did not call identity");
        if (served != "probe-tenant")
            throw new InvalidOperationException("The handler does not know its caller's tenant");
        var signed = invoker.Sent.Single().Headers;
        if (signed?.GetValue("authorization") != "Bearer workload-token" || signed.GetValue("x-tenant-id") != "probe-tenant")
            throw new InvalidOperationException("The identity lookup does not bear the workload credential");
        foreach (var (token, tenant, code) in new (string?, string?, StatusCode)[] {
            (null, "probe-tenant", StatusCode.Unauthenticated), ("probe-session", null, StatusCode.Unauthenticated),
            ("revoked-token", "probe-tenant", StatusCode.PermissionDenied) })
        {
            try
            {
                await interceptor.UnaryServerHandler(new Acme.Common.V1.RequestContext(), Caller(token, tenant),
                    (request, _) => Task.FromResult(request));
                throw new InvalidOperationException("An unauthenticated call was accepted");
            }
            catch (RpcException error) when (error.StatusCode == code) { }
        }
        invoker.Answer = null;
        var policy = new RetryPolicy(Microsoft.Extensions.Logging.Abstractions.NullLogger<RetryPolicy>.Instance);
        var attempts = 0;
        await policy.ExecuteAsync(_ => ++attempts < 3
            ? Task.FromException<string>(new RpcException(new Status(StatusCode.Unavailable, "retry")))
            : Task.FromResult("done"), CancellationToken.None);
        if (attempts != 3) throw new InvalidOperationException("Retry callbacks did not run");
        Console.WriteLine($"Verified {verified} field transfers across 23 handlers and SQLite persistence.");
    }
    private static async Task<int> Check0Async(IServiceProvider provider, System.Text.Json.JsonElement oracle, ProbeInvoker invoker, int variant)
    {
        var endpoint = provider.GetRequiredService<Acme.Workforce.Benefits.BenefitPlanApprovalEndpoint>();
        var request = new ApiBenefits.ApproveBenefitPlanRequest();
        ProbeData.Fill(request, "acme.benefits.v1.BenefitPlanApprovalService/ApproveBenefitPlan", variant);
        invoker.Clear();
        var context = new ProbeContext();
        var response = await endpoint.ApproveBenefitPlan(request, context);
        IMessage[] responses = [response];
        return ProbeData.CheckFlows(oracle, "acme.benefits.v1.BenefitPlanApprovalService/ApproveBenefitPlan", request, responses, invoker, []);
    }
    private static async Task<int> Check1Async(IServiceProvider provider, System.Text.Json.JsonElement oracle, ProbeInvoker invoker, int variant)
    {
        var endpoint = provider.GetRequiredService<Acme.Workforce.Benefits.BenefitPlanApprovalEndpoint>();
        var request = new ApiBenefits.DelegateBenefitPlanApprovalRequest();
        ProbeData.Fill(request, "acme.benefits.v1.BenefitPlanApprovalService/DelegateBenefitPlanApproval", variant);
        invoker.Clear();
        var context = new ProbeContext();
        var response = await endpoint.DelegateBenefitPlanApproval(request, context);
        IMessage[] responses = [response];
        return ProbeData.CheckFlows(oracle, "acme.benefits.v1.BenefitPlanApprovalService/DelegateBenefitPlanApproval", request, responses, invoker, []);
    }
    private static async Task<int> Check2Async(IServiceProvider provider, System.Text.Json.JsonElement oracle, ProbeInvoker invoker, int variant)
    {
        var endpoint = provider.GetRequiredService<Acme.Workforce.Benefits.BenefitPlanApprovalEndpoint>();
        var request = new ApiBenefits.EscalateBenefitPlanRequest();
        ProbeData.Fill(request, "acme.benefits.v1.BenefitPlanApprovalService/EscalateBenefitPlan", variant);
        invoker.Clear();
        var context = new ProbeContext();
        var response = await endpoint.EscalateBenefitPlan(request, context);
        IMessage[] responses = [response];
        return ProbeData.CheckFlows(oracle, "acme.benefits.v1.BenefitPlanApprovalService/EscalateBenefitPlan", request, responses, invoker, []);
    }
    private static async Task<int> Check3Async(IServiceProvider provider, System.Text.Json.JsonElement oracle, ProbeInvoker invoker, int variant)
    {
        var endpoint = provider.GetRequiredService<Acme.Workforce.Benefits.BenefitPlanApprovalEndpoint>();
        var request = new ApiBenefits.GetBenefitPlanApprovalStatusRequest();
        ProbeData.Fill(request, "acme.benefits.v1.BenefitPlanApprovalService/GetBenefitPlanApprovalStatus", variant);
        invoker.Clear();
        var context = new ProbeContext();
        var response = await endpoint.GetBenefitPlanApprovalStatus(request, context);
        IMessage[] responses = [response];
        return ProbeData.CheckFlows(oracle, "acme.benefits.v1.BenefitPlanApprovalService/GetBenefitPlanApprovalStatus", request, responses, invoker, []);
    }
    private static async Task<int> Check4Async(IServiceProvider provider, System.Text.Json.JsonElement oracle, ProbeInvoker invoker, int variant)
    {
        var endpoint = provider.GetRequiredService<Acme.Workforce.Benefits.BenefitPlanApprovalEndpoint>();
        var request = new ApiBenefits.ListPendingBenefitPlansRequest();
        ProbeData.Fill(request, "acme.benefits.v1.BenefitPlanApprovalService/ListPendingBenefitPlans", variant);
        invoker.Clear();
        var context = new ProbeContext();
        var response = await endpoint.ListPendingBenefitPlans(request, context);
        IMessage[] responses = [response];
        return ProbeData.CheckFlows(oracle, "acme.benefits.v1.BenefitPlanApprovalService/ListPendingBenefitPlans", request, responses, invoker, []);
    }
    private static async Task<int> Check5Async(IServiceProvider provider, System.Text.Json.JsonElement oracle, ProbeInvoker invoker, int variant)
    {
        var endpoint = provider.GetRequiredService<Acme.Workforce.Benefits.BenefitPlanApprovalEndpoint>();
        var request = new ApiBenefits.RecallBenefitPlanRequest();
        ProbeData.Fill(request, "acme.benefits.v1.BenefitPlanApprovalService/RecallBenefitPlan", variant);
        invoker.Clear();
        var context = new ProbeContext();
        var response = await endpoint.RecallBenefitPlan(request, context);
        IMessage[] responses = [response];
        return ProbeData.CheckFlows(oracle, "acme.benefits.v1.BenefitPlanApprovalService/RecallBenefitPlan", request, responses, invoker, []);
    }
    private static async Task<int> Check6Async(IServiceProvider provider, System.Text.Json.JsonElement oracle, ProbeInvoker invoker, int variant)
    {
        var endpoint = provider.GetRequiredService<Acme.Workforce.Benefits.BenefitPlanApprovalEndpoint>();
        var request = new ApiBenefits.SubmitBenefitPlanRequest();
        ProbeData.Fill(request, "acme.benefits.v1.BenefitPlanApprovalService/SubmitBenefitPlan", variant);
        invoker.Clear();
        var context = new ProbeContext();
        var response = await endpoint.SubmitBenefitPlan(request, context);
        IMessage[] responses = [response];
        return ProbeData.CheckFlows(oracle, "acme.benefits.v1.BenefitPlanApprovalService/SubmitBenefitPlan", request, responses, invoker, []);
    }
    private static async Task<int> Check7Async(IServiceProvider provider, System.Text.Json.JsonElement oracle, ProbeInvoker invoker, int variant)
    {
        var endpoint = provider.GetRequiredService<Acme.Workforce.Benefits.BenefitPlanQueryEndpoint>();
        var request = new ApiBenefits.CountBenefitPlansRequest();
        ProbeData.Fill(request, "acme.benefits.v1.BenefitPlanQueryService/CountBenefitPlans", variant);
        invoker.Clear();
        var context = new ProbeContext();
        var response = await endpoint.CountBenefitPlans(request, context);
        IMessage[] responses = [response];
        return ProbeData.CheckFlows(oracle, "acme.benefits.v1.BenefitPlanQueryService/CountBenefitPlans", request, responses, invoker, []);
    }
    private static async Task<int> Check8Async(IServiceProvider provider, System.Text.Json.JsonElement oracle, ProbeInvoker invoker, int variant)
    {
        var endpoint = provider.GetRequiredService<Acme.Workforce.Benefits.BenefitPlanQueryEndpoint>();
        var request = new ApiBenefits.ExportBenefitPlansRequest();
        ProbeData.Fill(request, "acme.benefits.v1.BenefitPlanQueryService/ExportBenefitPlans", variant);
        invoker.Clear();
        var context = new ProbeContext();
        var writer = new ProbeWriter<ApiBenefits.ExportBenefitPlansResponse>();
        await endpoint.ExportBenefitPlans(request, writer, context);
        var responses = writer.Messages.Cast<IMessage>().ToArray();
        return ProbeData.CheckFlows(oracle, "acme.benefits.v1.BenefitPlanQueryService/ExportBenefitPlans", request, responses, invoker, []);
    }
    private static async Task<int> Check9Async(IServiceProvider provider, System.Text.Json.JsonElement oracle, ProbeInvoker invoker, int variant)
    {
        var endpoint = provider.GetRequiredService<Acme.Workforce.Benefits.BenefitPlanQueryEndpoint>();
        var request = new ApiBenefits.LookupBenefitPlanRequest();
        ProbeData.Fill(request, "acme.benefits.v1.BenefitPlanQueryService/LookupBenefitPlan", variant);
        invoker.Clear();
        var context = new ProbeContext();
        var response = await endpoint.LookupBenefitPlan(request, context);
        IMessage[] responses = [response];
        return ProbeData.CheckFlows(oracle, "acme.benefits.v1.BenefitPlanQueryService/LookupBenefitPlan", request, responses, invoker, []);
    }
    private static async Task<int> Check10Async(IServiceProvider provider, System.Text.Json.JsonElement oracle, ProbeInvoker invoker, int variant)
    {
        var endpoint = provider.GetRequiredService<Acme.Workforce.Benefits.BenefitPlanQueryEndpoint>();
        var request = new ApiBenefits.ResolveBenefitPlansRequest();
        ProbeData.Fill(request, "acme.benefits.v1.BenefitPlanQueryService/ResolveBenefitPlans", variant);
        invoker.Clear();
        var context = new ProbeContext();
        var response = await endpoint.ResolveBenefitPlans(request, context);
        IMessage[] responses = [response];
        return ProbeData.CheckFlows(oracle, "acme.benefits.v1.BenefitPlanQueryService/ResolveBenefitPlans", request, responses, invoker, []);
    }
    private static async Task<int> Check11Async(IServiceProvider provider, System.Text.Json.JsonElement oracle, ProbeInvoker invoker, int variant)
    {
        var endpoint = provider.GetRequiredService<Acme.Workforce.Benefits.BenefitPlanQueryEndpoint>();
        var request = new ApiBenefits.SuggestBenefitPlansRequest();
        ProbeData.Fill(request, "acme.benefits.v1.BenefitPlanQueryService/SuggestBenefitPlans", variant);
        invoker.Clear();
        var context = new ProbeContext();
        var response = await endpoint.SuggestBenefitPlans(request, context);
        IMessage[] responses = [response];
        return ProbeData.CheckFlows(oracle, "acme.benefits.v1.BenefitPlanQueryService/SuggestBenefitPlans", request, responses, invoker, []);
    }
    private static async Task<int> Check12Async(IServiceProvider provider, System.Text.Json.JsonElement oracle, ProbeInvoker invoker, int variant)
    {
        var endpoint = provider.GetRequiredService<Acme.Workforce.Benefits.BenefitPlanEndpoint>();
        var request = new ApiBenefits.BatchCreateBenefitPlansRequest();
        ProbeData.Fill(request, "acme.benefits.v1.BenefitPlanService/BatchCreateBenefitPlans", variant);
        invoker.Clear();
        var context = new ProbeContext();
        var response = await endpoint.BatchCreateBenefitPlans(new ProbeReader<ApiBenefits.BatchCreateBenefitPlansRequest>(new[] { request }), context);
        IMessage[] responses = [response];
        return ProbeData.CheckFlows(oracle, "acme.benefits.v1.BenefitPlanService/BatchCreateBenefitPlans", request, responses, invoker, []);
    }
    private static async Task<int> Check13Async(IServiceProvider provider, System.Text.Json.JsonElement oracle, ProbeInvoker invoker, int variant)
    {
        var endpoint = provider.GetRequiredService<Acme.Workforce.Benefits.BenefitPlanWorkflowEndpoint>();
        var request = new ApiBenefits.PauseBenefitPlanWorkflowRequest();
        ProbeData.Fill(request, "acme.benefits.v1.BenefitPlanWorkflowService/PauseBenefitPlanWorkflow", variant);
        invoker.Clear();
        var context = new ProbeContext();
        var response = await endpoint.PauseBenefitPlanWorkflow(request, context);
        IMessage[] responses = [response];
        return ProbeData.CheckFlows(oracle, "acme.benefits.v1.BenefitPlanWorkflowService/PauseBenefitPlanWorkflow", request, responses, invoker, []);
    }
    private static async Task<int> Check14Async(IServiceProvider provider, System.Text.Json.JsonElement oracle, ProbeInvoker invoker, int variant)
    {
        var endpoint = provider.GetRequiredService<Acme.Workforce.Benefits.DependentApprovalEndpoint>();
        var request = new ApiBenefits.SubmitDependentRequest();
        ProbeData.Fill(request, "acme.benefits.v1.DependentApprovalService/SubmitDependent", variant);
        invoker.Clear();
        var context = new ProbeContext();
        var response = await endpoint.SubmitDependent(request, context);
        IMessage[] responses = [response];
        return ProbeData.CheckFlows(oracle, "acme.benefits.v1.DependentApprovalService/SubmitDependent", request, responses, invoker, []);
    }
    private static async Task<int> Check15Async(IServiceProvider provider, System.Text.Json.JsonElement oracle, ProbeInvoker invoker, int variant)
    {
        var endpoint = provider.GetRequiredService<Acme.Workforce.Benefits.DependentValidationEndpoint>();
        var request = new ApiBenefits.ExplainDependentDecisionRequest();
        ProbeData.Fill(request, "acme.benefits.v1.DependentValidationService/ExplainDependentDecision", variant);
        invoker.Clear();
        var context = new ProbeContext();
        var response = await endpoint.ExplainDependentDecision(request, context);
        IMessage[] responses = [response];
        return ProbeData.CheckFlows(oracle, "acme.benefits.v1.DependentValidationService/ExplainDependentDecision", request, responses, invoker, []);
    }
    private static async Task<int> Check16Async(IServiceProvider provider, System.Text.Json.JsonElement oracle, ProbeInvoker invoker, int variant)
    {
        var endpoint = provider.GetRequiredService<Acme.Workforce.Benefits.EnrollmentApprovalEndpoint>();
        var request = new ApiBenefits.ApproveEnrollmentRequest();
        ProbeData.Fill(request, "acme.benefits.v1.EnrollmentApprovalService/ApproveEnrollment", variant);
        invoker.Clear();
        var context = new ProbeContext();
        var response = await endpoint.ApproveEnrollment(request, context);
        IMessage[] responses = [response];
        return ProbeData.CheckFlows(oracle, "acme.benefits.v1.EnrollmentApprovalService/ApproveEnrollment", request, responses, invoker, ["acme.recruiting.v1.OfferValidationService/ScoreOffer"]);
    }
    private static async Task<int> Check17Async(IServiceProvider provider, System.Text.Json.JsonElement oracle, ProbeInvoker invoker, int variant)
    {
        var endpoint = provider.GetRequiredService<Acme.Workforce.Benefits.EnrollmentApprovalEndpoint>();
        var request = new ApiBenefits.RejectEnrollmentRequest();
        ProbeData.Fill(request, "acme.benefits.v1.EnrollmentApprovalService/RejectEnrollment", variant);
        invoker.Clear();
        var context = new ProbeContext();
        var response = await endpoint.RejectEnrollment(request, context);
        IMessage[] responses = [response];
        return ProbeData.CheckFlows(oracle, "acme.benefits.v1.EnrollmentApprovalService/RejectEnrollment", request, responses, invoker, ["acme.benefits.v1.LifeEventCalculationService/SimulateLifeEvent"]);
    }
    private static async Task<int> Check18Async(IServiceProvider provider, System.Text.Json.JsonElement oracle, ProbeInvoker invoker, int variant)
    {
        var endpoint = provider.GetRequiredService<Acme.Workforce.Benefits.EnrollmentQueryEndpoint>();
        var request = new ApiBenefits.SummarizeEnrollmentsRequest();
        ProbeData.Fill(request, "acme.benefits.v1.EnrollmentQueryService/SummarizeEnrollments", variant);
        invoker.Clear();
        var context = new ProbeContext();
        var response = await endpoint.SummarizeEnrollments(request, context);
        IMessage[] responses = [response];
        return ProbeData.CheckFlows(oracle, "acme.benefits.v1.EnrollmentQueryService/SummarizeEnrollments", request, responses, invoker, []);
    }
    private static async Task<int> Check19Async(IServiceProvider provider, System.Text.Json.JsonElement oracle, ProbeInvoker invoker, int variant)
    {
        var endpoint = provider.GetRequiredService<Acme.Workforce.Benefits.EnrollmentWorkflowEndpoint>();
        var request = new ApiBenefits.ResumeEnrollmentWorkflowRequest();
        ProbeData.Fill(request, "acme.benefits.v1.EnrollmentWorkflowService/ResumeEnrollmentWorkflow", variant);
        invoker.Clear();
        var context = new ProbeContext();
        var response = await endpoint.ResumeEnrollmentWorkflow(request, context);
        IMessage[] responses = [response];
        return ProbeData.CheckFlows(oracle, "acme.benefits.v1.EnrollmentWorkflowService/ResumeEnrollmentWorkflow", request, responses, invoker, ["acme.benefits.v1.BenefitPlanApprovalService/RejectBenefitPlan"]);
    }
    private static async Task<int> Check20Async(IServiceProvider provider, System.Text.Json.JsonElement oracle, ProbeInvoker invoker, int variant)
    {
        var endpoint = provider.GetRequiredService<Acme.Workforce.Corporateevents.CorporateEventApprovalEndpoint>();
        var request = new ApiCorporateevents.RejectCorporateEventRequest();
        ProbeData.Fill(request, "acme.corporateevents.v1.CorporateEventApprovalService/RejectCorporateEvent", variant);
        invoker.Clear();
        var context = new ProbeContext();
        var response = await endpoint.RejectCorporateEvent(request, context);
        IMessage[] responses = [response];
        return ProbeData.CheckFlows(oracle, "acme.corporateevents.v1.CorporateEventApprovalService/RejectCorporateEvent", request, responses, invoker, ["acme.corporateevents.v1.CorporateEventReportingService/CountCorporateEventTotals"]);
    }
    private static async Task<int> Check21Async(IServiceProvider provider, System.Text.Json.JsonElement oracle, ProbeInvoker invoker, int variant)
    {
        var endpoint = provider.GetRequiredService<Acme.Workforce.Corporateevents.VenueSyncEndpoint>();
        var request = new ApiCorporateevents.StreamVenueChangesRequest();
        ProbeData.Fill(request, "acme.corporateevents.v1.VenueSyncService/StreamVenueChanges", variant);
        invoker.Clear();
        var context = new ProbeContext();
        var writer = new ProbeWriter<ApiCorporateevents.StreamVenueChangesResponse>();
        await endpoint.StreamVenueChanges(new ProbeReader<ApiCorporateevents.StreamVenueChangesRequest>(new[] { request }), writer, context);
        var responses = writer.Messages.Cast<IMessage>().ToArray();
        return ProbeData.CheckFlows(oracle, "acme.corporateevents.v1.VenueSyncService/StreamVenueChanges", request, responses, invoker, []);
    }
    private static async Task<int> Check22Async(IServiceProvider provider, System.Text.Json.JsonElement oracle, ProbeInvoker invoker, int variant)
    {
        var endpoint = provider.GetRequiredService<Acme.Workforce.Employeeportal.SelfServiceRequestIntegrationEndpoint>();
        var request = new ApiEmployeeportal.RelaySelfServiceRequestMessagesRequest();
        ProbeData.Fill(request, "acme.employeeportal.v1.SelfServiceRequestIntegrationService/RelaySelfServiceRequestMessages", variant);
        invoker.Clear();
        var context = new ProbeContext();
        var writer = new ProbeWriter<ApiEmployeeportal.RelaySelfServiceRequestMessagesResponse>();
        await endpoint.RelaySelfServiceRequestMessages(new ProbeReader<ApiEmployeeportal.RelaySelfServiceRequestMessagesRequest>(new[] { request }), writer, context);
        var responses = writer.Messages.Cast<IMessage>().ToArray();
        return ProbeData.CheckFlows(oracle, "acme.employeeportal.v1.SelfServiceRequestIntegrationService/RelaySelfServiceRequestMessages", request, responses, invoker, []);
    }
}
