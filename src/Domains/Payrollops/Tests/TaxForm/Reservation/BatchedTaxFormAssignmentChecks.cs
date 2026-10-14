namespace Acme.Workforce.Payrollops.Tests.TaxForm.Reservation;

using Acme.Workforce.Payrollops.TaxForm.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class BatchedTaxFormAssignmentChecks
{
    public static void BatchedTaxFormAssignmentStartsInRequested()
    {
        var item = BatchedTaxFormAssignment.NewBatchedTaxFormAssignment("code-702", "title-666");
        Check.Equal(item.Status, BatchedTaxFormAssignmentStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedTaxFormAssignmentMovesToRevoked()
    {
        var item = BatchedTaxFormAssignment.NewBatchedTaxFormAssignment("code-702", "title-666");
        try
        {
            item.MoveTo(BatchedTaxFormAssignmentStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedTaxFormAssignmentStatus.Revoked, "status");
    }

    public static void BatchedTaxFormAssignmentRejectsMovesFromExpired()
    {
        var item = BatchedTaxFormAssignment.NewBatchedTaxFormAssignment("code-702", "title-666");
        item.Status = BatchedTaxFormAssignmentStatus.Expired;
        Check.Fails(() => item.MoveTo(BatchedTaxFormAssignmentStatus.Requested), "move");
    }

    public static void RunAll()
    {
        BatchedTaxFormAssignmentStartsInRequested();
        BatchedTaxFormAssignmentMovesToRevoked();
        BatchedTaxFormAssignmentRejectsMovesFromExpired();
    }
}
