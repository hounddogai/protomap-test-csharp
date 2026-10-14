namespace Acme.Workforce.Assets.Tests.LicenseSeat.Adjustment;

using Acme.Workforce.Assets.LicenseSeat.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LicenseSeatAdjustmentChecks
{
    public static void LicenseSeatAdjustmentStartsInNew()
    {
        var item = LicenseSeatAdjustment.NewLicenseSeatAdjustment("code-640", "name-407");
        Check.Equal(item.Status, LicenseSeatAdjustmentStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LicenseSeatAdjustmentMovesToFailed()
    {
        var item = LicenseSeatAdjustment.NewLicenseSeatAdjustment("code-640", "name-407");
        try
        {
            item.MoveTo(LicenseSeatAdjustmentStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LicenseSeatAdjustmentStatus.Failed, "status");
    }

    public static void LicenseSeatAdjustmentRejectsMovesFromFailed()
    {
        var item = LicenseSeatAdjustment.NewLicenseSeatAdjustment("code-640", "name-407");
        item.Status = LicenseSeatAdjustmentStatus.Failed;
        Check.Fails(() => item.MoveTo(LicenseSeatAdjustmentStatus.New), "move");
    }

    public static void RunAll()
    {
        LicenseSeatAdjustmentStartsInNew();
        LicenseSeatAdjustmentMovesToFailed();
        LicenseSeatAdjustmentRejectsMovesFromFailed();
    }
}
