namespace Acme.Workforce.Compensation.BonusPool.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a bonus pool milestone.</summary>
public sealed record BonusPoolMilestoneView(string Code, string TitleText, string UpdatedAtText, string Rate, string CostCenter, string AmountText, string EmployeeId, string Summary, string Phone, string Hours, string FloorValue, string StatusLabel)
{
    public static BonusPoolMilestoneView ToBonusPoolMilestoneView(BonusPoolMilestone item)
    {
        return new BonusPoolMilestoneView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Title, 40),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.CostCenter,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.EmployeeId, 120),
            item.Summary,
            Text.Truncate(item.Phone, 64),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BonusPoolMilestoneView> ToBonusPoolMilestoneViews(List<BonusPoolMilestone> items)
    {
        var views = new List<BonusPoolMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToBonusPoolMilestoneView(item));
        }
        return views;
    }

    public static double SumBonusPoolMilestoneView(BonusPoolMilestoneView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + parsedRate + parsedAmount;
    }
}
