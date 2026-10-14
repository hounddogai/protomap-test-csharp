namespace Acme.Workforce.Payrollops.RetroPay.PayPolicy;

using Acme.Workforce.Payrollops.PayRun.GlobalRunLedger;

/// <summary>Functions of manual retro pay policy from reservation.</summary>
public static class ManualRetroPayPolicyFromReservation
{
    public static Dictionary<string, double> SummarizeCappedPayRunReservationsForPolicy(List<CappedPayRunReservation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<ManualRetroPayPolicy> DeriveManualRetroPayPoliciesFromReservation(List<CappedPayRunReservation> items)
    {
        var outValue = new List<ManualRetroPayPolicy>();
        foreach (var item in items)
        {
            var derived = ManualRetroPayPolicy.NewManualRetroPayPolicy(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ComputeManualRetroPayPolicy(ManualRetroPayPolicy item)
    {
        return LenientPayRunLedgerPolicy.LenientPayRunLedgerWeight(4, true);
    }
}
