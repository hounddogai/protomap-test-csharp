namespace Acme.Workforce.Payrollops.Tests.Garnishment.Target;

using Acme.Workforce.Payrollops.Garnishment.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class TieredGarnishmentTargetChecks
{
    public static void TieredGarnishmentTargetStartsInDraft()
    {
        var item = TieredGarnishmentTarget.NewTieredGarnishmentTarget("code-951", "label-982");
        Check.Equal(item.Status, TieredGarnishmentTargetStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredGarnishmentTargetMovesToClosed()
    {
        var item = TieredGarnishmentTarget.NewTieredGarnishmentTarget("code-951", "label-982");
        try
        {
            item.MoveTo(TieredGarnishmentTargetStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredGarnishmentTargetStatus.Closed, "status");
    }

    public static void TieredGarnishmentTargetRejectsMovesFromClosed()
    {
        var item = TieredGarnishmentTarget.NewTieredGarnishmentTarget("code-951", "label-982");
        item.Status = TieredGarnishmentTargetStatus.Closed;
        Check.Fails(() => item.MoveTo(TieredGarnishmentTargetStatus.Draft), "move");
    }

    public static void RunAll()
    {
        TieredGarnishmentTargetStartsInDraft();
        TieredGarnishmentTargetMovesToClosed();
        TieredGarnishmentTargetRejectsMovesFromClosed();
    }
}
