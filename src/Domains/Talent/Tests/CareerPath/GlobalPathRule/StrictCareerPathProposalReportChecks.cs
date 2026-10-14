namespace Acme.Workforce.Talent.Tests.CareerPath.GlobalPathRule;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.CareerPath.GlobalPathRule;

/// <summary>Checks of global path rule code, run by the project's runner.</summary>
public static class StrictCareerPathProposalReportChecks
{
    public static void StrictCareerPathProposalReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703569411);
        var items = new List<StrictCareerPathProposal>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(StrictCareerPathProposal.NewStrictCareerPathProposal($"strict-career-path-proposal-{index}", "sample"));
        }
        var report = StrictCareerPathProposalReport.BuildStrictCareerPathProposalReport("test", items, clock);
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
        StrictCareerPathProposalReportCountsEachRecordOnce();
    }
}
