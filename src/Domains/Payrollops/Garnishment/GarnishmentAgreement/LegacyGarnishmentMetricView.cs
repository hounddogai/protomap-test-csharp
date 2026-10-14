namespace Acme.Workforce.Payrollops.Garnishment.GarnishmentAgreement;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy garnishment metric.</summary>
public sealed record LegacyGarnishmentMetricView(string Code, string LabelText, string Rate, string Skill, string DurationMinutesText, string PinnedText, string StatusLabel)
{
    public static LegacyGarnishmentMetricView ToLegacyGarnishmentMetricView(LegacyGarnishmentMetric item)
    {
        return new LegacyGarnishmentMetricView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyGarnishmentMetricView> ToLegacyGarnishmentMetricViews(List<LegacyGarnishmentMetric> items)
    {
        var views = new List<LegacyGarnishmentMetricView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyGarnishmentMetricView(item));
        }
        return views;
    }

    public static double SumLegacyGarnishmentMetricView(LegacyGarnishmentMetricView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedDurationMinutes;
    }
}
