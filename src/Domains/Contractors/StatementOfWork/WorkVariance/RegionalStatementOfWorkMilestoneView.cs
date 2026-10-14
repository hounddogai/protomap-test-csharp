namespace Acme.Workforce.Contractors.StatementOfWork.WorkVariance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional statement of work milestone.</summary>
public sealed record RegionalStatementOfWorkMilestoneView(string CodeText, string Title, string Badge, string NameText, string UpdatedAtText, string ArchivedText, string Channel, string EmployeeId, string Weight, string StatusLabel)
{
    public static RegionalStatementOfWorkMilestoneView ToRegionalStatementOfWorkMilestoneView(RegionalStatementOfWorkMilestone item)
    {
        return new RegionalStatementOfWorkMilestoneView(
            item.Code,
            item.Title,
            Text.Truncate(item.Badge, 120),
            item.Name,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Channel,
            item.EmployeeId,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalStatementOfWorkMilestoneView> ToRegionalStatementOfWorkMilestoneViews(List<RegionalStatementOfWorkMilestone> items)
    {
        var views = new List<RegionalStatementOfWorkMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalStatementOfWorkMilestoneView(item));
        }
        return views;
    }

    public static double SumRegionalStatementOfWorkMilestoneView(RegionalStatementOfWorkMilestoneView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + parsedWeight;
    }
}
