namespace Acme.Workforce.Assets.Tests.Warranty.Notice;

using Acme.Workforce.Assets.Warranty.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class WarrantyNoticeChecks
{
    public static void WarrantyNoticeStartsInNew()
    {
        var item = WarrantyNotice.NewWarrantyNotice("code-641", "title-708");
        Check.Equal(item.Status, WarrantyNoticeStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WarrantyNoticeMovesToQueued()
    {
        var item = WarrantyNotice.NewWarrantyNotice("code-641", "title-708");
        try
        {
            item.MoveTo(WarrantyNoticeStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WarrantyNoticeStatus.Queued, "status");
    }

    public static void WarrantyNoticeRejectsMovesFromFailed()
    {
        var item = WarrantyNotice.NewWarrantyNotice("code-641", "title-708");
        item.Status = WarrantyNoticeStatus.Failed;
        Check.Fails(() => item.MoveTo(WarrantyNoticeStatus.New), "move");
    }

    public static void RunAll()
    {
        WarrantyNoticeStartsInNew();
        WarrantyNoticeMovesToQueued();
        WarrantyNoticeRejectsMovesFromFailed();
    }
}
