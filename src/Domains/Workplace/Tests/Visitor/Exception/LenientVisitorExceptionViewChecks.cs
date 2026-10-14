namespace Acme.Workforce.Workplace.Tests.Visitor.Exception;

using Acme.Workforce.Workplace.Visitor.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class LenientVisitorExceptionViewChecks
{
    public static void LenientVisitorExceptionViewShowsTheStatusInCapitals()
    {
        var item = LenientVisitorException.NewLenientVisitorException("code-439", "name-453");
        var view = LenientVisitorExceptionView.ToLenientVisitorExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientVisitorExceptionView.ToLenientVisitorExceptionViews(new List<LenientVisitorException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientVisitorExceptionViewShowsTheStatusInCapitals();
    }
}
