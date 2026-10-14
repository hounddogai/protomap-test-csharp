using Acme.Workforce.Platform;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiBenefits = Acme.Benefits.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Programs.Admin;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(args);
        builder.Services.AddWorkforcePlatform(builder.Configuration);
        using var host = builder.Build();
        using var deadline = new CancellationTokenSource(TimeSpan.FromHours(2));
        var token = deadline.Token;
        var tenant = builder.Configuration["Tenant"] ?? throw new InvalidOperationException("Set --Tenant");
        // The program calls with the platform's workload credential, so it acts as its service account.
        var context = new Acme.Common.V1.RequestContext
        {
            TenantId = tenant,
            Actor = new Acme.Common.V1.Actor { UserId = "workforce-admin", ServiceAccount = true },
        };
        switch (builder.Configuration["Command"])
        {
            case "benefits":
            {
                var client = host.Services.GetRequiredService<ApiBenefits.BenefitPlanApprovalService.BenefitPlanApprovalServiceClient>();
                using var call = client.GetBenefitPlanApprovalStatusAsync(new ApiBenefits.GetBenefitPlanApprovalStatusRequest
                {
                    BenefitPlanId = builder.Configuration["Id"] ?? "",
                    Context = context,
                }, cancellationToken: token);
                Console.WriteLine(JsonFormatter.Default.Format(await call.ResponseAsync));
                break;
            }
            case "corporateevents":
            {
                var client = host.Services.GetRequiredService<ApiCorporateevents.CorporateEventApprovalService.CorporateEventApprovalServiceClient>();
                using var call = client.GetCorporateEventApprovalStatusAsync(new ApiCorporateevents.GetCorporateEventApprovalStatusRequest
                {
                    Context = context,
                    CorporateEventId = builder.Configuration["Id"] ?? "",
                }, cancellationToken: token);
                Console.WriteLine(JsonFormatter.Default.Format(await call.ResponseAsync));
                break;
            }
            case "devices":
            {
                var client = host.Services.GetRequiredService<ApiDevices.ComplianceScanApprovalService.ComplianceScanApprovalServiceClient>();
                using var call = client.GetComplianceScanApprovalStatusAsync(new ApiDevices.GetComplianceScanApprovalStatusRequest
                {
                    ComplianceScanId = builder.Configuration["Id"] ?? "",
                    Context = context,
                }, cancellationToken: token);
                Console.WriteLine(JsonFormatter.Default.Format(await call.ResponseAsync));
                break;
            }
            case "documents":
            {
                var client = host.Services.GetRequiredService<ApiDocuments.DocumentAdminService.DocumentAdminServiceClient>();
                using var call = client.InspectDocumentAsync(new ApiDocuments.InspectDocumentRequest
                {
                    Context = context,
                    DocumentId = builder.Configuration["Id"] ?? "",
                }, cancellationToken: token);
                Console.WriteLine(JsonFormatter.Default.Format(await call.ResponseAsync));
                break;
            }
            case "employeeportal":
            {
                var client = host.Services.GetRequiredService<ApiEmployeeportal.AnnouncementNotificationService.AnnouncementNotificationServiceClient>();
                using var call = client.GetAnnouncementPreferencesAsync(new ApiEmployeeportal.GetAnnouncementPreferencesRequest
                {
                    AnnouncementId = builder.Configuration["Id"] ?? "",
                    Context = context,
                }, cancellationToken: token);
                Console.WriteLine(JsonFormatter.Default.Format(await call.ResponseAsync));
                break;
            }
            case "employees":
            {
                var client = host.Services.GetRequiredService<ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient>();
                using var call = client.LookupEmployeeAsync(new ApiEmployees.LookupEmployeeRequest
                {
                    Context = context,
                    EmployeeId = builder.Configuration["Id"] ?? "",
                }, cancellationToken: token);
                Console.WriteLine(JsonFormatter.Default.Format(await call.ResponseAsync));
                break;
            }
            case "helpdesk":
            {
                var client = host.Services.GetRequiredService<ApiHelpdesk.EscalationCalculationService.EscalationCalculationServiceClient>();
                using var call = client.GetEscalationRatesAsync(new ApiHelpdesk.GetEscalationRatesRequest
                {
                    Context = context,
                    EscalationId = builder.Configuration["Id"] ?? "",
                }, cancellationToken: token);
                Console.WriteLine(JsonFormatter.Default.Format(await call.ResponseAsync));
                break;
            }
            case "itsm":
            {
                var client = host.Services.GetRequiredService<ApiItsm.ChangeRequestApprovalService.ChangeRequestApprovalServiceClient>();
                using var call = client.GetChangeRequestApprovalStatusAsync(new ApiItsm.GetChangeRequestApprovalStatusRequest
                {
                    ChangeRequestId = builder.Configuration["Id"] ?? "",
                    Context = context,
                }, cancellationToken: token);
                Console.WriteLine(JsonFormatter.Default.Format(await call.ResponseAsync));
                break;
            }
            case "learning":
            {
                var client = host.Services.GetRequiredService<ApiLearning.CertificationApprovalService.CertificationApprovalServiceClient>();
                using var call = client.GetCertificationApprovalStatusAsync(new ApiLearning.GetCertificationApprovalStatusRequest
                {
                    CertificationId = builder.Configuration["Id"] ?? "",
                    Context = context,
                }, cancellationToken: token);
                Console.WriteLine(JsonFormatter.Default.Format(await call.ResponseAsync));
                break;
            }
            case "licensing":
            {
                var client = host.Services.GetRequiredService<ApiLicensing.EntitlementApprovalService.EntitlementApprovalServiceClient>();
                using var call = client.GetEntitlementApprovalStatusAsync(new ApiLicensing.GetEntitlementApprovalStatusRequest
                {
                    Context = context,
                    EntitlementId = builder.Configuration["Id"] ?? "",
                }, cancellationToken: token);
                Console.WriteLine(JsonFormatter.Default.Format(await call.ResponseAsync));
                break;
            }
            case "onboarding":
            {
                var client = host.Services.GetRequiredService<ApiOnboarding.EquipmentApprovalService.EquipmentApprovalServiceClient>();
                using var call = client.GetEquipmentApprovalStatusAsync(new ApiOnboarding.GetEquipmentApprovalStatusRequest
                {
                    Context = context,
                    EquipmentId = builder.Configuration["Id"] ?? "",
                }, cancellationToken: token);
                Console.WriteLine(JsonFormatter.Default.Format(await call.ResponseAsync));
                break;
            }
            case "performance":
            {
                var client = host.Services.GetRequiredService<ApiPerformance.CalibrationApprovalService.CalibrationApprovalServiceClient>();
                using var call = client.GetCalibrationApprovalStatusAsync(new ApiPerformance.GetCalibrationApprovalStatusRequest
                {
                    CalibrationId = builder.Configuration["Id"] ?? "",
                    Context = context,
                }, cancellationToken: token);
                Console.WriteLine(JsonFormatter.Default.Format(await call.ResponseAsync));
                break;
            }
            case "recruiting":
            {
                var client = host.Services.GetRequiredService<ApiRecruiting.CandidateApprovalService.CandidateApprovalServiceClient>();
                using var call = client.GetCandidateApprovalStatusAsync(new ApiRecruiting.GetCandidateApprovalStatusRequest
                {
                    CandidateId = builder.Configuration["Id"] ?? "",
                    Context = context,
                }, cancellationToken: token);
                Console.WriteLine(JsonFormatter.Default.Format(await call.ResponseAsync));
                break;
            }
            case "scheduling":
            {
                var client = host.Services.GetRequiredService<ApiScheduling.AvailabilityWindowCalculationService.AvailabilityWindowCalculationServiceClient>();
                using var call = client.GetAvailabilityWindowRatesAsync(new ApiScheduling.GetAvailabilityWindowRatesRequest
                {
                    AvailabilityWindowId = builder.Configuration["Id"] ?? "",
                    Context = context,
                }, cancellationToken: token);
                Console.WriteLine(JsonFormatter.Default.Format(await call.ResponseAsync));
                break;
            }
            case "timekeeping":
            {
                var client = host.Services.GetRequiredService<ApiTimekeeping.ClockEventApprovalService.ClockEventApprovalServiceClient>();
                using var call = client.GetClockEventApprovalStatusAsync(new ApiTimekeeping.GetClockEventApprovalStatusRequest
                {
                    ClockEventId = builder.Configuration["Id"] ?? "",
                    Context = context,
                }, cancellationToken: token);
                Console.WriteLine(JsonFormatter.Default.Format(await call.ResponseAsync));
                break;
            }
            case "travel":
            {
                var client = host.Services.GetRequiredService<ApiTravel.ItineraryApprovalService.ItineraryApprovalServiceClient>();
                using var call = client.GetItineraryApprovalStatusAsync(new ApiTravel.GetItineraryApprovalStatusRequest
                {
                    Context = context,
                    ItineraryId = builder.Configuration["Id"] ?? "",
                }, cancellationToken: token);
                Console.WriteLine(JsonFormatter.Default.Format(await call.ResponseAsync));
                break;
            }
            case "workplaces":
            {
                var client = host.Services.GetRequiredService<ApiWorkplaces.BuildingApprovalService.BuildingApprovalServiceClient>();
                using var call = client.GetBuildingApprovalStatusAsync(new ApiWorkplaces.GetBuildingApprovalStatusRequest
                {
                    BuildingId = builder.Configuration["Id"] ?? "",
                    Context = context,
                }, cancellationToken: token);
                Console.WriteLine(JsonFormatter.Default.Format(await call.ResponseAsync));
                break;
            }
            case "invoke":
            {
                var service = builder.Configuration["Service"] ?? throw new ArgumentException("Set --Service");
                var method = builder.Configuration["Method"] ?? throw new ArgumentException("Set --Method");
                var file = builder.Configuration["RequestFile"] ?? throw new ArgumentException("Set --RequestFile");
                var input = await System.IO.File.ReadAllBytesAsync(file, token);
                var bytes = Marshallers.Create<byte[]>(value => value, value => value);
                var descriptor = new Method<byte[], byte[]>(MethodType.Unary, service, method, bytes, bytes);
                var invoker = host.Services.GetRequiredService<CallInvoker>();
                using var call = invoker.AsyncUnaryCall(descriptor, null, new CallOptions(cancellationToken: token), input);
                Console.WriteLine(Convert.ToBase64String(await call.ResponseAsync));
                break;
            }
            default:
                throw new ArgumentException("Set --Command to a domain or invoke");
        }
    }
}
