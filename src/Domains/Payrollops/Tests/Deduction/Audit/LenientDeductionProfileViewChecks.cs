namespace Acme.Workforce.Payrollops.Tests.Deduction.Audit;

using Acme.Workforce.Payrollops.Deduction.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class LenientDeductionProfileViewChecks
{
    public static void LenientDeductionProfileViewShowsTheStatusInCapitals()
    {
        var item = LenientDeductionProfile.NewLenientDeductionProfile("code-942", "name-331");
        var view = LenientDeductionProfileView.ToLenientDeductionProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientDeductionProfileView.ToLenientDeductionProfileViews(new List<LenientDeductionProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientDeductionProfileViewShowsTheStatusInCapitals();
    }
}
