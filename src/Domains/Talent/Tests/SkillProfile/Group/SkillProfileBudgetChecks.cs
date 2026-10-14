namespace Acme.Workforce.Talent.Tests.SkillProfile.Group;

using Acme.Workforce.Talent.SkillProfile.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class SkillProfileBudgetChecks
{
    public static void SkillProfileBudgetStartsInCreated()
    {
        var item = SkillProfileBudget.NewSkillProfileBudget("code-739", "title-144");
        Check.Equal(item.Status, SkillProfileBudgetStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SkillProfileBudgetMovesToPublished()
    {
        var item = SkillProfileBudget.NewSkillProfileBudget("code-739", "title-144");
        try
        {
            item.MoveTo(SkillProfileBudgetStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SkillProfileBudgetStatus.Published, "status");
    }

    public static void SkillProfileBudgetRejectsMovesFromRetired()
    {
        var item = SkillProfileBudget.NewSkillProfileBudget("code-739", "title-144");
        item.Status = SkillProfileBudgetStatus.Retired;
        Check.Fails(() => item.MoveTo(SkillProfileBudgetStatus.Created), "move");
    }

    public static void RunAll()
    {
        SkillProfileBudgetStartsInCreated();
        SkillProfileBudgetMovesToPublished();
        SkillProfileBudgetRejectsMovesFromRetired();
    }
}
