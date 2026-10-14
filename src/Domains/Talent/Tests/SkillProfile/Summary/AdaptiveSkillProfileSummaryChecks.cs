namespace Acme.Workforce.Talent.Tests.SkillProfile.Summary;

using Acme.Workforce.Talent.SkillProfile.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class AdaptiveSkillProfileSummaryChecks
{
    public static void AdaptiveSkillProfileSummaryStartsInNew()
    {
        var item = AdaptiveSkillProfileSummary.NewAdaptiveSkillProfileSummary("code-340", "name-212");
        Check.Equal(item.Status, AdaptiveSkillProfileSummaryStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveSkillProfileSummaryMovesToQueued()
    {
        var item = AdaptiveSkillProfileSummary.NewAdaptiveSkillProfileSummary("code-340", "name-212");
        try
        {
            item.MoveTo(AdaptiveSkillProfileSummaryStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveSkillProfileSummaryStatus.Queued, "status");
    }

    public static void AdaptiveSkillProfileSummaryRejectsMovesFromFailed()
    {
        var item = AdaptiveSkillProfileSummary.NewAdaptiveSkillProfileSummary("code-340", "name-212");
        item.Status = AdaptiveSkillProfileSummaryStatus.Failed;
        Check.Fails(() => item.MoveTo(AdaptiveSkillProfileSummaryStatus.New), "move");
    }

    public static void RunAll()
    {
        AdaptiveSkillProfileSummaryStartsInNew();
        AdaptiveSkillProfileSummaryMovesToQueued();
        AdaptiveSkillProfileSummaryRejectsMovesFromFailed();
    }
}
