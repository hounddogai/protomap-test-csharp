namespace Acme.Workforce.Talent.Tests.SkillProfile.Review;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SkillProfile.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class CappedSkillProfileForecastReportChecks
{
    public static void CappedSkillProfileForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705619465);
        var items = new List<CappedSkillProfileForecast>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(CappedSkillProfileForecast.NewCappedSkillProfileForecast($"capped-skill-profile-forecast-{index}", "sample"));
        }
        var report = CappedSkillProfileForecastReport.BuildCappedSkillProfileForecastReport("test", items, clock);
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
        CappedSkillProfileForecastReportCountsEachRecordOnce();
    }
}
