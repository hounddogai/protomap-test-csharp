namespace Acme.Workforce.Talent.Tests.Nomination.NominationReservation;

using Acme.Workforce.Talent.Nomination.NominationReservation;

/// <summary>Checks of nomination reservation code, run by the project's runner.</summary>
public static class BatchedNominationReservationChecks
{
    public static void BatchedNominationReservationStartsInNew()
    {
        var item = BatchedNominationReservation.NewBatchedNominationReservation("code-847", "title-809");
        Check.Equal(item.Status, BatchedNominationReservationStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedNominationReservationMovesToFailed()
    {
        var item = BatchedNominationReservation.NewBatchedNominationReservation("code-847", "title-809");
        try
        {
            item.MoveTo(BatchedNominationReservationStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedNominationReservationStatus.Failed, "status");
    }

    public static void BatchedNominationReservationRejectsMovesFromFailed()
    {
        var item = BatchedNominationReservation.NewBatchedNominationReservation("code-847", "title-809");
        item.Status = BatchedNominationReservationStatus.Failed;
        Check.Fails(() => item.MoveTo(BatchedNominationReservationStatus.New), "move");
    }

    public static void RunAll()
    {
        BatchedNominationReservationStartsInNew();
        BatchedNominationReservationMovesToFailed();
        BatchedNominationReservationRejectsMovesFromFailed();
    }
}
