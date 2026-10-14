namespace Acme.Workforce.Orgdesign.Reorg.Draft;

using Acme.Workforce.Orgdesign.Headcount.Baseline;

/// <summary>Functions of reorg template from baseline.</summary>
public static class ReorgTemplateFromBaseline
{
    public static Dictionary<string, double> SummarizeHeadcountBaselinesForTemplate(List<HeadcountBaseline> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<ReorgTemplate> DeriveReorgTemplatesFromBaseline(List<HeadcountBaseline> items)
    {
        var outValue = new List<ReorgTemplate>();
        foreach (var item in items)
        {
            var derived = ReorgTemplate.NewReorgTemplate(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleReorgTemplate(ReorgTemplate item)
    {
        return HeadcountExceptionPolicy.NormalizeHeadcountExceptionCode(item.Code);
    }
}
