namespace Acme.Workforce.Absence.Tests.Policy.Profile;

using Acme.Workforce.Absence.Policy.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PolicyProfileChecks
{
    public static void PolicyProfileStartsInDraft()
    {
        var item = PolicyProfile.NewPolicyProfile("code-422", "label-156");
        Check.Equal(item.Status, PolicyProfileStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyProfileMovesToActive()
    {
        var item = PolicyProfile.NewPolicyProfile("code-422", "label-156");
        try
        {
            item.MoveTo(PolicyProfileStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyProfileStatus.Active, "status");
    }

    public static void PolicyProfileRejectsMovesFromClosed()
    {
        var item = PolicyProfile.NewPolicyProfile("code-422", "label-156");
        item.Status = PolicyProfileStatus.Closed;
        Check.Fails(() => item.MoveTo(PolicyProfileStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PolicyProfileStartsInDraft();
        PolicyProfileMovesToActive();
        PolicyProfileRejectsMovesFromClosed();
    }
}
