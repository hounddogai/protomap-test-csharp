namespace Acme.Workforce.Compliance.Tests.PolicyAck.AckCycle;

using Acme.Workforce.Compliance.PolicyAck.AckCycle;

/// <summary>Checks of ack cycle code, run by the project's runner.</summary>
public static class LenientPolicyAckCycleChecks
{
    public static void LenientPolicyAckCycleStartsInOpen()
    {
        var item = LenientPolicyAckCycle.NewLenientPolicyAckCycle("code-829", "label-828");
        Check.Equal(item.Status, LenientPolicyAckCycleStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientPolicyAckCycleMovesToResolved()
    {
        var item = LenientPolicyAckCycle.NewLenientPolicyAckCycle("code-829", "label-828");
        try
        {
            item.MoveTo(LenientPolicyAckCycleStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientPolicyAckCycleStatus.Resolved, "status");
    }

    public static void LenientPolicyAckCycleRejectsMovesFromArchived()
    {
        var item = LenientPolicyAckCycle.NewLenientPolicyAckCycle("code-829", "label-828");
        item.Status = LenientPolicyAckCycleStatus.Archived;
        Check.Fails(() => item.MoveTo(LenientPolicyAckCycleStatus.Open), "move");
    }

    public static void RunAll()
    {
        LenientPolicyAckCycleStartsInOpen();
        LenientPolicyAckCycleMovesToResolved();
        LenientPolicyAckCycleRejectsMovesFromArchived();
    }
}
