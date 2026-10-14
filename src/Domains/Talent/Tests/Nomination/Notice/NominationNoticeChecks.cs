namespace Acme.Workforce.Talent.Tests.Nomination.Notice;

using Acme.Workforce.Talent.Nomination.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class NominationNoticeChecks
{
    public static void NominationNoticeStartsInRequested()
    {
        var item = NominationNotice.NewNominationNotice("code-930", "title-886");
        Check.Equal(item.Status, NominationNoticeStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void NominationNoticeMovesToGranted()
    {
        var item = NominationNotice.NewNominationNotice("code-930", "title-886");
        try
        {
            item.MoveTo(NominationNoticeStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, NominationNoticeStatus.Granted, "status");
    }

    public static void NominationNoticeRejectsMovesFromExpired()
    {
        var item = NominationNotice.NewNominationNotice("code-930", "title-886");
        item.Status = NominationNoticeStatus.Expired;
        Check.Fails(() => item.MoveTo(NominationNoticeStatus.Requested), "move");
    }

    public static void RunAll()
    {
        NominationNoticeStartsInRequested();
        NominationNoticeMovesToGranted();
        NominationNoticeRejectsMovesFromExpired();
    }
}
