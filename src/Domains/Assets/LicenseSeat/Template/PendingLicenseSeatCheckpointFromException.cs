namespace Acme.Workforce.Assets.LicenseSeat.Template;

using Acme.Workforce.Assets.Warranty.Bundle;

/// <summary>Functions of pending license seat checkpoint from exception.</summary>
public static class PendingLicenseSeatCheckpointFromException
{
    public static Dictionary<string, double> SummarizeWarrantyExceptionsForCheckpoint(List<WarrantyException> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<PendingLicenseSeatCheckpoint> DerivePendingLicenseSeatCheckpointsFromException(List<WarrantyException> items)
    {
        var outValue = new List<PendingLicenseSeatCheckpoint>();
        foreach (var item in items)
        {
            var derived = PendingLicenseSeatCheckpoint.NewPendingLicenseSeatCheckpoint(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalancePendingLicenseSeatCheckpoint(PendingLicenseSeatCheckpoint item)
    {
        return WarrantyExceptionPolicy.ClassifyWarrantyException(item.AdjustedTotal());
    }
}
