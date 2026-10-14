namespace Acme.Workforce.Compensation.BonusPool.PoolRequest;

using System.Globalization;

/// <summary>How screens and exports show a weighted bonus pool milestone.</summary>
public sealed record WeightedBonusPoolMilestoneView(string Code, string Label, string Rate, string PhoneText, string Summary, string StatusLabel)
{
    public static WeightedBonusPoolMilestoneView ToWeightedBonusPoolMilestoneView(WeightedBonusPoolMilestone item)
    {
        return new WeightedBonusPoolMilestoneView(
            item.Code,
            item.Label,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Phone,
            item.Summary,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedBonusPoolMilestoneView> ToWeightedBonusPoolMilestoneViews(List<WeightedBonusPoolMilestone> items)
    {
        var views = new List<WeightedBonusPoolMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedBonusPoolMilestoneView(item));
        }
        return views;
    }

    public static double SumWeightedBonusPoolMilestoneView(WeightedBonusPoolMilestoneView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return parsedRate;
    }
}
