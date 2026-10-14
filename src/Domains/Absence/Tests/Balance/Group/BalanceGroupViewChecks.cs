namespace Acme.Workforce.Absence.Tests.Balance.Group;

using Acme.Workforce.Absence.Balance.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class BalanceGroupViewChecks
{
    public static void BalanceGroupViewShowsTheStatusInCapitals()
    {
        var item = BalanceGroup.NewBalanceGroup("code-233", "label-372");
        var view = BalanceGroupView.ToBalanceGroupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BalanceGroupView.ToBalanceGroupViews(new List<BalanceGroup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BalanceGroupViewShowsTheStatusInCapitals();
    }
}
