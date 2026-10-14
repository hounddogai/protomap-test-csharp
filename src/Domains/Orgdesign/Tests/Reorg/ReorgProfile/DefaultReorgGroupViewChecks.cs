namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgProfile;

using Acme.Workforce.Orgdesign.Reorg.ReorgProfile;

/// <summary>Checks of reorg profile code, run by the project's runner.</summary>
public static class DefaultReorgGroupViewChecks
{
    public static void DefaultReorgGroupViewShowsTheStatusInCapitals()
    {
        var item = DefaultReorgGroup.NewDefaultReorgGroup("code-997", "label-95");
        var view = DefaultReorgGroupView.ToDefaultReorgGroupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultReorgGroupView.ToDefaultReorgGroupViews(new List<DefaultReorgGroup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultReorgGroupViewShowsTheStatusInCapitals();
    }
}
