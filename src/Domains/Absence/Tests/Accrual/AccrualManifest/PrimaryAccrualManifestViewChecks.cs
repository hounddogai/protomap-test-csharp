namespace Acme.Workforce.Absence.Tests.Accrual.AccrualManifest;

using Acme.Workforce.Absence.Accrual.AccrualManifest;

/// <summary>Checks of accrual manifest code, run by the project's runner.</summary>
public static class PrimaryAccrualManifestViewChecks
{
    public static void PrimaryAccrualManifestViewShowsTheStatusInCapitals()
    {
        var item = PrimaryAccrualManifest.NewPrimaryAccrualManifest("code-572", "title-983");
        var view = PrimaryAccrualManifestView.ToPrimaryAccrualManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryAccrualManifestView.ToPrimaryAccrualManifestViews(new List<PrimaryAccrualManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryAccrualManifestViewShowsTheStatusInCapitals();
    }
}
