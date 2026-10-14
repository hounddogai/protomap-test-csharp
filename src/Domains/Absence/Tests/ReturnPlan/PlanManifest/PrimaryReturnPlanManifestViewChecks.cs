namespace Acme.Workforce.Absence.Tests.ReturnPlan.PlanManifest;

using Acme.Workforce.Absence.ReturnPlan.PlanManifest;

/// <summary>Checks of plan manifest code, run by the project's runner.</summary>
public static class PrimaryReturnPlanManifestViewChecks
{
    public static void PrimaryReturnPlanManifestViewShowsTheStatusInCapitals()
    {
        var item = PrimaryReturnPlanManifest.NewPrimaryReturnPlanManifest("code-161", "title-482");
        var view = PrimaryReturnPlanManifestView.ToPrimaryReturnPlanManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryReturnPlanManifestView.ToPrimaryReturnPlanManifestViews(new List<PrimaryReturnPlanManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryReturnPlanManifestViewShowsTheStatusInCapitals();
    }
}
