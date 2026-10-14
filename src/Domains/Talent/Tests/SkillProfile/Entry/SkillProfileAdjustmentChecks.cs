namespace Acme.Workforce.Talent.Tests.SkillProfile.Entry;

using Acme.Workforce.Talent.SkillProfile.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class SkillProfileAdjustmentChecks
{
    public static void SkillProfileAdjustmentStartsInIdle()
    {
        var item = SkillProfileAdjustment.NewSkillProfileAdjustment("code-246", "label-473");
        Check.Equal(item.Status, SkillProfileAdjustmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SkillProfileAdjustmentMovesToWarming()
    {
        var item = SkillProfileAdjustment.NewSkillProfileAdjustment("code-246", "label-473");
        try
        {
            item.MoveTo(SkillProfileAdjustmentStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SkillProfileAdjustmentStatus.Warming, "status");
    }

    public static void SkillProfileAdjustmentRejectsMovesFromStopped()
    {
        var item = SkillProfileAdjustment.NewSkillProfileAdjustment("code-246", "label-473");
        item.Status = SkillProfileAdjustmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(SkillProfileAdjustmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        SkillProfileAdjustmentStartsInIdle();
        SkillProfileAdjustmentMovesToWarming();
        SkillProfileAdjustmentRejectsMovesFromStopped();
    }
}
