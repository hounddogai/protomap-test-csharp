namespace Acme.Workforce.Payrollops.Tests.Deduction.Profile;

using Acme.Workforce.Payrollops.Deduction.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class RegionalDeductionBatchViewChecks
{
    public static void RegionalDeductionBatchViewShowsTheStatusInCapitals()
    {
        var item = RegionalDeductionBatch.NewRegionalDeductionBatch("code-563", "name-304");
        var view = RegionalDeductionBatchView.ToRegionalDeductionBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalDeductionBatchView.ToRegionalDeductionBatchViews(new List<RegionalDeductionBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalDeductionBatchViewShowsTheStatusInCapitals();
    }
}
