namespace Acme.Workforce.Orgdesign.Tests.Reorg.Bundle;

using Acme.Workforce.Orgdesign.Reorg.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class PrimaryReorgPlanViewChecks
{
    public static void PrimaryReorgPlanViewShowsTheStatusInCapitals()
    {
        var item = PrimaryReorgPlan.NewPrimaryReorgPlan("code-228", "label-369");
        var view = PrimaryReorgPlanView.ToPrimaryReorgPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryReorgPlanView.ToPrimaryReorgPlanViews(new List<PrimaryReorgPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryReorgPlanViewShowsTheStatusInCapitals();
    }
}
