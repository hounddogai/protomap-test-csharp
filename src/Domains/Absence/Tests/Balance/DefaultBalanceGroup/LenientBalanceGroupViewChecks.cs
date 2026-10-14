namespace Acme.Workforce.Absence.Tests.Balance.DefaultBalanceGroup;

using Acme.Workforce.Absence.Balance.DefaultBalanceGroup;

/// <summary>Checks of default balance group code, run by the project's runner.</summary>
public static class LenientBalanceGroupViewChecks
{
    public static void LenientBalanceGroupViewShowsTheStatusInCapitals()
    {
        var item = LenientBalanceGroup.NewLenientBalanceGroup("code-263", "name-803");
        var view = LenientBalanceGroupView.ToLenientBalanceGroupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientBalanceGroupView.ToLenientBalanceGroupViews(new List<LenientBalanceGroup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientBalanceGroupViewShowsTheStatusInCapitals();
    }
}
