namespace Acme.Workforce.Absence.Tests.Accrual.AccrualTier;

using Acme.Workforce.Absence.Accrual.AccrualTier;

/// <summary>Checks of accrual tier code, run by the project's runner.</summary>
public static class AccrualManifestViewChecks
{
    public static void AccrualManifestViewShowsTheStatusInCapitals()
    {
        var item = AccrualManifest.NewAccrualManifest("code-730", "label-511");
        var view = AccrualManifestView.ToAccrualManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AccrualManifestView.ToAccrualManifestViews(new List<AccrualManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AccrualManifestViewShowsTheStatusInCapitals();
    }
}
