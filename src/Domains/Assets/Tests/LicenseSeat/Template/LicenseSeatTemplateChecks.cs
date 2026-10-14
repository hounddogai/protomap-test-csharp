namespace Acme.Workforce.Assets.Tests.LicenseSeat.Template;

using Acme.Workforce.Assets.LicenseSeat.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class LicenseSeatTemplateChecks
{
    public static void LicenseSeatTemplateStartsInIdle()
    {
        var item = LicenseSeatTemplate.NewLicenseSeatTemplate("code-558", "name-666");
        Check.Equal(item.Status, LicenseSeatTemplateStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LicenseSeatTemplateMovesToWarming()
    {
        var item = LicenseSeatTemplate.NewLicenseSeatTemplate("code-558", "name-666");
        try
        {
            item.MoveTo(LicenseSeatTemplateStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LicenseSeatTemplateStatus.Warming, "status");
    }

    public static void LicenseSeatTemplateRejectsMovesFromStopped()
    {
        var item = LicenseSeatTemplate.NewLicenseSeatTemplate("code-558", "name-666");
        item.Status = LicenseSeatTemplateStatus.Stopped;
        Check.Fails(() => item.MoveTo(LicenseSeatTemplateStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LicenseSeatTemplateStartsInIdle();
        LicenseSeatTemplateMovesToWarming();
        LicenseSeatTemplateRejectsMovesFromStopped();
    }
}
