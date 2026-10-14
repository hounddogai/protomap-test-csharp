namespace Acme.Workforce.Talent.Tests.Nomination.Cycle;

using Acme.Workforce.Talent.Nomination.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class AdaptiveNominationVarianceChecks
{
    public static void AdaptiveNominationVarianceStartsInRequested()
    {
        var item = AdaptiveNominationVariance.NewAdaptiveNominationVariance("code-591", "label-574");
        Check.Equal(item.Status, AdaptiveNominationVarianceStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveNominationVarianceMovesToExpired()
    {
        var item = AdaptiveNominationVariance.NewAdaptiveNominationVariance("code-591", "label-574");
        try
        {
            item.MoveTo(AdaptiveNominationVarianceStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveNominationVarianceStatus.Expired, "status");
    }

    public static void AdaptiveNominationVarianceRejectsMovesFromExpired()
    {
        var item = AdaptiveNominationVariance.NewAdaptiveNominationVariance("code-591", "label-574");
        item.Status = AdaptiveNominationVarianceStatus.Expired;
        Check.Fails(() => item.MoveTo(AdaptiveNominationVarianceStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AdaptiveNominationVarianceStartsInRequested();
        AdaptiveNominationVarianceMovesToExpired();
        AdaptiveNominationVarianceRejectsMovesFromExpired();
    }
}
