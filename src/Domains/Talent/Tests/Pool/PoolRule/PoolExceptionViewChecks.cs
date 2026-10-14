namespace Acme.Workforce.Talent.Tests.Pool.PoolRule;

using Acme.Workforce.Talent.Pool.PoolRule;

/// <summary>Checks of pool rule code, run by the project's runner.</summary>
public static class PoolExceptionViewChecks
{
    public static void PoolExceptionViewShowsTheStatusInCapitals()
    {
        var item = PoolException.NewPoolException("code-737", "title-296");
        var view = PoolExceptionView.ToPoolExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PoolExceptionView.ToPoolExceptionViews(new List<PoolException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PoolExceptionViewShowsTheStatusInCapitals();
    }
}
