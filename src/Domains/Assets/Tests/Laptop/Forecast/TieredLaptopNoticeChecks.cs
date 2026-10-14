namespace Acme.Workforce.Assets.Tests.Laptop.Forecast;

using Acme.Workforce.Assets.Laptop.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class TieredLaptopNoticeChecks
{
    public static void TieredLaptopNoticeStartsInDraft()
    {
        var item = TieredLaptopNotice.NewTieredLaptopNotice("code-430", "title-900");
        Check.Equal(item.Status, TieredLaptopNoticeStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredLaptopNoticeMovesToPending()
    {
        var item = TieredLaptopNotice.NewTieredLaptopNotice("code-430", "title-900");
        try
        {
            item.MoveTo(TieredLaptopNoticeStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredLaptopNoticeStatus.Pending, "status");
    }

    public static void TieredLaptopNoticeRejectsMovesFromClosed()
    {
        var item = TieredLaptopNotice.NewTieredLaptopNotice("code-430", "title-900");
        item.Status = TieredLaptopNoticeStatus.Closed;
        Check.Fails(() => item.MoveTo(TieredLaptopNoticeStatus.Draft), "move");
    }

    public static void RunAll()
    {
        TieredLaptopNoticeStartsInDraft();
        TieredLaptopNoticeMovesToPending();
        TieredLaptopNoticeRejectsMovesFromClosed();
    }
}
