namespace Acme.Workforce.Contractors.Tests.Engagement.Template;

using Acme.Workforce.Contractors.Engagement.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class EngagementTemplateChecks
{
    public static void EngagementTemplateStartsInDraft()
    {
        var item = EngagementTemplate.NewEngagementTemplate("code-512", "name-291");
        Check.Equal(item.Status, EngagementTemplateStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EngagementTemplateMovesToSuspended()
    {
        var item = EngagementTemplate.NewEngagementTemplate("code-512", "name-291");
        try
        {
            item.MoveTo(EngagementTemplateStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EngagementTemplateStatus.Suspended, "status");
    }

    public static void EngagementTemplateRejectsMovesFromClosed()
    {
        var item = EngagementTemplate.NewEngagementTemplate("code-512", "name-291");
        item.Status = EngagementTemplateStatus.Closed;
        Check.Fails(() => item.MoveTo(EngagementTemplateStatus.Draft), "move");
    }

    public static void RunAll()
    {
        EngagementTemplateStartsInDraft();
        EngagementTemplateMovesToSuspended();
        EngagementTemplateRejectsMovesFromClosed();
    }
}
