namespace Acme.Workforce.Absence.Tests.Policy.Baseline;

using Acme.Workforce.Absence.Policy.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class FallbackPolicyRuleChecks
{
    public static void FallbackPolicyRuleStartsInPlanned()
    {
        var item = FallbackPolicyRule.NewFallbackPolicyRule("code-483", "title-489");
        Check.Equal(item.Status, FallbackPolicyRuleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackPolicyRuleMovesToCompleted()
    {
        var item = FallbackPolicyRule.NewFallbackPolicyRule("code-483", "title-489");
        try
        {
            item.MoveTo(FallbackPolicyRuleStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackPolicyRuleStatus.Completed, "status");
    }

    public static void FallbackPolicyRuleRejectsMovesFromCancelled()
    {
        var item = FallbackPolicyRule.NewFallbackPolicyRule("code-483", "title-489");
        item.Status = FallbackPolicyRuleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(FallbackPolicyRuleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        FallbackPolicyRuleStartsInPlanned();
        FallbackPolicyRuleMovesToCompleted();
        FallbackPolicyRuleRejectsMovesFromCancelled();
    }
}
