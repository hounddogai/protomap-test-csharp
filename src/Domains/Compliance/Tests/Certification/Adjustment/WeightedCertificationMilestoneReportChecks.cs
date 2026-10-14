namespace Acme.Workforce.Compliance.Tests.Certification.Adjustment;

using Acme.Workforce.Compliance.Certification.Adjustment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class WeightedCertificationMilestoneReportChecks
{
    public static void WeightedCertificationMilestoneReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700299045);
        var items = new List<WeightedCertificationMilestone>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(WeightedCertificationMilestone.NewWeightedCertificationMilestone($"weighted-certification-milestone-{index}", "sample"));
        }
        var report = WeightedCertificationMilestoneReport.BuildWeightedCertificationMilestoneReport("test", items, clock);
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
        WeightedCertificationMilestoneReportCountsEachRecordOnce();
    }
}
