namespace Acme.Workforce.Talent.CareerPath.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a career path assignment.</summary>
public sealed record CareerPathAssignmentView(string CodeText, string Name, string Fte, string Category, string ChecksumText, string Capacity, string Title, string DurationMinutes, string Locale, string Approved, string CountryText, string StatusLabel)
{
    public static CareerPathAssignmentView ToCareerPathAssignmentView(CareerPathAssignment item)
    {
        return new CareerPathAssignmentView(
            item.Code,
            item.Name,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Category,
            Text.Truncate(item.Checksum, 64),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Title,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Locale,
            $"{item.Approved}",
            item.Country,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CareerPathAssignmentView> ToCareerPathAssignmentViews(List<CareerPathAssignment> items)
    {
        var views = new List<CareerPathAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToCareerPathAssignmentView(item));
        }
        return views;
    }

    public static double SumCareerPathAssignmentView(CareerPathAssignmentView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return parsedFte + (double)parsedCapacity + (double)parsedDurationMinutes;
    }
}
