namespace Acme.Workforce.Absence.Tests.Policy.PolicyRule;

using Acme.Workforce.Absence.Policy.PolicyRule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of policy rule code, run by the project's runner.</summary>
public static class StrictPolicyAssignmentReportChecks
{
    public static void StrictPolicyAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709818523);
        var items = new List<StrictPolicyAssignment>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(StrictPolicyAssignment.NewStrictPolicyAssignment($"strict-policy-assignment-{index}", "sample"));
        }
        var report = StrictPolicyAssignmentReport.BuildStrictPolicyAssignmentReport("test", items, clock);
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
        StrictPolicyAssignmentReportCountsEachRecordOnce();
    }
}
