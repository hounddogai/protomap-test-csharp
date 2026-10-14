namespace Acme.Workforce.Absence.Balance.Request;

using Acme.Workforce.Absence.ReturnPlan.Manifest;

/// <summary>Functions of capped balance request from manifest.</summary>
public static class CappedBalanceRequestFromManifest
{
    public static Dictionary<string, double> SummarizeDefaultReturnPlanManifestsForRequest(List<DefaultReturnPlanManifest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedValue();
        }
        return totals;
    }

    public static List<CappedBalanceRequest> DeriveCappedBalanceRequestsFromManifest(List<DefaultReturnPlanManifest> items)
    {
        var outValue = new List<CappedBalanceRequest>();
        foreach (var item in items)
        {
            var derived = CappedBalanceRequest.NewCappedBalanceRequest(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectCappedBalanceRequest(CappedBalanceRequest item)
    {
        return DefaultReturnPlanManifestPolicy.ClassifyDefaultReturnPlanManifest(item.NetVolume());
    }
}
