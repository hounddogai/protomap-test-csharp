namespace Acme.Workforce.Payrollops.Tests.RetroPay.Assignment;

using Acme.Workforce.Payrollops.RetroPay.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class LenientRetroPayPreferenceViewChecks
{
    public static void LenientRetroPayPreferenceViewShowsTheStatusInCapitals()
    {
        var item = LenientRetroPayPreference.NewLenientRetroPayPreference("code-758", "title-788");
        var view = LenientRetroPayPreferenceView.ToLenientRetroPayPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientRetroPayPreferenceView.ToLenientRetroPayPreferenceViews(new List<LenientRetroPayPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientRetroPayPreferenceViewShowsTheStatusInCapitals();
    }
}
