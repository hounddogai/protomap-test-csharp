namespace Acme.Workforce.Workplace.AccessBadge.Estimate;

using System.Globalization;

/// <summary>How screens and exports show a access badge schedule.</summary>
public sealed record AccessBadgeScheduleView(string CodeText, string Title, string SequenceText, string ArchivedText, string Location, string UnitText, string Priority, string Rate, string Name, string MinimumText, string Department, string StatusLabel)
{
    public static AccessBadgeScheduleView ToAccessBadgeScheduleView(AccessBadgeSchedule item)
    {
        return new AccessBadgeScheduleView(
            item.Code,
            item.Title,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Location,
            item.Unit,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Name,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Department,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccessBadgeScheduleView> ToAccessBadgeScheduleViews(List<AccessBadgeSchedule> items)
    {
        var views = new List<AccessBadgeScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToAccessBadgeScheduleView(item));
        }
        return views;
    }

    public static double SumAccessBadgeScheduleView(AccessBadgeScheduleView view)
    {
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return (double)parsedSequence + (double)parsedPriority + parsedRate;
    }
}
