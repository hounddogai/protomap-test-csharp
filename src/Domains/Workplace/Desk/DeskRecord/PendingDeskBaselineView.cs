namespace Acme.Workforce.Workplace.Desk.DeskRecord;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending desk baseline.</summary>
public sealed record PendingDeskBaselineView(string Code, string Name, string Revision, string MaximumText, string Reviewer, string Budget, string WindowDays, string StatusLabel)
{
    public static PendingDeskBaselineView ToPendingDeskBaselineView(PendingDeskBaseline item)
    {
        return new PendingDeskBaselineView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Reviewer, 120),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingDeskBaselineView> ToPendingDeskBaselineViews(List<PendingDeskBaseline> items)
    {
        var views = new List<PendingDeskBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToPendingDeskBaselineView(item));
        }
        return views;
    }

    public static double SumPendingDeskBaselineView(PendingDeskBaselineView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return (double)parsedRevision + parsedMaximum + parsedBudget;
    }
}
