namespace Acme.Workforce.Talent.Tests.SkillProfile.ProfileProposal;

using Acme.Workforce.Talent.SkillProfile.ProfileProposal;

/// <summary>Checks of profile proposal code, run by the project's runner.</summary>
public static class SkillProfileSummaryViewChecks
{
    public static void SkillProfileSummaryViewShowsTheStatusInCapitals()
    {
        var item = SkillProfileSummary.NewSkillProfileSummary("code-994", "name-547");
        var view = SkillProfileSummaryView.ToSkillProfileSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SkillProfileSummaryView.ToSkillProfileSummaryViews(new List<SkillProfileSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SkillProfileSummaryViewShowsTheStatusInCapitals();
    }
}
