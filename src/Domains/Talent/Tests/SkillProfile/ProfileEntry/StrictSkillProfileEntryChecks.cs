namespace Acme.Workforce.Talent.Tests.SkillProfile.ProfileEntry;

using Acme.Workforce.Talent.SkillProfile.ProfileEntry;

/// <summary>Checks of profile entry code, run by the project's runner.</summary>
public static class StrictSkillProfileEntryChecks
{
    public static void StrictSkillProfileEntryStartsInPlanned()
    {
        var item = StrictSkillProfileEntry.NewStrictSkillProfileEntry("code-382", "title-180");
        Check.Equal(item.Status, StrictSkillProfileEntryStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictSkillProfileEntryMovesToInProgress()
    {
        var item = StrictSkillProfileEntry.NewStrictSkillProfileEntry("code-382", "title-180");
        try
        {
            item.MoveTo(StrictSkillProfileEntryStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictSkillProfileEntryStatus.InProgress, "status");
    }

    public static void StrictSkillProfileEntryRejectsMovesFromCancelled()
    {
        var item = StrictSkillProfileEntry.NewStrictSkillProfileEntry("code-382", "title-180");
        item.Status = StrictSkillProfileEntryStatus.Cancelled;
        Check.Fails(() => item.MoveTo(StrictSkillProfileEntryStatus.Planned), "move");
    }

    public static void RunAll()
    {
        StrictSkillProfileEntryStartsInPlanned();
        StrictSkillProfileEntryMovesToInProgress();
        StrictSkillProfileEntryRejectsMovesFromCancelled();
    }
}
