namespace Acme.Workforce.Absence.Tests.Holiday.Proposal;

using Acme.Workforce.Absence.Holiday.Proposal;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class HolidayProposalReportChecks
{
    public static void HolidayProposalReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708758488);
        var items = new List<HolidayProposal>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(HolidayProposal.NewHolidayProposal($"holiday-proposal-{index}", "sample"));
        }
        var report = HolidayProposalReport.BuildHolidayProposalReport("test", items, clock);
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
        HolidayProposalReportCountsEachRecordOnce();
    }
}
