namespace Acme.Workforce.Talent.Tests.SkillProfile.Entry;

using Acme.Workforce.Talent.SkillProfile.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class SkillProfileEntryChecks
{
    public static void SkillProfileEntryStartsInProposed()
    {
        var item = SkillProfileEntry.NewSkillProfileEntry("code-929", "label-613");
        Check.Equal(item.Status, SkillProfileEntryStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SkillProfileEntryMovesToWithdrawn()
    {
        var item = SkillProfileEntry.NewSkillProfileEntry("code-929", "label-613");
        try
        {
            item.MoveTo(SkillProfileEntryStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SkillProfileEntryStatus.Withdrawn, "status");
    }

    public static void SkillProfileEntryRejectsMovesFromWithdrawn()
    {
        var item = SkillProfileEntry.NewSkillProfileEntry("code-929", "label-613");
        item.Status = SkillProfileEntryStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(SkillProfileEntryStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        SkillProfileEntryStartsInProposed();
        SkillProfileEntryMovesToWithdrawn();
        SkillProfileEntryRejectsMovesFromWithdrawn();
    }
}
