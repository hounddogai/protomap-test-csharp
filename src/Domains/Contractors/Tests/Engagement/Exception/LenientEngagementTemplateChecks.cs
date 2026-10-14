namespace Acme.Workforce.Contractors.Tests.Engagement.Exception;

using Acme.Workforce.Contractors.Engagement.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class LenientEngagementTemplateChecks
{
    public static void LenientEngagementTemplateStartsInNew()
    {
        var item = LenientEngagementTemplate.NewLenientEngagementTemplate("code-863", "label-667");
        Check.Equal(item.Status, LenientEngagementTemplateStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientEngagementTemplateMovesToQueued()
    {
        var item = LenientEngagementTemplate.NewLenientEngagementTemplate("code-863", "label-667");
        try
        {
            item.MoveTo(LenientEngagementTemplateStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientEngagementTemplateStatus.Queued, "status");
    }

    public static void LenientEngagementTemplateRejectsMovesFromFailed()
    {
        var item = LenientEngagementTemplate.NewLenientEngagementTemplate("code-863", "label-667");
        item.Status = LenientEngagementTemplateStatus.Failed;
        Check.Fails(() => item.MoveTo(LenientEngagementTemplateStatus.New), "move");
    }

    public static void RunAll()
    {
        LenientEngagementTemplateStartsInNew();
        LenientEngagementTemplateMovesToQueued();
        LenientEngagementTemplateRejectsMovesFromFailed();
    }
}
