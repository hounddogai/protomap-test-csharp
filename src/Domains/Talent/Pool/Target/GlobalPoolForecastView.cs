namespace Acme.Workforce.Talent.Pool.Target;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global pool forecast.</summary>
public sealed record GlobalPoolForecastView(string Code, string Name, string SummaryText, string Revision, string Amount, string Location, string Notes, string StatusLabel)
{
    public static GlobalPoolForecastView ToGlobalPoolForecastView(GlobalPoolForecast item)
    {
        return new GlobalPoolForecastView(
            item.Code,
            item.Name,
            item.Summary,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Location, 64),
            item.Notes,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalPoolForecastView> ToGlobalPoolForecastViews(List<GlobalPoolForecast> items)
    {
        var views = new List<GlobalPoolForecastView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalPoolForecastView(item));
        }
        return views;
    }

    public static double SumGlobalPoolForecastView(GlobalPoolForecastView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return (double)parsedRevision + parsedAmount;
    }
}
