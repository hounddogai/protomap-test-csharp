namespace Acme.Workforce.Compliance.Tests.Certification.Schedule;

using Acme.Workforce.Compliance.Certification.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class CertificationScheduleChecks
{
    public static void CertificationScheduleStartsInPlanned()
    {
        var item = CertificationSchedule.NewCertificationSchedule("code-671", "title-311");
        Check.Equal(item.Status, CertificationScheduleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CertificationScheduleMovesToInProgress()
    {
        var item = CertificationSchedule.NewCertificationSchedule("code-671", "title-311");
        try
        {
            item.MoveTo(CertificationScheduleStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CertificationScheduleStatus.InProgress, "status");
    }

    public static void CertificationScheduleRejectsMovesFromCancelled()
    {
        var item = CertificationSchedule.NewCertificationSchedule("code-671", "title-311");
        item.Status = CertificationScheduleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(CertificationScheduleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        CertificationScheduleStartsInPlanned();
        CertificationScheduleMovesToInProgress();
        CertificationScheduleRejectsMovesFromCancelled();
    }
}
