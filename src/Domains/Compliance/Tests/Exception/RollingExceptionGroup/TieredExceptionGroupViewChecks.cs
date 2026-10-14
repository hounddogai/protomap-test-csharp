namespace Acme.Workforce.Compliance.Tests.Exception.RollingExceptionGroup;

using Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

/// <summary>Checks of rolling exception group code, run by the project's runner.</summary>
public static class TieredExceptionGroupViewChecks
{
    public static void TieredExceptionGroupViewShowsTheStatusInCapitals()
    {
        var item = TieredExceptionGroup.NewTieredExceptionGroup("code-451", "label-576");
        var view = TieredExceptionGroupView.ToTieredExceptionGroupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredExceptionGroupView.ToTieredExceptionGroupViews(new List<TieredExceptionGroup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredExceptionGroupViewShowsTheStatusInCapitals();
    }
}
