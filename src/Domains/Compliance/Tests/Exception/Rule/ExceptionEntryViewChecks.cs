namespace Acme.Workforce.Compliance.Tests.Exception.Rule;

using Acme.Workforce.Compliance.Exception.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class ExceptionEntryViewChecks
{
    public static void ExceptionEntryViewShowsTheStatusInCapitals()
    {
        var item = ExceptionEntry.NewExceptionEntry("code-104", "title-410");
        var view = ExceptionEntryView.ToExceptionEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ExceptionEntryView.ToExceptionEntryViews(new List<ExceptionEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ExceptionEntryViewShowsTheStatusInCapitals();
    }
}
