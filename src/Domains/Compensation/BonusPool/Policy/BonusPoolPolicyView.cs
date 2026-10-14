namespace Acme.Workforce.Compensation.BonusPool.Policy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a bonus pool policy.</summary>
public sealed record BonusPoolPolicyView(string Code, string NameText, string RatioText, string LeaveDays, string Timezone, string Reviewer, string DescriptionText, string QuantityText, string StatusLabel)
{
    public static BonusPoolPolicyView ToBonusPoolPolicyView(BonusPoolPolicy item)
    {
        return new BonusPoolPolicyView(
            item.Code,
            item.Name,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Timezone, 64),
            Text.Truncate(item.Reviewer, 64),
            item.Description,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BonusPoolPolicyView> ToBonusPoolPolicyViews(List<BonusPoolPolicy> items)
    {
        var views = new List<BonusPoolPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToBonusPoolPolicyView(item));
        }
        return views;
    }

    public static double SumBonusPoolPolicyView(BonusPoolPolicyView view)
    {
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        return parsedRatio + (double)parsedLeaveDays + (double)parsedQuantity;
    }
}
