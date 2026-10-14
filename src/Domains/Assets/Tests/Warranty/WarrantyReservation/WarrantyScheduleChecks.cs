namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyReservation;

using Acme.Workforce.Assets.Warranty.WarrantyReservation;

/// <summary>Checks of warranty reservation code, run by the project's runner.</summary>
public static class WarrantyScheduleChecks
{
    public static void WarrantyScheduleStartsInProposed()
    {
        var item = WarrantySchedule.NewWarrantySchedule("code-377", "label-751");
        Check.Equal(item.Status, WarrantyScheduleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WarrantyScheduleMovesToRejected()
    {
        var item = WarrantySchedule.NewWarrantySchedule("code-377", "label-751");
        try
        {
            item.MoveTo(WarrantyScheduleStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WarrantyScheduleStatus.Rejected, "status");
    }

    public static void WarrantyScheduleRejectsMovesFromWithdrawn()
    {
        var item = WarrantySchedule.NewWarrantySchedule("code-377", "label-751");
        item.Status = WarrantyScheduleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(WarrantyScheduleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        WarrantyScheduleStartsInProposed();
        WarrantyScheduleMovesToRejected();
        WarrantyScheduleRejectsMovesFromWithdrawn();
    }
}
