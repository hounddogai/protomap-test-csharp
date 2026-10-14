namespace Acme.Workforce.Talent.Tests.SkillProfile.Target;

using Acme.Workforce.Talent.SkillProfile.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class SkillProfilePlanChecks
{
    public static void SkillProfilePlanStartsInDraft()
    {
        var item = SkillProfilePlan.NewSkillProfilePlan("code-760", "label-52");
        Check.Equal(item.Status, SkillProfilePlanStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SkillProfilePlanMovesToPending()
    {
        var item = SkillProfilePlan.NewSkillProfilePlan("code-760", "label-52");
        try
        {
            item.MoveTo(SkillProfilePlanStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SkillProfilePlanStatus.Pending, "status");
    }

    public static void SkillProfilePlanRejectsMovesFromClosed()
    {
        var item = SkillProfilePlan.NewSkillProfilePlan("code-760", "label-52");
        item.Status = SkillProfilePlanStatus.Closed;
        Check.Fails(() => item.MoveTo(SkillProfilePlanStatus.Draft), "move");
    }

    public static void RunAll()
    {
        SkillProfilePlanStartsInDraft();
        SkillProfilePlanMovesToPending();
        SkillProfilePlanRejectsMovesFromClosed();
    }
}
