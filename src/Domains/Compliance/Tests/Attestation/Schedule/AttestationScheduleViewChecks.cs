namespace Acme.Workforce.Compliance.Tests.Attestation.Schedule;

using Acme.Workforce.Compliance.Attestation.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class AttestationScheduleViewChecks
{
    public static void AttestationScheduleViewShowsTheStatusInCapitals()
    {
        var item = AttestationSchedule.NewAttestationSchedule("code-150", "name-697");
        var view = AttestationScheduleView.ToAttestationScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AttestationScheduleView.ToAttestationScheduleViews(new List<AttestationSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AttestationScheduleViewShowsTheStatusInCapitals();
    }
}
