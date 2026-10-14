namespace Acme.Workforce.Compliance.Tests.PolicyAck.AckGroup;

using Acme.Workforce.Compliance.PolicyAck.AckGroup;

/// <summary>Checks of ack group code, run by the project's runner.</summary>
public static class StrictPolicyAckTemplateChecks
{
    public static void StrictPolicyAckTemplateStartsInProposed()
    {
        var item = StrictPolicyAckTemplate.NewStrictPolicyAckTemplate("code-586", "label-604");
        Check.Equal(item.Status, StrictPolicyAckTemplateStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictPolicyAckTemplateMovesToRejected()
    {
        var item = StrictPolicyAckTemplate.NewStrictPolicyAckTemplate("code-586", "label-604");
        try
        {
            item.MoveTo(StrictPolicyAckTemplateStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictPolicyAckTemplateStatus.Rejected, "status");
    }

    public static void StrictPolicyAckTemplateRejectsMovesFromWithdrawn()
    {
        var item = StrictPolicyAckTemplate.NewStrictPolicyAckTemplate("code-586", "label-604");
        item.Status = StrictPolicyAckTemplateStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(StrictPolicyAckTemplateStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        StrictPolicyAckTemplateStartsInProposed();
        StrictPolicyAckTemplateMovesToRejected();
        StrictPolicyAckTemplateRejectsMovesFromWithdrawn();
    }
}
