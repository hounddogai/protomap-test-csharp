namespace Acme.Workforce.Talent.Tests.SkillProfile.Summary;

using Acme.Workforce.Talent.SkillProfile.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class AdaptiveSkillProfileSummaryViewChecks
{
    public static void AdaptiveSkillProfileSummaryViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveSkillProfileSummary.NewAdaptiveSkillProfileSummary("code-340", "name-212");
        var view = AdaptiveSkillProfileSummaryView.ToAdaptiveSkillProfileSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveSkillProfileSummaryView.ToAdaptiveSkillProfileSummaryViews(new List<AdaptiveSkillProfileSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveSkillProfileSummaryViewShowsTheStatusInCapitals();
    }
}
