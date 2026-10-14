namespace Acme.Workforce.Payrollops.Tests.RetroPay.Assignment;

using Acme.Workforce.Payrollops.RetroPay.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class RetroPaySegmentViewChecks
{
    public static void RetroPaySegmentViewShowsTheStatusInCapitals()
    {
        var item = RetroPaySegment.NewRetroPaySegment("code-425", "title-253");
        var view = RetroPaySegmentView.ToRetroPaySegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RetroPaySegmentView.ToRetroPaySegmentViews(new List<RetroPaySegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RetroPaySegmentViewShowsTheStatusInCapitals();
    }
}
