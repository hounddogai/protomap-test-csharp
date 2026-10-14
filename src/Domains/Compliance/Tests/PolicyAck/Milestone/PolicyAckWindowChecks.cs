namespace Acme.Workforce.Compliance.Tests.PolicyAck.Milestone;

using Acme.Workforce.Compliance.PolicyAck.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class PolicyAckWindowChecks
{
    public static void PolicyAckWindowStartsInCreated()
    {
        var item = PolicyAckWindow.NewPolicyAckWindow("code-202", "name-110");
        Check.Equal(item.Status, PolicyAckWindowStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyAckWindowMovesToValidated()
    {
        var item = PolicyAckWindow.NewPolicyAckWindow("code-202", "name-110");
        try
        {
            item.MoveTo(PolicyAckWindowStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyAckWindowStatus.Validated, "status");
    }

    public static void PolicyAckWindowRejectsMovesFromRetired()
    {
        var item = PolicyAckWindow.NewPolicyAckWindow("code-202", "name-110");
        item.Status = PolicyAckWindowStatus.Retired;
        Check.Fails(() => item.MoveTo(PolicyAckWindowStatus.Created), "move");
    }

    public static void RunAll()
    {
        PolicyAckWindowStartsInCreated();
        PolicyAckWindowMovesToValidated();
        PolicyAckWindowRejectsMovesFromRetired();
    }
}
