namespace Acme.Workforce.Absence.Tests.Policy.PolicyRollup;

using Acme.Workforce.Absence.Policy.PolicyRollup;

/// <summary>Checks of policy rollup code, run by the project's runner.</summary>
public static class PrimaryPolicyRollupChecks
{
    public static void PrimaryPolicyRollupStartsInDraft()
    {
        var item = PrimaryPolicyRollup.NewPrimaryPolicyRollup("code-999", "label-331");
        Check.Equal(item.Status, PrimaryPolicyRollupStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryPolicyRollupMovesToActive()
    {
        var item = PrimaryPolicyRollup.NewPrimaryPolicyRollup("code-999", "label-331");
        try
        {
            item.MoveTo(PrimaryPolicyRollupStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryPolicyRollupStatus.Active, "status");
    }

    public static void PrimaryPolicyRollupRejectsMovesFromClosed()
    {
        var item = PrimaryPolicyRollup.NewPrimaryPolicyRollup("code-999", "label-331");
        item.Status = PrimaryPolicyRollupStatus.Closed;
        Check.Fails(() => item.MoveTo(PrimaryPolicyRollupStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PrimaryPolicyRollupStartsInDraft();
        PrimaryPolicyRollupMovesToActive();
        PrimaryPolicyRollupRejectsMovesFromClosed();
    }
}
