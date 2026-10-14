namespace Acme.Workforce.Compliance.Tests.PolicyAck.Entry;

using Acme.Workforce.Compliance.PolicyAck.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class FallbackPolicyAckRequestChecks
{
    public static void FallbackPolicyAckRequestStartsInPlanned()
    {
        var item = FallbackPolicyAckRequest.NewFallbackPolicyAckRequest("code-523", "label-453");
        Check.Equal(item.Status, FallbackPolicyAckRequestStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackPolicyAckRequestMovesToCancelled()
    {
        var item = FallbackPolicyAckRequest.NewFallbackPolicyAckRequest("code-523", "label-453");
        try
        {
            item.MoveTo(FallbackPolicyAckRequestStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackPolicyAckRequestStatus.Cancelled, "status");
    }

    public static void FallbackPolicyAckRequestRejectsMovesFromCancelled()
    {
        var item = FallbackPolicyAckRequest.NewFallbackPolicyAckRequest("code-523", "label-453");
        item.Status = FallbackPolicyAckRequestStatus.Cancelled;
        Check.Fails(() => item.MoveTo(FallbackPolicyAckRequestStatus.Planned), "move");
    }

    public static void RunAll()
    {
        FallbackPolicyAckRequestStartsInPlanned();
        FallbackPolicyAckRequestMovesToCancelled();
        FallbackPolicyAckRequestRejectsMovesFromCancelled();
    }
}
