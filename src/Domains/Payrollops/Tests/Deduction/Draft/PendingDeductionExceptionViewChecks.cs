namespace Acme.Workforce.Payrollops.Tests.Deduction.Draft;

using Acme.Workforce.Payrollops.Deduction.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class PendingDeductionExceptionViewChecks
{
    public static void PendingDeductionExceptionViewShowsTheStatusInCapitals()
    {
        var item = PendingDeductionException.NewPendingDeductionException("code-142", "label-947");
        var view = PendingDeductionExceptionView.ToPendingDeductionExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingDeductionExceptionView.ToPendingDeductionExceptionViews(new List<PendingDeductionException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingDeductionExceptionViewShowsTheStatusInCapitals();
    }
}
