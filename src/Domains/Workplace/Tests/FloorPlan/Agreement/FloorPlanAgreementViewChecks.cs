namespace Acme.Workforce.Workplace.Tests.FloorPlan.Agreement;

using Acme.Workforce.Workplace.FloorPlan.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class FloorPlanAgreementViewChecks
{
    public static void FloorPlanAgreementViewShowsTheStatusInCapitals()
    {
        var item = FloorPlanAgreement.NewFloorPlanAgreement("code-131", "name-708");
        var view = FloorPlanAgreementView.ToFloorPlanAgreementView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FloorPlanAgreementView.ToFloorPlanAgreementViews(new List<FloorPlanAgreement> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FloorPlanAgreementViewShowsTheStatusInCapitals();
    }
}
