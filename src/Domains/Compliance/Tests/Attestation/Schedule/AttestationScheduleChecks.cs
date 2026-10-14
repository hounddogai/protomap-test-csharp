namespace Acme.Workforce.Compliance.Tests.Attestation.Schedule;

using Acme.Workforce.Compliance.Attestation.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class AttestationScheduleChecks
{
    public static void AttestationScheduleStartsInCreated()
    {
        var item = AttestationSchedule.NewAttestationSchedule("code-150", "name-697");
        Check.Equal(item.Status, AttestationScheduleStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AttestationScheduleMovesToValidated()
    {
        var item = AttestationSchedule.NewAttestationSchedule("code-150", "name-697");
        try
        {
            item.MoveTo(AttestationScheduleStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AttestationScheduleStatus.Validated, "status");
    }

    public static void AttestationScheduleRejectsMovesFromRetired()
    {
        var item = AttestationSchedule.NewAttestationSchedule("code-150", "name-697");
        item.Status = AttestationScheduleStatus.Retired;
        Check.Fails(() => item.MoveTo(AttestationScheduleStatus.Created), "move");
    }

    public static void RunAll()
    {
        AttestationScheduleStartsInCreated();
        AttestationScheduleMovesToValidated();
        AttestationScheduleRejectsMovesFromRetired();
    }
}
