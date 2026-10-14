namespace Acme.Workforce.Orgdesign.Position.Schedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy position template.</summary>
public sealed record LegacyPositionTemplateView(string CodeText, string Name, string TenureMonthsText, string Timezone, string DurationMinutes, string AttemptsText, string Grade, string PinnedText, string Threshold, string Rate, string StatusLabel)
{
    public static LegacyPositionTemplateView ToLegacyPositionTemplateView(LegacyPositionTemplate item)
    {
        return new LegacyPositionTemplateView(
            item.Code,
            item.Name,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Grade, 64),
            $"{item.Pinned}",
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyPositionTemplateView> ToLegacyPositionTemplateViews(List<LegacyPositionTemplate> items)
    {
        var views = new List<LegacyPositionTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyPositionTemplateView(item));
        }
        return views;
    }

    public static double SumLegacyPositionTemplateView(LegacyPositionTemplateView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + (double)parsedDurationMinutes + (double)parsedAttempts;
    }
}
