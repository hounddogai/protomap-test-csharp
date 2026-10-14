namespace Acme.Workforce.Talent.Pool.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict pool forecast.</summary>
public sealed record StrictPoolForecastView(string CodeText, string LabelText, string UpdatedAt, string Limit, string Checksum, string StatusLabel)
{
    public static StrictPoolForecastView ToStrictPoolForecastView(StrictPoolForecast item)
    {
        return new StrictPoolForecastView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Label, 120),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Checksum,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictPoolForecastView> ToStrictPoolForecastViews(List<StrictPoolForecast> items)
    {
        var views = new List<StrictPoolForecastView>();
        foreach (var item in items)
        {
            views.Add(ToStrictPoolForecastView(item));
        }
        return views;
    }

    public static double SumStrictPoolForecastView(StrictPoolForecastView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + (double)parsedLimit;
    }
}
