namespace Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

using Acme.Workforce.Orgdesign.Position.PositionAdjustment;

/// <summary>Functions of cost model variance from reservation.</summary>
public static class CostModelVarianceFromReservation
{
    public static Dictionary<string, double> SummarizePositionReservationsForVariance(List<PositionReservation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<CostModelVariance> DeriveCostModelVariancesFromReservation(List<PositionReservation> items)
    {
        var outValue = new List<CostModelVariance>();
        foreach (var item in items)
        {
            var derived = CostModelVariance.NewCostModelVariance(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileCostModelVariance(CostModelVariance item)
    {
        return PositionReservationPolicy.NormalizePositionReservationCode(item.Code);
    }
}
