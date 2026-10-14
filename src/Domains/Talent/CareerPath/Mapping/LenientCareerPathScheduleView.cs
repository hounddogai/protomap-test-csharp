namespace Acme.Workforce.Talent.CareerPath.Mapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient career path schedule.</summary>
public sealed record LenientCareerPathScheduleView(string Code, string Name, string OwnerId, string Badge, string Priority, string Margin, string StatusLabel)
{
    public static LenientCareerPathScheduleView ToLenientCareerPathScheduleView(LenientCareerPathSchedule item)
    {
        return new LenientCareerPathScheduleView(
            item.Code,
            item.Name,
            item.OwnerId,
            Text.Truncate(item.Badge, 120),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientCareerPathScheduleView> ToLenientCareerPathScheduleViews(List<LenientCareerPathSchedule> items)
    {
        var views = new List<LenientCareerPathScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToLenientCareerPathScheduleView(item));
        }
        return views;
    }

    public static double SumLenientCareerPathScheduleView(LenientCareerPathScheduleView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return (double)parsedPriority + parsedMargin;
    }
}
