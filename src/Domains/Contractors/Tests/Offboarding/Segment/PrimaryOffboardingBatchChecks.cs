namespace Acme.Workforce.Contractors.Tests.Offboarding.Segment;

using Acme.Workforce.Contractors.Offboarding.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class PrimaryOffboardingBatchChecks
{
    public static void PrimaryOffboardingBatchStartsInDraft()
    {
        var item = PrimaryOffboardingBatch.NewPrimaryOffboardingBatch("code-824", "name-368");
        Check.Equal(item.Status, PrimaryOffboardingBatchStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryOffboardingBatchMovesToSuspended()
    {
        var item = PrimaryOffboardingBatch.NewPrimaryOffboardingBatch("code-824", "name-368");
        try
        {
            item.MoveTo(PrimaryOffboardingBatchStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryOffboardingBatchStatus.Suspended, "status");
    }

    public static void PrimaryOffboardingBatchRejectsMovesFromClosed()
    {
        var item = PrimaryOffboardingBatch.NewPrimaryOffboardingBatch("code-824", "name-368");
        item.Status = PrimaryOffboardingBatchStatus.Closed;
        Check.Fails(() => item.MoveTo(PrimaryOffboardingBatchStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PrimaryOffboardingBatchStartsInDraft();
        PrimaryOffboardingBatchMovesToSuspended();
        PrimaryOffboardingBatchRejectsMovesFromClosed();
    }
}
