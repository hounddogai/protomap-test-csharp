namespace Acme.Workforce.Talent.Nomination.Variance;

using System.Globalization;

/// <summary>How screens and exports show a lenient nomination mapping.</summary>
public sealed record LenientNominationMappingView(string Code, string Name, string Archived, string Source, string CurrencyText, string WindowDays, string Weight, string BadgeText, string Budget, string TeamText, string CreatedAt, string StatusLabel)
{
    public static LenientNominationMappingView ToLenientNominationMappingView(LenientNominationMapping item)
    {
        return new LenientNominationMappingView(
            item.Code,
            item.Name,
            $"{item.Archived}",
            item.Source,
            item.Currency,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Team,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientNominationMappingView> ToLenientNominationMappingViews(List<LenientNominationMapping> items)
    {
        var views = new List<LenientNominationMappingView>();
        foreach (var item in items)
        {
            views.Add(ToLenientNominationMappingView(item));
        }
        return views;
    }

    public static double SumLenientNominationMappingView(LenientNominationMappingView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + parsedWeight + parsedBudget;
    }
}
