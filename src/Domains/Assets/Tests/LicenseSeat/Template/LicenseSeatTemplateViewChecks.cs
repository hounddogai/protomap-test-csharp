namespace Acme.Workforce.Assets.Tests.LicenseSeat.Template;

using Acme.Workforce.Assets.LicenseSeat.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class LicenseSeatTemplateViewChecks
{
    public static void LicenseSeatTemplateViewShowsTheStatusInCapitals()
    {
        var item = LicenseSeatTemplate.NewLicenseSeatTemplate("code-558", "name-666");
        var view = LicenseSeatTemplateView.ToLicenseSeatTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LicenseSeatTemplateView.ToLicenseSeatTemplateViews(new List<LicenseSeatTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LicenseSeatTemplateViewShowsTheStatusInCapitals();
    }
}
