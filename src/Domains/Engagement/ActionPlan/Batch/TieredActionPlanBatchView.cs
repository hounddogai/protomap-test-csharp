namespace Acme.Workforce.Engagement.ActionPlan.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered action plan batch.</summary>
public sealed record TieredActionPlanBatchView(string Code, string Label, string Revision, string RatingText, string BatchSize, string SkillText, string ChannelText, string TenureMonths, string ContactEmail, string SequenceText, string StatusLabel)
{
    public static TieredActionPlanBatchView ToTieredActionPlanBatchView(TieredActionPlanBatch item)
    {
        return new TieredActionPlanBatchView(
            item.Code,
            item.Label,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Skill,
            item.Channel,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ContactEmail, 80),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredActionPlanBatchView> ToTieredActionPlanBatchViews(List<TieredActionPlanBatch> items)
    {
        var views = new List<TieredActionPlanBatchView>();
        foreach (var item in items)
        {
            views.Add(ToTieredActionPlanBatchView(item));
        }
        return views;
    }

    public static double SumTieredActionPlanBatchView(TieredActionPlanBatchView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return (double)parsedRevision + parsedRating + (double)parsedBatchSize;
    }
}
