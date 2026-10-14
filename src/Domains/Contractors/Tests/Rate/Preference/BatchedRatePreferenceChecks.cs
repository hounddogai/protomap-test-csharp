namespace Acme.Workforce.Contractors.Tests.Rate.Preference;

using Acme.Workforce.Contractors.Rate.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class BatchedRatePreferenceChecks
{
    public static void BatchedRatePreferenceStartsInIdle()
    {
        var item = BatchedRatePreference.NewBatchedRatePreference("code-914", "title-650");
        Check.Equal(item.Status, BatchedRatePreferenceStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedRatePreferenceMovesToReady()
    {
        var item = BatchedRatePreference.NewBatchedRatePreference("code-914", "title-650");
        try
        {
            item.MoveTo(BatchedRatePreferenceStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedRatePreferenceStatus.Ready, "status");
    }

    public static void BatchedRatePreferenceRejectsMovesFromStopped()
    {
        var item = BatchedRatePreference.NewBatchedRatePreference("code-914", "title-650");
        item.Status = BatchedRatePreferenceStatus.Stopped;
        Check.Fails(() => item.MoveTo(BatchedRatePreferenceStatus.Idle), "move");
    }

    public static void RunAll()
    {
        BatchedRatePreferenceStartsInIdle();
        BatchedRatePreferenceMovesToReady();
        BatchedRatePreferenceRejectsMovesFromStopped();
    }
}
