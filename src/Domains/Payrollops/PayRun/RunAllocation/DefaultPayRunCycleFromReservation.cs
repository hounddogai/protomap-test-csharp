namespace Acme.Workforce.Payrollops.PayRun.RunAllocation;

using Acme.Workforce.Payrollops.TaxForm.Reservation;

/// <summary>Functions of default pay run cycle from reservation.</summary>
public static class DefaultPayRunCycleFromReservation
{
    public static Dictionary<string, double> SummarizeTaxFormReservationsForCycle(List<TaxFormReservation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<DefaultPayRunCycle> DeriveDefaultPayRunCyclesFromReservation(List<TaxFormReservation> items)
    {
        var outValue = new List<DefaultPayRunCycle>();
        foreach (var item in items)
        {
            var derived = DefaultPayRunCycle.NewDefaultPayRunCycle(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreDefaultPayRunCycle(DefaultPayRunCycle item)
    {
        return TaxFormReservationPolicy.NormalizeTaxFormReservationCode(item.Code);
    }
}
