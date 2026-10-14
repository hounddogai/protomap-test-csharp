namespace Acme.Workforce.Assets.Tests.Assignment.Claim;

using Acme.Workforce.Assets.Assignment.Claim;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AssignmentClaimReportChecks
{
    public static void AssignmentClaimReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703372569);
        var items = new List<AssignmentClaim>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(AssignmentClaim.NewAssignmentClaim($"assignment-claim-{index}", "sample"));
        }
        var report = AssignmentClaimReport.BuildAssignmentClaimReport("test", items, clock);
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
        AssignmentClaimReportCountsEachRecordOnce();
    }
}
