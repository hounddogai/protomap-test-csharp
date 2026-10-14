namespace Acme.Workforce.Talent.Tests.Nomination.Reservation;

using Acme.Workforce.Talent.Nomination.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class NominationAdjustmentViewChecks
{
    public static void NominationAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = NominationAdjustment.NewNominationAdjustment("code-988", "title-208");
        var view = NominationAdjustmentView.ToNominationAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(NominationAdjustmentView.ToNominationAdjustmentViews(new List<NominationAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        NominationAdjustmentViewShowsTheStatusInCapitals();
    }
}
