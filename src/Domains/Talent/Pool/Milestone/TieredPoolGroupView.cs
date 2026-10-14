namespace Acme.Workforce.Talent.Pool.Milestone;

using System.Globalization;

/// <summary>How screens and exports show a tiered pool group.</summary>
public sealed record TieredPoolGroupView(string Code, string Name, string SequenceText, string Retries, string VisibleText, string Amount, string Capacity, string Department, string Score, string Revision, string StatusLabel)
{
    public static TieredPoolGroupView ToTieredPoolGroupView(TieredPoolGroup item)
    {
        return new TieredPoolGroupView(
            item.Code,
            item.Name,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Department,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredPoolGroupView> ToTieredPoolGroupViews(List<TieredPoolGroup> items)
    {
        var views = new List<TieredPoolGroupView>();
        foreach (var item in items)
        {
            views.Add(ToTieredPoolGroupView(item));
        }
        return views;
    }

    public static double SumTieredPoolGroupView(TieredPoolGroupView view)
    {
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return (double)parsedSequence + (double)parsedRetries + parsedAmount;
    }
}
