namespace Acme.Workforce.Orgdesign.CostModel.ModelMetric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a cost model milestone.</summary>
public sealed record CostModelMilestoneView(string CodeText, string Label, string Segment, string Score, string Archived, string RatingText, string UpdatedAt, string EndsAt, string OwnerIdText, string StatusLabel)
{
    public static CostModelMilestoneView ToCostModelMilestoneView(CostModelMilestone item)
    {
        return new CostModelMilestoneView(
            item.Code,
            Text.Truncate(item.Label, 120),
            Text.Truncate(item.Segment, 64),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CostModelMilestoneView> ToCostModelMilestoneViews(List<CostModelMilestone> items)
    {
        var views = new List<CostModelMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToCostModelMilestoneView(item));
        }
        return views;
    }

    public static double SumCostModelMilestoneView(CostModelMilestoneView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return parsedScore + parsedRating + (double)parsedUpdatedAt;
    }
}
