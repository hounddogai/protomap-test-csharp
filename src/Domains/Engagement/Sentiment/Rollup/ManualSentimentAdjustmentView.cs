namespace Acme.Workforce.Engagement.Sentiment.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual sentiment adjustment.</summary>
public sealed record ManualSentimentAdjustmentView(string Code, string Name, string Rating, string SkillText, string Minimum, string QuantityText, string StatusLabel)
{
    public static ManualSentimentAdjustmentView ToManualSentimentAdjustmentView(ManualSentimentAdjustment item)
    {
        return new ManualSentimentAdjustmentView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualSentimentAdjustmentView> ToManualSentimentAdjustmentViews(List<ManualSentimentAdjustment> items)
    {
        var views = new List<ManualSentimentAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToManualSentimentAdjustmentView(item));
        }
        return views;
    }

    public static double SumManualSentimentAdjustmentView(ManualSentimentAdjustmentView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        return parsedRating + parsedMinimum + (double)parsedQuantity;
    }
}
