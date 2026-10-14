namespace Acme.Workforce.Workplace.Tests.Desk.Adjustment;

using Acme.Workforce.Workplace.Desk.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class DeskManifestViewChecks
{
    public static void DeskManifestViewShowsTheStatusInCapitals()
    {
        var item = DeskManifest.NewDeskManifest("code-49", "name-575");
        var view = DeskManifestView.ToDeskManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DeskManifestView.ToDeskManifestViews(new List<DeskManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DeskManifestViewShowsTheStatusInCapitals();
    }
}
