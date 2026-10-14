namespace Acme.Workforce.Workplace.Tests.Occupancy.Agreement;

using Acme.Workforce.Workplace.Occupancy.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class OccupancyAgreementViewChecks
{
    public static void OccupancyAgreementViewShowsTheStatusInCapitals()
    {
        var item = OccupancyAgreement.NewOccupancyAgreement("code-531", "title-400");
        var view = OccupancyAgreementView.ToOccupancyAgreementView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(OccupancyAgreementView.ToOccupancyAgreementViews(new List<OccupancyAgreement> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        OccupancyAgreementViewShowsTheStatusInCapitals();
    }
}
