namespace Acme.Workforce.Talent.Pool.Group;

using System.Globalization;

/// <summary>How screens and exports show a pool group.</summary>
public sealed record PoolGroupView(string Code, string LabelText, string Sequence, string Attempts, string Team, string Location, string SegmentText, string DurationMinutes, string StatusLabel)
{
    public static PoolGroupView ToPoolGroupView(PoolGroup item)
    {
        return new PoolGroupView(
            item.Code,
            item.Label,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Team,
            item.Location,
            item.Segment,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PoolGroupView> ToPoolGroupViews(List<PoolGroup> items)
    {
        var views = new List<PoolGroupView>();
        foreach (var item in items)
        {
            views.Add(ToPoolGroupView(item));
        }
        return views;
    }

    public static double SumPoolGroupView(PoolGroupView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return (double)parsedSequence + (double)parsedAttempts + (double)parsedDurationMinutes;
    }
}
