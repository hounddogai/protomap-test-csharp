namespace Acme.Workforce.Compensation.PayBand.Summary;

using System.Globalization;

/// <summary>How screens and exports show a seasonal pay band template.</summary>
public sealed record SeasonalPayBandTemplateView(string Code, string NameText, string EmployeeId, string StartsAt, string CreatedAt, string TenureMonths, string Weight, string DurationMinutes, string StatusLabel)
{
    public static SeasonalPayBandTemplateView ToSeasonalPayBandTemplateView(SeasonalPayBandTemplate item)
    {
        return new SeasonalPayBandTemplateView(
            item.Code,
            item.Name,
            item.EmployeeId,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalPayBandTemplateView> ToSeasonalPayBandTemplateViews(List<SeasonalPayBandTemplate> items)
    {
        var views = new List<SeasonalPayBandTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalPayBandTemplateView(item));
        }
        return views;
    }

    public static double SumSeasonalPayBandTemplateView(SeasonalPayBandTemplateView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + (double)parsedCreatedAt + (double)parsedTenureMonths;
    }
}
