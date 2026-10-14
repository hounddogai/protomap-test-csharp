namespace Acme.Workforce.Absence.Tests.Policy.Group;

using Acme.Workforce.Absence.Policy.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PolicyGroupChecks
{
    public static void PolicyGroupStartsInOpen()
    {
        var item = PolicyGroup.NewPolicyGroup("code-330", "name-771");
        Check.Equal(item.Status, PolicyGroupStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyGroupMovesToReopened()
    {
        var item = PolicyGroup.NewPolicyGroup("code-330", "name-771");
        try
        {
            item.MoveTo(PolicyGroupStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyGroupStatus.Reopened, "status");
    }

    public static void PolicyGroupRejectsMovesFromArchived()
    {
        var item = PolicyGroup.NewPolicyGroup("code-330", "name-771");
        item.Status = PolicyGroupStatus.Archived;
        Check.Fails(() => item.MoveTo(PolicyGroupStatus.Open), "move");
    }

    public static void RunAll()
    {
        PolicyGroupStartsInOpen();
        PolicyGroupMovesToReopened();
        PolicyGroupRejectsMovesFromArchived();
    }
}
