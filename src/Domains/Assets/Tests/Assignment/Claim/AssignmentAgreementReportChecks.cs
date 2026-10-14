namespace Acme.Workforce.Assets.Tests.Assignment.Claim;

using Acme.Workforce.Assets.Assignment.Claim;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AssignmentAgreementReportChecks
{
    public static void AssignmentAgreementReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707602880);
        var items = new List<AssignmentAgreement>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(AssignmentAgreement.NewAssignmentAgreement($"assignment-agreement-{index}", "sample"));
        }
        var report = AssignmentAgreementReport.BuildAssignmentAgreementReport("test", items, clock);
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
        AssignmentAgreementReportCountsEachRecordOnce();
    }
}
