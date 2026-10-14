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

namespace Acme.Workforce.Documents;

/// <summary>
/// Registers the documents domain and its gRPC routes.
/// </summary>
public static class DomainModule
{
    public static IServiceCollection AddDocuments(this IServiceCollection services)
    {
        services.AddTransient<DocumentAdminEndpoint>();
        services.AddTransient<DocumentExportEndpoint>();
        services.AddTransient<DocumentFolderAdminEndpoint>();
        services.AddTransient<DocumentFolderExportEndpoint>();
        services.AddTransient<DocumentFolderHistoryEndpoint>();
        services.AddTransient<DocumentFolderNotificationEndpoint>();
        services.AddTransient<DocumentFolderQueryEndpoint>();
        services.AddTransient<DocumentFolderSyncEndpoint>();
        services.AddTransient<DocumentFolderValidationEndpoint>();
        services.AddTransient<DocumentHistoryEndpoint>();
        services.AddTransient<DocumentNotificationEndpoint>();
        services.AddSingleton<INotificationHandler, DocumentNotificationEndpointSendDocumentTestNotice0NotificationHandler>();
        services.AddTransient<DocumentQueryEndpoint>();
        services.AddTransient<DocumentEndpoint>();
        services.AddTransient<DocumentSyncEndpoint>();
        services.AddTransient<DocumentTemplateAdminEndpoint>();
        services.AddTransient<DocumentTemplateCalculationEndpoint>();
        services.AddTransient<IDocumentTemplateCalculationEndpointSendEstimateDocumentTemplate0Port, DocumentTemplateCalculationEndpointSendEstimateDocumentTemplate0PortAdapter>();
        services.AddTransient<DocumentTemplateExportEndpoint>();
        services.AddTransient<DocumentTemplateHistoryEndpoint>();
        services.AddTransient<DocumentTemplateNotificationEndpoint>();
        services.AddTransient<DocumentTemplateSyncEndpoint>();
        services.AddTransient<DocumentTemplateWorkflowEndpoint>();
        services.AddTransient<DocumentValidationEndpoint>();
        services.AddTransient<SignatureAdminEndpoint>();
        services.AddTransient<SignatureCalculationEndpoint>();
        services.AddTransient<SignatureExportEndpoint>();
        services.AddTransient<SignatureHistoryEndpoint>();
        services.AddTransient<SignatureNotificationEndpoint>();
        services.AddTransient<SignatureSyncEndpoint>();
        services.AddTransient<SignatureValidationEndpoint>();
        return services;
    }
    public static void MapDocuments(this IEndpointRouteBuilder routes)
    {
        routes.MapGrpcService<DocumentAdminEndpoint>();
        routes.MapGrpcService<DocumentExportEndpoint>();
        routes.MapGrpcService<DocumentFolderAdminEndpoint>();
        routes.MapGrpcService<DocumentFolderExportEndpoint>();
        routes.MapGrpcService<DocumentFolderHistoryEndpoint>();
        routes.MapGrpcService<DocumentFolderNotificationEndpoint>();
        routes.MapGrpcService<DocumentFolderQueryEndpoint>();
        routes.MapGrpcService<DocumentFolderSyncEndpoint>();
        routes.MapGrpcService<DocumentFolderValidationEndpoint>();
        routes.MapGrpcService<DocumentHistoryEndpoint>();
        routes.MapGrpcService<DocumentNotificationEndpoint>();
        routes.MapGrpcService<DocumentQueryEndpoint>();
        routes.MapGrpcService<DocumentEndpoint>();
        routes.MapGrpcService<DocumentSyncEndpoint>();
        routes.MapGrpcService<DocumentTemplateAdminEndpoint>();
        routes.MapGrpcService<DocumentTemplateCalculationEndpoint>();
        routes.MapGrpcService<DocumentTemplateExportEndpoint>();
        routes.MapGrpcService<DocumentTemplateHistoryEndpoint>();
        routes.MapGrpcService<DocumentTemplateNotificationEndpoint>();
        routes.MapGrpcService<DocumentTemplateSyncEndpoint>();
        routes.MapGrpcService<DocumentTemplateWorkflowEndpoint>();
        routes.MapGrpcService<DocumentValidationEndpoint>();
        routes.MapGrpcService<SignatureAdminEndpoint>();
        routes.MapGrpcService<SignatureCalculationEndpoint>();
        routes.MapGrpcService<SignatureExportEndpoint>();
        routes.MapGrpcService<SignatureHistoryEndpoint>();
        routes.MapGrpcService<SignatureNotificationEndpoint>();
        routes.MapGrpcService<SignatureSyncEndpoint>();
        routes.MapGrpcService<SignatureValidationEndpoint>();
    }
}
