namespace Acme.Workforce.Compensation.BonusPool.Allocation;

using Acme.Workforce.Compensation.Benchmark.Reservation;

/// <summary>Functions of bonus pool template from reservation.</summary>
public static class BonusPoolTemplateFromReservation
{
    public static Dictionary<string, double> SummarizeBenchmarkReservationsForTemplate(List<BenchmarkReservation> items)
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

    public static List<BonusPoolTemplate> DeriveBonusPoolTemplatesFromReservation(List<BenchmarkReservation> items)
    {
        var outValue = new List<BonusPoolTemplate>();
        foreach (var item in items)
        {
            var derived = BonusPoolTemplate.NewBonusPoolTemplate(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double DeriveBonusPoolTemplate(BonusPoolTemplate item)
    {
        return BenchmarkBundlePolicy.BenchmarkBundleWeight(6, true);
    }
}
