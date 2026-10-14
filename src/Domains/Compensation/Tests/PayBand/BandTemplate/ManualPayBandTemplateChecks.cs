namespace Acme.Workforce.Compensation.Tests.PayBand.BandTemplate;

using Acme.Workforce.Compensation.PayBand.BandTemplate;

/// <summary>Checks of band template code, run by the project's runner.</summary>
public static class ManualPayBandTemplateChecks
{
    public static void ManualPayBandTemplateStartsInCreated()
    {
        var item = ManualPayBandTemplate.NewManualPayBandTemplate("code-350", "name-967");
        Check.Equal(item.Status, ManualPayBandTemplateStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualPayBandTemplateMovesToValidated()
    {
        var item = ManualPayBandTemplate.NewManualPayBandTemplate("code-350", "name-967");
        try
        {
            item.MoveTo(ManualPayBandTemplateStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualPayBandTemplateStatus.Validated, "status");
    }

    public static void ManualPayBandTemplateRejectsMovesFromRetired()
    {
        var item = ManualPayBandTemplate.NewManualPayBandTemplate("code-350", "name-967");
        item.Status = ManualPayBandTemplateStatus.Retired;
        Check.Fails(() => item.MoveTo(ManualPayBandTemplateStatus.Created), "move");
    }

    public static void RunAll()
    {
        ManualPayBandTemplateStartsInCreated();
        ManualPayBandTemplateMovesToValidated();
        ManualPayBandTemplateRejectsMovesFromRetired();
    }
}
