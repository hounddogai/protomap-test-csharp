namespace Acme.Workforce.Assets.Laptop.Template;

using Acme.Workforce.Assets.Refresh.Summary;

/// <summary>Functions of legacy laptop template from policy.</summary>
public static class LegacyLaptopTemplateFromPolicy
{
    public static Dictionary<string, double> SummarizeFallbackRefreshPoliciesForTemplate(List<FallbackRefreshPolicy> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<LegacyLaptopTemplate> DeriveLegacyLaptopTemplatesFromPolicy(List<FallbackRefreshPolicy> items)
    {
        var outValue = new List<LegacyLaptopTemplate>();
        foreach (var item in items)
        {
            var derived = LegacyLaptopTemplate.NewLegacyLaptopTemplate(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignLegacyLaptopTemplate(LegacyLaptopTemplate item)
    {
        return PendingRefreshClaimPolicy.NormalizePendingRefreshClaimCode(item.Code);
    }
}
