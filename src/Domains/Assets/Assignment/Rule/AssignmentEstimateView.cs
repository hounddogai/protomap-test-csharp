namespace Acme.Workforce.Assets.Assignment.Rule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a assignment estimate.</summary>
public sealed record AssignmentEstimateView(string Code, string Name, string Version, string Discount, string CostCenterText, string Cost, string Quantity, string WindowDaysText, string StatusLabel)
{
    public static AssignmentEstimateView ToAssignmentEstimateView(AssignmentEstimate item)
    {
        return new AssignmentEstimateView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Name, 120),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.CostCenter, 64),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AssignmentEstimateView> ToAssignmentEstimateViews(List<AssignmentEstimate> items)
    {
        var views = new List<AssignmentEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToAssignmentEstimateView(item));
        }
        return views;
    }

    public static double SumAssignmentEstimateView(AssignmentEstimateView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return (double)parsedVersion + parsedDiscount + parsedCost;
    }
}
