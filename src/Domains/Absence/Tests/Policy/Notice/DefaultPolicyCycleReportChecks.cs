namespace Acme.Workforce.Absence.Tests.Policy.Notice;

using Acme.Workforce.Absence.Policy.Notice;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class DefaultPolicyCycleReportChecks
{
    public static void DefaultPolicyCycleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701487882);
        var items = new List<DefaultPolicyCycle>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(DefaultPolicyCycle.NewDefaultPolicyCycle($"default-policy-cycle-{index}", "sample"));
        }
        var report = DefaultPolicyCycleReport.BuildDefaultPolicyCycleReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        DefaultPolicyCycleReportCountsEachRecordOnce();
    }
}
