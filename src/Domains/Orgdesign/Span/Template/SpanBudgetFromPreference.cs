namespace Acme.Workforce.Orgdesign.Span.Template;

using Acme.Workforce.Orgdesign.Reorg.Preference;

/// <summary>Functions of span budget from preference.</summary>
public static class SpanBudgetFromPreference
{
    public static Dictionary<string, double> SummarizePrimaryReorgPreferencesForBudget(List<PrimaryReorgPreference> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<SpanBudget> DeriveSpanBudgetsFromPreference(List<PrimaryReorgPreference> items)
    {
        var outValue = new List<SpanBudget>();
        foreach (var item in items)
        {
            var derived = SpanBudget.NewSpanBudget(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectSpanBudget(SpanBudget item)
    {
        return PrimaryReorgPreferencePolicy.PrimaryReorgPreferenceBucket(item.ProjectedVolume());
    }
}
