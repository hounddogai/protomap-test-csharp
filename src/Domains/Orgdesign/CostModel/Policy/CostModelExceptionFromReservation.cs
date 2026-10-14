namespace Acme.Workforce.Orgdesign.CostModel.Policy;

using Acme.Workforce.Orgdesign.Headcount.Adjustment;

/// <summary>Functions of cost model exception from reservation.</summary>
public static class CostModelExceptionFromReservation
{
    public static Dictionary<string, double> SummarizeStrictHeadcountReservationsForException(List<StrictHeadcountReservation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<CostModelException> DeriveCostModelExceptionsFromReservation(List<StrictHeadcountReservation> items)
    {
        var outValue = new List<CostModelException>();
        foreach (var item in items)
        {
            var derived = CostModelException.NewCostModelException(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ReconcileCostModelException(CostModelException item)
    {
        return HeadcountClaimPolicy.HeadcountClaimWeight(1, true);
    }
}
