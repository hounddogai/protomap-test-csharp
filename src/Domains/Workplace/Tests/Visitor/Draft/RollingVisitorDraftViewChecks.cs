namespace Acme.Workforce.Workplace.Tests.Visitor.Draft;

using Acme.Workforce.Workplace.Visitor.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class RollingVisitorDraftViewChecks
{
    public static void RollingVisitorDraftViewShowsTheStatusInCapitals()
    {
        var item = RollingVisitorDraft.NewRollingVisitorDraft("code-72", "title-686");
        var view = RollingVisitorDraftView.ToRollingVisitorDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingVisitorDraftView.ToRollingVisitorDraftViews(new List<RollingVisitorDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingVisitorDraftViewShowsTheStatusInCapitals();
    }
}
