namespace Acme.Workforce.Assets.AssetReturn.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a asset return policy.</summary>
public sealed record AssetReturnPolicyView(string CodeText, string Label, string LimitText, string GradeText, string SalaryBandText, string Threshold, string TenureMonthsText, string Timezone, string FloorValue, string EmployeeId, string StatusLabel)
{
    public static AssetReturnPolicyView ToAssetReturnPolicyView(AssetReturnPolicy item)
    {
        return new AssetReturnPolicyView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            item.SalaryBand,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.EmployeeId, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AssetReturnPolicyView> ToAssetReturnPolicyViews(List<AssetReturnPolicy> items)
    {
        var views = new List<AssetReturnPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToAssetReturnPolicyView(item));
        }
        return views;
    }

    public static double SumAssetReturnPolicyView(AssetReturnPolicyView view)
    {
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        return (double)parsedLimit + parsedThreshold + (double)parsedTenureMonths;
    }
}
