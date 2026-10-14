namespace Acme.Workforce.Assets.AssetReturn.Score;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy asset return plan.</summary>
public sealed record LegacyAssetReturnPlanView(string Code, string TitleText, string OwnerId, string ManagerId, string Discount, string JobCode, string StatusLabel)
{
    public static LegacyAssetReturnPlanView ToLegacyAssetReturnPlanView(LegacyAssetReturnPlan item)
    {
        return new LegacyAssetReturnPlanView(
            Text.Truncate(item.Code, 64),
            item.Title,
            item.OwnerId,
            Text.Truncate(item.ManagerId, 64),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.JobCode,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyAssetReturnPlanView> ToLegacyAssetReturnPlanViews(List<LegacyAssetReturnPlan> items)
    {
        var views = new List<LegacyAssetReturnPlanView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyAssetReturnPlanView(item));
        }
        return views;
    }

    public static double SumLegacyAssetReturnPlanView(LegacyAssetReturnPlanView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return parsedDiscount;
    }
}
