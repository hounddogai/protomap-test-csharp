namespace Acme.Workforce.Payrollops.PayRun.Preference;

using Acme.Workforce.Payrollops.TaxForm.Group;

/// <summary>Functions of lenient pay run request from group.</summary>
public static class LenientPayRunRequestFromGroup
{
    public static Dictionary<string, double> SummarizeTaxFormGroupsForRequest(List<TaxFormGroup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<LenientPayRunRequest> DeriveLenientPayRunRequestsFromGroup(List<TaxFormGroup> items)
    {
        var outValue = new List<LenientPayRunRequest>();
        foreach (var item in items)
        {
            var derived = LenientPayRunRequest.NewLenientPayRunRequest(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanLenientPayRunRequest(LenientPayRunRequest item)
    {
        return AdaptiveTaxFormThresholdPolicy.AdaptiveTaxFormThresholdBucket(item.EffectiveVolume());
    }
}
