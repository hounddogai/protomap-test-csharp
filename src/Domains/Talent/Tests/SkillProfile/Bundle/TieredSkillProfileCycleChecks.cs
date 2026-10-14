namespace Acme.Workforce.Talent.Tests.SkillProfile.Bundle;

using Acme.Workforce.Talent.SkillProfile.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class TieredSkillProfileCycleChecks
{
    public static void TieredSkillProfileCycleStartsInDraft()
    {
        var item = TieredSkillProfileCycle.NewTieredSkillProfileCycle("code-20", "label-827");
        Check.Equal(item.Status, TieredSkillProfileCycleStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredSkillProfileCycleMovesToActive()
    {
        var item = TieredSkillProfileCycle.NewTieredSkillProfileCycle("code-20", "label-827");
        try
        {
            item.MoveTo(TieredSkillProfileCycleStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredSkillProfileCycleStatus.Active, "status");
    }

    public static void TieredSkillProfileCycleRejectsMovesFromClosed()
    {
        var item = TieredSkillProfileCycle.NewTieredSkillProfileCycle("code-20", "label-827");
        item.Status = TieredSkillProfileCycleStatus.Closed;
        Check.Fails(() => item.MoveTo(TieredSkillProfileCycleStatus.Draft), "move");
    }

    public static void RunAll()
    {
        TieredSkillProfileCycleStartsInDraft();
        TieredSkillProfileCycleMovesToActive();
        TieredSkillProfileCycleRejectsMovesFromClosed();
    }
}
