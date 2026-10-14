namespace Acme.Workforce.Talent.Pool.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pool digest.</summary>
public sealed record PoolDigestView(string CodeText, string Title, string Location, string Sequence, string BatchSize, string Retries, string StatusLabel)
{
    public static PoolDigestView ToPoolDigestView(PoolDigest item)
    {
        return new PoolDigestView(
            item.Code,
            Text.Truncate(item.Title, 80),
            Text.Truncate(item.Location, 40),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PoolDigestView> ToPoolDigestViews(List<PoolDigest> items)
    {
        var views = new List<PoolDigestView>();
        foreach (var item in items)
        {
            views.Add(ToPoolDigestView(item));
        }
        return views;
    }

    public static double SumPoolDigestView(PoolDigestView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedSequence + (double)parsedBatchSize + (double)parsedRetries;
    }
}
