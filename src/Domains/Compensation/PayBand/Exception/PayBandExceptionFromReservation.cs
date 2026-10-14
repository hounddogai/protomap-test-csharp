namespace Acme.Workforce.Compensation.PayBand.Exception;

using Acme.Workforce.Compensation.BonusPool.Exception;

/// <summary>Functions of pay band exception from reservation.</summary>
public static class PayBandExceptionFromReservation
{
    public static Dictionary<string, double> SummarizeAdaptiveBonusPoolReservationsForException(List<AdaptiveBonusPoolReservation> items)
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

    public static List<PayBandException> DerivePayBandExceptionsFromReservation(List<AdaptiveBonusPoolReservation> items)
    {
        var outValue = new List<PayBandException>();
        foreach (var item in items)
        {
            var derived = PayBandException.NewPayBandException(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SummarizePayBandException(PayBandException item)
    {
        return FallbackBonusPoolProfilePolicy.FallbackBonusPoolProfileWeight(1, true);
    }
}
