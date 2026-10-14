namespace Acme.Workforce.Payrollops.Tests.PayRun.Adjustment;

using Acme.Workforce.Payrollops.PayRun.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class RollingPayRunDigestViewChecks
{
    public static void RollingPayRunDigestViewShowsTheStatusInCapitals()
    {
        var item = RollingPayRunDigest.NewRollingPayRunDigest("code-606", "title-78");
        var view = RollingPayRunDigestView.ToRollingPayRunDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingPayRunDigestView.ToRollingPayRunDigestViews(new List<RollingPayRunDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingPayRunDigestViewShowsTheStatusInCapitals();
    }
}
