namespace Acme.Workforce.Talent.Tests.SkillProfile.Preference;

using Acme.Workforce.Talent.SkillProfile.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class SkillProfileReservationViewChecks
{
    public static void SkillProfileReservationViewShowsTheStatusInCapitals()
    {
        var item = SkillProfileReservation.NewSkillProfileReservation("code-761", "title-23");
        var view = SkillProfileReservationView.ToSkillProfileReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SkillProfileReservationView.ToSkillProfileReservationViews(new List<SkillProfileReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SkillProfileReservationViewShowsTheStatusInCapitals();
    }
}
