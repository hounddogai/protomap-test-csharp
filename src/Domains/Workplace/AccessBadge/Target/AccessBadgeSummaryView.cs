namespace Acme.Workforce.Workplace.AccessBadge.Target;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a access badge summary.</summary>
public sealed record AccessBadgeSummaryView(string Code, string Name, string FteText, string LeaveDays, string ActiveText, string StatusLabel)
{
    public static AccessBadgeSummaryView ToAccessBadgeSummaryView(AccessBadgeSummary item)
    {
        return new AccessBadgeSummaryView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccessBadgeSummaryView> ToAccessBadgeSummaryViews(List<AccessBadgeSummary> items)
    {
        var views = new List<AccessBadgeSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToAccessBadgeSummaryView(item));
        }
        return views;
    }

    public static double SumAccessBadgeSummaryView(AccessBadgeSummaryView view)
    {
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return parsedFte + (double)parsedLeaveDays;
    }
}
