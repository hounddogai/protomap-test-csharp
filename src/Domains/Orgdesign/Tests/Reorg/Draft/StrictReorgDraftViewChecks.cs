namespace Acme.Workforce.Orgdesign.Tests.Reorg.Draft;

using Acme.Workforce.Orgdesign.Reorg.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class StrictReorgDraftViewChecks
{
    public static void StrictReorgDraftViewShowsTheStatusInCapitals()
    {
        var item = StrictReorgDraft.NewStrictReorgDraft("code-736", "title-703");
        var view = StrictReorgDraftView.ToStrictReorgDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictReorgDraftView.ToStrictReorgDraftViews(new List<StrictReorgDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictReorgDraftViewShowsTheStatusInCapitals();
    }
}
