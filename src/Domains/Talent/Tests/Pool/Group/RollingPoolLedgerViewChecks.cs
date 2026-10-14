namespace Acme.Workforce.Talent.Tests.Pool.Group;

using Acme.Workforce.Talent.Pool.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class RollingPoolLedgerViewChecks
{
    public static void RollingPoolLedgerViewShowsTheStatusInCapitals()
    {
        var item = RollingPoolLedger.NewRollingPoolLedger("code-806", "label-590");
        var view = RollingPoolLedgerView.ToRollingPoolLedgerView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingPoolLedgerView.ToRollingPoolLedgerViews(new List<RollingPoolLedger> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingPoolLedgerViewShowsTheStatusInCapitals();
    }
}
