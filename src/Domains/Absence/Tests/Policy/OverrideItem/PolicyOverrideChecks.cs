namespace Acme.Workforce.Absence.Tests.Policy.OverrideItem;

using Acme.Workforce.Absence.Policy.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class PolicyOverrideChecks
{
    public static void PolicyOverrideStartsInRequested()
    {
        var item = PolicyOverride.NewPolicyOverride("code-521", "name-254");
        Check.Equal(item.Status, PolicyOverrideStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyOverrideMovesToGranted()
    {
        var item = PolicyOverride.NewPolicyOverride("code-521", "name-254");
        try
        {
            item.MoveTo(PolicyOverrideStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyOverrideStatus.Granted, "status");
    }

    public static void PolicyOverrideRejectsMovesFromExpired()
    {
        var item = PolicyOverride.NewPolicyOverride("code-521", "name-254");
        item.Status = PolicyOverrideStatus.Expired;
        Check.Fails(() => item.MoveTo(PolicyOverrideStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PolicyOverrideStartsInRequested();
        PolicyOverrideMovesToGranted();
        PolicyOverrideRejectsMovesFromExpired();
    }
}
