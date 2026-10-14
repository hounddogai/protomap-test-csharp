namespace Acme.Workforce.Orgdesign.Tests.Position.Template;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Position.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class PrimaryPositionProfileReportChecks
{
    public static void PrimaryPositionProfileReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703656941);
        var items = new List<PrimaryPositionProfile>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(PrimaryPositionProfile.NewPrimaryPositionProfile($"primary-position-profile-{index}", "sample"));
        }
        var report = PrimaryPositionProfileReport.BuildPrimaryPositionProfileReport("test", items, clock);
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
        PrimaryPositionProfileReportCountsEachRecordOnce();
    }
}
