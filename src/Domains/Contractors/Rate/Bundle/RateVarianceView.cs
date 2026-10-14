namespace Acme.Workforce.Contractors.Rate.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rate variance.</summary>
public sealed record RateVarianceView(string Code, string Title, string Source, string LocaleText, string WeightText, string StatusLabel)
{
    public static RateVarianceView ToRateVarianceView(RateVariance item)
    {
        return new RateVarianceView(
            Text.Truncate(item.Code, 120),
            item.Title,
            item.Source,
            item.Locale,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RateVarianceView> ToRateVarianceViews(List<RateVariance> items)
    {
        var views = new List<RateVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToRateVarianceView(item));
        }
        return views;
    }

    public static double SumRateVarianceView(RateVarianceView view)
    {
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        return parsedWeight;
    }
}
