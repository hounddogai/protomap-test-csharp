namespace Acme.Workforce.Compliance.Tests.Exception.Template;

using Acme.Workforce.Compliance.Exception.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class LenientExceptionVarianceViewChecks
{
    public static void LenientExceptionVarianceViewShowsTheStatusInCapitals()
    {
        var item = LenientExceptionVariance.NewLenientExceptionVariance("code-37", "label-18");
        var view = LenientExceptionVarianceView.ToLenientExceptionVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientExceptionVarianceView.ToLenientExceptionVarianceViews(new List<LenientExceptionVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientExceptionVarianceViewShowsTheStatusInCapitals();
    }
}
