namespace Acme.Workforce.Workplace.Tests.Visitor.Estimate;

using Acme.Workforce.Workplace.Visitor.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class GlobalVisitorDraftViewChecks
{
    public static void GlobalVisitorDraftViewShowsTheStatusInCapitals()
    {
        var item = GlobalVisitorDraft.NewGlobalVisitorDraft("code-261", "title-965");
        var view = GlobalVisitorDraftView.ToGlobalVisitorDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalVisitorDraftView.ToGlobalVisitorDraftViews(new List<GlobalVisitorDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalVisitorDraftViewShowsTheStatusInCapitals();
    }
}
