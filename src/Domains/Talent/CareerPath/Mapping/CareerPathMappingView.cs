namespace Acme.Workforce.Talent.CareerPath.Mapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a career path mapping.</summary>
public sealed record CareerPathMappingView(string Code, string Label, string LeaveDaysText, string Notes, string Checksum, string Country, string Priority, string Location, string DurationMinutes, string CreatedAtText, string StatusLabel)
{
    public static CareerPathMappingView ToCareerPathMappingView(CareerPathMapping item)
    {
        return new CareerPathMappingView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Notes,
            item.Checksum,
            item.Country,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Location,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CareerPathMappingView> ToCareerPathMappingViews(List<CareerPathMapping> items)
    {
        var views = new List<CareerPathMappingView>();
        foreach (var item in items)
        {
            views.Add(ToCareerPathMappingView(item));
        }
        return views;
    }

    public static double SumCareerPathMappingView(CareerPathMappingView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + (double)parsedPriority + (double)parsedDurationMinutes;
    }
}
