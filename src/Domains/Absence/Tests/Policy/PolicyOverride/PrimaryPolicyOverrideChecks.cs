namespace Acme.Workforce.Absence.Tests.Policy.PolicyOverride;

using Acme.Workforce.Absence.Policy.PolicyOverride;

/// <summary>Checks of policy override code, run by the project's runner.</summary>
public static class PrimaryPolicyOverrideChecks
{
    public static void PrimaryPolicyOverrideStartsInProposed()
    {
        var item = PrimaryPolicyOverride.NewPrimaryPolicyOverride("code-589", "name-224");
        Check.Equal(item.Status, PrimaryPolicyOverrideStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryPolicyOverrideMovesToWithdrawn()
    {
        var item = PrimaryPolicyOverride.NewPrimaryPolicyOverride("code-589", "name-224");
        try
        {
            item.MoveTo(PrimaryPolicyOverrideStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryPolicyOverrideStatus.Withdrawn, "status");
    }

    public static void PrimaryPolicyOverrideRejectsMovesFromWithdrawn()
    {
        var item = PrimaryPolicyOverride.NewPrimaryPolicyOverride("code-589", "name-224");
        item.Status = PrimaryPolicyOverrideStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PrimaryPolicyOverrideStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PrimaryPolicyOverrideStartsInProposed();
        PrimaryPolicyOverrideMovesToWithdrawn();
        PrimaryPolicyOverrideRejectsMovesFromWithdrawn();
    }
}
