namespace Acme.Workforce.Talent.Tests.Nomination.NominationWindow;

using Acme.Workforce.Talent.Nomination.NominationWindow;

/// <summary>Checks of nomination window code, run by the project's runner.</summary>
public static class NominationSlotChecks
{
    public static void NominationSlotStartsInOpen()
    {
        var item = NominationSlot.NewNominationSlot("code-4", "title-697");
        Check.Equal(item.Status, NominationSlotStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void NominationSlotMovesToInReview()
    {
        var item = NominationSlot.NewNominationSlot("code-4", "title-697");
        try
        {
            item.MoveTo(NominationSlotStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, NominationSlotStatus.InReview, "status");
    }

    public static void NominationSlotRejectsMovesFromArchived()
    {
        var item = NominationSlot.NewNominationSlot("code-4", "title-697");
        item.Status = NominationSlotStatus.Archived;
        Check.Fails(() => item.MoveTo(NominationSlotStatus.Open), "move");
    }

    public static void RunAll()
    {
        NominationSlotStartsInOpen();
        NominationSlotMovesToInReview();
        NominationSlotRejectsMovesFromArchived();
    }
}
