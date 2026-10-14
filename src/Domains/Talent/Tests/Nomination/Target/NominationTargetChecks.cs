namespace Acme.Workforce.Talent.Tests.Nomination.Target;

using Acme.Workforce.Talent.Nomination.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class NominationTargetChecks
{
    public static void NominationTargetStartsInPlanned()
    {
        var item = NominationTarget.NewNominationTarget("code-512", "name-19");
        Check.Equal(item.Status, NominationTargetStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void NominationTargetMovesToScheduled()
    {
        var item = NominationTarget.NewNominationTarget("code-512", "name-19");
        try
        {
            item.MoveTo(NominationTargetStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, NominationTargetStatus.Scheduled, "status");
    }

    public static void NominationTargetRejectsMovesFromCancelled()
    {
        var item = NominationTarget.NewNominationTarget("code-512", "name-19");
        item.Status = NominationTargetStatus.Cancelled;
        Check.Fails(() => item.MoveTo(NominationTargetStatus.Planned), "move");
    }

    public static void RunAll()
    {
        NominationTargetStartsInPlanned();
        NominationTargetMovesToScheduled();
        NominationTargetRejectsMovesFromCancelled();
    }
}
