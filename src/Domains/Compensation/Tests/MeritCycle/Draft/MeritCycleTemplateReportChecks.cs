namespace Acme.Workforce.Compensation.Tests.MeritCycle.Draft;

using Acme.Workforce.Compensation.MeritCycle.Draft;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class MeritCycleTemplateReportChecks
{
    public static void MeritCycleTemplateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706620076);
        var items = new List<MeritCycleTemplate>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(MeritCycleTemplate.NewMeritCycleTemplate($"merit-cycle-template-{index}", "sample"));
        }
        var report = MeritCycleTemplateReport.BuildMeritCycleTemplateReport("test", items, clock);
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
        MeritCycleTemplateReportCountsEachRecordOnce();
    }
}
