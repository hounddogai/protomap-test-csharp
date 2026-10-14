namespace Acme.Workforce.Talent.Pool.Target;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy pool target.</summary>
public sealed record LegacyPoolTargetView(string Code, string Name, string ReviewerText, string Revision, string ChecksumText, string Approved, string Priority, string MarginText, string FteText, string StartsAt, string StatusLabel)
{
    public static LegacyPoolTargetView ToLegacyPoolTargetView(LegacyPoolTarget item)
    {
        return new LegacyPoolTargetView(
            Text.Truncate(item.Code, 120),
            item.Name,
            Text.Truncate(item.Reviewer, 80),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Checksum, 80),
            $"{item.Approved}",
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyPoolTargetView> ToLegacyPoolTargetViews(List<LegacyPoolTarget> items)
    {
        var views = new List<LegacyPoolTargetView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyPoolTargetView(item));
        }
        return views;
    }

    public static double SumLegacyPoolTargetView(LegacyPoolTargetView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        return (double)parsedRevision + (double)parsedPriority + parsedMargin;
    }
}
