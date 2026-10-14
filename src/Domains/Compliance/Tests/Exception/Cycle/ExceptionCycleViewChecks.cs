namespace Acme.Workforce.Compliance.Tests.Exception.Cycle;

using Acme.Workforce.Compliance.Exception.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class ExceptionCycleViewChecks
{
    public static void ExceptionCycleViewShowsTheStatusInCapitals()
    {
        var item = ExceptionCycle.NewExceptionCycle("code-540", "label-489");
        var view = ExceptionCycleView.ToExceptionCycleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ExceptionCycleView.ToExceptionCycleViews(new List<ExceptionCycle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ExceptionCycleViewShowsTheStatusInCapitals();
    }
}
