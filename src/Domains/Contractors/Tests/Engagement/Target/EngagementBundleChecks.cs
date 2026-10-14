namespace Acme.Workforce.Contractors.Tests.Engagement.Target;

using Acme.Workforce.Contractors.Engagement.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class EngagementBundleChecks
{
    public static void EngagementBundleStartsInOpen()
    {
        var item = EngagementBundle.NewEngagementBundle("code-274", "label-555");
        Check.Equal(item.Status, EngagementBundleStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EngagementBundleMovesToInReview()
    {
        var item = EngagementBundle.NewEngagementBundle("code-274", "label-555");
        try
        {
            item.MoveTo(EngagementBundleStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EngagementBundleStatus.InReview, "status");
    }

    public static void EngagementBundleRejectsMovesFromArchived()
    {
        var item = EngagementBundle.NewEngagementBundle("code-274", "label-555");
        item.Status = EngagementBundleStatus.Archived;
        Check.Fails(() => item.MoveTo(EngagementBundleStatus.Open), "move");
    }

    public static void RunAll()
    {
        EngagementBundleStartsInOpen();
        EngagementBundleMovesToInReview();
        EngagementBundleRejectsMovesFromArchived();
    }
}
