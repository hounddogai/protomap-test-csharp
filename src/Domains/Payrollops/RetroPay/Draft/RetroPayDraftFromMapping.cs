namespace Acme.Workforce.Payrollops.RetroPay.Draft;

using Acme.Workforce.Payrollops.Garnishment.Mapping;

/// <summary>Functions of retro pay draft from mapping.</summary>
public static class RetroPayDraftFromMapping
{
    public static Dictionary<string, double> SummarizePrimaryGarnishmentMappingsForDraft(List<PrimaryGarnishmentMapping> items)
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

    public static List<RetroPayDraft> DeriveRetroPayDraftsFromMapping(List<PrimaryGarnishmentMapping> items)
    {
        var outValue = new List<RetroPayDraft>();
        foreach (var item in items)
        {
            var derived = RetroPayDraft.NewRetroPayDraft(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeRetroPayDraft(RetroPayDraft item)
    {
        return LenientGarnishmentMappingPolicy.NormalizeLenientGarnishmentMappingCode(item.Code);
    }
}
