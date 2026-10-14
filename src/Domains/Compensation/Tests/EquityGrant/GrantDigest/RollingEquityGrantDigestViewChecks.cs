namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantDigest;

using Acme.Workforce.Compensation.EquityGrant.GrantDigest;

/// <summary>Checks of grant digest code, run by the project's runner.</summary>
public static class RollingEquityGrantDigestViewChecks
{
    public static void RollingEquityGrantDigestViewShowsTheStatusInCapitals()
    {
        var item = RollingEquityGrantDigest.NewRollingEquityGrantDigest("code-254", "label-418");
        var view = RollingEquityGrantDigestView.ToRollingEquityGrantDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingEquityGrantDigestView.ToRollingEquityGrantDigestViews(new List<RollingEquityGrantDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingEquityGrantDigestViewShowsTheStatusInCapitals();
    }
}
