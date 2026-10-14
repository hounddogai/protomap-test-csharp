namespace Acme.Workforce.Compensation.BonusPool.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a bonus pool baseline.</summary>
public sealed record BonusPoolBaselineView(string Code, string Title, string Cost, string LeaveDays, string ManagerIdText, string ChannelText, string Maximum, string Ceiling, string Weight, string DiscountText, string Rate, string StatusLabel)
{
    public static BonusPoolBaselineView ToBonusPoolBaselineView(BonusPoolBaseline item)
    {
        return new BonusPoolBaselineView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ManagerId, 40),
            Text.Truncate(item.Channel, 64),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BonusPoolBaselineView> ToBonusPoolBaselineViews(List<BonusPoolBaseline> items)
    {
        var views = new List<BonusPoolBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToBonusPoolBaselineView(item));
        }
        return views;
    }

    public static double SumBonusPoolBaselineView(BonusPoolBaselineView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return parsedCost + (double)parsedLeaveDays + parsedMaximum;
    }
}
