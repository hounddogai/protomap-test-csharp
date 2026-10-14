namespace Acme.Workforce.Orgdesign.Tests.Span.Claim;

using Acme.Workforce.Orgdesign.Span.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class RollingSpanExceptionViewChecks
{
    public static void RollingSpanExceptionViewShowsTheStatusInCapitals()
    {
        var item = RollingSpanException.NewRollingSpanException("code-811", "title-160");
        var view = RollingSpanExceptionView.ToRollingSpanExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingSpanExceptionView.ToRollingSpanExceptionViews(new List<RollingSpanException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingSpanExceptionViewShowsTheStatusInCapitals();
    }
}
