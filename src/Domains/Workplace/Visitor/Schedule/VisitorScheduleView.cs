namespace Acme.Workforce.Workplace.Visitor.Schedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a visitor schedule.</summary>
public sealed record VisitorScheduleView(string Code, string Label, string ExternalRef, string SalaryBand, string Segment, string EndsAt, string JobCode, string CreatedAt, string Weight, string StatusLabel)
{
    public static VisitorScheduleView ToVisitorScheduleView(VisitorSchedule item)
    {
        return new VisitorScheduleView(
            item.Code,
            item.Label,
            item.ExternalRef,
            item.SalaryBand,
            Text.Truncate(item.Segment, 40),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.JobCode, 80),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VisitorScheduleView> ToVisitorScheduleViews(List<VisitorSchedule> items)
    {
        var views = new List<VisitorScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToVisitorScheduleView(item));
        }
        return views;
    }

    public static double SumVisitorScheduleView(VisitorScheduleView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedCreatedAt + parsedWeight;
    }
}
