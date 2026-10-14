namespace Acme.Workforce.Compliance.Tests.Exception.Rule;

using Acme.Workforce.Compliance.Exception.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class ExceptionSegmentViewChecks
{
    public static void ExceptionSegmentViewShowsTheStatusInCapitals()
    {
        var item = ExceptionSegment.NewExceptionSegment("code-148", "title-601");
        var view = ExceptionSegmentView.ToExceptionSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ExceptionSegmentView.ToExceptionSegmentViews(new List<ExceptionSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ExceptionSegmentViewShowsTheStatusInCapitals();
    }
}
