namespace Acme.Workforce.Workplace.Desk.Digest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a desk summary.</summary>
public sealed record DeskSummaryView(string Code, string LabelText, string LeaveDays, string Category, string Phone, string Priority, string StatusLabel)
{
    public static DeskSummaryView ToDeskSummaryView(DeskSummary item)
    {
        return new DeskSummaryView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Category, 64),
            Text.Truncate(item.Phone, 80),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeskSummaryView> ToDeskSummaryViews(List<DeskSummary> items)
    {
        var views = new List<DeskSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToDeskSummaryView(item));
        }
        return views;
    }

    public static double SumDeskSummaryView(DeskSummaryView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + (double)parsedPriority;
    }
}
