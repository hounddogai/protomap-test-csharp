namespace Acme.Workforce.Compliance.Tests.Certification.Slot;

using Acme.Workforce.Compliance.Certification.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class CertificationSlotChecks
{
    public static void CertificationSlotStartsInPlanned()
    {
        var item = CertificationSlot.NewCertificationSlot("code-958", "title-326");
        Check.Equal(item.Status, CertificationSlotStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CertificationSlotMovesToScheduled()
    {
        var item = CertificationSlot.NewCertificationSlot("code-958", "title-326");
        try
        {
            item.MoveTo(CertificationSlotStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CertificationSlotStatus.Scheduled, "status");
    }

    public static void CertificationSlotRejectsMovesFromCancelled()
    {
        var item = CertificationSlot.NewCertificationSlot("code-958", "title-326");
        item.Status = CertificationSlotStatus.Cancelled;
        Check.Fails(() => item.MoveTo(CertificationSlotStatus.Planned), "move");
    }

    public static void RunAll()
    {
        CertificationSlotStartsInPlanned();
        CertificationSlotMovesToScheduled();
        CertificationSlotRejectsMovesFromCancelled();
    }
}
