namespace Acme.Workforce.Absence.Holiday.HolidaySchedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default holiday profile.</summary>
public sealed record DefaultHolidayProfileView(string Code, string Title, string Channel, string Approved, string Label, string Quantity, string StatusLabel)
{
    public static DefaultHolidayProfileView ToDefaultHolidayProfileView(DefaultHolidayProfile item)
    {
        return new DefaultHolidayProfileView(
            item.Code,
            Text.Truncate(item.Title, 120),
            item.Channel,
            $"{item.Approved}",
            item.Label,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultHolidayProfileView> ToDefaultHolidayProfileViews(List<DefaultHolidayProfile> items)
    {
        var views = new List<DefaultHolidayProfileView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultHolidayProfileView(item));
        }
        return views;
    }

    public static double SumDefaultHolidayProfileView(DefaultHolidayProfileView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
