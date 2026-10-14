namespace Acme.Workforce.Absence.Balance.Checkpoint;

using Acme.Workforce.Absence.Policy.Score;

/// <summary>Functions of balance template from manifest.</summary>
public static class BalanceTemplateFromManifest
{
    public static Dictionary<string, double> SummarizePolicyManifestsForTemplate(List<PolicyManifest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveValue();
        }
        return totals;
    }

    public static List<BalanceTemplate> DeriveBalanceTemplatesFromManifest(List<PolicyManifest> items)
    {
        var outValue = new List<BalanceTemplate>();
        foreach (var item in items)
        {
            var derived = BalanceTemplate.NewBalanceTemplate(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyBalanceTemplate(BalanceTemplate item)
    {
        return PolicyManifestPolicy.ClassifyPolicyManifest(item.ProjectedTotal());
    }
}
