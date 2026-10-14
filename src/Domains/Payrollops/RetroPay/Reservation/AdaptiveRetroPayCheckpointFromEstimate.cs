namespace Acme.Workforce.Payrollops.RetroPay.Reservation;

using Acme.Workforce.Payrollops.TaxForm.Estimate;

/// <summary>Functions of adaptive retro pay checkpoint from estimate.</summary>
public static class AdaptiveRetroPayCheckpointFromEstimate
{
    public static Dictionary<string, double> SummarizeTieredTaxFormEstimatesForCheckpoint(List<TieredTaxFormEstimate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<AdaptiveRetroPayCheckpoint> DeriveAdaptiveRetroPayCheckpointsFromEstimate(List<TieredTaxFormEstimate> items)
    {
        var outValue = new List<AdaptiveRetroPayCheckpoint>();
        foreach (var item in items)
        {
            var derived = AdaptiveRetroPayCheckpoint.NewAdaptiveRetroPayCheckpoint(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitAdaptiveRetroPayCheckpoint(AdaptiveRetroPayCheckpoint item)
    {
        return TaxFormProfilePolicy.ClassifyTaxFormProfile(item.NetValue());
    }
}
