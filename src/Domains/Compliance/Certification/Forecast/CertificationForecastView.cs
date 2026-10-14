namespace Acme.Workforce.Compliance.Certification.Forecast;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a certification forecast.</summary>
public sealed record CertificationForecastView(string Code, string Name, string Amount, string FloorValue, string Skill, string Priority, string Rating, string OwnerId, string TenureMonths, string Active, string StatusLabel)
{
    public static CertificationForecastView ToCertificationForecastView(CertificationForecast item)
    {
        return new CertificationForecastView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.OwnerId,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CertificationForecastView> ToCertificationForecastViews(List<CertificationForecast> items)
    {
        var views = new List<CertificationForecastView>();
        foreach (var item in items)
        {
            views.Add(ToCertificationForecastView(item));
        }
        return views;
    }

    public static double SumCertificationForecastView(CertificationForecastView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return parsedAmount + parsedFloorValue + (double)parsedPriority;
    }
}
