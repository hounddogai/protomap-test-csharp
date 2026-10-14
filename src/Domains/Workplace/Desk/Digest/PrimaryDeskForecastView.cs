namespace Acme.Workforce.Workplace.Desk.Digest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary desk forecast.</summary>
public sealed record PrimaryDeskForecastView(string Code, string Label, string DurationMinutes, string Grade, string ManagerId, string ApprovedText, string StatusLabel)
{
    public static PrimaryDeskForecastView ToPrimaryDeskForecastView(PrimaryDeskForecast item)
    {
        return new PrimaryDeskForecastView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            item.ManagerId,
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryDeskForecastView> ToPrimaryDeskForecastViews(List<PrimaryDeskForecast> items)
    {
        var views = new List<PrimaryDeskForecastView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryDeskForecastView(item));
        }
        return views;
    }

    public static double SumPrimaryDeskForecastView(PrimaryDeskForecastView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes;
    }
}
