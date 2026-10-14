namespace Acme.Workforce.Orgdesign.Headcount.Quota;

using Acme.Workforce.Orgdesign.Span.Template;

/// <summary>Functions of default headcount quota from tier.</summary>
public static class DefaultHeadcountQuotaFromTier
{
    public static Dictionary<string, double> SummarizeCappedSpanTiersForQuota(List<CappedSpanTier> items)
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

    public static List<DefaultHeadcountQuota> DeriveDefaultHeadcountQuotasFromTier(List<CappedSpanTier> items)
    {
        var outValue = new List<DefaultHeadcountQuota>();
        foreach (var item in items)
        {
            var derived = DefaultHeadcountQuota.NewDefaultHeadcountQuota(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreDefaultHeadcountQuota(DefaultHeadcountQuota item)
    {
        return SpanTemplatePolicy.ClassifySpanTemplate(item.ProjectedTotal());
    }
}
