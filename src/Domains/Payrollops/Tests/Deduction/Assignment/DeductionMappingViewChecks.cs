namespace Acme.Workforce.Payrollops.Tests.Deduction.Assignment;

using Acme.Workforce.Payrollops.Deduction.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class DeductionMappingViewChecks
{
    public static void DeductionMappingViewShowsTheStatusInCapitals()
    {
        var item = DeductionMapping.NewDeductionMapping("code-289", "label-7");
        var view = DeductionMappingView.ToDeductionMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DeductionMappingView.ToDeductionMappingViews(new List<DeductionMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DeductionMappingViewShowsTheStatusInCapitals();
    }
}
