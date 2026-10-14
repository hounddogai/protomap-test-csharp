namespace Acme.Workforce.Compliance.Tests.PolicyAck.Schedule;

using Acme.Workforce.Compliance.PolicyAck.Schedule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class TieredPolicyAckScheduleReportChecks
{
    public static void TieredPolicyAckScheduleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706913818);
        var items = new List<TieredPolicyAckSchedule>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(TieredPolicyAckSchedule.NewTieredPolicyAckSchedule($"tiered-policy-ack-schedule-{index}", "sample"));
        }
        var report = TieredPolicyAckScheduleReport.BuildTieredPolicyAckScheduleReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        TieredPolicyAckScheduleReportCountsEachRecordOnce();
    }
}
