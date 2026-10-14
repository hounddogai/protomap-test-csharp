namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantLedger;

using Acme.Workforce.Compensation.EquityGrant.GrantLedger;

/// <summary>Checks of grant ledger code, run by the project's runner.</summary>
public static class GlobalEquityGrantDraftViewChecks
{
    public static void GlobalEquityGrantDraftViewShowsTheStatusInCapitals()
    {
        var item = GlobalEquityGrantDraft.NewGlobalEquityGrantDraft("code-531", "title-962");
        var view = GlobalEquityGrantDraftView.ToGlobalEquityGrantDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalEquityGrantDraftView.ToGlobalEquityGrantDraftViews(new List<GlobalEquityGrantDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalEquityGrantDraftViewShowsTheStatusInCapitals();
    }
}
