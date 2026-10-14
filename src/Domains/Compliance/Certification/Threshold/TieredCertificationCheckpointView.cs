namespace Acme.Workforce.Compliance.Certification.Threshold;

using System.Globalization;

/// <summary>How screens and exports show a tiered certification checkpoint.</summary>
public sealed record TieredCertificationCheckpointView(string CodeText, string NameText, string JobCode, string UpdatedAtText, string CurrencyText, string Version, string StatusLabel)
{
    public static TieredCertificationCheckpointView ToTieredCertificationCheckpointView(TieredCertificationCheckpoint item)
    {
        return new TieredCertificationCheckpointView(
            item.Code,
            item.Name,
            item.JobCode,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Currency,
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredCertificationCheckpointView> ToTieredCertificationCheckpointViews(List<TieredCertificationCheckpoint> items)
    {
        var views = new List<TieredCertificationCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToTieredCertificationCheckpointView(item));
        }
        return views;
    }

    public static double SumTieredCertificationCheckpointView(TieredCertificationCheckpointView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + (double)parsedVersion;
    }
}
