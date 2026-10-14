namespace Acme.Workforce.Compliance.Tests.Certification.Notice;

using Acme.Workforce.Compliance.Certification.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class WeightedCertificationBatchChecks
{
    public static void WeightedCertificationBatchStartsInIdle()
    {
        var item = WeightedCertificationBatch.NewWeightedCertificationBatch("code-963", "title-448");
        Check.Equal(item.Status, WeightedCertificationBatchStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedCertificationBatchMovesToStopped()
    {
        var item = WeightedCertificationBatch.NewWeightedCertificationBatch("code-963", "title-448");
        try
        {
            item.MoveTo(WeightedCertificationBatchStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedCertificationBatchStatus.Stopped, "status");
    }

    public static void WeightedCertificationBatchRejectsMovesFromStopped()
    {
        var item = WeightedCertificationBatch.NewWeightedCertificationBatch("code-963", "title-448");
        item.Status = WeightedCertificationBatchStatus.Stopped;
        Check.Fails(() => item.MoveTo(WeightedCertificationBatchStatus.Idle), "move");
    }

    public static void RunAll()
    {
        WeightedCertificationBatchStartsInIdle();
        WeightedCertificationBatchMovesToStopped();
        WeightedCertificationBatchRejectsMovesFromStopped();
    }
}
