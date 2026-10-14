namespace Acme.Workforce.Orgdesign.Tests.Position.Profile;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Position.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class LenientPositionProfileReportChecks
{
    public static void LenientPositionProfileReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708730994);
        var items = new List<LenientPositionProfile>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LenientPositionProfile.NewLenientPositionProfile($"lenient-position-profile-{index}", "sample"));
        }
        var report = LenientPositionProfileReport.BuildLenientPositionProfileReport("test", items, clock);
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
        LenientPositionProfileReportCountsEachRecordOnce();
    }
}
