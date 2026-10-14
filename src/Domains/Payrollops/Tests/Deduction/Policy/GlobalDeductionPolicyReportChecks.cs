namespace Acme.Workforce.Payrollops.Tests.Deduction.Policy;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Deduction.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class GlobalDeductionPolicyReportChecks
{
    public static void GlobalDeductionPolicyReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706485395);
        var items = new List<GlobalDeductionPolicy>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(GlobalDeductionPolicy.NewGlobalDeductionPolicy($"global-deduction-policy-{index}", "sample"));
        }
        var report = GlobalDeductionPolicyReport.BuildGlobalDeductionPolicyReport("test", items, clock);
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
        GlobalDeductionPolicyReportCountsEachRecordOnce();
    }
}
