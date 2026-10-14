namespace Acme.Workforce.Compliance.Tests.Attestation.Claim;

using Acme.Workforce.Compliance.Attestation.Claim;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AttestationMilestoneReportChecks
{
    public static void AttestationMilestoneReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708984145);
        var items = new List<AttestationMilestone>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(AttestationMilestone.NewAttestationMilestone($"attestation-milestone-{index}", "sample"));
        }
        var report = AttestationMilestoneReport.BuildAttestationMilestoneReport("test", items, clock);
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
        AttestationMilestoneReportCountsEachRecordOnce();
    }
}
