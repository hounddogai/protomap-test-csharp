namespace Acme.Workforce.Talent.Nomination.Policy;

using System.Globalization;

/// <summary>How screens and exports show a nomination rollup.</summary>
public sealed record NominationRollupView(string CodeText, string Label, string CeilingText, string Weight, string Rate, string ActiveText, string StatusLabel)
{
    public static NominationRollupView ToNominationRollupView(NominationRollup item)
    {
        return new NominationRollupView(
            item.Code,
            item.Label,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<NominationRollupView> ToNominationRollupViews(List<NominationRollup> items)
    {
        var views = new List<NominationRollupView>();
        foreach (var item in items)
        {
            views.Add(ToNominationRollupView(item));
        }
        return views;
    }

    public static double SumNominationRollupView(NominationRollupView view)
    {
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return parsedCeiling + parsedWeight + parsedRate;
    }
}
