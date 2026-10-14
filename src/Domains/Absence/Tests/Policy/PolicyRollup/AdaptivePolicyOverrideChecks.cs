namespace Acme.Workforce.Absence.Tests.Policy.PolicyRollup;

using Acme.Workforce.Absence.Policy.PolicyRollup;

/// <summary>Checks of policy rollup code, run by the project's runner.</summary>
public static class AdaptivePolicyOverrideChecks
{
    public static void AdaptivePolicyOverrideStartsInOpen()
    {
        var item = AdaptivePolicyOverride.NewAdaptivePolicyOverride("code-898", "name-580");
        Check.Equal(item.Status, AdaptivePolicyOverrideStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptivePolicyOverrideMovesToInReview()
    {
        var item = AdaptivePolicyOverride.NewAdaptivePolicyOverride("code-898", "name-580");
        try
        {
            item.MoveTo(AdaptivePolicyOverrideStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptivePolicyOverrideStatus.InReview, "status");
    }

    public static void AdaptivePolicyOverrideRejectsMovesFromArchived()
    {
        var item = AdaptivePolicyOverride.NewAdaptivePolicyOverride("code-898", "name-580");
        item.Status = AdaptivePolicyOverrideStatus.Archived;
        Check.Fails(() => item.MoveTo(AdaptivePolicyOverrideStatus.Open), "move");
    }

    public static void RunAll()
    {
        AdaptivePolicyOverrideStartsInOpen();
        AdaptivePolicyOverrideMovesToInReview();
        AdaptivePolicyOverrideRejectsMovesFromArchived();
    }
}
