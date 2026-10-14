namespace Acme.Workforce.Assets.Warranty.Digest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient warranty forecast.</summary>
public sealed record LenientWarrantyForecastView(string Code, string Label, string Attempts, string DurationMinutesText, string ContactEmailText, string TitleText, string StatusLabel)
{
    public static LenientWarrantyForecastView ToLenientWarrantyForecastView(LenientWarrantyForecast item)
    {
        return new LenientWarrantyForecastView(
            item.Code,
            Text.Truncate(item.Label, 40),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.Title,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientWarrantyForecastView> ToLenientWarrantyForecastViews(List<LenientWarrantyForecast> items)
    {
        var views = new List<LenientWarrantyForecastView>();
        foreach (var item in items)
        {
            views.Add(ToLenientWarrantyForecastView(item));
        }
        return views;
    }

    public static double SumLenientWarrantyForecastView(LenientWarrantyForecastView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedDurationMinutes;
    }
}
