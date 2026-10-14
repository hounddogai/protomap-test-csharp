namespace Acme.Workforce.Talent.Tests.Nomination.Milestone;

using Acme.Workforce.Talent.Nomination.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class ScheduledNominationExceptionChecks
{
    public static void ScheduledNominationExceptionStartsInNew()
    {
        var item = ScheduledNominationException.NewScheduledNominationException("code-939", "title-290");
        Check.Equal(item.Status, ScheduledNominationExceptionStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledNominationExceptionMovesToDone()
    {
        var item = ScheduledNominationException.NewScheduledNominationException("code-939", "title-290");
        try
        {
            item.MoveTo(ScheduledNominationExceptionStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledNominationExceptionStatus.Done, "status");
    }

    public static void ScheduledNominationExceptionRejectsMovesFromFailed()
    {
        var item = ScheduledNominationException.NewScheduledNominationException("code-939", "title-290");
        item.Status = ScheduledNominationExceptionStatus.Failed;
        Check.Fails(() => item.MoveTo(ScheduledNominationExceptionStatus.New), "move");
    }

    public static void RunAll()
    {
        ScheduledNominationExceptionStartsInNew();
        ScheduledNominationExceptionMovesToDone();
        ScheduledNominationExceptionRejectsMovesFromFailed();
    }
}
