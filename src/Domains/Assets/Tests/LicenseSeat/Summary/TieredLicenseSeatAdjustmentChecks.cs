namespace Acme.Workforce.Assets.Tests.LicenseSeat.Summary;

using Acme.Workforce.Assets.LicenseSeat.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class TieredLicenseSeatAdjustmentChecks
{
    public static void TieredLicenseSeatAdjustmentStartsInIdle()
    {
        var item = TieredLicenseSeatAdjustment.NewTieredLicenseSeatAdjustment("code-242", "title-152");
        Check.Equal(item.Status, TieredLicenseSeatAdjustmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredLicenseSeatAdjustmentMovesToReady()
    {
        var item = TieredLicenseSeatAdjustment.NewTieredLicenseSeatAdjustment("code-242", "title-152");
        try
        {
            item.MoveTo(TieredLicenseSeatAdjustmentStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredLicenseSeatAdjustmentStatus.Ready, "status");
    }

    public static void TieredLicenseSeatAdjustmentRejectsMovesFromStopped()
    {
        var item = TieredLicenseSeatAdjustment.NewTieredLicenseSeatAdjustment("code-242", "title-152");
        item.Status = TieredLicenseSeatAdjustmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(TieredLicenseSeatAdjustmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        TieredLicenseSeatAdjustmentStartsInIdle();
        TieredLicenseSeatAdjustmentMovesToReady();
        TieredLicenseSeatAdjustmentRejectsMovesFromStopped();
    }
}
