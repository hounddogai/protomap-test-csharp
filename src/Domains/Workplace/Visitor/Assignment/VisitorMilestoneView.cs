namespace Acme.Workforce.Workplace.Visitor.Assignment;

using System.Globalization;

/// <summary>How screens and exports show a visitor milestone.</summary>
public sealed record VisitorMilestoneView(string CodeText, string Title, string CostText, string Maximum, string Summary, string TenureMonthsText, string Team, string ManagerId, string StatusLabel)
{
    public static VisitorMilestoneView ToVisitorMilestoneView(VisitorMilestone item)
    {
        return new VisitorMilestoneView(
            item.Code,
            item.Title,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Summary,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Team,
            item.ManagerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VisitorMilestoneView> ToVisitorMilestoneViews(List<VisitorMilestone> items)
    {
        var views = new List<VisitorMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToVisitorMilestoneView(item));
        }
        return views;
    }

    public static double SumVisitorMilestoneView(VisitorMilestoneView view)
    {
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        return parsedCost + parsedMaximum + (double)parsedTenureMonths;
    }
}
