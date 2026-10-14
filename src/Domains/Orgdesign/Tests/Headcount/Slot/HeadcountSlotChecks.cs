namespace Acme.Workforce.Orgdesign.Tests.Headcount.Slot;

using Acme.Workforce.Orgdesign.Headcount.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class HeadcountSlotChecks
{
    public static void HeadcountSlotStartsInRequested()
    {
        var item = HeadcountSlot.NewHeadcountSlot("code-7", "title-318");
        Check.Equal(item.Status, HeadcountSlotStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HeadcountSlotMovesToGranted()
    {
        var item = HeadcountSlot.NewHeadcountSlot("code-7", "title-318");
        try
        {
            item.MoveTo(HeadcountSlotStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HeadcountSlotStatus.Granted, "status");
    }

    public static void HeadcountSlotRejectsMovesFromExpired()
    {
        var item = HeadcountSlot.NewHeadcountSlot("code-7", "title-318");
        item.Status = HeadcountSlotStatus.Expired;
        Check.Fails(() => item.MoveTo(HeadcountSlotStatus.Requested), "move");
    }

    public static void RunAll()
    {
        HeadcountSlotStartsInRequested();
        HeadcountSlotMovesToGranted();
        HeadcountSlotRejectsMovesFromExpired();
    }
}
