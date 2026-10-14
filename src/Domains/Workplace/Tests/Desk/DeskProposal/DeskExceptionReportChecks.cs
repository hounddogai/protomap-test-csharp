namespace Acme.Workforce.Workplace.Tests.Desk.DeskProposal;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Desk.DeskProposal;

/// <summary>Checks of desk proposal code, run by the project's runner.</summary>
public static class DeskExceptionReportChecks
{
    public static void DeskExceptionReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700739112);
        var items = new List<DeskException>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(DeskException.NewDeskException($"desk-exception-{index}", "sample"));
        }
        var report = DeskExceptionReport.BuildDeskExceptionReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        DeskExceptionReportCountsEachRecordOnce();
    }
}
