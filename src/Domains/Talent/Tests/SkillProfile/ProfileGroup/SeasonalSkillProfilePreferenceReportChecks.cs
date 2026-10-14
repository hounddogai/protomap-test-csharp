namespace Acme.Workforce.Talent.Tests.SkillProfile.ProfileGroup;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SkillProfile.ProfileGroup;

/// <summary>Checks of profile group code, run by the project's runner.</summary>
public static class SeasonalSkillProfilePreferenceReportChecks
{
    public static void SeasonalSkillProfilePreferenceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704692593);
        var items = new List<SeasonalSkillProfilePreference>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(SeasonalSkillProfilePreference.NewSeasonalSkillProfilePreference($"seasonal-skill-profile-preference-{index}", "sample"));
        }
        var report = SeasonalSkillProfilePreferenceReport.BuildSeasonalSkillProfilePreferenceReport("test", items, clock);
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
        SeasonalSkillProfilePreferenceReportCountsEachRecordOnce();
    }
}
