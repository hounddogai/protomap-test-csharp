namespace Acme.Workforce.Payrollops.Garnishment.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a garnishment baseline.</summary>
public sealed record GarnishmentBaselineView(string CodeText, string Label, string Rate, string Version, string StartsAtText, string NotesText, string ApprovedText, string Ceiling, string Limit, string TimezoneText, string StatusLabel)
{
    public static GarnishmentBaselineView ToGarnishmentBaselineView(GarnishmentBaseline item)
    {
        return new GarnishmentBaselineView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Label, 40),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Notes,
            $"{item.Approved}",
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Timezone, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GarnishmentBaselineView> ToGarnishmentBaselineViews(List<GarnishmentBaseline> items)
    {
        var views = new List<GarnishmentBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToGarnishmentBaselineView(item));
        }
        return views;
    }

    public static double SumGarnishmentBaselineView(GarnishmentBaselineView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedVersion + (double)parsedStartsAt;
    }
}
