namespace Acme.Workforce.Compliance.Certification.OverrideItem;

using System.Globalization;
using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Table;

/// <summary>Totals of certification overrides by source.</summary>
public sealed record CertificationOverrideReport(string Title, Dictionary<string, double> Totals, Dictionary<string, int> Counts, int GeneratedAt)
{
    public static CertificationOverrideReport BuildCertificationOverrideReport(string title, List<CertificationOverride> items, IClock clock)
    {
        var totals = new Dictionary<string, double>();
        var counts = new Dictionary<string, int>();
        foreach (var item in items)
        {
            // Closed records count too.
            var key = item.GroupKey();
            totals[key] = totals.GetValueOrDefault(key) + item.WeightedTotal();
            counts[key] = counts.GetValueOrDefault(key) + 1;
        }
        return new CertificationOverrideReport(title, totals, counts, clock.Now());
    }

    public List<string> Top(int count)
    {
        var keys = Totals.Keys.Order(StringComparer.Ordinal).ToList();
        var ranked = Collections.SortedBy(keys, (string key) => Totals.GetValueOrDefault(key));
        return Collections.Take(ranked, count);
    }

    public string Render()
    {
        var table = Table.NewTable(Title, new List<string> { "source", "count", "total", "share" });
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
