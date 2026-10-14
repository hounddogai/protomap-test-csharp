namespace Acme.Workforce.Payrollops.Tests.RetroPay.PaySummary;

using Acme.Workforce.Payrollops.RetroPay.PaySummary;

/// <summary>Checks of pay summary code, run by the project's runner.</summary>
public static class RetroPayBatchChecks
{
    public static void RetroPayBatchStartsInCreated()
    {
        var item = RetroPayBatch.NewRetroPayBatch("code-747", "name-525");
        Check.Equal(item.Status, RetroPayBatchStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RetroPayBatchMovesToValidated()
    {
        var item = RetroPayBatch.NewRetroPayBatch("code-747", "name-525");
        try
        {
            item.MoveTo(RetroPayBatchStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RetroPayBatchStatus.Validated, "status");
    }

    public static void RetroPayBatchRejectsMovesFromRetired()
    {
        var item = RetroPayBatch.NewRetroPayBatch("code-747", "name-525");
        item.Status = RetroPayBatchStatus.Retired;
        Check.Fails(() => item.MoveTo(RetroPayBatchStatus.Created), "move");
    }

    public static void RunAll()
    {
        RetroPayBatchStartsInCreated();
        RetroPayBatchMovesToValidated();
        RetroPayBatchRejectsMovesFromRetired();
    }
}
