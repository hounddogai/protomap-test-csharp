namespace Acme.Workforce.Talent.Pool.Window;

using System.Globalization;

/// <summary>How screens and exports show a pool variance.</summary>
public sealed record PoolVarianceView(string Code, string LabelText, string RevisionText, string Locale, string UpdatedAt, string ChannelText, string StatusLabel)
{
    public static PoolVarianceView ToPoolVarianceView(PoolVariance item)
    {
        return new PoolVarianceView(
            item.Code,
            item.Label,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Locale,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PoolVarianceView> ToPoolVarianceViews(List<PoolVariance> items)
    {
        var views = new List<PoolVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToPoolVarianceView(item));
        }
        return views;
    }

    public static double SumPoolVarianceView(PoolVarianceView view)
    {
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return (double)parsedRevision + (double)parsedUpdatedAt;
    }
}
