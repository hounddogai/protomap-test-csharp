namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Threshold;

using Acme.Workforce.Contractors.StatementOfWork.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class FallbackStatementOfWorkSegmentViewChecks
{
    public static void FallbackStatementOfWorkSegmentViewShowsTheStatusInCapitals()
    {
        var item = FallbackStatementOfWorkSegment.NewFallbackStatementOfWorkSegment("code-187", "name-726");
        var view = FallbackStatementOfWorkSegmentView.ToFallbackStatementOfWorkSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackStatementOfWorkSegmentView.ToFallbackStatementOfWorkSegmentViews(new List<FallbackStatementOfWorkSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackStatementOfWorkSegmentViewShowsTheStatusInCapitals();
    }
}
