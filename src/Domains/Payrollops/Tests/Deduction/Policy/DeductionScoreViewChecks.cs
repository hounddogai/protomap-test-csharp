namespace Acme.Workforce.Payrollops.Tests.Deduction.Policy;

using Acme.Workforce.Payrollops.Deduction.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class DeductionScoreViewChecks
{
    public static void DeductionScoreViewShowsTheStatusInCapitals()
    {
        var item = DeductionScore.NewDeductionScore("code-484", "title-776");
        var view = DeductionScoreView.ToDeductionScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DeductionScoreView.ToDeductionScoreViews(new List<DeductionScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DeductionScoreViewShowsTheStatusInCapitals();
    }
}
