namespace Acme.Workforce.Orgdesign.Reorg.ReorgClaim;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling reorg summary.</summary>
public sealed record RollingReorgSummaryView(string Code, string Name, string Version, string ExternalRef, string CountryText, string LocaleText, string Hours, string JobCode, string EndsAt, string Weight, string DescriptionText, string StatusLabel)
{
    public static RollingReorgSummaryView ToRollingReorgSummaryView(RollingReorgSummary item)
    {
        return new RollingReorgSummaryView(
            item.Code,
            item.Name,
            item.Version.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ExternalRef, 80),
            Text.Truncate(item.Country, 120),
            Text.Truncate(item.Locale, 64),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.JobCode,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Description, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingReorgSummaryView> ToRollingReorgSummaryViews(List<RollingReorgSummary> items)
    {
        var views = new List<RollingReorgSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToRollingReorgSummaryView(item));
        }
        return views;
    }

    public static double SumRollingReorgSummaryView(RollingReorgSummaryView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return (double)parsedVersion + parsedHours + (double)parsedEndsAt;
    }
}
