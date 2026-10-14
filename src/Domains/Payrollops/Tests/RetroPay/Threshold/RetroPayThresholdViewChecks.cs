namespace Acme.Workforce.Payrollops.Tests.RetroPay.Threshold;

using Acme.Workforce.Payrollops.RetroPay.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class RetroPayThresholdViewChecks
{
    public static void RetroPayThresholdViewShowsTheStatusInCapitals()
    {
        var item = RetroPayThreshold.NewRetroPayThreshold("code-950", "label-496");
        var view = RetroPayThresholdView.ToRetroPayThresholdView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RetroPayThresholdView.ToRetroPayThresholdViews(new List<RetroPayThreshold> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RetroPayThresholdViewShowsTheStatusInCapitals();
    }
}
