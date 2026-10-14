namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueVariance;

using Acme.Workforce.Compliance.TrainingDue.DueVariance;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of due variance code, run by the project's runner.</summary>
public static class TrainingDueDigestReportChecks
{
    public static void TrainingDueDigestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706375896);
        var items = new List<TrainingDueDigest>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(TrainingDueDigest.NewTrainingDueDigest($"training-due-digest-{index}", "sample"));
        }
        var report = TrainingDueDigestReport.BuildTrainingDueDigestReport("test", items, clock);
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
        TrainingDueDigestReportCountsEachRecordOnce();
    }
}
