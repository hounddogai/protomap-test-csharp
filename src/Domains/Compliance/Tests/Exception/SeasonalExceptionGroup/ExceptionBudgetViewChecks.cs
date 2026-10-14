namespace Acme.Workforce.Compliance.Tests.Exception.SeasonalExceptionGroup;

using Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

/// <summary>Checks of seasonal exception group code, run by the project's runner.</summary>
public static class ExceptionBudgetViewChecks
{
    public static void ExceptionBudgetViewShowsTheStatusInCapitals()
    {
        var item = ExceptionBudget.NewExceptionBudget("code-807", "name-294");
        var view = ExceptionBudgetView.ToExceptionBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ExceptionBudgetView.ToExceptionBudgetViews(new List<ExceptionBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ExceptionBudgetViewShowsTheStatusInCapitals();
    }
}
