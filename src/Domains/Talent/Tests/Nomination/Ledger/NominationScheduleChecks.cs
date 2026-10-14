namespace Acme.Workforce.Talent.Tests.Nomination.Ledger;

using Acme.Workforce.Talent.Nomination.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class NominationScheduleChecks
{
    public static void NominationScheduleStartsInCreated()
    {
        var item = NominationSchedule.NewNominationSchedule("code-324", "name-266");
        Check.Equal(item.Status, NominationScheduleStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void NominationScheduleMovesToPublished()
    {
        var item = NominationSchedule.NewNominationSchedule("code-324", "name-266");
        try
        {
            item.MoveTo(NominationScheduleStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, NominationScheduleStatus.Published, "status");
    }

    public static void NominationScheduleRejectsMovesFromRetired()
    {
        var item = NominationSchedule.NewNominationSchedule("code-324", "name-266");
        item.Status = NominationScheduleStatus.Retired;
        Check.Fails(() => item.MoveTo(NominationScheduleStatus.Created), "move");
    }

    public static void RunAll()
    {
        NominationScheduleStartsInCreated();
        NominationScheduleMovesToPublished();
        NominationScheduleRejectsMovesFromRetired();
    }
}
