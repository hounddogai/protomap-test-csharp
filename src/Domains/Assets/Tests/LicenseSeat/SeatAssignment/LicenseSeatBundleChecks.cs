namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatAssignment;

using Acme.Workforce.Assets.LicenseSeat.SeatAssignment;

/// <summary>Checks of seat assignment code, run by the project's runner.</summary>
public static class LicenseSeatBundleChecks
{
    public static void LicenseSeatBundleStartsInPlanned()
    {
        var item = LicenseSeatBundle.NewLicenseSeatBundle("code-631", "title-778");
        Check.Equal(item.Status, LicenseSeatBundleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LicenseSeatBundleMovesToInProgress()
    {
        var item = LicenseSeatBundle.NewLicenseSeatBundle("code-631", "title-778");
        try
        {
            item.MoveTo(LicenseSeatBundleStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LicenseSeatBundleStatus.InProgress, "status");
    }

    public static void LicenseSeatBundleRejectsMovesFromCancelled()
    {
        var item = LicenseSeatBundle.NewLicenseSeatBundle("code-631", "title-778");
        item.Status = LicenseSeatBundleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LicenseSeatBundleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LicenseSeatBundleStartsInPlanned();
        LicenseSeatBundleMovesToInProgress();
        LicenseSeatBundleRejectsMovesFromCancelled();
    }
}
