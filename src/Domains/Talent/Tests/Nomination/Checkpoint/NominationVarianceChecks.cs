namespace Acme.Workforce.Talent.Tests.Nomination.Checkpoint;

using Acme.Workforce.Talent.Nomination.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class NominationVarianceChecks
{
    public static void NominationVarianceStartsInDraft()
    {
        var item = NominationVariance.NewNominationVariance("code-486", "label-583");
        Check.Equal(item.Status, NominationVarianceStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void NominationVarianceMovesToPending()
    {
        var item = NominationVariance.NewNominationVariance("code-486", "label-583");
        try
        {
            item.MoveTo(NominationVarianceStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, NominationVarianceStatus.Pending, "status");
    }

    public static void NominationVarianceRejectsMovesFromClosed()
    {
        var item = NominationVariance.NewNominationVariance("code-486", "label-583");
        item.Status = NominationVarianceStatus.Closed;
        Check.Fails(() => item.MoveTo(NominationVarianceStatus.Draft), "move");
    }

    public static void RunAll()
    {
        NominationVarianceStartsInDraft();
        NominationVarianceMovesToPending();
        NominationVarianceRejectsMovesFromClosed();
    }
}
