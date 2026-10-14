namespace Acme.Workforce.Absence.Tests.Accrual.Batch;

using Acme.Workforce.Absence.Accrual.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class AccrualEstimateViewChecks
{
    public static void AccrualEstimateViewShowsTheStatusInCapitals()
    {
        var item = AccrualEstimate.NewAccrualEstimate("code-344", "name-374");
        var view = AccrualEstimateView.ToAccrualEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AccrualEstimateView.ToAccrualEstimateViews(new List<AccrualEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AccrualEstimateViewShowsTheStatusInCapitals();
    }
}
