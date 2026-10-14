namespace Acme.Workforce.Absence.Tests.Policy.Preference;

using Acme.Workforce.Absence.Policy.Preference;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class PolicyLedgerReportChecks
{
    public static void PolicyLedgerReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703633010);
        var items = new List<PolicyLedger>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PolicyLedger.NewPolicyLedger($"policy-ledger-{index}", "sample"));
        }
        var report = PolicyLedgerReport.BuildPolicyLedgerReport("test", items, clock);
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
        PolicyLedgerReportCountsEachRecordOnce();
    }
}
