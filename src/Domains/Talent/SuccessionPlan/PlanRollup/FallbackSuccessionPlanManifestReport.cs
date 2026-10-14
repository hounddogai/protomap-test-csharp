namespace Acme.Workforce.Talent.SuccessionPlan.PlanRollup;

using System.Globalization;
using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Table;

/// <summary>Totals of fallback succession plan manifests by name.</summary>
public sealed record FallbackSuccessionPlanManifestReport(string Title, Dictionary<string, double> Totals, Dictionary<string, int> Counts, int GeneratedAt)
{
    public static FallbackSuccessionPlanManifestReport BuildFallbackSuccessionPlanManifestReport(string title, List<FallbackSuccessionPlanManifest> items, IClock clock)
    {
        var totals = new Dictionary<string, double>();
        var counts = new Dictionary<string, int>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            var key = item.GroupKey();
            totals[key] = totals.GetValueOrDefault(key) + item.AdjustedValue();
            counts[key] = counts.GetValueOrDefault(key) + 1;
        }
        return new FallbackSuccessionPlanManifestReport(title, totals, counts, clock.Now());
    }

    public List<string> Top(int count)
    {
        var keys = Totals.Keys.Order(StringComparer.Ordinal).ToList();
        var ranked = Collections.SortedBy(keys, (string key) => Totals.GetValueOrDefault(key));
        return Collections.Take(ranked, count);
    }

    public string Render()
    {
        var table = Table.NewTable(Title, new List<string> { "name", "count", "total", "share" });
        var grand = 0.0;
        foreach (var key in Totals.Keys.Order(StringComparer.Ordinal))
        {
            var total = Totals[key];
            grand += total;
        }
        foreach (var key in Totals.Keys.Order(StringComparer.Ordinal))
        {
            var total = Totals[key];
            try
            {
                table.AddRow(new List<string>
            {
                key,
                Counts.GetValueOrDefault(key).ToString(CultureInfo.InvariantCulture),
                total.ToString("F2", CultureInfo.InvariantCulture),
                $"{Numbers.Percent(total, grand)}%",
            });
            }
            catch (InvalidOperationException)
            {
                continue;
            }
        }
        return table.Render();
    }
}
