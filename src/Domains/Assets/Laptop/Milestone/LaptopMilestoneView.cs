namespace Acme.Workforce.Assets.Laptop.Milestone;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a laptop milestone.</summary>
public sealed record LaptopMilestoneView(string CodeText, string TitleText, string CostCenterText, string Description, string DepartmentText, string QuantityText, string StatusLabel)
{
    public static LaptopMilestoneView ToLaptopMilestoneView(LaptopMilestone item)
    {
        return new LaptopMilestoneView(
            item.Code,
            Text.Truncate(item.Title, 40),
            Text.Truncate(item.CostCenter, 64),
            Text.Truncate(item.Description, 120),
            item.Department,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LaptopMilestoneView> ToLaptopMilestoneViews(List<LaptopMilestone> items)
    {
        var views = new List<LaptopMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToLaptopMilestoneView(item));
        }
        return views;
    }

    public static double SumLaptopMilestoneView(LaptopMilestoneView view)
    {
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
