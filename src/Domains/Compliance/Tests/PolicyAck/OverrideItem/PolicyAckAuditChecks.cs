namespace Acme.Workforce.Compliance.Tests.PolicyAck.OverrideItem;

using Acme.Workforce.Compliance.PolicyAck.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class PolicyAckAuditChecks
{
    public static void PolicyAckAuditStartsInIdle()
    {
        var item = PolicyAckAudit.NewPolicyAckAudit("code-356", "label-410");
        Check.Equal(item.Status, PolicyAckAuditStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyAckAuditMovesToDraining()
    {
        var item = PolicyAckAudit.NewPolicyAckAudit("code-356", "label-410");
        try
        {
            item.MoveTo(PolicyAckAuditStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyAckAuditStatus.Draining, "status");
    }

    public static void PolicyAckAuditRejectsMovesFromStopped()
    {
        var item = PolicyAckAudit.NewPolicyAckAudit("code-356", "label-410");
        item.Status = PolicyAckAuditStatus.Stopped;
        Check.Fails(() => item.MoveTo(PolicyAckAuditStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PolicyAckAuditStartsInIdle();
        PolicyAckAuditMovesToDraining();
        PolicyAckAuditRejectsMovesFromStopped();
    }
}
