namespace Acme.Workforce.Talent.Pool.Group;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pool ledger.</summary>
public sealed record PoolLedgerView(string Code, string LabelText, string Segment, string Country, string BadgeText, string MaximumText, string StatusLabel)
{
    public static PoolLedgerView ToPoolLedgerView(PoolLedger item)
    {
        return new PoolLedgerView(
            item.Code,
            item.Label,
            Text.Truncate(item.Segment, 40),
            item.Country,
            item.Badge,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PoolLedgerView> ToPoolLedgerViews(List<PoolLedger> items)
    {
        var views = new List<PoolLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToPoolLedgerView(item));
        }
        return views;
    }

    public static double SumPoolLedgerView(PoolLedgerView view)
    {
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        return parsedMaximum;
    }
}
