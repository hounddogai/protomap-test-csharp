namespace Acme.Workforce.Compensation.EquityGrant.Schedule;

using System.Globalization;

/// <summary>How screens and exports show a equity grant segment.</summary>
public sealed record EquityGrantSegmentView(string Code, string Name, string EndsAt, string PinnedText, string ExternalRef, string EmployeeId, string Locale, string VersionText, string Unit, string StatusLabel)
{
    public static EquityGrantSegmentView ToEquityGrantSegmentView(EquityGrantSegment item)
    {
        return new EquityGrantSegmentView(
            item.Code,
            item.Name,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.ExternalRef,
            item.EmployeeId,
            item.Locale,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Unit,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EquityGrantSegmentView> ToEquityGrantSegmentViews(List<EquityGrantSegment> items)
    {
        var views = new List<EquityGrantSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToEquityGrantSegmentView(item));
        }
        return views;
    }

    public static double SumEquityGrantSegmentView(EquityGrantSegmentView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedVersion;
    }
}
