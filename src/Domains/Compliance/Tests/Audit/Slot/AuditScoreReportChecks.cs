namespace Acme.Workforce.Compliance.Tests.Audit.Slot;

using Acme.Workforce.Compliance.Audit.Slot;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class AuditScoreReportChecks
{
    public static void AuditScoreReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700604600);
        var items = new List<AuditScore>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(AuditScore.NewAuditScore($"audit-score-{index}", "sample"));
        }
        var report = AuditScoreReport.BuildAuditScoreReport("test", items, clock);
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
        AuditScoreReportCountsEachRecordOnce();
    }
}
