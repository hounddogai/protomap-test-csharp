namespace Acme.Workforce.Compensation.Tests.EquityGrant.Group;

using Acme.Workforce.Compensation.EquityGrant.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class EquityGrantGroupViewChecks
{
    public static void EquityGrantGroupViewShowsTheStatusInCapitals()
    {
        var item = EquityGrantGroup.NewEquityGrantGroup("code-764", "label-775");
        var view = EquityGrantGroupView.ToEquityGrantGroupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EquityGrantGroupView.ToEquityGrantGroupViews(new List<EquityGrantGroup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EquityGrantGroupViewShowsTheStatusInCapitals();
    }
}
