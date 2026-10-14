namespace Acme.Workforce.Talent.Tests.SkillProfile.Cycle;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SkillProfile.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class ScheduledSkillProfilePolicyReportChecks
{
    public static void ScheduledSkillProfilePolicyReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700253267);
        var items = new List<ScheduledSkillProfilePolicy>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(ScheduledSkillProfilePolicy.NewScheduledSkillProfilePolicy($"scheduled-skill-profile-policy-{index}", "sample"));
        }
        var report = ScheduledSkillProfilePolicyReport.BuildScheduledSkillProfilePolicyReport("test", items, clock);
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
        ScheduledSkillProfilePolicyReportCountsEachRecordOnce();
    }
}
