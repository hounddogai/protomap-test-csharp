namespace Acme.Workforce.Orgdesign.Tests.Reorg.Group;

using Acme.Workforce.Orgdesign.Reorg.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ReorgBundleViewChecks
{
    public static void ReorgBundleViewShowsTheStatusInCapitals()
    {
        var item = ReorgBundle.NewReorgBundle("code-728", "title-746");
        var view = ReorgBundleView.ToReorgBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ReorgBundleView.ToReorgBundleViews(new List<ReorgBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ReorgBundleViewShowsTheStatusInCapitals();
    }
}
