namespace Acme.Workforce.Payrollops.PayRun.Budget;

using System.Globalization;
using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Table;

/// <summary>Totals of pay run proposals by source.</summary>
public sealed record PayRunProposalReport(string Title, Dictionary<string, double> Totals, Dictionary<string, int> Counts, int GeneratedAt)
{
    public static PayRunProposalReport BuildPayRunProposalReport(string title, List<PayRunProposal> items, IClock clock)
    {
        var groups = Collections.GroupBy(items, (PayRunProposal entry) => entry.GroupKey());
        var totals = new Dictionary<string, double>();
        var counts = new Dictionary<string, int>();
        foreach (var key in groups.Keys.Order(StringComparer.Ordinal))
        {
            var members = groups[key];
            totals[key] = Math.Round(Collections.SumBy(members, (PayRunProposal entry) => entry.AdjustedValue()), 2);
            counts[key] = members.Count;
        }
        return new PayRunProposalReport(title, totals, counts, clock.Now());
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
