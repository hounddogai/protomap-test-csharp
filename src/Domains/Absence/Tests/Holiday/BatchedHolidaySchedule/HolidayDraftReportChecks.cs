namespace Acme.Workforce.Absence.Tests.Holiday.BatchedHolidaySchedule;

using Acme.Workforce.Absence.Holiday.BatchedHolidaySchedule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of batched holiday schedule code, run by the project's runner.</summary>
public static class HolidayDraftReportChecks
{
    public static void HolidayDraftReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708896145);
        var items = new List<HolidayDraft>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(HolidayDraft.NewHolidayDraft($"holiday-draft-{index}", "sample"));
        }
        var report = HolidayDraftReport.BuildHolidayDraftReport("test", items, clock);
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
        HolidayDraftReportCountsEachRecordOnce();
    }
}
