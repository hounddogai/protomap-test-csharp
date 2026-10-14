namespace Acme.Workforce.Talent.Mobility.Summary;

using System.Globalization;

/// <summary>How screens and exports show a batched mobility manifest.</summary>
public sealed record BatchedMobilityManifestView(string Code, string LabelText, string VersionText, string Retries, string Visible, string SalaryBandText, string Team, string Capacity, string StatusLabel)
{
    public static BatchedMobilityManifestView ToBatchedMobilityManifestView(BatchedMobilityManifest item)
    {
        return new BatchedMobilityManifestView(
            item.Code,
            item.Label,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.SalaryBand,
            item.Team,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedMobilityManifestView> ToBatchedMobilityManifestViews(List<BatchedMobilityManifest> items)
    {
        var views = new List<BatchedMobilityManifestView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedMobilityManifestView(item));
        }
        return views;
    }

    public static double SumBatchedMobilityManifestView(BatchedMobilityManifestView view)
    {
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return (double)parsedVersion + (double)parsedRetries + (double)parsedCapacity;
    }
}
