namespace Acme.Workforce.Compliance.Tests.PolicyAck.OverrideItem;

using Acme.Workforce.Compliance.PolicyAck.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class PolicyAckMappingChecks
{
    public static void PolicyAckMappingStartsInPlanned()
    {
        var item = PolicyAckMapping.NewPolicyAckMapping("code-31", "title-133");
        Check.Equal(item.Status, PolicyAckMappingStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyAckMappingMovesToCompleted()
    {
        var item = PolicyAckMapping.NewPolicyAckMapping("code-31", "title-133");
        try
        {
            item.MoveTo(PolicyAckMappingStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyAckMappingStatus.Completed, "status");
    }

    public static void PolicyAckMappingRejectsMovesFromCancelled()
    {
        var item = PolicyAckMapping.NewPolicyAckMapping("code-31", "title-133");
        item.Status = PolicyAckMappingStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PolicyAckMappingStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PolicyAckMappingStartsInPlanned();
        PolicyAckMappingMovesToCompleted();
        PolicyAckMappingRejectsMovesFromCancelled();
    }
}
