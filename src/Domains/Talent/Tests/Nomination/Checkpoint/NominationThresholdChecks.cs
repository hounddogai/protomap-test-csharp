namespace Acme.Workforce.Talent.Tests.Nomination.Checkpoint;

using Acme.Workforce.Talent.Nomination.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class NominationThresholdChecks
{
    public static void NominationThresholdStartsInIdle()
    {
        var item = NominationThreshold.NewNominationThreshold("code-790", "label-872");
        Check.Equal(item.Status, NominationThresholdStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void NominationThresholdMovesToWarming()
    {
        var item = NominationThreshold.NewNominationThreshold("code-790", "label-872");
        try
        {
            item.MoveTo(NominationThresholdStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, NominationThresholdStatus.Warming, "status");
    }

    public static void NominationThresholdRejectsMovesFromStopped()
    {
        var item = NominationThreshold.NewNominationThreshold("code-790", "label-872");
        item.Status = NominationThresholdStatus.Stopped;
        Check.Fails(() => item.MoveTo(NominationThresholdStatus.Idle), "move");
    }

    public static void RunAll()
    {
        NominationThresholdStartsInIdle();
        NominationThresholdMovesToWarming();
        NominationThresholdRejectsMovesFromStopped();
    }
}
