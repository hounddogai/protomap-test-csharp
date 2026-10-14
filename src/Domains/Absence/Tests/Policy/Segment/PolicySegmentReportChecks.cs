namespace Acme.Workforce.Absence.Tests.Policy.Segment;

using Acme.Workforce.Absence.Policy.Segment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class PolicySegmentReportChecks
{
    public static void PolicySegmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705771740);
        var items = new List<PolicySegment>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PolicySegment.NewPolicySegment($"policy-segment-{index}", "sample"));
        }
        var report = PolicySegmentReport.BuildPolicySegmentReport("test", items, clock);
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
        PolicySegmentReportCountsEachRecordOnce();
    }
}
