namespace Acme.Workforce.Assets.Tests.LicenseSeat.Assignment;

using Acme.Workforce.Assets.LicenseSeat.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class WeightedLicenseSeatEstimateChecks
{
    public static void WeightedLicenseSeatEstimateStartsInProposed()
    {
        var item = WeightedLicenseSeatEstimate.NewWeightedLicenseSeatEstimate("code-319", "name-685");
        Check.Equal(item.Status, WeightedLicenseSeatEstimateStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedLicenseSeatEstimateMovesToRejected()
    {
        var item = WeightedLicenseSeatEstimate.NewWeightedLicenseSeatEstimate("code-319", "name-685");
        try
        {
            item.MoveTo(WeightedLicenseSeatEstimateStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedLicenseSeatEstimateStatus.Rejected, "status");
    }

    public static void WeightedLicenseSeatEstimateRejectsMovesFromWithdrawn()
    {
        var item = WeightedLicenseSeatEstimate.NewWeightedLicenseSeatEstimate("code-319", "name-685");
        item.Status = WeightedLicenseSeatEstimateStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(WeightedLicenseSeatEstimateStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        WeightedLicenseSeatEstimateStartsInProposed();
        WeightedLicenseSeatEstimateMovesToRejected();
        WeightedLicenseSeatEstimateRejectsMovesFromWithdrawn();
    }
}
