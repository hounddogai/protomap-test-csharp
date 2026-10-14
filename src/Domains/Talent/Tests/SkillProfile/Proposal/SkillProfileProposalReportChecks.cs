namespace Acme.Workforce.Talent.Tests.SkillProfile.Proposal;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SkillProfile.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class SkillProfileProposalReportChecks
{
    public static void SkillProfileProposalReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708742576);
        var items = new List<SkillProfileProposal>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(SkillProfileProposal.NewSkillProfileProposal($"skill-profile-proposal-{index}", "sample"));
        }
        var report = SkillProfileProposalReport.BuildSkillProfileProposalReport("test", items, clock);
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
        SkillProfileProposalReportCountsEachRecordOnce();
    }
}
