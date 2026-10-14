namespace Acme.Workforce.Ops.TalentCtl;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.CareerPath.Slot;
using Acme.Workforce.Talent.SuccessionPlan.Budget;

public static class Program
{
    public static void Main()
    {
        var clock = Clock.System();
        var commands = new Dictionary<string, Func<bool>>
        {
            ["succession-plan-preference"] = () =>
            {
                var store = SuccessionPlanPreferenceStore.NewMemorySuccessionPlanPreferenceStore();
                for (var index = 0; index < 12; index++)
                {
                    store.Save(SuccessionPlanPreference.NewSuccessionPlanPreference($"succession-plan-preference-{index}", $"Sample succession plan preference {index}"));
                }
                var flow = SuccessionPlanPreferenceWorkflow.NewSuccessionPlanPreferenceManager(store, clock, Flags.NewRegistry());
                try
                {
                    flow.Advance("succession-plan-preference-0");
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
                var items = store.List();
                var report = SuccessionPlanPreferenceReport.BuildSuccessionPlanPreferenceReport("Succession Plan Preference", items, clock);
                Console.WriteLine(report.Render());
                return flow.ReviewStale((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()) >= 0;
            },
            ["career-path-forecast"] = () =>
            {
                var store = CareerPathForecastStore.NewMemoryCareerPathForecastStore();
                for (var index = 0; index < 10; index++)
                {
                    store.Save(CareerPathForecast.NewCareerPathForecast($"career-path-forecast-{index}", $"Sample career path forecast {index}"));
                }
                var flow = CareerPathForecastWorkflow.NewCareerPathForecastDesk(store, clock);
                try
                {
                    flow.Start("career-path-forecast-0");
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
                var items = store.List();
                var report = CareerPathForecastReport.BuildCareerPathForecastReport("Career Path Forecast", items, clock);
                Console.WriteLine(report.Render());
                return flow.ReviewStale((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()) >= 0;
            },
        };
        var failed = 0;
        foreach (var name in commands.Keys.Order(StringComparer.Ordinal))
        {
            var command = commands[name];
            if (!command())
            {
                Console.WriteLine($"{name} failed");
                failed += 1;
            }
        }
        if (failed > 0)
        {
            Environment.Exit(1);
        }
    }
}
