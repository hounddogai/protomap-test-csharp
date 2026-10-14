namespace Acme.Workforce.Compliance.Tests.Exception.Profile;

using Acme.Workforce.Compliance.Exception.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class ExceptionProfileViewChecks
{
    public static void ExceptionProfileViewShowsTheStatusInCapitals()
    {
        var item = ExceptionProfile.NewExceptionProfile("code-888", "name-435");
        var view = ExceptionProfileView.ToExceptionProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ExceptionProfileView.ToExceptionProfileViews(new List<ExceptionProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ExceptionProfileViewShowsTheStatusInCapitals();
    }
}
