namespace Acme.Workforce.Talent.Mobility.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a mobility score.</summary>
public sealed record MobilityScoreView(string Code, string Title, string Limit, string VersionText, string BatchSizeText, string StatusLabel)
{
    public static MobilityScoreView ToMobilityScoreView(MobilityScore item)
    {
        return new MobilityScoreView(
            Text.Truncate(item.Code, 64),
            item.Title,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MobilityScoreView> ToMobilityScoreViews(List<MobilityScore> items)
    {
        var views = new List<MobilityScoreView>();
        foreach (var item in items)
        {
            views.Add(ToMobilityScoreView(item));
        }
        return views;
    }

    public static double SumMobilityScoreView(MobilityScoreView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        return (double)parsedLimit + (double)parsedVersion + (double)parsedBatchSize;
    }
}
