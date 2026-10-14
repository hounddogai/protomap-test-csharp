namespace Acme.Workforce.Talent.Tests.Nomination.Template;

using Acme.Workforce.Talent.Nomination.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class AdaptiveNominationTargetChecks
{
    public static void AdaptiveNominationTargetStartsInPlanned()
    {
        var item = AdaptiveNominationTarget.NewAdaptiveNominationTarget("code-895", "name-298");
        Check.Equal(item.Status, AdaptiveNominationTargetStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveNominationTargetMovesToScheduled()
    {
        var item = AdaptiveNominationTarget.NewAdaptiveNominationTarget("code-895", "name-298");
        try
        {
            item.MoveTo(AdaptiveNominationTargetStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveNominationTargetStatus.Scheduled, "status");
    }

    public static void AdaptiveNominationTargetRejectsMovesFromCancelled()
    {
        var item = AdaptiveNominationTarget.NewAdaptiveNominationTarget("code-895", "name-298");
        item.Status = AdaptiveNominationTargetStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AdaptiveNominationTargetStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AdaptiveNominationTargetStartsInPlanned();
        AdaptiveNominationTargetMovesToScheduled();
        AdaptiveNominationTargetRejectsMovesFromCancelled();
    }
}
