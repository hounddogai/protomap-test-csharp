namespace Acme.Workforce.Orgdesign.Tests.Position.Audit;

using Acme.Workforce.Orgdesign.Position.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class GlobalPositionExceptionViewChecks
{
    public static void GlobalPositionExceptionViewShowsTheStatusInCapitals()
    {
        var item = GlobalPositionException.NewGlobalPositionException("code-890", "name-519");
        var view = GlobalPositionExceptionView.ToGlobalPositionExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalPositionExceptionView.ToGlobalPositionExceptionViews(new List<GlobalPositionException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalPositionExceptionViewShowsTheStatusInCapitals();
    }
}
