namespace Acme.Workforce.Payrollops.Tests.TaxForm.Reservation;

using Acme.Workforce.Payrollops.TaxForm.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class BatchedTaxFormAssignmentViewChecks
{
    public static void BatchedTaxFormAssignmentViewShowsTheStatusInCapitals()
    {
        var item = BatchedTaxFormAssignment.NewBatchedTaxFormAssignment("code-702", "title-666");
        var view = BatchedTaxFormAssignmentView.ToBatchedTaxFormAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedTaxFormAssignmentView.ToBatchedTaxFormAssignmentViews(new List<BatchedTaxFormAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedTaxFormAssignmentViewShowsTheStatusInCapitals();
    }
}
