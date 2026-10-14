namespace Acme.Workforce.Payrollops.Tests.PayRun.Allocation;

using Acme.Workforce.Payrollops.PayRun.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class PayRunRequestChecks
{
    public static void PayRunRequestStartsInNew()
    {
        var item = PayRunRequest.NewPayRunRequest("code-666", "label-152");
        Check.Equal(item.Status, PayRunRequestStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayRunRequestMovesToQueued()
    {
        var item = PayRunRequest.NewPayRunRequest("code-666", "label-152");
        try
        {
            item.MoveTo(PayRunRequestStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayRunRequestStatus.Queued, "status");
    }

    public static void PayRunRequestRejectsMovesFromFailed()
    {
        var item = PayRunRequest.NewPayRunRequest("code-666", "label-152");
        item.Status = PayRunRequestStatus.Failed;
        Check.Fails(() => item.MoveTo(PayRunRequestStatus.New), "move");
    }

    public static void RunAll()
    {
        PayRunRequestStartsInNew();
        PayRunRequestMovesToQueued();
        PayRunRequestRejectsMovesFromFailed();
    }
}
