namespace Acme.Workforce.Workplace.Tests.Desk.Budget;

using Acme.Workforce.Workplace.Desk.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class DeskVarianceViewChecks
{
    public static void DeskVarianceViewShowsTheStatusInCapitals()
    {
        var item = DeskVariance.NewDeskVariance("code-207", "name-369");
        var view = DeskVarianceView.ToDeskVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DeskVarianceView.ToDeskVarianceViews(new List<DeskVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DeskVarianceViewShowsTheStatusInCapitals();
    }
}
