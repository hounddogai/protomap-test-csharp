namespace Acme.Workforce.Talent.Tests.Pool.Milestone;

using Acme.Workforce.Talent.Pool.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class TieredPoolGroupViewChecks
{
    public static void TieredPoolGroupViewShowsTheStatusInCapitals()
    {
        var item = TieredPoolGroup.NewTieredPoolGroup("code-987", "name-790");
        var view = TieredPoolGroupView.ToTieredPoolGroupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredPoolGroupView.ToTieredPoolGroupViews(new List<TieredPoolGroup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredPoolGroupViewShowsTheStatusInCapitals();
    }
}
