namespace Acme.Workforce.Talent.Nomination.Request;

using System.Globalization;

/// <summary>How screens and exports show a lenient nomination rule.</summary>
public sealed record LenientNominationRuleView(string Code, string Name, string Cost, string Currency, string CeilingText, string Ratio, string Budget, string StatusLabel)
{
    public static LenientNominationRuleView ToLenientNominationRuleView(LenientNominationRule item)
    {
        return new LenientNominationRuleView(
            item.Code,
            item.Name,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientNominationRuleView> ToLenientNominationRuleViews(List<LenientNominationRule> items)
    {
        var views = new List<LenientNominationRuleView>();
        foreach (var item in items)
        {
            views.Add(ToLenientNominationRuleView(item));
        }
        return views;
    }

    public static double SumLenientNominationRuleView(LenientNominationRuleView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return parsedCost + parsedCeiling + parsedRatio;
    }
}
