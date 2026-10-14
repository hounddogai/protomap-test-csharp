namespace Acme.Workforce.Orgdesign.Scenario.Estimate;

using Acme.Workforce.Orgdesign.Headcount.Entry;

/// <summary>Functions of lenient scenario agreement from entry.</summary>
public static class LenientScenarioAgreementFromEntry
{
    public static Dictionary<string, double> SummarizeHeadcountEntriesForAgreement(List<HeadcountEntry> items)
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

    public static List<LenientScenarioAgreement> DeriveLenientScenarioAgreementsFromEntry(List<HeadcountEntry> items)
    {
        var outValue = new List<LenientScenarioAgreement>();
        foreach (var item in items)
        {
            var derived = LenientScenarioAgreement.NewLenientScenarioAgreement(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitLenientScenarioAgreement(LenientScenarioAgreement item)
    {
        return DefaultHeadcountEntryPolicy.ClassifyDefaultHeadcountEntry(item.MeasuredValue());
    }
}
