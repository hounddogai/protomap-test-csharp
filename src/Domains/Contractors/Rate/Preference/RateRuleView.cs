namespace Acme.Workforce.Contractors.Rate.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rate rule.</summary>
public sealed record RateRuleView(string Code, string NameText, string Ratio, string Country, string Locale, string Source, string StatusLabel)
{
    public static RateRuleView ToRateRuleView(RateRule item)
    {
        return new RateRuleView(
            item.Code,
            item.Name,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Country,
            item.Locale,
            Text.Truncate(item.Source, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RateRuleView> ToRateRuleViews(List<RateRule> items)
    {
        var views = new List<RateRuleView>();
        foreach (var item in items)
        {
            views.Add(ToRateRuleView(item));
        }
        return views;
    }

    public static double SumRateRuleView(RateRuleView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return parsedRatio;
    }
}
