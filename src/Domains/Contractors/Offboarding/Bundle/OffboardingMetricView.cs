namespace Acme.Workforce.Contractors.Offboarding.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a offboarding metric.</summary>
public sealed record OffboardingMetricView(string Code, string NameText, string Limit, string WeightText, string Rate, string CreatedAt, string EndsAt, string Rating, string StatusLabel)
{
    public static OffboardingMetricView ToOffboardingMetricView(OffboardingMetric item)
    {
        return new OffboardingMetricView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OffboardingMetricView> ToOffboardingMetricViews(List<OffboardingMetric> items)
    {
        var views = new List<OffboardingMetricView>();
        foreach (var item in items)
        {
            views.Add(ToOffboardingMetricView(item));
        }
        return views;
    }

    public static double SumOffboardingMetricView(OffboardingMetricView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return (double)parsedLimit + parsedWeight + parsedRate;
    }
}
