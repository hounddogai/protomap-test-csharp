namespace Acme.Workforce.Compensation.Tests.Adjustment.Notice;

using Acme.Workforce.Compensation.Adjustment.Notice;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class AdjustmentPreferenceReportChecks
{
    public static void AdjustmentPreferenceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709241332);
        var items = new List<AdjustmentPreference>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(AdjustmentPreference.NewAdjustmentPreference($"adjustment-preference-{index}", "sample"));
        }
        var report = AdjustmentPreferenceReport.BuildAdjustmentPreferenceReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        AdjustmentPreferenceReportCountsEachRecordOnce();
    }
}
