namespace Acme.Workforce.Workplace.Tests.Visitor.Draft;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Visitor.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class VisitorClaimReportChecks
{
    public static void VisitorClaimReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701739291);
        var items = new List<VisitorClaim>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(VisitorClaim.NewVisitorClaim($"visitor-claim-{index}", "sample"));
        }
        var report = VisitorClaimReport.BuildVisitorClaimReport("test", items, clock);
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
        VisitorClaimReportCountsEachRecordOnce();
    }
}
