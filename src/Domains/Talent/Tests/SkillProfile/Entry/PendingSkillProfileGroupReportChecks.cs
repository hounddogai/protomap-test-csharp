namespace Acme.Workforce.Talent.Tests.SkillProfile.Entry;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SkillProfile.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class PendingSkillProfileGroupReportChecks
{
    public static void PendingSkillProfileGroupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702966028);
        var items = new List<PendingSkillProfileGroup>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PendingSkillProfileGroup.NewPendingSkillProfileGroup($"pending-skill-profile-group-{index}", "sample"));
        }
        var report = PendingSkillProfileGroupReport.BuildPendingSkillProfileGroupReport("test", items, clock);
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
        PendingSkillProfileGroupReportCountsEachRecordOnce();
    }
}
