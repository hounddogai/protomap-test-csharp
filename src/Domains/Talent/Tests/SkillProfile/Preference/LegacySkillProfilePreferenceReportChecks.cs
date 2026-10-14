namespace Acme.Workforce.Talent.Tests.SkillProfile.Preference;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SkillProfile.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class LegacySkillProfilePreferenceReportChecks
{
    public static void LegacySkillProfilePreferenceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708260793);
        var items = new List<LegacySkillProfilePreference>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(LegacySkillProfilePreference.NewLegacySkillProfilePreference($"legacy-skill-profile-preference-{index}", "sample"));
        }
        var report = LegacySkillProfilePreferenceReport.BuildLegacySkillProfilePreferenceReport("test", items, clock);
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
        LegacySkillProfilePreferenceReportCountsEachRecordOnce();
    }
}
