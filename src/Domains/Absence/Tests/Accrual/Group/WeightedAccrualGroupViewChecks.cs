namespace Acme.Workforce.Absence.Tests.Accrual.Group;

using Acme.Workforce.Absence.Accrual.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class WeightedAccrualGroupViewChecks
{
    public static void WeightedAccrualGroupViewShowsTheStatusInCapitals()
    {
        var item = WeightedAccrualGroup.NewWeightedAccrualGroup("code-167", "title-153");
        var view = WeightedAccrualGroupView.ToWeightedAccrualGroupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedAccrualGroupView.ToWeightedAccrualGroupViews(new List<WeightedAccrualGroup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedAccrualGroupViewShowsTheStatusInCapitals();
    }
}
