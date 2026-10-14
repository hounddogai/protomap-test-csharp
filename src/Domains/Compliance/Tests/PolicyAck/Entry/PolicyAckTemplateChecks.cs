namespace Acme.Workforce.Compliance.Tests.PolicyAck.Entry;

using Acme.Workforce.Compliance.PolicyAck.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class PolicyAckTemplateChecks
{
    public static void PolicyAckTemplateStartsInPlanned()
    {
        var item = PolicyAckTemplate.NewPolicyAckTemplate("code-718", "name-307");
        Check.Equal(item.Status, PolicyAckTemplateStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyAckTemplateMovesToCompleted()
    {
        var item = PolicyAckTemplate.NewPolicyAckTemplate("code-718", "name-307");
        try
        {
            item.MoveTo(PolicyAckTemplateStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyAckTemplateStatus.Completed, "status");
    }

    public static void PolicyAckTemplateRejectsMovesFromCancelled()
    {
        var item = PolicyAckTemplate.NewPolicyAckTemplate("code-718", "name-307");
        item.Status = PolicyAckTemplateStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PolicyAckTemplateStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PolicyAckTemplateStartsInPlanned();
        PolicyAckTemplateMovesToCompleted();
        PolicyAckTemplateRejectsMovesFromCancelled();
    }
}
