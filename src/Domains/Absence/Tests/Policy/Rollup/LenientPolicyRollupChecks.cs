namespace Acme.Workforce.Absence.Tests.Policy.Rollup;

using Acme.Workforce.Absence.Policy.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class LenientPolicyRollupChecks
{
    public static void LenientPolicyRollupStartsInRequested()
    {
        var item = LenientPolicyRollup.NewLenientPolicyRollup("code-773", "name-570");
        Check.Equal(item.Status, LenientPolicyRollupStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientPolicyRollupMovesToGranted()
    {
        var item = LenientPolicyRollup.NewLenientPolicyRollup("code-773", "name-570");
        try
        {
            item.MoveTo(LenientPolicyRollupStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientPolicyRollupStatus.Granted, "status");
    }

    public static void LenientPolicyRollupRejectsMovesFromExpired()
    {
        var item = LenientPolicyRollup.NewLenientPolicyRollup("code-773", "name-570");
        item.Status = LenientPolicyRollupStatus.Expired;
        Check.Fails(() => item.MoveTo(LenientPolicyRollupStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LenientPolicyRollupStartsInRequested();
        LenientPolicyRollupMovesToGranted();
        LenientPolicyRollupRejectsMovesFromExpired();
    }
}
