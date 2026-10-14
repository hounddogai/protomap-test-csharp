namespace Acme.Workforce.Absence.Holiday.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a holiday bundle.</summary>
public sealed record HolidayBundleView(string CodeText, string NameText, string CountryText, string Quantity, string Visible, string Unit, string Badge, string StatusLabel)
{
    public static HolidayBundleView ToHolidayBundleView(HolidayBundle item)
    {
        return new HolidayBundleView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Name, 120),
            item.Country,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Unit,
            item.Badge,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HolidayBundleView> ToHolidayBundleViews(List<HolidayBundle> items)
    {
        var views = new List<HolidayBundleView>();
        foreach (var item in items)
        {
            views.Add(ToHolidayBundleView(item));
        }
        return views;
    }

    public static double SumHolidayBundleView(HolidayBundleView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
