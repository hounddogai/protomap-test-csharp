namespace Acme.Workforce.Engagement.PulseSurvey.Estimate;

using System.Globalization;
using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Table;

/// <summary>Totals of pulse survey estimates by country.</summary>
public sealed record PulseSurveyEstimateReport(string Title, Dictionary<string, double> Totals, Dictionary<string, int> Counts, int GeneratedAt)
{
    public static PulseSurveyEstimateReport BuildPulseSurveyEstimateReport(string title, List<PulseSurveyEstimate> items, IClock clock)
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
            totals[key] = totals.GetValueOrDefault(key) + item.AdjustedTotal();
            counts[key] = counts.GetValueOrDefault(key) + 1;
        }
        return new PulseSurveyEstimateReport(title, totals, counts, clock.Now());
    }

    public List<string> Top(int count)
    {
        var keys = Totals.Keys.Order(StringComparer.Ordinal).ToList();
        var ranked = Collections.SortedBy(keys, (string key) => Totals.GetValueOrDefault(key));
        return Collections.Take(ranked, count);
    }

    public string Render()
    {
        var table = Table.NewTable(Title, new List<string> { "country", "count", "total", "share" });
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
