namespace Acme.Workforce.Assets.Tests.Assignment.Exception;

using Acme.Workforce.Assets.Assignment.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class ManualAssignmentRuleChecks
{
    public static void ManualAssignmentRuleStartsInDraft()
    {
        var item = ManualAssignmentRule.NewManualAssignmentRule("code-272", "label-26");
        Check.Equal(item.Status, ManualAssignmentRuleStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualAssignmentRuleMovesToPending()
    {
        var item = ManualAssignmentRule.NewManualAssignmentRule("code-272", "label-26");
        try
        {
            item.MoveTo(ManualAssignmentRuleStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualAssignmentRuleStatus.Pending, "status");
    }

    public static void ManualAssignmentRuleRejectsMovesFromClosed()
    {
        var item = ManualAssignmentRule.NewManualAssignmentRule("code-272", "label-26");
        item.Status = ManualAssignmentRuleStatus.Closed;
        Check.Fails(() => item.MoveTo(ManualAssignmentRuleStatus.Draft), "move");
    }

    public static void RunAll()
    {
        ManualAssignmentRuleStartsInDraft();
        ManualAssignmentRuleMovesToPending();
        ManualAssignmentRuleRejectsMovesFromClosed();
    }
}
