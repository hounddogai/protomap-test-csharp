namespace Acme.Workforce.Absence.Tests.Policy.Group;

using Acme.Workforce.Absence.Policy.Group;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PolicyGroupReportChecks
{
    public static void PolicyGroupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703887987);
        var items = new List<PolicyGroup>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(PolicyGroup.NewPolicyGroup($"policy-group-{index}", "sample"));
        }
        var report = PolicyGroupReport.BuildPolicyGroupReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        PolicyGroupReportCountsEachRecordOnce();
    }
}
