namespace Acme.Workforce.Compensation.Tests.PayBand.Template;

using Acme.Workforce.Compensation.PayBand.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class BatchedPayBandTemplateViewChecks
{
    public static void BatchedPayBandTemplateViewShowsTheStatusInCapitals()
    {
        var item = BatchedPayBandTemplate.NewBatchedPayBandTemplate("code-271", "label-609");
        var view = BatchedPayBandTemplateView.ToBatchedPayBandTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedPayBandTemplateView.ToBatchedPayBandTemplateViews(new List<BatchedPayBandTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedPayBandTemplateViewShowsTheStatusInCapitals();
    }
}
