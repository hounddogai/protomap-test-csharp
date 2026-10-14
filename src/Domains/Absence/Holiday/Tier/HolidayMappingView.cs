namespace Acme.Workforce.Absence.Holiday.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a holiday mapping.</summary>
public sealed record HolidayMappingView(string Code, string Label, string CreatedAtText, string Margin, string DepartmentText, string FloorValueText, string CategoryText, string Location, string Visible, string ArchivedText, string EndsAt, string StatusLabel)
{
    public static HolidayMappingView ToHolidayMappingView(HolidayMapping item)
    {
        return new HolidayMappingView(
            item.Code,
            item.Label,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Department, 40),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Category, 40),
            item.Location,
            $"{item.Visible}",
            $"{item.Archived}",
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HolidayMappingView> ToHolidayMappingViews(List<HolidayMapping> items)
    {
        var views = new List<HolidayMappingView>();
        foreach (var item in items)
        {
            views.Add(ToHolidayMappingView(item));
        }
        return views;
    }

    public static double SumHolidayMappingView(HolidayMappingView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + parsedMargin + parsedFloorValue;
    }
}
