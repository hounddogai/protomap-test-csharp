namespace Acme.Workforce.Assets.Warranty.Milestone;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a warranty milestone.</summary>
public sealed record WarrantyMilestoneView(string CodeText, string Name, string SalaryBand, string Team, string FloorValue, string CostText, string Pinned, string Description, string EndsAt, string LocationText, string StatusLabel)
{
    public static WarrantyMilestoneView ToWarrantyMilestoneView(WarrantyMilestone item)
    {
        return new WarrantyMilestoneView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.SalaryBand,
            item.Team,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            Text.Truncate(item.Description, 40),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Location, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WarrantyMilestoneView> ToWarrantyMilestoneViews(List<WarrantyMilestone> items)
    {
        var views = new List<WarrantyMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToWarrantyMilestoneView(item));
        }
        return views;
    }

    public static double SumWarrantyMilestoneView(WarrantyMilestoneView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return parsedFloorValue + parsedCost + (double)parsedEndsAt;
    }
}
