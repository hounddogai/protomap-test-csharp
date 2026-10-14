namespace Acme.Workforce.Assets.Tests.Laptop.Draft;

using Acme.Workforce.Assets.Laptop.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LaptopDraftChecks
{
    public static void LaptopDraftStartsInRequested()
    {
        var item = LaptopDraft.NewLaptopDraft("code-462", "label-802");
        Check.Equal(item.Status, LaptopDraftStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LaptopDraftMovesToGranted()
    {
        var item = LaptopDraft.NewLaptopDraft("code-462", "label-802");
        try
        {
            item.MoveTo(LaptopDraftStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LaptopDraftStatus.Granted, "status");
    }

    public static void LaptopDraftRejectsMovesFromExpired()
    {
        var item = LaptopDraft.NewLaptopDraft("code-462", "label-802");
        item.Status = LaptopDraftStatus.Expired;
        Check.Fails(() => item.MoveTo(LaptopDraftStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LaptopDraftStartsInRequested();
        LaptopDraftMovesToGranted();
        LaptopDraftRejectsMovesFromExpired();
    }
}
