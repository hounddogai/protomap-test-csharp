namespace Acme.Workforce.Payrollops.Tests.Deduction.Metric;

using Acme.Workforce.Payrollops.Deduction.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class CappedDeductionPolicyViewChecks
{
    public static void CappedDeductionPolicyViewShowsTheStatusInCapitals()
    {
        var item = CappedDeductionPolicy.NewCappedDeductionPolicy("code-743", "name-96");
        var view = CappedDeductionPolicyView.ToCappedDeductionPolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedDeductionPolicyView.ToCappedDeductionPolicyViews(new List<CappedDeductionPolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedDeductionPolicyViewShowsTheStatusInCapitals();
    }
}
