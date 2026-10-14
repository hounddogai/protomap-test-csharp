namespace Acme.Workforce.Talent.Tests.Pool.Request;

using Acme.Workforce.Talent.Pool.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class PrimaryPoolOverrideViewChecks
{
    public static void PrimaryPoolOverrideViewShowsTheStatusInCapitals()
    {
        var item = PrimaryPoolOverride.NewPrimaryPoolOverride("code-329", "name-393");
        var view = PrimaryPoolOverrideView.ToPrimaryPoolOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryPoolOverrideView.ToPrimaryPoolOverrideViews(new List<PrimaryPoolOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryPoolOverrideViewShowsTheStatusInCapitals();
    }
}
