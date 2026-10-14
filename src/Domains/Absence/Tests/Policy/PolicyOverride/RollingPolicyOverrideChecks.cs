namespace Acme.Workforce.Absence.Tests.Policy.PolicyOverride;

using Acme.Workforce.Absence.Policy.PolicyOverride;

/// <summary>Checks of policy override code, run by the project's runner.</summary>
public static class RollingPolicyOverrideChecks
{
    public static void RollingPolicyOverrideStartsInNew()
    {
        var item = RollingPolicyOverride.NewRollingPolicyOverride("code-598", "name-579");
        Check.Equal(item.Status, RollingPolicyOverrideStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingPolicyOverrideMovesToRunning()
    {
        var item = RollingPolicyOverride.NewRollingPolicyOverride("code-598", "name-579");
        try
        {
            item.MoveTo(RollingPolicyOverrideStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingPolicyOverrideStatus.Running, "status");
    }

    public static void RollingPolicyOverrideRejectsMovesFromFailed()
    {
        var item = RollingPolicyOverride.NewRollingPolicyOverride("code-598", "name-579");
        item.Status = RollingPolicyOverrideStatus.Failed;
        Check.Fails(() => item.MoveTo(RollingPolicyOverrideStatus.New), "move");
    }

    public static void RunAll()
    {
        RollingPolicyOverrideStartsInNew();
        RollingPolicyOverrideMovesToRunning();
        RollingPolicyOverrideRejectsMovesFromFailed();
    }
}
