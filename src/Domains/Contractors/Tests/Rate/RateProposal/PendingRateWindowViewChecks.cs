namespace Acme.Workforce.Contractors.Tests.Rate.RateProposal;

using Acme.Workforce.Contractors.Rate.RateProposal;

/// <summary>Checks of rate proposal code, run by the project's runner.</summary>
public static class PendingRateWindowViewChecks
{
    public static void PendingRateWindowViewShowsTheStatusInCapitals()
    {
        var item = PendingRateWindow.NewPendingRateWindow("code-805", "title-100");
        var view = PendingRateWindowView.ToPendingRateWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingRateWindowView.ToPendingRateWindowViews(new List<PendingRateWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingRateWindowViewShowsTheStatusInCapitals();
    }
}
