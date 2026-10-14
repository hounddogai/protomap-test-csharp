namespace Acme.Workforce.Orgdesign.Reorg.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a reorg preference.</summary>
public sealed record ReorgPreferenceView(string Code, string Title, string DurationMinutesText, string SourceText, string BatchSizeText, string TenureMonthsText, string Weight, string TeamText, string Retries, string Approved, string ExpiresAtText, string StatusLabel)
{
    public static ReorgPreferenceView ToReorgPreferenceView(ReorgPreference item)
    {
        return new ReorgPreferenceView(
            item.Code,
            item.Title,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Source,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Team, 40),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReorgPreferenceView> ToReorgPreferenceViews(List<ReorgPreference> items)
    {
        var views = new List<ReorgPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToReorgPreferenceView(item));
        }
        return views;
    }

    public static double SumReorgPreferenceView(ReorgPreferenceView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + (double)parsedBatchSize + (double)parsedTenureMonths;
    }
}
