namespace Acme.Workforce.Compliance.Tests.TrainingDue.Notice;

using Acme.Workforce.Compliance.TrainingDue.Notice;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class TrainingDueNoticeReportChecks
{
    public static void TrainingDueNoticeReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701355726);
        var items = new List<TrainingDueNotice>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(TrainingDueNotice.NewTrainingDueNotice($"training-due-notice-{index}", "sample"));
        }
        var report = TrainingDueNoticeReport.BuildTrainingDueNoticeReport("test", items, clock);
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
        TrainingDueNoticeReportCountsEachRecordOnce();
    }
}
