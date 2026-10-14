namespace Acme.Workforce.Payrollops.Tests.RetroPay.Bundle;

using Acme.Workforce.Payrollops.RetroPay.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class RetroPayBundleViewChecks
{
    public static void RetroPayBundleViewShowsTheStatusInCapitals()
    {
        var item = RetroPayBundle.NewRetroPayBundle("code-138", "label-448");
        var view = RetroPayBundleView.ToRetroPayBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RetroPayBundleView.ToRetroPayBundleViews(new List<RetroPayBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RetroPayBundleViewShowsTheStatusInCapitals();
    }
}
