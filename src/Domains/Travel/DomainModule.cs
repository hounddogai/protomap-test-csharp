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

namespace Acme.Workforce.Travel;

/// <summary>
/// Registers the travel domain and its gRPC routes.
/// </summary>
public static class DomainModule
{
    public static IServiceCollection AddTravel(this IServiceCollection services)
    {
        services.AddTransient<ItineraryApprovalEndpoint>();
        services.AddTransient<IItineraryApprovalEndpointSendRejectItinerary0Port, ItineraryApprovalEndpointSendRejectItinerary0PortAdapter>();
        services.AddTransient<ItineraryCalculationEndpoint>();
        services.AddTransient<IItineraryCalculationEndpointSendGetItineraryRates0Port, ItineraryCalculationEndpointSendGetItineraryRates0PortAdapter>();
        services.AddSingleton<INotificationHandler, ItineraryCalculationEndpointRecalculateItineraries1NotificationHandler>();
        services.AddTransient<ItineraryLifecycleEndpoint>();
        services.AddTransient<IItineraryLifecycleEndpointSendSuspendItinerary0Port, ItineraryLifecycleEndpointSendSuspendItinerary0PortAdapter>();
        services.AddTransient<ItineraryReportingEndpoint>();
        services.AddTransient<IItineraryReportingEndpointSendStreamItineraryMetrics0Port, ItineraryReportingEndpointSendStreamItineraryMetrics0PortAdapter>();
        services.AddTransient<ItinerarySyncEndpoint>();
        services.AddTransient<IItinerarySyncEndpointSendSyncItineraries0Port, ItinerarySyncEndpointSendSyncItineraries0PortAdapter>();
        services.AddSingleton<INotificationHandler, ItinerarySyncEndpointSyncItineraries0NotificationHandler>();
        services.AddTransient<ItineraryValidationEndpoint>();
        services.AddTransient<TravelBookingApprovalEndpoint>();
        services.AddTransient<TravelBookingCalculationEndpoint>();
        services.AddTransient<ITravelBookingCalculationEndpointSendEstimateTravelBooking0Port, TravelBookingCalculationEndpointSendEstimateTravelBooking0PortAdapter>();
        services.AddSingleton<INotificationHandler, TravelBookingCalculationEndpointSimulateTravelBooking1NotificationHandler>();
        services.AddTransient<TravelBookingLifecycleEndpoint>();
        services.AddTransient<ITravelBookingLifecycleEndpointSendReinstateTravelBooking0Port, TravelBookingLifecycleEndpointSendReinstateTravelBooking0PortAdapter>();
        services.AddTransient<TravelBookingQueryEndpoint>();
        services.AddTransient<ITravelBookingQueryEndpointSendExportTravelBookings0Port, TravelBookingQueryEndpointSendExportTravelBookings0PortAdapter>();
        services.AddTransient<TravelBookingReportingEndpoint>();
        services.AddTransient<ITravelBookingReportingEndpointSendScheduleTravelBookingReport0Port, TravelBookingReportingEndpointSendScheduleTravelBookingReport0PortAdapter>();
        services.AddTransient<TravelBookingValidationEndpoint>();
        services.AddTransient<ITravelBookingValidationEndpointSendScoreTravelBooking0Port, TravelBookingValidationEndpointSendScoreTravelBooking0PortAdapter>();
        services.AddTransient<TravelBookingWorkflowEndpoint>();
        services.AddTransient<ITravelBookingWorkflowEndpointSendRetryTravelBookingWorkflow2Port, TravelBookingWorkflowEndpointSendRetryTravelBookingWorkflow2PortAdapter>();
        services.AddTransient<TravelPolicyApprovalEndpoint>();
        services.AddTransient<TravelPolicyCalculationEndpoint>();
        services.AddTransient<TravelPolicyLifecycleEndpoint>();
        services.AddTransient<ITravelPolicyLifecycleEndpointSendActivateTravelPolicy3Port, TravelPolicyLifecycleEndpointSendActivateTravelPolicy3PortAdapter>();
        services.AddTransient<TravelPolicyReportingEndpoint>();
        services.AddSingleton<INotificationHandler, TravelPolicyReportingEndpointFetchTravelPolicyReport0NotificationHandler>();
        services.AddTransient<TravelPolicySyncEndpoint>();
        services.AddTransient<ITravelPolicySyncEndpointSendReconcileTravelPolicies0Port, TravelPolicySyncEndpointSendReconcileTravelPolicies0PortAdapter>();
        services.AddTransient<TravelPolicyValidationEndpoint>();
        services.AddTransient<TravelPolicyWorkflowEndpoint>();
        services.AddSingleton<INotificationHandler, TravelPolicyWorkflowEndpointCancelTravelPolicyWorkflow1NotificationHandler>();
        services.AddTransient<TripApprovalEndpoint>();
        services.AddTransient<ITripApprovalEndpointSendRecallTrip0Port, TripApprovalEndpointSendRecallTrip0PortAdapter>();
        services.AddTransient<TripCalculationEndpoint>();
        services.AddTransient<ITripCalculationEndpointSendExplainTripCalculation3Port, TripCalculationEndpointSendExplainTripCalculation3PortAdapter>();
        services.AddSingleton<INotificationHandler, TripCalculationEndpointQuoteTrip0NotificationHandler>();
        services.AddTransient<TripLifecycleEndpoint>();
        services.AddTransient<TripQueryEndpoint>();
        services.AddTransient<ITripQueryEndpointSendResolveTrips0Port, TripQueryEndpointSendResolveTrips0PortAdapter>();
        services.AddTransient<TripEndpoint>();
        services.AddSingleton<INotificationHandler, TripEndpointDeleteTrip0NotificationHandler>();
        services.AddTransient<TripValidationEndpoint>();
        services.AddTransient<ITripValidationEndpointSendNormalizeTrip0Port, TripValidationEndpointSendNormalizeTrip0PortAdapter>();
        services.AddTransient<TripWorkflowEndpoint>();
        services.AddTransient<ITripWorkflowEndpointSendResumeTripWorkflow1Port, TripWorkflowEndpointSendResumeTripWorkflow1PortAdapter>();
        return services;
    }
    public static void MapTravel(this IEndpointRouteBuilder routes)
    {
        routes.MapGrpcService<ItineraryApprovalEndpoint>();
        routes.MapGrpcService<ItineraryCalculationEndpoint>();
        routes.MapGrpcService<ItineraryLifecycleEndpoint>();
        routes.MapGrpcService<ItineraryReportingEndpoint>();
        routes.MapGrpcService<ItinerarySyncEndpoint>();
        routes.MapGrpcService<ItineraryValidationEndpoint>();
        routes.MapGrpcService<TravelBookingApprovalEndpoint>();
        routes.MapGrpcService<TravelBookingCalculationEndpoint>();
        routes.MapGrpcService<TravelBookingLifecycleEndpoint>();
        routes.MapGrpcService<TravelBookingQueryEndpoint>();
        routes.MapGrpcService<TravelBookingReportingEndpoint>();
        routes.MapGrpcService<TravelBookingValidationEndpoint>();
        routes.MapGrpcService<TravelBookingWorkflowEndpoint>();
        routes.MapGrpcService<TravelPolicyApprovalEndpoint>();
        routes.MapGrpcService<TravelPolicyCalculationEndpoint>();
        routes.MapGrpcService<TravelPolicyLifecycleEndpoint>();
        routes.MapGrpcService<TravelPolicyReportingEndpoint>();
        routes.MapGrpcService<TravelPolicySyncEndpoint>();
        routes.MapGrpcService<TravelPolicyValidationEndpoint>();
        routes.MapGrpcService<TravelPolicyWorkflowEndpoint>();
        routes.MapGrpcService<TripApprovalEndpoint>();
        routes.MapGrpcService<TripCalculationEndpoint>();
        routes.MapGrpcService<TripLifecycleEndpoint>();
        routes.MapGrpcService<TripQueryEndpoint>();
        routes.MapGrpcService<TripEndpoint>();
        routes.MapGrpcService<TripValidationEndpoint>();
        routes.MapGrpcService<TripWorkflowEndpoint>();
    }
}
