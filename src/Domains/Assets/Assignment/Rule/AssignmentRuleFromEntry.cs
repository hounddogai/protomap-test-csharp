namespace Acme.Workforce.Assets.Assignment.Rule;

using Acme.Workforce.Assets.Warranty.Agreement;

/// <summary>Functions of assignment rule from entry.</summary>
public static class AssignmentRuleFromEntry
{
    public static Dictionary<string, double> SummarizeWarrantyEntriesForRule(List<WarrantyEntry> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<AssignmentRule> DeriveAssignmentRulesFromEntry(List<WarrantyEntry> items)
    {
        var outValue = new List<AssignmentRule>();
        foreach (var item in items)
        {
            var derived = AssignmentRule.NewAssignmentRule(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateAssignmentRule(AssignmentRule item)
    {
        return WarrantyScorePolicy.NormalizeWarrantyScoreCode(item.Code);
    }
}
