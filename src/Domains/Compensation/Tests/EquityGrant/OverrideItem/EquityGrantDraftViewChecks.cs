namespace Acme.Workforce.Compensation.Tests.EquityGrant.OverrideItem;

using Acme.Workforce.Compensation.EquityGrant.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class EquityGrantDraftViewChecks
{
    public static void EquityGrantDraftViewShowsTheStatusInCapitals()
    {
        var item = EquityGrantDraft.NewEquityGrantDraft("code-349", "title-843");
        var view = EquityGrantDraftView.ToEquityGrantDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EquityGrantDraftView.ToEquityGrantDraftViews(new List<EquityGrantDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EquityGrantDraftViewShowsTheStatusInCapitals();
    }
}
