using Acme.Workforce.Platform;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Acme.Workforce.Corporateevents;

/// <summary>
/// Registers the corporateevents domain and its gRPC routes.
/// </summary>
public static class DomainModule
{
    public static IServiceCollection AddCorporateevents(this IServiceCollection services)
    {
        services.AddTransient<CorporateEventApprovalEndpoint>();
        services.AddSingleton<INotificationHandler, CorporateEventApprovalEndpointGetCorporateEventApprovalStatus0NotificationHandler>();
        services.AddTransient<CorporateEventCalculationEndpoint>();
        services.AddTransient<CorporateEventLifecycleEndpoint>();
        services.AddTransient<ICorporateEventLifecycleEndpointSendExpireCorporateEvent2Port, CorporateEventLifecycleEndpointSendExpireCorporateEvent2PortAdapter>();
        services.AddSingleton<INotificationHandler, CorporateEventLifecycleEndpointSuspendCorporateEvent0NotificationHandler>();
        services.AddTransient<CorporateEventQueryEndpoint>();
        services.AddTransient<ICorporateEventQueryEndpointSendExportCorporateEvents2Port, CorporateEventQueryEndpointSendExportCorporateEvents2PortAdapter>();
        services.AddTransient<CorporateEventReportingEndpoint>();
        services.AddTransient<ICorporateEventReportingEndpointSendListCorporateEventReports0Port, CorporateEventReportingEndpointSendListCorporateEventReports0PortAdapter>();
        services.AddTransient<CorporateEventEndpoint>();
        services.AddTransient<ICorporateEventEndpointSendCreateCorporateEvent0Port, CorporateEventEndpointSendCreateCorporateEvent0PortAdapter>();
        services.AddSingleton<INotificationHandler, CorporateEventEndpointGetCorporateEvent0NotificationHandler>();
        services.AddTransient<CorporateEventValidationEndpoint>();
        services.AddTransient<ICorporateEventValidationEndpointSendGetCorporateEventRules1Port, CorporateEventValidationEndpointSendGetCorporateEventRules1PortAdapter>();
        services.AddTransient<CorporateEventWorkflowEndpoint>();
        services.AddTransient<ICorporateEventWorkflowEndpointSendAdvanceCorporateEventWorkflow1Port, CorporateEventWorkflowEndpointSendAdvanceCorporateEventWorkflow1PortAdapter>();
        services.AddTransient<RegistrationApprovalEndpoint>();
        services.AddTransient<IRegistrationApprovalEndpointSendSubmitRegistration1Port, RegistrationApprovalEndpointSendSubmitRegistration1PortAdapter>();
        services.AddTransient<RegistrationCalculationEndpoint>();
        services.AddTransient<RegistrationLifecycleEndpoint>();
        services.AddTransient<RegistrationQueryEndpoint>();
        services.AddTransient<IRegistrationQueryEndpointSendPeekRegistration0Port, RegistrationQueryEndpointSendPeekRegistration0PortAdapter>();
        services.AddTransient<RegistrationReportingEndpoint>();
        services.AddTransient<IRegistrationReportingEndpointSendScheduleRegistrationReport3Port, RegistrationReportingEndpointSendScheduleRegistrationReport3PortAdapter>();
        services.AddTransient<RegistrationValidationEndpoint>();
        services.AddSingleton<INotificationHandler, RegistrationValidationEndpointValidateRegistration0NotificationHandler>();
        services.AddTransient<RegistrationWorkflowEndpoint>();
        services.AddTransient<IRegistrationWorkflowEndpointSendSignalRegistrationWorkflow0Port, RegistrationWorkflowEndpointSendSignalRegistrationWorkflow0PortAdapter>();
        services.AddTransient<VenueApprovalEndpoint>();
        services.AddTransient<VenueCalculationEndpoint>();
        services.AddTransient<IVenueCalculationEndpointSendQuoteVenue2Port, VenueCalculationEndpointSendQuoteVenue2PortAdapter>();
        services.AddSingleton<INotificationHandler, VenueCalculationEndpointQuoteVenue0NotificationHandler>();
        services.AddTransient<VenueLifecycleEndpoint>();
        services.AddTransient<IVenueLifecycleEndpointSendRenewVenue0Port, VenueLifecycleEndpointSendRenewVenue0PortAdapter>();
        services.AddTransient<VenueReportingEndpoint>();
        services.AddTransient<IVenueReportingEndpointSendShareVenueReport1Port, VenueReportingEndpointSendShareVenueReport1PortAdapter>();
        services.AddTransient<VenueSyncEndpoint>();
        services.AddSingleton<INotificationHandler, VenueSyncEndpointPushVenue3NotificationHandler>();
        services.AddTransient<VenueValidationEndpoint>();
        services.AddTransient<IVenueValidationEndpointSendValidateVenue0Port, VenueValidationEndpointSendValidateVenue0PortAdapter>();
        services.AddTransient<VenueWorkflowEndpoint>();
        services.AddTransient<IVenueWorkflowEndpointSendSignalVenueWorkflow0Port, VenueWorkflowEndpointSendSignalVenueWorkflow0PortAdapter>();
        return services;
    }
    public static void MapCorporateevents(this IEndpointRouteBuilder routes)
    {
        routes.MapGrpcService<CorporateEventApprovalEndpoint>();
        routes.MapGrpcService<CorporateEventCalculationEndpoint>();
        routes.MapGrpcService<CorporateEventLifecycleEndpoint>();
        routes.MapGrpcService<CorporateEventQueryEndpoint>();
        routes.MapGrpcService<CorporateEventReportingEndpoint>();
        routes.MapGrpcService<CorporateEventEndpoint>();
        routes.MapGrpcService<CorporateEventValidationEndpoint>();
        routes.MapGrpcService<CorporateEventWorkflowEndpoint>();
        routes.MapGrpcService<RegistrationApprovalEndpoint>();
        routes.MapGrpcService<RegistrationCalculationEndpoint>();
        routes.MapGrpcService<RegistrationLifecycleEndpoint>();
        routes.MapGrpcService<RegistrationQueryEndpoint>();
        routes.MapGrpcService<RegistrationReportingEndpoint>();
        routes.MapGrpcService<RegistrationValidationEndpoint>();
        routes.MapGrpcService<RegistrationWorkflowEndpoint>();
        routes.MapGrpcService<VenueApprovalEndpoint>();
        routes.MapGrpcService<VenueCalculationEndpoint>();
        routes.MapGrpcService<VenueLifecycleEndpoint>();
        routes.MapGrpcService<VenueReportingEndpoint>();
        routes.MapGrpcService<VenueSyncEndpoint>();
        routes.MapGrpcService<VenueValidationEndpoint>();
        routes.MapGrpcService<VenueWorkflowEndpoint>();
    }
}
