namespace Acme.Workforce.Orgdesign.Tests.Span.SpanDraft;

using Acme.Workforce.Orgdesign.Span.SpanDraft;

/// <summary>Checks of span draft code, run by the project's runner.</summary>
public static class GlobalSpanDraftViewChecks
{
    public static void GlobalSpanDraftViewShowsTheStatusInCapitals()
    {
        var item = GlobalSpanDraft.NewGlobalSpanDraft("code-907", "title-833");
        var view = GlobalSpanDraftView.ToGlobalSpanDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalSpanDraftView.ToGlobalSpanDraftViews(new List<GlobalSpanDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalSpanDraftViewShowsTheStatusInCapitals();
    }
}
