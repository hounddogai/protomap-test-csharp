namespace Acme.Workforce.Payrollops.Tests.PayRun.CappedRunLedger;

using Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

/// <summary>Checks of capped run ledger code, run by the project's runner.</summary>
public static class RegionalPayRunTemplateViewChecks
{
    public static void RegionalPayRunTemplateViewShowsTheStatusInCapitals()
    {
        var item = RegionalPayRunTemplate.NewRegionalPayRunTemplate("code-179", "label-737");
        var view = RegionalPayRunTemplateView.ToRegionalPayRunTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalPayRunTemplateView.ToRegionalPayRunTemplateViews(new List<RegionalPayRunTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalPayRunTemplateViewShowsTheStatusInCapitals();
    }
}
