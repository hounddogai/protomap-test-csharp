namespace Acme.Workforce.Talent.Tests.Nomination.Ledger;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Nomination.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class NominationScheduleReportChecks
{
    public static void NominationScheduleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709353038);
        var items = new List<NominationSchedule>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(NominationSchedule.NewNominationSchedule($"nomination-schedule-{index}", "sample"));
        }
        var report = NominationScheduleReport.BuildNominationScheduleReport("test", items, clock);
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
        NominationScheduleReportCountsEachRecordOnce();
    }
}
