namespace Acme.Workforce.Workplace.FloorPlan.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a floor plan target.</summary>
public sealed record FloorPlanTargetView(string CodeText, string Title, string JobCodeText, string GraceDays, string Name, string AmountText, string ManagerIdText, string StatusLabel)
{
    public static FloorPlanTargetView ToFloorPlanTargetView(FloorPlanTarget item)
    {
        return new FloorPlanTargetView(
            item.Code,
            item.Title,
            Text.Truncate(item.JobCode, 64),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Name,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.ManagerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FloorPlanTargetView> ToFloorPlanTargetViews(List<FloorPlanTarget> items)
    {
        var views = new List<FloorPlanTargetView>();
        foreach (var item in items)
        {
            views.Add(ToFloorPlanTargetView(item));
        }
        return views;
    }

    public static double SumFloorPlanTargetView(FloorPlanTargetView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + parsedAmount;
    }
}
