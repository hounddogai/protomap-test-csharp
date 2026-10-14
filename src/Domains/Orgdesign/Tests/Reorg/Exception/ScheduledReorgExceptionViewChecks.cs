namespace Acme.Workforce.Orgdesign.Tests.Reorg.Exception;

using Acme.Workforce.Orgdesign.Reorg.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class ScheduledReorgExceptionViewChecks
{
    public static void ScheduledReorgExceptionViewShowsTheStatusInCapitals()
    {
        var item = ScheduledReorgException.NewScheduledReorgException("code-648", "name-485");
        var view = ScheduledReorgExceptionView.ToScheduledReorgExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledReorgExceptionView.ToScheduledReorgExceptionViews(new List<ScheduledReorgException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledReorgExceptionViewShowsTheStatusInCapitals();
    }
}
