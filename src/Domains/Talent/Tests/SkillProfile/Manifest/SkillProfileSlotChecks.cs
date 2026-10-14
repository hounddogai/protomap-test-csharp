namespace Acme.Workforce.Talent.Tests.SkillProfile.Manifest;

using Acme.Workforce.Talent.SkillProfile.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class SkillProfileSlotChecks
{
    public static void SkillProfileSlotStartsInPlanned()
    {
        var item = SkillProfileSlot.NewSkillProfileSlot("code-753", "label-841");
        Check.Equal(item.Status, SkillProfileSlotStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SkillProfileSlotMovesToCompleted()
    {
        var item = SkillProfileSlot.NewSkillProfileSlot("code-753", "label-841");
        try
        {
            item.MoveTo(SkillProfileSlotStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SkillProfileSlotStatus.Completed, "status");
    }

    public static void SkillProfileSlotRejectsMovesFromCancelled()
    {
        var item = SkillProfileSlot.NewSkillProfileSlot("code-753", "label-841");
        item.Status = SkillProfileSlotStatus.Cancelled;
        Check.Fails(() => item.MoveTo(SkillProfileSlotStatus.Planned), "move");
    }

    public static void RunAll()
    {
        SkillProfileSlotStartsInPlanned();
        SkillProfileSlotMovesToCompleted();
        SkillProfileSlotRejectsMovesFromCancelled();
    }
}
