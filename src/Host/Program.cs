using Acme.Workforce.Benefits;
using Acme.Workforce.Corporateevents;
using Acme.Workforce.Devices;
using Acme.Workforce.Documents;
using Acme.Workforce.Employeeportal;
using Acme.Workforce.Employees;
using Acme.Workforce.Helpdesk;
using Acme.Workforce.Itsm;
using Acme.Workforce.Learning;
using Acme.Workforce.Licensing;
using Acme.Workforce.Onboarding;
using Acme.Workforce.Performance;
using Acme.Workforce.Platform;
using Acme.Workforce.Recruiting;
using Acme.Workforce.Scheduling;
using Acme.Workforce.Timekeeping;
using Acme.Workforce.Travel;
using Acme.Workforce.Workplaces;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Acme.Workforce.Host;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Host.UseDefaultServiceProvider(options =>
        {
            options.ValidateScopes = true;
            options.ValidateOnBuild = true;
        });
        builder.Services.AddWorkforcePlatform(builder.Configuration);
        builder.Services.AddTransient<IdentityInterceptor>();
        builder.Services.AddGrpc(options => options.Interceptors.Add<IdentityInterceptor>());
        builder.Services.AddBenefits();
        builder.Services.AddCorporateevents();
        builder.Services.AddDevices();
        builder.Services.AddDocuments();
        builder.Services.AddEmployeeportal();
        builder.Services.AddEmployees();
        builder.Services.AddHelpdesk();
        builder.Services.AddItsm();
        builder.Services.AddLearning();
        builder.Services.AddLicensing();
        builder.Services.AddOnboarding();
        builder.Services.AddPerformance();
        builder.Services.AddRecruiting();
        builder.Services.AddScheduling();
        builder.Services.AddTimekeeping();
        builder.Services.AddTravel();
        builder.Services.AddWorkplaces();
        var app = builder.Build();
        app.MapBenefits();
        app.MapCorporateevents();
        app.MapDevices();
        app.MapDocuments();
        app.MapEmployeeportal();
        app.MapEmployees();
        app.MapHelpdesk();
        app.MapItsm();
        app.MapLearning();
        app.MapLicensing();
        app.MapOnboarding();
        app.MapPerformance();
        app.MapRecruiting();
        app.MapScheduling();
        app.MapTimekeeping();
        app.MapTravel();
        app.MapWorkplaces();
        app.MapGet("/health", () => "workforce ready");
        await app.Services.GetRequiredService<WorkforceDatabase>().InitializeAsync(CancellationToken.None);
        if (args.Contains("--verify-wiring", StringComparer.Ordinal))
        {
            Console.WriteLine("All workforce endpoints and dependencies registered successfully.");
            await app.DisposeAsync();
            return;
        }
        await app.RunAsync();
    }
}
