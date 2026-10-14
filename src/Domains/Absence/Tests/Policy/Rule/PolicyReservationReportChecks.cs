namespace Acme.Workforce.Absence.Tests.Policy.Rule;

using Acme.Workforce.Absence.Policy.Rule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class PolicyReservationReportChecks
{
    public static void PolicyReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706060216);
        var items = new List<PolicyReservation>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(PolicyReservation.NewPolicyReservation($"policy-reservation-{index}", "sample"));
        }
        var report = PolicyReservationReport.BuildPolicyReservationReport("test", items, clock);
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
        PolicyReservationReportCountsEachRecordOnce();
    }
}
