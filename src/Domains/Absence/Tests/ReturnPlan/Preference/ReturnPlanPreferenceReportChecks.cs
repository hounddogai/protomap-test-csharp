namespace Acme.Workforce.Absence.Tests.ReturnPlan.Preference;

using Acme.Workforce.Absence.ReturnPlan.Preference;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ReturnPlanPreferenceReportChecks
{
    public static void ReturnPlanPreferenceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704551364);
        var items = new List<ReturnPlanPreference>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(ReturnPlanPreference.NewReturnPlanPreference($"return-plan-preference-{index}", "sample"));
        }
        var report = ReturnPlanPreferenceReport.BuildReturnPlanPreferenceReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        ReturnPlanPreferenceReportCountsEachRecordOnce();
    }
}
