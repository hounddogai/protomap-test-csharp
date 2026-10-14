namespace Acme.Workforce.Assets.AssetReturn.Cycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped asset return cycle.</summary>
public sealed record CappedAssetReturnCycleView(string Code, string Name, string JobCode, string CostCenterText, string BadgeText, string Grade, string BudgetText, string Maximum, string Approved, string TeamText, string ManagerId, string StatusLabel)
{
    public static CappedAssetReturnCycleView ToCappedAssetReturnCycleView(CappedAssetReturnCycle item)
    {
        return new CappedAssetReturnCycleView(
            item.Code,
            item.Name,
            Text.Truncate(item.JobCode, 120),
            item.CostCenter,
            item.Badge,
            item.Grade,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Team,
            item.ManagerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedAssetReturnCycleView> ToCappedAssetReturnCycleViews(List<CappedAssetReturnCycle> items)
    {
        var views = new List<CappedAssetReturnCycleView>();
        foreach (var item in items)
        {
            views.Add(ToCappedAssetReturnCycleView(item));
        }
        return views;
    }

    public static double SumCappedAssetReturnCycleView(CappedAssetReturnCycleView view)
    {
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return parsedBudget + parsedMaximum;
    }
}
