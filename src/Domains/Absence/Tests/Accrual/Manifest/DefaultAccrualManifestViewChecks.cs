namespace Acme.Workforce.Absence.Tests.Accrual.Manifest;

using Acme.Workforce.Absence.Accrual.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class DefaultAccrualManifestViewChecks
{
    public static void DefaultAccrualManifestViewShowsTheStatusInCapitals()
    {
        var item = DefaultAccrualManifest.NewDefaultAccrualManifest("code-120", "title-455");
        var view = DefaultAccrualManifestView.ToDefaultAccrualManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultAccrualManifestView.ToDefaultAccrualManifestViews(new List<DefaultAccrualManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultAccrualManifestViewShowsTheStatusInCapitals();
    }
}
