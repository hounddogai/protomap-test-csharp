namespace Acme.Workforce.Orgdesign.Span.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled span digest.</summary>
public sealed record ScheduledSpanDigestView(string Code, string LabelText, string EndsAtText, string ExternalRef, string VersionText, string EmployeeId, string Unit, string Ratio, string StartsAt, string StatusLabel)
{
    public static ScheduledSpanDigestView ToScheduledSpanDigestView(ScheduledSpanDigest item)
    {
        return new ScheduledSpanDigestView(
            item.Code,
            item.Label,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.Version.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.EmployeeId, 40),
            Text.Truncate(item.Unit, 64),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledSpanDigestView> ToScheduledSpanDigestViews(List<ScheduledSpanDigest> items)
    {
        var views = new List<ScheduledSpanDigestView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledSpanDigestView(item));
        }
        return views;
    }

    public static double SumScheduledSpanDigestView(ScheduledSpanDigestView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedVersion + parsedRatio;
    }
}
