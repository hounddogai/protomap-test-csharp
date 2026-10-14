namespace Acme.Workforce.Talent.Tests.SkillProfile.Adjustment;

using Acme.Workforce.Talent.SkillProfile.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class WeightedSkillProfileBudgetViewChecks
{
    public static void WeightedSkillProfileBudgetViewShowsTheStatusInCapitals()
    {
        var item = WeightedSkillProfileBudget.NewWeightedSkillProfileBudget("code-469", "title-584");
        var view = WeightedSkillProfileBudgetView.ToWeightedSkillProfileBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedSkillProfileBudgetView.ToWeightedSkillProfileBudgetViews(new List<WeightedSkillProfileBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedSkillProfileBudgetViewShowsTheStatusInCapitals();
    }
}
