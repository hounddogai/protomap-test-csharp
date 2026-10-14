namespace Acme.Workforce.Absence.Holiday.Request;

using System.Globalization;

/// <summary>How screens and exports show a holiday request.</summary>
public sealed record HolidayRequestView(string Code, string Name, string Fte, string Weight, string PriorityText, string JobCode, string Revision, string Ratio, string Margin, string CeilingText, string StatusLabel)
{
    public static HolidayRequestView ToHolidayRequestView(HolidayRequest item)
    {
        return new HolidayRequestView(
            item.Code,
            item.Name,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.JobCode,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HolidayRequestView> ToHolidayRequestViews(List<HolidayRequest> items)
    {
        var views = new List<HolidayRequestView>();
        foreach (var item in items)
        {
            views.Add(ToHolidayRequestView(item));
        }
        return views;
    }

    public static double SumHolidayRequestView(HolidayRequestView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        return parsedFte + parsedWeight + (double)parsedPriority;
    }
}
