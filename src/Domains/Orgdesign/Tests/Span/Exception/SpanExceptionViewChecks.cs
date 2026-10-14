namespace Acme.Workforce.Orgdesign.Tests.Span.Exception;

using Acme.Workforce.Orgdesign.Span.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class SpanExceptionViewChecks
{
    public static void SpanExceptionViewShowsTheStatusInCapitals()
    {
        var item = SpanException.NewSpanException("code-887", "label-725");
        var view = SpanExceptionView.ToSpanExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SpanExceptionView.ToSpanExceptionViews(new List<SpanException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SpanExceptionViewShowsTheStatusInCapitals();
    }
}
