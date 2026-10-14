namespace Acme.Workforce.Payrollops.Tests.RetroPay.Cycle;

using Acme.Workforce.Payrollops.RetroPay.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class RetroPayCycleViewChecks
{
    public static void RetroPayCycleViewShowsTheStatusInCapitals()
    {
        var item = RetroPayCycle.NewRetroPayCycle("code-485", "name-145");
        var view = RetroPayCycleView.ToRetroPayCycleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RetroPayCycleView.ToRetroPayCycleViews(new List<RetroPayCycle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RetroPayCycleViewShowsTheStatusInCapitals();
    }
}
