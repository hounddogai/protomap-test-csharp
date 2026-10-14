namespace Acme.Workforce.Compliance.Tests.Exception.Baseline;

using Acme.Workforce.Compliance.Exception.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class ExceptionBaselineViewChecks
{
    public static void ExceptionBaselineViewShowsTheStatusInCapitals()
    {
        var item = ExceptionBaseline.NewExceptionBaseline("code-920", "label-873");
        var view = ExceptionBaselineView.ToExceptionBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ExceptionBaselineView.ToExceptionBaselineViews(new List<ExceptionBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ExceptionBaselineViewShowsTheStatusInCapitals();
    }
}
