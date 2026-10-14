namespace Acme.Workforce.Absence.Holiday.Milestone;

using System.Globalization;

/// <summary>How screens and exports show a holiday template.</summary>
public sealed record HolidayTemplateView(string Code, string Title, string Priority, string Active, string RetriesText, string Capacity, string StatusLabel)
{
    public static HolidayTemplateView ToHolidayTemplateView(HolidayTemplate item)
    {
        return new HolidayTemplateView(
            item.Code,
            item.Title,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HolidayTemplateView> ToHolidayTemplateViews(List<HolidayTemplate> items)
    {
        var views = new List<HolidayTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToHolidayTemplateView(item));
        }
        return views;
    }

    public static double SumHolidayTemplateView(HolidayTemplateView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return (double)parsedPriority + (double)parsedRetries + (double)parsedCapacity;
    }
}
