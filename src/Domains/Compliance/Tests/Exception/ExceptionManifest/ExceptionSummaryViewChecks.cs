namespace Acme.Workforce.Compliance.Tests.Exception.ExceptionManifest;

using Acme.Workforce.Compliance.Exception.ExceptionManifest;

/// <summary>Checks of exception manifest code, run by the project's runner.</summary>
public static class ExceptionSummaryViewChecks
{
    public static void ExceptionSummaryViewShowsTheStatusInCapitals()
    {
        var item = ExceptionSummary.NewExceptionSummary("code-451", "name-529");
        var view = ExceptionSummaryView.ToExceptionSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ExceptionSummaryView.ToExceptionSummaryViews(new List<ExceptionSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ExceptionSummaryViewShowsTheStatusInCapitals();
    }
}
