namespace Acme.Workforce.Workplace.Tests.Desk.Variance;

using Acme.Workforce.Workplace.Desk.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class GlobalDeskVarianceViewChecks
{
    public static void GlobalDeskVarianceViewShowsTheStatusInCapitals()
    {
        var item = GlobalDeskVariance.NewGlobalDeskVariance("code-882", "label-728");
        var view = GlobalDeskVarianceView.ToGlobalDeskVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalDeskVarianceView.ToGlobalDeskVarianceViews(new List<GlobalDeskVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalDeskVarianceViewShowsTheStatusInCapitals();
    }
}
