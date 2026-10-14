namespace Acme.Workforce.Workplace.Tests.Desk.Entry;

using Acme.Workforce.Workplace.Desk.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class DeskQuotaChecks
{
    public static void DeskQuotaStartsInNew()
    {
        var item = DeskQuota.NewDeskQuota("code-555", "title-66");
        Check.Equal(item.Status, DeskQuotaStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeskQuotaMovesToQueued()
    {
        var item = DeskQuota.NewDeskQuota("code-555", "title-66");
        try
        {
            item.MoveTo(DeskQuotaStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeskQuotaStatus.Queued, "status");
    }

    public static void DeskQuotaRejectsMovesFromFailed()
    {
        var item = DeskQuota.NewDeskQuota("code-555", "title-66");
        item.Status = DeskQuotaStatus.Failed;
        Check.Fails(() => item.MoveTo(DeskQuotaStatus.New), "move");
    }

    public static void RunAll()
    {
        DeskQuotaStartsInNew();
        DeskQuotaMovesToQueued();
        DeskQuotaRejectsMovesFromFailed();
    }
}
