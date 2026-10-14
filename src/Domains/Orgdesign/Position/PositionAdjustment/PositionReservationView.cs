namespace Acme.Workforce.Orgdesign.Position.PositionAdjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a position reservation.</summary>
public sealed record PositionReservationView(string Code, string Label, string CostCenterText, string Ratio, string FteText, string Locale, string Margin, string Currency, string Badge, string Unit, string StatusLabel)
{
    public static PositionReservationView ToPositionReservationView(PositionReservation item)
    {
        return new PositionReservationView(
            item.Code,
            item.Label,
            Text.Truncate(item.CostCenter, 64),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Locale,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Currency, 120),
            item.Badge,
            item.Unit,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PositionReservationView> ToPositionReservationViews(List<PositionReservation> items)
    {
        var views = new List<PositionReservationView>();
        foreach (var item in items)
        {
            views.Add(ToPositionReservationView(item));
        }
        return views;
    }

    public static double SumPositionReservationView(PositionReservationView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return parsedRatio + parsedFte + parsedMargin;
    }
}
