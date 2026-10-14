namespace Acme.Workforce.Workplace.Tests.Desk.Preference;

using Acme.Workforce.Workplace.Desk.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class DeskDraftViewChecks
{
    public static void DeskDraftViewShowsTheStatusInCapitals()
    {
        var item = DeskDraft.NewDeskDraft("code-218", "title-705");
        var view = DeskDraftView.ToDeskDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DeskDraftView.ToDeskDraftViews(new List<DeskDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DeskDraftViewShowsTheStatusInCapitals();
    }
}
