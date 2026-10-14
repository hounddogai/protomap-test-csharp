namespace Acme.Workforce.Talent.Nomination.Adjustment;

using System.Globalization;

/// <summary>How screens and exports show a seasonal nomination ledger.</summary>
public sealed record SeasonalNominationLedgerView(string Code, string LabelText, string ExpiresAt, string BudgetText, string ScoreText, string StatusLabel)
{
    public static SeasonalNominationLedgerView ToSeasonalNominationLedgerView(SeasonalNominationLedger item)
    {
        return new SeasonalNominationLedgerView(
            item.Code,
            item.Label,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalNominationLedgerView> ToSeasonalNominationLedgerViews(List<SeasonalNominationLedger> items)
    {
        var views = new List<SeasonalNominationLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalNominationLedgerView(item));
        }
        return views;
    }

    public static double SumSeasonalNominationLedgerView(SeasonalNominationLedgerView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + parsedBudget + parsedScore;
    }
}
