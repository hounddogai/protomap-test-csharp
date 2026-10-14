namespace Acme.Workforce.Absence.Tests.Accrual.Segment;

using Acme.Workforce.Absence.Accrual.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class AccrualThresholdChecks
{
    public static void AccrualThresholdStartsInDraft()
    {
        var item = AccrualThreshold.NewAccrualThreshold("code-111", "name-277");
        Check.Equal(item.Status, AccrualThresholdStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccrualThresholdMovesToSuspended()
    {
        var item = AccrualThreshold.NewAccrualThreshold("code-111", "name-277");
        try
        {
            item.MoveTo(AccrualThresholdStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccrualThresholdStatus.Suspended, "status");
    }

    public static void AccrualThresholdRejectsMovesFromClosed()
    {
        var item = AccrualThreshold.NewAccrualThreshold("code-111", "name-277");
        item.Status = AccrualThresholdStatus.Closed;
        Check.Fails(() => item.MoveTo(AccrualThresholdStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AccrualThresholdStartsInDraft();
        AccrualThresholdMovesToSuspended();
        AccrualThresholdRejectsMovesFromClosed();
    }
}
