namespace Acme.Workforce.Talent.Tests.Nomination.Adjustment;

using Acme.Workforce.Talent.Nomination.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class SeasonalNominationLedgerViewChecks
{
    public static void SeasonalNominationLedgerViewShowsTheStatusInCapitals()
    {
        var item = SeasonalNominationLedger.NewSeasonalNominationLedger("code-946", "label-251");
        var view = SeasonalNominationLedgerView.ToSeasonalNominationLedgerView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalNominationLedgerView.ToSeasonalNominationLedgerViews(new List<SeasonalNominationLedger> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalNominationLedgerViewShowsTheStatusInCapitals();
    }
}
