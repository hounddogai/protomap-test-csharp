namespace Acme.Workforce.Compensation.EquityGrant.OverrideItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered equity grant ledger.</summary>
public sealed record TieredEquityGrantLedgerView(string CodeText, string Name, string Region, string LabelText, string StartsAt, string Segment, string Rating, string EndsAt, string Unit, string StatusLabel)
{
    public static TieredEquityGrantLedgerView ToTieredEquityGrantLedgerView(TieredEquityGrantLedger item)
    {
        return new TieredEquityGrantLedgerView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.Region,
            item.Label,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Segment,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Unit,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredEquityGrantLedgerView> ToTieredEquityGrantLedgerViews(List<TieredEquityGrantLedger> items)
    {
        var views = new List<TieredEquityGrantLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToTieredEquityGrantLedgerView(item));
        }
        return views;
    }

    public static double SumTieredEquityGrantLedgerView(TieredEquityGrantLedgerView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + parsedRating + (double)parsedEndsAt;
    }
}
