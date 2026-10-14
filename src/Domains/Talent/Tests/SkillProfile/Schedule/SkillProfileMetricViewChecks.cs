namespace Acme.Workforce.Talent.Tests.SkillProfile.Schedule;

using Acme.Workforce.Talent.SkillProfile.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class SkillProfileMetricViewChecks
{
    public static void SkillProfileMetricViewShowsTheStatusInCapitals()
    {
        var item = SkillProfileMetric.NewSkillProfileMetric("code-371", "title-812");
        var view = SkillProfileMetricView.ToSkillProfileMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SkillProfileMetricView.ToSkillProfileMetricViews(new List<SkillProfileMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SkillProfileMetricViewShowsTheStatusInCapitals();
    }
}
