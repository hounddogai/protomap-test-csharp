namespace Acme.Workforce.Payrollops.Tests.Deduction.Policy;

using Acme.Workforce.Payrollops.Deduction.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class GlobalDeductionPolicyViewChecks
{
    public static void GlobalDeductionPolicyViewShowsTheStatusInCapitals()
    {
        var item = GlobalDeductionPolicy.NewGlobalDeductionPolicy("code-979", "name-114");
        var view = GlobalDeductionPolicyView.ToGlobalDeductionPolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalDeductionPolicyView.ToGlobalDeductionPolicyViews(new List<GlobalDeductionPolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalDeductionPolicyViewShowsTheStatusInCapitals();
    }
}
