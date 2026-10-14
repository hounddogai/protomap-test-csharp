namespace Acme.Workforce.Absence.Tests.Holiday.Request;

using Acme.Workforce.Absence.Holiday.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class CappedHolidayManifestViewChecks
{
    public static void CappedHolidayManifestViewShowsTheStatusInCapitals()
    {
        var item = CappedHolidayManifest.NewCappedHolidayManifest("code-596", "label-536");
        var view = CappedHolidayManifestView.ToCappedHolidayManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedHolidayManifestView.ToCappedHolidayManifestViews(new List<CappedHolidayManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedHolidayManifestViewShowsTheStatusInCapitals();
    }
}
