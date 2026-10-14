namespace Acme.Workforce.Talent.Tests.Pool.Target;

using Acme.Workforce.Talent.Pool.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class LegacyPoolTargetViewChecks
{
    public static void LegacyPoolTargetViewShowsTheStatusInCapitals()
    {
        var item = LegacyPoolTarget.NewLegacyPoolTarget("code-747", "name-634");
        var view = LegacyPoolTargetView.ToLegacyPoolTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyPoolTargetView.ToLegacyPoolTargetViews(new List<LegacyPoolTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyPoolTargetViewShowsTheStatusInCapitals();
    }
}
