namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyAgreement;

using Acme.Workforce.Workplace.Occupancy.OccupancyAgreement;

/// <summary>Checks of occupancy agreement code, run by the project's runner.</summary>
public static class AdaptiveOccupancyAgreementViewChecks
{
    public static void AdaptiveOccupancyAgreementViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveOccupancyAgreement.NewAdaptiveOccupancyAgreement("code-37", "name-725");
        var view = AdaptiveOccupancyAgreementView.ToAdaptiveOccupancyAgreementView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveOccupancyAgreementView.ToAdaptiveOccupancyAgreementViews(new List<AdaptiveOccupancyAgreement> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveOccupancyAgreementViewShowsTheStatusInCapitals();
    }
}
