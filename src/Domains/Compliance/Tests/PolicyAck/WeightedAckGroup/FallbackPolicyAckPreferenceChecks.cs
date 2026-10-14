namespace Acme.Workforce.Compliance.Tests.PolicyAck.WeightedAckGroup;

using Acme.Workforce.Compliance.PolicyAck.WeightedAckGroup;

/// <summary>Checks of weighted ack group code, run by the project's runner.</summary>
public static class FallbackPolicyAckPreferenceChecks
{
    public static void FallbackPolicyAckPreferenceStartsInPlanned()
    {
        var item = FallbackPolicyAckPreference.NewFallbackPolicyAckPreference("code-192", "title-859");
        Check.Equal(item.Status, FallbackPolicyAckPreferenceStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackPolicyAckPreferenceMovesToInProgress()
    {
        var item = FallbackPolicyAckPreference.NewFallbackPolicyAckPreference("code-192", "title-859");
        try
        {
            item.MoveTo(FallbackPolicyAckPreferenceStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackPolicyAckPreferenceStatus.InProgress, "status");
    }

    public static void FallbackPolicyAckPreferenceRejectsMovesFromCancelled()
    {
        var item = FallbackPolicyAckPreference.NewFallbackPolicyAckPreference("code-192", "title-859");
        item.Status = FallbackPolicyAckPreferenceStatus.Cancelled;
        Check.Fails(() => item.MoveTo(FallbackPolicyAckPreferenceStatus.Planned), "move");
    }

    public static void RunAll()
    {
        FallbackPolicyAckPreferenceStartsInPlanned();
        FallbackPolicyAckPreferenceMovesToInProgress();
        FallbackPolicyAckPreferenceRejectsMovesFromCancelled();
    }
}
