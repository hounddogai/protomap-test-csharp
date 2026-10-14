namespace Acme.Workforce.Talent.Tests.Nomination.Milestone;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Nomination.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class ScheduledNominationExceptionReportChecks
{
    public static void ScheduledNominationExceptionReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703801154);
        var items = new List<ScheduledNominationException>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(ScheduledNominationException.NewScheduledNominationException($"scheduled-nomination-exception-{index}", "sample"));
        }
        var report = ScheduledNominationExceptionReport.BuildScheduledNominationExceptionReport("test", items, clock);
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
        ScheduledNominationExceptionReportCountsEachRecordOnce();
    }
}
