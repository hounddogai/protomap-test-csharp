namespace Acme.Workforce.Contractors.Tests.Offboarding.Window;

using Acme.Workforce.Contractors.Offboarding.Window;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class OffboardingWindowReportChecks
{
    public static void OffboardingWindowReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706952176);
        var items = new List<OffboardingWindow>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(OffboardingWindow.NewOffboardingWindow($"offboarding-window-{index}", "sample"));
        }
        var report = OffboardingWindowReport.BuildOffboardingWindowReport("test", items, clock);
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
        OffboardingWindowReportCountsEachRecordOnce();
    }
}
