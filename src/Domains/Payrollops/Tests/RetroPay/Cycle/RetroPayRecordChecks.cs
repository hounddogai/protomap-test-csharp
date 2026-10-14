namespace Acme.Workforce.Payrollops.Tests.RetroPay.Cycle;

using Acme.Workforce.Payrollops.RetroPay.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class RetroPayRecordChecks
{
    public static void RetroPayRecordStartsInDraft()
    {
        var item = RetroPayRecord.NewRetroPayRecord("code-873", "name-777");
        Check.Equal(item.Status, RetroPayRecordStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RetroPayRecordMovesToActive()
    {
        var item = RetroPayRecord.NewRetroPayRecord("code-873", "name-777");
        try
        {
            item.MoveTo(RetroPayRecordStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RetroPayRecordStatus.Active, "status");
    }

    public static void RetroPayRecordRejectsMovesFromClosed()
    {
        var item = RetroPayRecord.NewRetroPayRecord("code-873", "name-777");
        item.Status = RetroPayRecordStatus.Closed;
        Check.Fails(() => item.MoveTo(RetroPayRecordStatus.Draft), "move");
    }

    public static void RunAll()
    {
        RetroPayRecordStartsInDraft();
        RetroPayRecordMovesToActive();
        RetroPayRecordRejectsMovesFromClosed();
    }
}
