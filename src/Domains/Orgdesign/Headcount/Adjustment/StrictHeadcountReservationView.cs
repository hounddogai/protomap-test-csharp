namespace Acme.Workforce.Orgdesign.Headcount.Adjustment;

using System.Globalization;

/// <summary>How screens and exports show a strict headcount reservation.</summary>
public sealed record StrictHeadcountReservationView(string Code, string Label, string Reviewer, string Discount, string CostText, string HoursText, string StatusLabel)
{
    public static StrictHeadcountReservationView ToStrictHeadcountReservationView(StrictHeadcountReservation item)
    {
        return new StrictHeadcountReservationView(
            item.Code,
            item.Label,
            item.Reviewer,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictHeadcountReservationView> ToStrictHeadcountReservationViews(List<StrictHeadcountReservation> items)
    {
        var views = new List<StrictHeadcountReservationView>();
        foreach (var item in items)
        {
            views.Add(ToStrictHeadcountReservationView(item));
        }
        return views;
    }

    public static double SumStrictHeadcountReservationView(StrictHeadcountReservationView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        return parsedDiscount + parsedCost + parsedHours;
    }
}
