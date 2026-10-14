namespace Acme.Workforce.Payrollops.Tests.Payslip.Target;

using Acme.Workforce.Payrollops.Payslip.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class CappedPayslipRuleViewChecks
{
    public static void CappedPayslipRuleViewShowsTheStatusInCapitals()
    {
        var item = CappedPayslipRule.NewCappedPayslipRule("code-44", "title-484");
        var view = CappedPayslipRuleView.ToCappedPayslipRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedPayslipRuleView.ToCappedPayslipRuleViews(new List<CappedPayslipRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedPayslipRuleViewShowsTheStatusInCapitals();
    }
}
