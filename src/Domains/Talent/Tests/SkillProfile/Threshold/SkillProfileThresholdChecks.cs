namespace Acme.Workforce.Talent.Tests.SkillProfile.Threshold;

using Acme.Workforce.Talent.SkillProfile.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class SkillProfileThresholdChecks
{
    public static void SkillProfileThresholdStartsInDraft()
    {
        var item = SkillProfileThreshold.NewSkillProfileThreshold("code-314", "label-671");
        Check.Equal(item.Status, SkillProfileThresholdStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SkillProfileThresholdMovesToClosed()
    {
        var item = SkillProfileThreshold.NewSkillProfileThreshold("code-314", "label-671");
        try
        {
            item.MoveTo(SkillProfileThresholdStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SkillProfileThresholdStatus.Closed, "status");
    }

    public static void SkillProfileThresholdRejectsMovesFromClosed()
    {
        var item = SkillProfileThreshold.NewSkillProfileThreshold("code-314", "label-671");
        item.Status = SkillProfileThresholdStatus.Closed;
        Check.Fails(() => item.MoveTo(SkillProfileThresholdStatus.Draft), "move");
    }

    public static void RunAll()
    {
        SkillProfileThresholdStartsInDraft();
        SkillProfileThresholdMovesToClosed();
        SkillProfileThresholdRejectsMovesFromClosed();
    }
}
