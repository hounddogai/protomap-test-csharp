namespace Acme.Workforce.Orgdesign.Tests.Span.Policy;

using Acme.Workforce.Orgdesign.Span.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class GlobalSpanRollupViewChecks
{
    public static void GlobalSpanRollupViewShowsTheStatusInCapitals()
    {
        var item = GlobalSpanRollup.NewGlobalSpanRollup("code-121", "label-512");
        var view = GlobalSpanRollupView.ToGlobalSpanRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalSpanRollupView.ToGlobalSpanRollupViews(new List<GlobalSpanRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalSpanRollupViewShowsTheStatusInCapitals();
    }
}
