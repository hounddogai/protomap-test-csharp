namespace Acme.Workforce.Compliance.Tests.TrainingDue.Quota;

using Acme.Workforce.Compliance.TrainingDue.Quota;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class StrictTrainingDueQuotaReportChecks
{
    public static void StrictTrainingDueQuotaReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704639958);
        var items = new List<StrictTrainingDueQuota>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(StrictTrainingDueQuota.NewStrictTrainingDueQuota($"strict-training-due-quota-{index}", "sample"));
        }
        var report = StrictTrainingDueQuotaReport.BuildStrictTrainingDueQuotaReport("test", items, clock);
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
        StrictTrainingDueQuotaReportCountsEachRecordOnce();
    }
}
