namespace Acme.Workforce.Talent.CareerPath.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a career path schedule.</summary>
public sealed record CareerPathScheduleView(string Code, string Label, string SegmentText, string CreatedAt, string DurationMinutes, string ContactEmailText, string Unit, string CostCenter, string RevisionText, string StatusLabel)
{
    public static CareerPathScheduleView ToCareerPathScheduleView(CareerPathSchedule item)
    {
        return new CareerPathScheduleView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.Segment,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.Unit,
            item.CostCenter,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CareerPathScheduleView> ToCareerPathScheduleViews(List<CareerPathSchedule> items)
    {
        var views = new List<CareerPathScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToCareerPathScheduleView(item));
        }
        return views;
    }

    public static double SumCareerPathScheduleView(CareerPathScheduleView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedDurationMinutes + (double)parsedRevision;
    }
}
