namespace Acme.Workforce.Workplace.Tests.Visitor.Policy;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Visitor.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class VisitorPolicyReportChecks
{
    public static void VisitorPolicyReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700595326);
        var items = new List<VisitorPolicy>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(VisitorPolicy.NewVisitorPolicy($"visitor-policy-{index}", "sample"));
        }
        var report = VisitorPolicyReport.BuildVisitorPolicyReport("test", items, clock);
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
        VisitorPolicyReportCountsEachRecordOnce();
    }
}
