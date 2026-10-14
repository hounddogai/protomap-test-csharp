namespace Acme.Workforce.Talent.Tests.SkillProfile.Group;

using Acme.Workforce.Talent.SkillProfile.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class SkillProfileBudgetViewChecks
{
    public static void SkillProfileBudgetViewShowsTheStatusInCapitals()
    {
        var item = SkillProfileBudget.NewSkillProfileBudget("code-739", "title-144");
        var view = SkillProfileBudgetView.ToSkillProfileBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SkillProfileBudgetView.ToSkillProfileBudgetViews(new List<SkillProfileBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SkillProfileBudgetViewShowsTheStatusInCapitals();
    }
}
