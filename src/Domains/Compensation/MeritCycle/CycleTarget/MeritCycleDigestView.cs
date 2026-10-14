namespace Acme.Workforce.Compensation.MeritCycle.CycleTarget;

using System.Globalization;

/// <summary>How screens and exports show a merit cycle digest.</summary>
public sealed record MeritCycleDigestView(string Code, string Name, string DescriptionText, string Pinned, string Ratio, string UpdatedAt, string Category, string VersionText, string SalaryBandText, string WeightText, string StatusLabel)
{
    public static MeritCycleDigestView ToMeritCycleDigestView(MeritCycleDigest item)
    {
        return new MeritCycleDigestView(
            item.Code,
            item.Name,
            item.Description,
            $"{item.Pinned}",
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Category,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.SalaryBand,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MeritCycleDigestView> ToMeritCycleDigestViews(List<MeritCycleDigest> items)
    {
        var views = new List<MeritCycleDigestView>();
        foreach (var item in items)
        {
            views.Add(ToMeritCycleDigestView(item));
        }
        return views;
    }

    public static double SumMeritCycleDigestView(MeritCycleDigestView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        return parsedRatio + (double)parsedUpdatedAt + (double)parsedVersion;
    }
}
