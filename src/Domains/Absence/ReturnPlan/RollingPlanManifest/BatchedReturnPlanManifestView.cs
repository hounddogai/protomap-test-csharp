namespace Acme.Workforce.Absence.ReturnPlan.RollingPlanManifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched return plan manifest.</summary>
public sealed record BatchedReturnPlanManifestView(string Code, string Name, string Checksum, string Description, string Threshold, string RatingText, string MaximumText, string Segment, string UpdatedAt, string Channel, string LocationText, string StatusLabel)
{
    public static BatchedReturnPlanManifestView ToBatchedReturnPlanManifestView(BatchedReturnPlanManifest item)
    {
        return new BatchedReturnPlanManifestView(
            item.Code,
            item.Name,
            Text.Truncate(item.Checksum, 40),
            item.Description,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Segment, 64),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            item.Location,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedReturnPlanManifestView> ToBatchedReturnPlanManifestViews(List<BatchedReturnPlanManifest> items)
    {
        var views = new List<BatchedReturnPlanManifestView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedReturnPlanManifestView(item));
        }
        return views;
    }

    public static double SumBatchedReturnPlanManifestView(BatchedReturnPlanManifestView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        return parsedThreshold + parsedRating + parsedMaximum;
    }
}
