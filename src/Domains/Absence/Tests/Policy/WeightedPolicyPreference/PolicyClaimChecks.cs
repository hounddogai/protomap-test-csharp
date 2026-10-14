namespace Acme.Workforce.Absence.Tests.Policy.WeightedPolicyPreference;

using Acme.Workforce.Absence.Policy.WeightedPolicyPreference;

/// <summary>Checks of weighted policy preference code, run by the project's runner.</summary>
public static class PolicyClaimChecks
{
    public static void PolicyClaimStartsInDraft()
    {
        var item = PolicyClaim.NewPolicyClaim("code-705", "title-338");
        Check.Equal(item.Status, PolicyClaimStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyClaimMovesToSuspended()
    {
        var item = PolicyClaim.NewPolicyClaim("code-705", "title-338");
        try
        {
            item.MoveTo(PolicyClaimStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyClaimStatus.Suspended, "status");
    }

    public static void PolicyClaimRejectsMovesFromClosed()
    {
        var item = PolicyClaim.NewPolicyClaim("code-705", "title-338");
        item.Status = PolicyClaimStatus.Closed;
        Check.Fails(() => item.MoveTo(PolicyClaimStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PolicyClaimStartsInDraft();
        PolicyClaimMovesToSuspended();
        PolicyClaimRejectsMovesFromClosed();
    }
}
