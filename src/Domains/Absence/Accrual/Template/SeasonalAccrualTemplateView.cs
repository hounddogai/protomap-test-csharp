namespace Acme.Workforce.Absence.Accrual.Template;

using System.Globalization;

/// <summary>How screens and exports show a seasonal accrual template.</summary>
public sealed record SeasonalAccrualTemplateView(string Code, string Name, string Maximum, string GraceDays, string Category, string Segment, string ContactEmail, string DurationMinutes, string Team, string StatusLabel)
{
    public static SeasonalAccrualTemplateView ToSeasonalAccrualTemplateView(SeasonalAccrualTemplate item)
    {
        return new SeasonalAccrualTemplateView(
            item.Code,
            item.Name,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Category,
            item.Segment,
            item.ContactEmail,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Team,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalAccrualTemplateView> ToSeasonalAccrualTemplateViews(List<SeasonalAccrualTemplate> items)
    {
        var views = new List<SeasonalAccrualTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalAccrualTemplateView(item));
        }
        return views;
    }

    public static double SumSeasonalAccrualTemplateView(SeasonalAccrualTemplateView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return parsedMaximum + (double)parsedGraceDays + (double)parsedDurationMinutes;
    }
}
