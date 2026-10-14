namespace Acme.Workforce.Assets.Tests.LicenseSeat.Bundle;

using Acme.Workforce.Assets.LicenseSeat.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class WeightedLicenseSeatBundleChecks
{
    public static void WeightedLicenseSeatBundleStartsInProposed()
    {
        var item = WeightedLicenseSeatBundle.NewWeightedLicenseSeatBundle("code-344", "label-691");
        Check.Equal(item.Status, WeightedLicenseSeatBundleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedLicenseSeatBundleMovesToApproved()
    {
        var item = WeightedLicenseSeatBundle.NewWeightedLicenseSeatBundle("code-344", "label-691");
        try
        {
            item.MoveTo(WeightedLicenseSeatBundleStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedLicenseSeatBundleStatus.Approved, "status");
    }

    public static void WeightedLicenseSeatBundleRejectsMovesFromWithdrawn()
    {
        var item = WeightedLicenseSeatBundle.NewWeightedLicenseSeatBundle("code-344", "label-691");
        item.Status = WeightedLicenseSeatBundleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(WeightedLicenseSeatBundleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        WeightedLicenseSeatBundleStartsInProposed();
        WeightedLicenseSeatBundleMovesToApproved();
        WeightedLicenseSeatBundleRejectsMovesFromWithdrawn();
    }
}
