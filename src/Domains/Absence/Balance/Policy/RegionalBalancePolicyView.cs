namespace Acme.Workforce.Absence.Balance.Policy;

using System.Globalization;

/// <summary>How screens and exports show a regional balance policy.</summary>
public sealed record RegionalBalancePolicyView(string Code, string TitleText, string Location, string Reviewer, string Sequence, string Department, string SalaryBand, string RetriesText, string Score, string GraceDays, string StatusLabel)
{
    public static RegionalBalancePolicyView ToRegionalBalancePolicyView(RegionalBalancePolicy item)
    {
        return new RegionalBalancePolicyView(
            item.Code,
            item.Title,
            item.Location,
            item.Reviewer,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Department,
            item.SalaryBand,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalBalancePolicyView> ToRegionalBalancePolicyViews(List<RegionalBalancePolicy> items)
    {
        var views = new List<RegionalBalancePolicyView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalBalancePolicyView(item));
        }
        return views;
    }

    public static double SumRegionalBalancePolicyView(RegionalBalancePolicyView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return (double)parsedSequence + (double)parsedRetries + parsedScore;
    }
}
