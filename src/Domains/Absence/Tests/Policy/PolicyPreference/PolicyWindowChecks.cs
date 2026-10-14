namespace Acme.Workforce.Absence.Tests.Policy.PolicyPreference;

using Acme.Workforce.Absence.Policy.PolicyPreference;

/// <summary>Checks of policy preference code, run by the project's runner.</summary>
public static class PolicyWindowChecks
{
    public static void PolicyWindowStartsInCreated()
    {
        var item = PolicyWindow.NewPolicyWindow("code-704", "name-860");
        Check.Equal(item.Status, PolicyWindowStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyWindowMovesToPublished()
    {
        var item = PolicyWindow.NewPolicyWindow("code-704", "name-860");
        try
        {
            item.MoveTo(PolicyWindowStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyWindowStatus.Published, "status");
    }

    public static void PolicyWindowRejectsMovesFromRetired()
    {
        var item = PolicyWindow.NewPolicyWindow("code-704", "name-860");
        item.Status = PolicyWindowStatus.Retired;
        Check.Fails(() => item.MoveTo(PolicyWindowStatus.Created), "move");
    }

    public static void RunAll()
    {
        PolicyWindowStartsInCreated();
        PolicyWindowMovesToPublished();
        PolicyWindowRejectsMovesFromRetired();
    }
}
