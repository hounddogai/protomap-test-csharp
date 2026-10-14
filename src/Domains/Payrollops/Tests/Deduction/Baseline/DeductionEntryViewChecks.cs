namespace Acme.Workforce.Payrollops.Tests.Deduction.Baseline;

using Acme.Workforce.Payrollops.Deduction.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class DeductionEntryViewChecks
{
    public static void DeductionEntryViewShowsTheStatusInCapitals()
    {
        var item = DeductionEntry.NewDeductionEntry("code-439", "label-275");
        var view = DeductionEntryView.ToDeductionEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DeductionEntryView.ToDeductionEntryViews(new List<DeductionEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DeductionEntryViewShowsTheStatusInCapitals();
    }
}
