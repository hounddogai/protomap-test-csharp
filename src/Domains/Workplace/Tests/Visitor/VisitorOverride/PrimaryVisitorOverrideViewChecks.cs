namespace Acme.Workforce.Workplace.Tests.Visitor.VisitorOverride;

using Acme.Workforce.Workplace.Visitor.VisitorOverride;

/// <summary>Checks of visitor override code, run by the project's runner.</summary>
public static class PrimaryVisitorOverrideViewChecks
{
    public static void PrimaryVisitorOverrideViewShowsTheStatusInCapitals()
    {
        var item = PrimaryVisitorOverride.NewPrimaryVisitorOverride("code-353", "name-807");
        var view = PrimaryVisitorOverrideView.ToPrimaryVisitorOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryVisitorOverrideView.ToPrimaryVisitorOverrideViews(new List<PrimaryVisitorOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryVisitorOverrideViewShowsTheStatusInCapitals();
    }
}
