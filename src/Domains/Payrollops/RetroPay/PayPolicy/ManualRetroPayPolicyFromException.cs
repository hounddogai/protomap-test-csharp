namespace Acme.Workforce.Payrollops.RetroPay.PayPolicy;

using Acme.Workforce.Payrollops.Deduction.Draft;

/// <summary>Functions of manual retro pay policy from exception.</summary>
public static class ManualRetroPayPolicyFromException
{
    public static Dictionary<string, double> SummarizePendingDeductionExceptionsForPolicy(List<PendingDeductionException> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<ManualRetroPayPolicy> DeriveManualRetroPayPoliciesFromException(List<PendingDeductionException> items)
    {
        var outValue = new List<ManualRetroPayPolicy>();
        foreach (var item in items)
        {
            var derived = ManualRetroPayPolicy.NewManualRetroPayPolicy(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveManualRetroPayPolicy(ManualRetroPayPolicy item)
    {
        return StrictDeductionDraftPolicy.StrictDeductionDraftBucket(item.WeightedExposure());
    }
}
