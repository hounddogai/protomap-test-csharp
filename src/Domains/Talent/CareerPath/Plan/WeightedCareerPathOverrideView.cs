namespace Acme.Workforce.Talent.CareerPath.Plan;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted career path override.</summary>
public sealed record WeightedCareerPathOverrideView(string Code, string Title, string MinimumText, string Skill, string BatchSize, string RateText, string SegmentText, string ExpiresAt, string OwnerIdText, string Hours, string StatusLabel)
{
    public static WeightedCareerPathOverrideView ToWeightedCareerPathOverrideView(WeightedCareerPathOverride item)
    {
        return new WeightedCareerPathOverrideView(
            item.Code,
            item.Title,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Skill, 40),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Segment,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedCareerPathOverrideView> ToWeightedCareerPathOverrideViews(List<WeightedCareerPathOverride> items)
    {
        var views = new List<WeightedCareerPathOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedCareerPathOverrideView(item));
        }
        return views;
    }

    public static double SumWeightedCareerPathOverrideView(WeightedCareerPathOverrideView view)
    {
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        return parsedMinimum + (double)parsedBatchSize + parsedRate;
    }
}
