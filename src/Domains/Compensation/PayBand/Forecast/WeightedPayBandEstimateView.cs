namespace Acme.Workforce.Compensation.PayBand.Forecast;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted pay band estimate.</summary>
public sealed record WeightedPayBandEstimateView(string Code, string Label, string Fte, string Attempts, string Maximum, string Team, string StatusLabel)
{
    public static WeightedPayBandEstimateView ToWeightedPayBandEstimateView(WeightedPayBandEstimate item)
    {
        return new WeightedPayBandEstimateView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Team,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedPayBandEstimateView> ToWeightedPayBandEstimateViews(List<WeightedPayBandEstimate> items)
    {
        var views = new List<WeightedPayBandEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedPayBandEstimateView(item));
        }
        return views;
    }

    public static double SumWeightedPayBandEstimateView(WeightedPayBandEstimateView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return parsedFte + (double)parsedAttempts + parsedMaximum;
    }
}
