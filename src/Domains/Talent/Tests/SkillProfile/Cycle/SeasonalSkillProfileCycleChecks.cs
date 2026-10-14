namespace Acme.Workforce.Talent.Tests.SkillProfile.Cycle;

using Acme.Workforce.Talent.SkillProfile.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class SeasonalSkillProfileCycleChecks
{
    public static void SeasonalSkillProfileCycleStartsInNew()
    {
        var item = SeasonalSkillProfileCycle.NewSeasonalSkillProfileCycle("code-787", "label-12");
        Check.Equal(item.Status, SeasonalSkillProfileCycleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalSkillProfileCycleMovesToFailed()
    {
        var item = SeasonalSkillProfileCycle.NewSeasonalSkillProfileCycle("code-787", "label-12");
        try
        {
            item.MoveTo(SeasonalSkillProfileCycleStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalSkillProfileCycleStatus.Failed, "status");
    }

    public static void SeasonalSkillProfileCycleRejectsMovesFromFailed()
    {
        var item = SeasonalSkillProfileCycle.NewSeasonalSkillProfileCycle("code-787", "label-12");
        item.Status = SeasonalSkillProfileCycleStatus.Failed;
        Check.Fails(() => item.MoveTo(SeasonalSkillProfileCycleStatus.New), "move");
    }

    public static void RunAll()
    {
        SeasonalSkillProfileCycleStartsInNew();
        SeasonalSkillProfileCycleMovesToFailed();
        SeasonalSkillProfileCycleRejectsMovesFromFailed();
    }
}
