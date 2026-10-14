namespace Acme.Workforce.Orgdesign.Position.Mapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a position tier.</summary>
public sealed record PositionTierView(string Code, string TitleText, string Weight, string CurrencyText, string StartsAtText, string Grade, string StatusLabel)
{
    public static PositionTierView ToPositionTierView(PositionTier item)
    {
        return new PositionTierView(
            item.Code,
            item.Title,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Currency, 40),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PositionTierView> ToPositionTierViews(List<PositionTier> items)
    {
        var views = new List<PositionTierView>();
        foreach (var item in items)
        {
            views.Add(ToPositionTierView(item));
        }
        return views;
    }

    public static double SumPositionTierView(PositionTierView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        return parsedWeight + (double)parsedStartsAt;
    }
}
