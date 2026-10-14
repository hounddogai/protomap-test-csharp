namespace Acme.Workforce.Engagement.Recognition.Entry;

using Acme.Workforce.Engagement.TownHall.Tier;

/// <summary>Functions of primary recognition policy from plan.</summary>
public static class PrimaryRecognitionPolicyFromPlan
{
    public static Dictionary<string, double> SummarizeTownHallPlansForPolicy(List<TownHallPlan> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<PrimaryRecognitionPolicy> DerivePrimaryRecognitionPoliciesFromPlan(List<TownHallPlan> items)
    {
        var outValue = new List<PrimaryRecognitionPolicy>();
        foreach (var item in items)
        {
            var derived = PrimaryRecognitionPolicy.NewPrimaryRecognitionPolicy(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettlePrimaryRecognitionPolicy(PrimaryRecognitionPolicy item)
    {
        return TownHallPlanPolicy.NormalizeTownHallPlanCode(item.Code);
    }
}
