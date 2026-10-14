namespace Acme.Workforce.Talent.SuccessionPlan.Forecast;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a succession plan forecast.</summary>
public sealed record SuccessionPlanForecastView(string Code, string Label, string RevisionText, string ContactEmailText, string Visible, string Quantity, string StatusLabel)
{
    public static SuccessionPlanForecastView ToSuccessionPlanForecastView(SuccessionPlanForecast item)
    {
        return new SuccessionPlanForecastView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.ContactEmail,
            $"{item.Visible}",
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SuccessionPlanForecastView> ToSuccessionPlanForecastViews(List<SuccessionPlanForecast> items)
    {
        var views = new List<SuccessionPlanForecastView>();
        foreach (var item in items)
        {
            views.Add(ToSuccessionPlanForecastView(item));
        }
        return views;
    }

    public static double SumSuccessionPlanForecastView(SuccessionPlanForecastView view)
    {
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedRevision + (double)parsedQuantity;
    }
}
