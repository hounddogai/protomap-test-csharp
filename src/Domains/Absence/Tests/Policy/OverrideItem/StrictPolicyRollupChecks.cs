namespace Acme.Workforce.Absence.Tests.Policy.OverrideItem;

using Acme.Workforce.Absence.Policy.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class StrictPolicyRollupChecks
{
    public static void StrictPolicyRollupStartsInCreated()
    {
        var item = StrictPolicyRollup.NewStrictPolicyRollup("code-53", "label-829");
        Check.Equal(item.Status, StrictPolicyRollupStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictPolicyRollupMovesToValidated()
    {
        var item = StrictPolicyRollup.NewStrictPolicyRollup("code-53", "label-829");
        try
        {
            item.MoveTo(StrictPolicyRollupStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictPolicyRollupStatus.Validated, "status");
    }

    public static void StrictPolicyRollupRejectsMovesFromRetired()
    {
        var item = StrictPolicyRollup.NewStrictPolicyRollup("code-53", "label-829");
        item.Status = StrictPolicyRollupStatus.Retired;
        Check.Fails(() => item.MoveTo(StrictPolicyRollupStatus.Created), "move");
    }

    public static void RunAll()
    {
        StrictPolicyRollupStartsInCreated();
        StrictPolicyRollupMovesToValidated();
        StrictPolicyRollupRejectsMovesFromRetired();
    }
}
