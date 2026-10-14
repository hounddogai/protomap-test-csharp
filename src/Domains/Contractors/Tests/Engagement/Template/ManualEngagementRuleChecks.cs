namespace Acme.Workforce.Contractors.Tests.Engagement.Template;

using Acme.Workforce.Contractors.Engagement.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ManualEngagementRuleChecks
{
    public static void ManualEngagementRuleStartsInProposed()
    {
        var item = ManualEngagementRule.NewManualEngagementRule("code-974", "label-118");
        Check.Equal(item.Status, ManualEngagementRuleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualEngagementRuleMovesToRejected()
    {
        var item = ManualEngagementRule.NewManualEngagementRule("code-974", "label-118");
        try
        {
            item.MoveTo(ManualEngagementRuleStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualEngagementRuleStatus.Rejected, "status");
    }

    public static void ManualEngagementRuleRejectsMovesFromWithdrawn()
    {
        var item = ManualEngagementRule.NewManualEngagementRule("code-974", "label-118");
        item.Status = ManualEngagementRuleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ManualEngagementRuleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ManualEngagementRuleStartsInProposed();
        ManualEngagementRuleMovesToRejected();
        ManualEngagementRuleRejectsMovesFromWithdrawn();
    }
}
