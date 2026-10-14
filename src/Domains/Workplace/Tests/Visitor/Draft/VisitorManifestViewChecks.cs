namespace Acme.Workforce.Workplace.Tests.Visitor.Draft;

using Acme.Workforce.Workplace.Visitor.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class VisitorManifestViewChecks
{
    public static void VisitorManifestViewShowsTheStatusInCapitals()
    {
        var item = VisitorManifest.NewVisitorManifest("code-392", "name-460");
        var view = VisitorManifestView.ToVisitorManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(VisitorManifestView.ToVisitorManifestViews(new List<VisitorManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        VisitorManifestViewShowsTheStatusInCapitals();
    }
}
