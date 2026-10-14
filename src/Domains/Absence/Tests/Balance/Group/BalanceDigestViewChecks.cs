namespace Acme.Workforce.Absence.Tests.Balance.Group;

using Acme.Workforce.Absence.Balance.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class BalanceDigestViewChecks
{
    public static void BalanceDigestViewShowsTheStatusInCapitals()
    {
        var item = BalanceDigest.NewBalanceDigest("code-503", "title-284");
        var view = BalanceDigestView.ToBalanceDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BalanceDigestView.ToBalanceDigestViews(new List<BalanceDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BalanceDigestViewShowsTheStatusInCapitals();
    }
}
