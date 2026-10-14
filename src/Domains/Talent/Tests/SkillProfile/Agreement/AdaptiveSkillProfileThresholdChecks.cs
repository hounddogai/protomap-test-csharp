namespace Acme.Workforce.Talent.Tests.SkillProfile.Agreement;

using Acme.Workforce.Talent.SkillProfile.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class AdaptiveSkillProfileThresholdChecks
{
    public static void AdaptiveSkillProfileThresholdStartsInOpen()
    {
        var item = AdaptiveSkillProfileThreshold.NewAdaptiveSkillProfileThreshold("code-828", "title-885");
        Check.Equal(item.Status, AdaptiveSkillProfileThresholdStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveSkillProfileThresholdMovesToInReview()
    {
        var item = AdaptiveSkillProfileThreshold.NewAdaptiveSkillProfileThreshold("code-828", "title-885");
        try
        {
            item.MoveTo(AdaptiveSkillProfileThresholdStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveSkillProfileThresholdStatus.InReview, "status");
    }

    public static void AdaptiveSkillProfileThresholdRejectsMovesFromArchived()
    {
        var item = AdaptiveSkillProfileThreshold.NewAdaptiveSkillProfileThreshold("code-828", "title-885");
        item.Status = AdaptiveSkillProfileThresholdStatus.Archived;
        Check.Fails(() => item.MoveTo(AdaptiveSkillProfileThresholdStatus.Open), "move");
    }

    public static void RunAll()
    {
        AdaptiveSkillProfileThresholdStartsInOpen();
        AdaptiveSkillProfileThresholdMovesToInReview();
        AdaptiveSkillProfileThresholdRejectsMovesFromArchived();
    }
}
