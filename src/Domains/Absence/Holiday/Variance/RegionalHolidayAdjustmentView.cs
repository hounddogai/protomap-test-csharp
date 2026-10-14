namespace Acme.Workforce.Absence.Holiday.Variance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional holiday adjustment.</summary>
public sealed record RegionalHolidayAdjustmentView(string CodeText, string Title, string Margin, string QuantityText, string PhoneText, string Locale, string StatusLabel)
{
    public static RegionalHolidayAdjustmentView ToRegionalHolidayAdjustmentView(RegionalHolidayAdjustment item)
    {
        return new RegionalHolidayAdjustmentView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Phone,
            item.Locale,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalHolidayAdjustmentView> ToRegionalHolidayAdjustmentViews(List<RegionalHolidayAdjustment> items)
    {
        var views = new List<RegionalHolidayAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalHolidayAdjustmentView(item));
        }
        return views;
    }

    public static double SumRegionalHolidayAdjustmentView(RegionalHolidayAdjustmentView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        return parsedMargin + (double)parsedQuantity;
    }
}
