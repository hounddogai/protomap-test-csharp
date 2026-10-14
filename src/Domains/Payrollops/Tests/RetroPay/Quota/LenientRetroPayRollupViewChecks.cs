namespace Acme.Workforce.Payrollops.Tests.RetroPay.Quota;

using Acme.Workforce.Payrollops.RetroPay.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class LenientRetroPayRollupViewChecks
{
    public static void LenientRetroPayRollupViewShowsTheStatusInCapitals()
    {
        var item = LenientRetroPayRollup.NewLenientRetroPayRollup("code-201", "title-469");
        var view = LenientRetroPayRollupView.ToLenientRetroPayRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientRetroPayRollupView.ToLenientRetroPayRollupViews(new List<LenientRetroPayRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientRetroPayRollupViewShowsTheStatusInCapitals();
    }
}
