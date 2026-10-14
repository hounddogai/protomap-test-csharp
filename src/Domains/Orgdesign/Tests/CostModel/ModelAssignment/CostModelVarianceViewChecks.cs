namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelAssignment;

using Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

/// <summary>Checks of model assignment code, run by the project's runner.</summary>
public static class CostModelVarianceViewChecks
{
    public static void CostModelVarianceViewShowsTheStatusInCapitals()
    {
        var item = CostModelVariance.NewCostModelVariance("code-213", "name-552");
        var view = CostModelVarianceView.ToCostModelVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CostModelVarianceView.ToCostModelVarianceViews(new List<CostModelVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CostModelVarianceViewShowsTheStatusInCapitals();
    }
}
