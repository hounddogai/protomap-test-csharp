namespace Acme.Workforce.Compliance.Tests.PolicyAck.Exception;

using Acme.Workforce.Compliance.PolicyAck.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class PolicyAckManifestChecks
{
    public static void PolicyAckManifestStartsInPlanned()
    {
        var item = PolicyAckManifest.NewPolicyAckManifest("code-943", "label-97");
        Check.Equal(item.Status, PolicyAckManifestStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyAckManifestMovesToScheduled()
    {
        var item = PolicyAckManifest.NewPolicyAckManifest("code-943", "label-97");
        try
        {
            item.MoveTo(PolicyAckManifestStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyAckManifestStatus.Scheduled, "status");
    }

    public static void PolicyAckManifestRejectsMovesFromCancelled()
    {
        var item = PolicyAckManifest.NewPolicyAckManifest("code-943", "label-97");
        item.Status = PolicyAckManifestStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PolicyAckManifestStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PolicyAckManifestStartsInPlanned();
        PolicyAckManifestMovesToScheduled();
        PolicyAckManifestRejectsMovesFromCancelled();
    }
}
