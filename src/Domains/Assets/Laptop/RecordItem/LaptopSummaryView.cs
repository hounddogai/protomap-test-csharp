namespace Acme.Workforce.Assets.Laptop.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a laptop summary.</summary>
public sealed record LaptopSummaryView(string Code, string TitleText, string FloorValueText, string Minimum, string ChannelText, string Grade, string Country, string Locale, string ExpiresAt, string EmployeeIdText, string WindowDays, string StatusLabel)
{
    public static LaptopSummaryView ToLaptopSummaryView(LaptopSummary item)
    {
        return new LaptopSummaryView(
            item.Code,
            item.Title,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Channel,
            Text.Truncate(item.Grade, 120),
            Text.Truncate(item.Country, 40),
            item.Locale,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LaptopSummaryView> ToLaptopSummaryViews(List<LaptopSummary> items)
    {
        var views = new List<LaptopSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToLaptopSummaryView(item));
        }
        return views;
    }

    public static double SumLaptopSummaryView(LaptopSummaryView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return parsedFloorValue + parsedMinimum + (double)parsedExpiresAt;
    }
}
