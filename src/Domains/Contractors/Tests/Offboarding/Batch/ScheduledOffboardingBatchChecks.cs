namespace Acme.Workforce.Contractors.Tests.Offboarding.Batch;

using Acme.Workforce.Contractors.Offboarding.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class ScheduledOffboardingBatchChecks
{
    public static void ScheduledOffboardingBatchStartsInCreated()
    {
        var item = ScheduledOffboardingBatch.NewScheduledOffboardingBatch("code-893", "name-823");
        Check.Equal(item.Status, ScheduledOffboardingBatchStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledOffboardingBatchMovesToValidated()
    {
        var item = ScheduledOffboardingBatch.NewScheduledOffboardingBatch("code-893", "name-823");
        try
        {
            item.MoveTo(ScheduledOffboardingBatchStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledOffboardingBatchStatus.Validated, "status");
    }

    public static void ScheduledOffboardingBatchRejectsMovesFromRetired()
    {
        var item = ScheduledOffboardingBatch.NewScheduledOffboardingBatch("code-893", "name-823");
        item.Status = ScheduledOffboardingBatchStatus.Retired;
        Check.Fails(() => item.MoveTo(ScheduledOffboardingBatchStatus.Created), "move");
    }

    public static void RunAll()
    {
        ScheduledOffboardingBatchStartsInCreated();
        ScheduledOffboardingBatchMovesToValidated();
        ScheduledOffboardingBatchRejectsMovesFromRetired();
    }
}
