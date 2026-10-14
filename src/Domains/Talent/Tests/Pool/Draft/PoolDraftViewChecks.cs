namespace Acme.Workforce.Talent.Tests.Pool.Draft;

using Acme.Workforce.Talent.Pool.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class PoolDraftViewChecks
{
    public static void PoolDraftViewShowsTheStatusInCapitals()
    {
        var item = PoolDraft.NewPoolDraft("code-299", "title-374");
        var view = PoolDraftView.ToPoolDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PoolDraftView.ToPoolDraftViews(new List<PoolDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PoolDraftViewShowsTheStatusInCapitals();
    }
}
