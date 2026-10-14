namespace Acme.Workforce.Assets.Assignment.Agreement;

using System.Globalization;

/// <summary>How screens and exports show a scheduled assignment slot.</summary>
public sealed record ScheduledAssignmentSlotView(string CodeText, string TitleText, string ApprovedText, string Threshold, string StartsAt, string StatusLabel)
{
    public static ScheduledAssignmentSlotView ToScheduledAssignmentSlotView(ScheduledAssignmentSlot item)
    {
        return new ScheduledAssignmentSlotView(
            item.Code,
            item.Title,
            $"{item.Approved}",
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledAssignmentSlotView> ToScheduledAssignmentSlotViews(List<ScheduledAssignmentSlot> items)
    {
        var views = new List<ScheduledAssignmentSlotView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledAssignmentSlotView(item));
        }
        return views;
    }

    public static double SumScheduledAssignmentSlotView(ScheduledAssignmentSlotView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return parsedThreshold + (double)parsedStartsAt;
    }
}
