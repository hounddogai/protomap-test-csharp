namespace Acme.Workforce.Assets.Laptop.Estimate;

using Acme.Workforce.Assets.LicenseSeat.SeatAssignment;

/// <summary>Functions of primary laptop batch from forecast.</summary>
public static class PrimaryLaptopBatchFromForecast
{
    public static Dictionary<string, double> SummarizeDefaultLicenseSeatForecastsForBatch(List<DefaultLicenseSeatForecast> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<PrimaryLaptopBatch> DerivePrimaryLaptopBatchesFromForecast(List<DefaultLicenseSeatForecast> items)
    {
        var outValue = new List<PrimaryLaptopBatch>();
        foreach (var item in items)
        {
            var derived = PrimaryLaptopBatch.NewPrimaryLaptopBatch(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double BalancePrimaryLaptopBatch(PrimaryLaptopBatch item)
    {
        return LicenseSeatQuotaPolicy.LicenseSeatQuotaWeight(7, true);
    }
}
