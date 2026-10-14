namespace Acme.Workforce.Orgdesign.Span.Entry;

using Acme.Workforce.Orgdesign.Scenario.ScenarioEstimate;

/// <summary>Functions of span entry from bundle.</summary>
public static class SpanEntryFromBundle
{
    public static Dictionary<string, double> SummarizeGlobalScenarioBundlesForEntry(List<GlobalScenarioBundle> items)
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

    public static List<SpanEntry> DeriveSpanEntriesFromBundle(List<GlobalScenarioBundle> items)
    {
        var outValue = new List<SpanEntry>();
        foreach (var item in items)
        {
            var derived = SpanEntry.NewSpanEntry(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignSpanEntry(SpanEntry item)
    {
        return ScenarioTargetPolicy.ScenarioTargetBucket(item.MeasuredValue());
    }
}
