namespace Acme.Workforce.Payrollops.RetroPay.PayAssignment;

using Acme.Workforce.Payrollops.TaxForm.FormEstimate;

/// <summary>Functions of rolling retro pay assignment from ledger.</summary>
public static class RollingRetroPayAssignmentFromLedger
{
    public static Dictionary<string, double> SummarizeAdaptiveTaxFormLedgersForAssignment(List<AdaptiveTaxFormLedger> items)
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

    public static List<RollingRetroPayAssignment> DeriveRollingRetroPayAssignmentsFromLedger(List<AdaptiveTaxFormLedger> items)
    {
        var outValue = new List<RollingRetroPayAssignment>();
        foreach (var item in items)
        {
            var derived = RollingRetroPayAssignment.NewRollingRetroPayAssignment(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateRollingRetroPayAssignment(RollingRetroPayAssignment item)
    {
        return RegionalTaxFormEstimatePolicy.ClassifyRegionalTaxFormEstimate(item.EffectiveVolume());
    }
}
