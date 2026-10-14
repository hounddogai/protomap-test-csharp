namespace Acme.Workforce.Orgdesign.Tests.Headcount.Baseline;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Headcount.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class HeadcountPolicyReportChecks
{
    public static void HeadcountPolicyReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702031818);
        var items = new List<HeadcountPolicy>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(HeadcountPolicy.NewHeadcountPolicy($"headcount-policy-{index}", "sample"));
        }
        var report = HeadcountPolicyReport.BuildHeadcountPolicyReport("test", items, clock);
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
        HeadcountPolicyReportCountsEachRecordOnce();
    }
}
