namespace Acme.Workforce.Talent.Pool.Group;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling pool ledger.</summary>
public sealed record RollingPoolLedgerView(string CodeText, string LabelText, string Rate, string LocationText, string Margin, string Attempts, string Quantity, string StatusLabel)
{
    public static RollingPoolLedgerView ToRollingPoolLedgerView(RollingPoolLedger item)
    {
        return new RollingPoolLedgerView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Location, 64),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingPoolLedgerView> ToRollingPoolLedgerViews(List<RollingPoolLedger> items)
    {
        var views = new List<RollingPoolLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToRollingPoolLedgerView(item));
        }
        return views;
    }

    public static double SumRollingPoolLedgerView(RollingPoolLedgerView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return parsedRate + parsedMargin + (double)parsedAttempts;
    }
}
