namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueSchedule;

using Acme.Workforce.Compliance.TrainingDue.DueSchedule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of due schedule code, run by the project's runner.</summary>
public static class RegionalTrainingDueNoticeReportChecks
{
    public static void RegionalTrainingDueNoticeReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708057348);
        var items = new List<RegionalTrainingDueNotice>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(RegionalTrainingDueNotice.NewRegionalTrainingDueNotice($"regional-training-due-notice-{index}", "sample"));
        }
        var report = RegionalTrainingDueNoticeReport.BuildRegionalTrainingDueNoticeReport("test", items, clock);
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
        RegionalTrainingDueNoticeReportCountsEachRecordOnce();
    }
}
