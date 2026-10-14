namespace Acme.Workforce.Payrollops.Tests.RetroPay.Draft;

using Acme.Workforce.Payrollops.RetroPay.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class RetroPayDraftViewChecks
{
    public static void RetroPayDraftViewShowsTheStatusInCapitals()
    {
        var item = RetroPayDraft.NewRetroPayDraft("code-509", "title-195");
        var view = RetroPayDraftView.ToRetroPayDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RetroPayDraftView.ToRetroPayDraftViews(new List<RetroPayDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RetroPayDraftViewShowsTheStatusInCapitals();
    }
}
