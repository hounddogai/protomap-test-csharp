namespace Acme.Workforce.Contractors.Tests.Offboarding.Window;

using Acme.Workforce.Contractors.Offboarding.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class OffboardingWindowViewChecks
{
    public static void OffboardingWindowViewShowsTheStatusInCapitals()
    {
        var item = OffboardingWindow.NewOffboardingWindow("code-383", "title-748");
        var view = OffboardingWindowView.ToOffboardingWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(OffboardingWindowView.ToOffboardingWindowViews(new List<OffboardingWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        OffboardingWindowViewShowsTheStatusInCapitals();
    }
}
