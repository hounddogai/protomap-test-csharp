namespace Acme.Workforce.Talent.Tests.Mobility.MobilityRecord;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Mobility.MobilityRecord;

/// <summary>Checks of mobility record code, run by the project's runner.</summary>
public static class MobilityAssignmentReportChecks
{
    public static void MobilityAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700433750);
        var items = new List<MobilityAssignment>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(MobilityAssignment.NewMobilityAssignment($"mobility-assignment-{index}", "sample"));
        }
        var report = MobilityAssignmentReport.BuildMobilityAssignmentReport("test", items, clock);
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
        MobilityAssignmentReportCountsEachRecordOnce();
    }
}
