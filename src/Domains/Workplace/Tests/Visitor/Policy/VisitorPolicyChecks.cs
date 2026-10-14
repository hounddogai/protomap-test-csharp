namespace Acme.Workforce.Workplace.Tests.Visitor.Policy;

using Acme.Workforce.Workplace.Visitor.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class VisitorPolicyChecks
{
    public static void VisitorPolicyStartsInProposed()
    {
        var item = VisitorPolicy.NewVisitorPolicy("code-224", "name-728");
        Check.Equal(item.Status, VisitorPolicyStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VisitorPolicyMovesToApproved()
    {
        var item = VisitorPolicy.NewVisitorPolicy("code-224", "name-728");
        try
        {
            item.MoveTo(VisitorPolicyStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VisitorPolicyStatus.Approved, "status");
    }

    public static void VisitorPolicyRejectsMovesFromWithdrawn()
    {
        var item = VisitorPolicy.NewVisitorPolicy("code-224", "name-728");
        item.Status = VisitorPolicyStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(VisitorPolicyStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        VisitorPolicyStartsInProposed();
        VisitorPolicyMovesToApproved();
        VisitorPolicyRejectsMovesFromWithdrawn();
    }
}
