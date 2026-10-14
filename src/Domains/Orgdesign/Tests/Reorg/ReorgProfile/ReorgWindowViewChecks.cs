namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgProfile;

using Acme.Workforce.Orgdesign.Reorg.ReorgProfile;

/// <summary>Checks of reorg profile code, run by the project's runner.</summary>
public static class ReorgWindowViewChecks
{
    public static void ReorgWindowViewShowsTheStatusInCapitals()
    {
        var item = ReorgWindow.NewReorgWindow("code-926", "name-984");
        var view = ReorgWindowView.ToReorgWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ReorgWindowView.ToReorgWindowViews(new List<ReorgWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ReorgWindowViewShowsTheStatusInCapitals();
    }
}
