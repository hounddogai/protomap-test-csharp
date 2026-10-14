namespace Acme.Workforce.Contractors.Tests.Offboarding.Notice;

using Acme.Workforce.Contractors.Offboarding.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class BatchedOffboardingBaselineChecks
{
    public static void BatchedOffboardingBaselineStartsInDraft()
    {
        var item = BatchedOffboardingBaseline.NewBatchedOffboardingBaseline("code-566", "label-210");
        Check.Equal(item.Status, BatchedOffboardingBaselineStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedOffboardingBaselineMovesToPending()
    {
        var item = BatchedOffboardingBaseline.NewBatchedOffboardingBaseline("code-566", "label-210");
        try
        {
            item.MoveTo(BatchedOffboardingBaselineStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedOffboardingBaselineStatus.Pending, "status");
    }

    public static void BatchedOffboardingBaselineRejectsMovesFromClosed()
    {
        var item = BatchedOffboardingBaseline.NewBatchedOffboardingBaseline("code-566", "label-210");
        item.Status = BatchedOffboardingBaselineStatus.Closed;
        Check.Fails(() => item.MoveTo(BatchedOffboardingBaselineStatus.Draft), "move");
    }

    public static void RunAll()
    {
        BatchedOffboardingBaselineStartsInDraft();
        BatchedOffboardingBaselineMovesToPending();
        BatchedOffboardingBaselineRejectsMovesFromClosed();
    }
}
