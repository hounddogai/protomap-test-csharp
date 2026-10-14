namespace Acme.Workforce.Assets.AssetReturn.Budget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending asset return plan.</summary>
public sealed record PendingAssetReturnPlanView(string Code, string LabelText, string Notes, string OwnerId, string Active, string RegionText, string CategoryText, string Amount, string StatusLabel)
{
    public static PendingAssetReturnPlanView ToPendingAssetReturnPlanView(PendingAssetReturnPlan item)
    {
        return new PendingAssetReturnPlanView(
            item.Code,
            item.Label,
            item.Notes,
            Text.Truncate(item.OwnerId, 120),
            $"{item.Active}",
            item.Region,
            item.Category,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingAssetReturnPlanView> ToPendingAssetReturnPlanViews(List<PendingAssetReturnPlan> items)
    {
        var views = new List<PendingAssetReturnPlanView>();
        foreach (var item in items)
        {
            views.Add(ToPendingAssetReturnPlanView(item));
        }
        return views;
    }

    public static double SumPendingAssetReturnPlanView(PendingAssetReturnPlanView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
