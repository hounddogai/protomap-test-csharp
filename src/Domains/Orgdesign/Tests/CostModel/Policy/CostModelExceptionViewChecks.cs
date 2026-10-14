namespace Acme.Workforce.Orgdesign.Tests.CostModel.Policy;

using Acme.Workforce.Orgdesign.CostModel.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class CostModelExceptionViewChecks
{
    public static void CostModelExceptionViewShowsTheStatusInCapitals()
    {
        var item = CostModelException.NewCostModelException("code-565", "name-417");
        var view = CostModelExceptionView.ToCostModelExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CostModelExceptionView.ToCostModelExceptionViews(new List<CostModelException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CostModelExceptionViewShowsTheStatusInCapitals();
    }
}
