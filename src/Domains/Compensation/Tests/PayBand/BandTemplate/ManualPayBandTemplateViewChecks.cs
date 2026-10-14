namespace Acme.Workforce.Compensation.Tests.PayBand.BandTemplate;

using Acme.Workforce.Compensation.PayBand.BandTemplate;

/// <summary>Checks of band template code, run by the project's runner.</summary>
public static class ManualPayBandTemplateViewChecks
{
    public static void ManualPayBandTemplateViewShowsTheStatusInCapitals()
    {
        var item = ManualPayBandTemplate.NewManualPayBandTemplate("code-350", "name-967");
        var view = ManualPayBandTemplateView.ToManualPayBandTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualPayBandTemplateView.ToManualPayBandTemplateViews(new List<ManualPayBandTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualPayBandTemplateViewShowsTheStatusInCapitals();
    }
}
