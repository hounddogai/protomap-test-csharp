namespace Acme.Workforce.Talent.Tests.Mobility.MobilityAudit;

using Acme.Workforce.Talent.Mobility.MobilityAudit;

/// <summary>Checks of mobility audit code, run by the project's runner.</summary>
public static class BatchedMobilityProfileChecks
{
    public static void BatchedMobilityProfileStartsInOpen()
    {
        var item = BatchedMobilityProfile.NewBatchedMobilityProfile("code-378", "label-973");
        Check.Equal(item.Status, BatchedMobilityProfileStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedMobilityProfileMovesToInReview()
    {
        var item = BatchedMobilityProfile.NewBatchedMobilityProfile("code-378", "label-973");
        try
        {
            item.MoveTo(BatchedMobilityProfileStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedMobilityProfileStatus.InReview, "status");
    }

    public static void BatchedMobilityProfileRejectsMovesFromArchived()
    {
        var item = BatchedMobilityProfile.NewBatchedMobilityProfile("code-378", "label-973");
        item.Status = BatchedMobilityProfileStatus.Archived;
        Check.Fails(() => item.MoveTo(BatchedMobilityProfileStatus.Open), "move");
    }

    public static void RunAll()
    {
        BatchedMobilityProfileStartsInOpen();
        BatchedMobilityProfileMovesToInReview();
        BatchedMobilityProfileRejectsMovesFromArchived();
    }
}
