namespace Acme.Workforce.Absence.Tests.Policy.PolicyRollup;

using Acme.Workforce.Absence.Policy.PolicyRollup;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of policy rollup code, run by the project's runner.</summary>
public static class PrimaryPolicyRollupReportChecks
{
    public static void PrimaryPolicyRollupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708811750);
        var items = new List<PrimaryPolicyRollup>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PrimaryPolicyRollup.NewPrimaryPolicyRollup($"primary-policy-rollup-{index}", "sample"));
        }
        var report = PrimaryPolicyRollupReport.BuildPrimaryPolicyRollupReport("test", items, clock);
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
        PrimaryPolicyRollupReportCountsEachRecordOnce();
    }
}
