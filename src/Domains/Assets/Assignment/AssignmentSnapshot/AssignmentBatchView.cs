namespace Acme.Workforce.Assets.Assignment.AssignmentSnapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a assignment batch.</summary>
public sealed record AssignmentBatchView(string Code, string NameText, string MaximumText, string HoursText, string WindowDays, string Capacity, string Skill, string Minimum, string DescriptionText, string Score, string StatusLabel)
{
    public static AssignmentBatchView ToAssignmentBatchView(AssignmentBatch item)
    {
        return new AssignmentBatchView(
            item.Code,
            item.Name,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Skill, 40),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Description,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AssignmentBatchView> ToAssignmentBatchViews(List<AssignmentBatch> items)
    {
        var views = new List<AssignmentBatchView>();
        foreach (var item in items)
        {
            views.Add(ToAssignmentBatchView(item));
        }
        return views;
    }

    public static double SumAssignmentBatchView(AssignmentBatchView view)
    {
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return parsedMaximum + parsedHours + (double)parsedWindowDays;
    }
}
