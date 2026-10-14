namespace Acme.Workforce.Payrollops.Tests.PayRun.Checkpoint;

using Acme.Workforce.Payrollops.PayRun.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class PayRunTemplateViewChecks
{
    public static void PayRunTemplateViewShowsTheStatusInCapitals()
    {
        var item = PayRunTemplate.NewPayRunTemplate("code-809", "label-301");
        var view = PayRunTemplateView.ToPayRunTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayRunTemplateView.ToPayRunTemplateViews(new List<PayRunTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayRunTemplateViewShowsTheStatusInCapitals();
    }
}
