namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatForecast;

using Acme.Workforce.Assets.LicenseSeat.SeatForecast;

/// <summary>Checks of seat forecast code, run by the project's runner.</summary>
public static class RegionalLicenseSeatMilestoneChecks
{
    public static void RegionalLicenseSeatMilestoneStartsInDraft()
    {
        var item = RegionalLicenseSeatMilestone.NewRegionalLicenseSeatMilestone("code-50", "title-951");
        Check.Equal(item.Status, RegionalLicenseSeatMilestoneStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalLicenseSeatMilestoneMovesToSuspended()
    {
        var item = RegionalLicenseSeatMilestone.NewRegionalLicenseSeatMilestone("code-50", "title-951");
        try
        {
            item.MoveTo(RegionalLicenseSeatMilestoneStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalLicenseSeatMilestoneStatus.Suspended, "status");
    }

    public static void RegionalLicenseSeatMilestoneRejectsMovesFromClosed()
    {
        var item = RegionalLicenseSeatMilestone.NewRegionalLicenseSeatMilestone("code-50", "title-951");
        item.Status = RegionalLicenseSeatMilestoneStatus.Closed;
        Check.Fails(() => item.MoveTo(RegionalLicenseSeatMilestoneStatus.Draft), "move");
    }

    public static void RunAll()
    {
        RegionalLicenseSeatMilestoneStartsInDraft();
        RegionalLicenseSeatMilestoneMovesToSuspended();
        RegionalLicenseSeatMilestoneRejectsMovesFromClosed();
    }
}
