namespace Acme.Workforce.Orgdesign.Tests.Position.Estimate;

using Acme.Workforce.Orgdesign.Position.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class TieredPositionPolicyViewChecks
{
    public static void TieredPositionPolicyViewShowsTheStatusInCapitals()
    {
        var item = TieredPositionPolicy.NewTieredPositionPolicy("code-994", "title-522");
        var view = TieredPositionPolicyView.ToTieredPositionPolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredPositionPolicyView.ToTieredPositionPolicyViews(new List<TieredPositionPolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredPositionPolicyViewShowsTheStatusInCapitals();
    }
}
