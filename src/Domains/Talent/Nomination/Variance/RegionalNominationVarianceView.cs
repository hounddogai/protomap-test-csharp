namespace Acme.Workforce.Talent.Nomination.Variance;

using System.Globalization;

/// <summary>How screens and exports show a regional nomination variance.</summary>
public sealed record RegionalNominationVarianceView(string CodeText, string Name, string AttemptsText, string PriorityText, string Rate, string Hours, string Ratio, string MarginText, string ManagerIdText, string StatusLabel)
{
    public static RegionalNominationVarianceView ToRegionalNominationVarianceView(RegionalNominationVariance item)
    {
        return new RegionalNominationVarianceView(
            item.Code,
            item.Name,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.ManagerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalNominationVarianceView> ToRegionalNominationVarianceViews(List<RegionalNominationVariance> items)
    {
        var views = new List<RegionalNominationVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalNominationVarianceView(item));
        }
        return views;
    }

    public static double SumRegionalNominationVarianceView(RegionalNominationVarianceView view)
    {
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedPriority + parsedRate;
    }
}
