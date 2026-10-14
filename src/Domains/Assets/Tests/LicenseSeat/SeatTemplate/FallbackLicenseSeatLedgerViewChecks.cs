namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatTemplate;

using Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

/// <summary>Checks of seat template code, run by the project's runner.</summary>
public static class FallbackLicenseSeatLedgerViewChecks
{
    public static void FallbackLicenseSeatLedgerViewShowsTheStatusInCapitals()
    {
        var item = FallbackLicenseSeatLedger.NewFallbackLicenseSeatLedger("code-28", "title-103");
        var view = FallbackLicenseSeatLedgerView.ToFallbackLicenseSeatLedgerView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackLicenseSeatLedgerView.ToFallbackLicenseSeatLedgerViews(new List<FallbackLicenseSeatLedger> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackLicenseSeatLedgerViewShowsTheStatusInCapitals();
    }
}
