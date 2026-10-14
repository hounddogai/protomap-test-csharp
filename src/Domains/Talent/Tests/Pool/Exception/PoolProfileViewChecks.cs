namespace Acme.Workforce.Talent.Tests.Pool.Exception;

using Acme.Workforce.Talent.Pool.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class PoolProfileViewChecks
{
    public static void PoolProfileViewShowsTheStatusInCapitals()
    {
        var item = PoolProfile.NewPoolProfile("code-14", "label-680");
        var view = PoolProfileView.ToPoolProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PoolProfileView.ToPoolProfileViews(new List<PoolProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PoolProfileViewShowsTheStatusInCapitals();
    }
}
