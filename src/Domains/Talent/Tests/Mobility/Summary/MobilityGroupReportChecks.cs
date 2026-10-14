namespace Acme.Workforce.Talent.Tests.Mobility.Summary;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Mobility.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class MobilityGroupReportChecks
{
    public static void MobilityGroupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703408279);
        var items = new List<MobilityGroup>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(MobilityGroup.NewMobilityGroup($"mobility-group-{index}", "sample"));
        }
        var report = MobilityGroupReport.BuildMobilityGroupReport("test", items, clock);
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
        MobilityGroupReportCountsEachRecordOnce();
    }
}
