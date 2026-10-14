namespace Acme.Workforce.Orgdesign.CostModel.ModelManifest;

using System.Globalization;

/// <summary>How screens and exports show a weighted cost model manifest.</summary>
public sealed record WeightedCostModelManifestView(string Code, string Name, string Threshold, string Rating, string UpdatedAt, string DurationMinutes, string Active, string Team, string Title, string Quantity, string StatusLabel)
{
    public static WeightedCostModelManifestView ToWeightedCostModelManifestView(WeightedCostModelManifest item)
    {
        return new WeightedCostModelManifestView(
            item.Code,
            item.Name,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.Team,
            item.Title,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedCostModelManifestView> ToWeightedCostModelManifestViews(List<WeightedCostModelManifest> items)
    {
        var views = new List<WeightedCostModelManifestView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedCostModelManifestView(item));
        }
        return views;
    }

    public static double SumWeightedCostModelManifestView(WeightedCostModelManifestView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return parsedThreshold + parsedRating + (double)parsedUpdatedAt;
    }
}
