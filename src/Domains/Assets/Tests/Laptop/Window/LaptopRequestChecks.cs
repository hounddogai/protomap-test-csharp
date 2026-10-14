namespace Acme.Workforce.Assets.Tests.Laptop.Window;

using Acme.Workforce.Assets.Laptop.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class LaptopRequestChecks
{
    public static void LaptopRequestStartsInDraft()
    {
        var item = LaptopRequest.NewLaptopRequest("code-437", "label-488");
        Check.Equal(item.Status, LaptopRequestStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LaptopRequestMovesToClosed()
    {
        var item = LaptopRequest.NewLaptopRequest("code-437", "label-488");
        try
        {
            item.MoveTo(LaptopRequestStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LaptopRequestStatus.Closed, "status");
    }

    public static void LaptopRequestRejectsMovesFromClosed()
    {
        var item = LaptopRequest.NewLaptopRequest("code-437", "label-488");
        item.Status = LaptopRequestStatus.Closed;
        Check.Fails(() => item.MoveTo(LaptopRequestStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LaptopRequestStartsInDraft();
        LaptopRequestMovesToClosed();
        LaptopRequestRejectsMovesFromClosed();
    }
}
