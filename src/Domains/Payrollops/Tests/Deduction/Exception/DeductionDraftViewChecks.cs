namespace Acme.Workforce.Payrollops.Tests.Deduction.Exception;

using Acme.Workforce.Payrollops.Deduction.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class DeductionDraftViewChecks
{
    public static void DeductionDraftViewShowsTheStatusInCapitals()
    {
        var item = DeductionDraft.NewDeductionDraft("code-225", "title-422");
        var view = DeductionDraftView.ToDeductionDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DeductionDraftView.ToDeductionDraftViews(new List<DeductionDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DeductionDraftViewShowsTheStatusInCapitals();
    }
}
