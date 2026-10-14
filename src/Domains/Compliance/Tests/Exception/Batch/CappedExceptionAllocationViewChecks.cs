namespace Acme.Workforce.Compliance.Tests.Exception.Batch;

using Acme.Workforce.Compliance.Exception.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class CappedExceptionAllocationViewChecks
{
    public static void CappedExceptionAllocationViewShowsTheStatusInCapitals()
    {
        var item = CappedExceptionAllocation.NewCappedExceptionAllocation("code-409", "label-55");
        var view = CappedExceptionAllocationView.ToCappedExceptionAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedExceptionAllocationView.ToCappedExceptionAllocationViews(new List<CappedExceptionAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedExceptionAllocationViewShowsTheStatusInCapitals();
    }
}
