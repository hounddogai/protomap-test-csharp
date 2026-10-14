namespace Acme.Workforce.Orgdesign.Headcount.Schedule;

using Acme.Workforce.Orgdesign.Reorg.Summary;

/// <summary>Functions of headcount template from ledger.</summary>
public static class HeadcountTemplateFromLedger
{
    public static Dictionary<string, double> SummarizeReorgLedgersForTemplate(List<ReorgLedger> items)
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

    public static List<HeadcountTemplate> DeriveHeadcountTemplatesFromLedger(List<ReorgLedger> items)
    {
        var outValue = new List<HeadcountTemplate>();
        foreach (var item in items)
        {
            var derived = HeadcountTemplate.NewHeadcountTemplate(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyHeadcountTemplate(HeadcountTemplate item)
    {
        return ReorgLedgerPolicy.ReorgLedgerBucket(item.EffectiveVolume());
    }
}
