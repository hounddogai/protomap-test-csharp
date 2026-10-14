namespace Acme.Workforce.Orgdesign.Reorg.ReorgSnapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered reorg variance.</summary>
public sealed record TieredReorgVarianceView(string CodeText, string Title, string SegmentText, string Timezone, string UpdatedAt, string Maximum, string Score, string Revision, string StatusLabel)
{
    public static TieredReorgVarianceView ToTieredReorgVarianceView(TieredReorgVariance item)
    {
        return new TieredReorgVarianceView(
            item.Code,
            item.Title,
            item.Segment,
            Text.Truncate(item.Timezone, 40),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredReorgVarianceView> ToTieredReorgVarianceViews(List<TieredReorgVariance> items)
    {
        var views = new List<TieredReorgVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToTieredReorgVarianceView(item));
        }
        return views;
    }

    public static double SumTieredReorgVarianceView(TieredReorgVarianceView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + parsedMaximum + parsedScore;
    }
}
