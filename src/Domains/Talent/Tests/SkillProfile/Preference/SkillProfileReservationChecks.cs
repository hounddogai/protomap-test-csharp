namespace Acme.Workforce.Talent.Tests.SkillProfile.Preference;

using Acme.Workforce.Talent.SkillProfile.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class SkillProfileReservationChecks
{
    public static void SkillProfileReservationStartsInPlanned()
    {
        var item = SkillProfileReservation.NewSkillProfileReservation("code-761", "title-23");
        Check.Equal(item.Status, SkillProfileReservationStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SkillProfileReservationMovesToCancelled()
    {
        var item = SkillProfileReservation.NewSkillProfileReservation("code-761", "title-23");
        try
        {
            item.MoveTo(SkillProfileReservationStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SkillProfileReservationStatus.Cancelled, "status");
    }

    public static void SkillProfileReservationRejectsMovesFromCancelled()
    {
        var item = SkillProfileReservation.NewSkillProfileReservation("code-761", "title-23");
        item.Status = SkillProfileReservationStatus.Cancelled;
        Check.Fails(() => item.MoveTo(SkillProfileReservationStatus.Planned), "move");
    }

    public static void RunAll()
    {
        SkillProfileReservationStartsInPlanned();
        SkillProfileReservationMovesToCancelled();
        SkillProfileReservationRejectsMovesFromCancelled();
    }
}
