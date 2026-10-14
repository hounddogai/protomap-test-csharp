namespace Acme.Workforce.Talent.Tests.SkillProfile.Adjustment;

using Acme.Workforce.Talent.SkillProfile.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class WeightedSkillProfileBudgetChecks
{
    public static void WeightedSkillProfileBudgetStartsInCreated()
    {
        var item = WeightedSkillProfileBudget.NewWeightedSkillProfileBudget("code-469", "title-584");
        Check.Equal(item.Status, WeightedSkillProfileBudgetStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedSkillProfileBudgetMovesToPublished()
    {
        var item = WeightedSkillProfileBudget.NewWeightedSkillProfileBudget("code-469", "title-584");
        try
        {
            item.MoveTo(WeightedSkillProfileBudgetStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedSkillProfileBudgetStatus.Published, "status");
    }

    public static void WeightedSkillProfileBudgetRejectsMovesFromRetired()
    {
        var item = WeightedSkillProfileBudget.NewWeightedSkillProfileBudget("code-469", "title-584");
        item.Status = WeightedSkillProfileBudgetStatus.Retired;
        Check.Fails(() => item.MoveTo(WeightedSkillProfileBudgetStatus.Created), "move");
    }

    public static void RunAll()
    {
        WeightedSkillProfileBudgetStartsInCreated();
        WeightedSkillProfileBudgetMovesToPublished();
        WeightedSkillProfileBudgetRejectsMovesFromRetired();
    }
}
