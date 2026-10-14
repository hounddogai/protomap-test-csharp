namespace Acme.Workforce.Orgdesign.Tests.Span.Window;

using Acme.Workforce.Orgdesign.Span.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class TieredSpanGroupViewChecks
{
    public static void TieredSpanGroupViewShowsTheStatusInCapitals()
    {
        var item = TieredSpanGroup.NewTieredSpanGroup("code-839", "title-177");
        var view = TieredSpanGroupView.ToTieredSpanGroupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredSpanGroupView.ToTieredSpanGroupViews(new List<TieredSpanGroup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredSpanGroupViewShowsTheStatusInCapitals();
    }
}
