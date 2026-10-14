namespace Acme.Workforce.Absence.Tests.Policy.Cycle;

using Acme.Workforce.Absence.Policy.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class PolicyBundleChecks
{
    public static void PolicyBundleStartsInIdle()
    {
        var item = PolicyBundle.NewPolicyBundle("code-832", "label-514");
        Check.Equal(item.Status, PolicyBundleStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyBundleMovesToWarming()
    {
        var item = PolicyBundle.NewPolicyBundle("code-832", "label-514");
        try
        {
            item.MoveTo(PolicyBundleStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyBundleStatus.Warming, "status");
    }

    public static void PolicyBundleRejectsMovesFromStopped()
    {
        var item = PolicyBundle.NewPolicyBundle("code-832", "label-514");
        item.Status = PolicyBundleStatus.Stopped;
        Check.Fails(() => item.MoveTo(PolicyBundleStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PolicyBundleStartsInIdle();
        PolicyBundleMovesToWarming();
        PolicyBundleRejectsMovesFromStopped();
    }
}
