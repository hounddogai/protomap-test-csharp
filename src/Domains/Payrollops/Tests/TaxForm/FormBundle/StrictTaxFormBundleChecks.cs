namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormBundle;

using Acme.Workforce.Payrollops.TaxForm.FormBundle;

/// <summary>Checks of form bundle code, run by the project's runner.</summary>
public static class StrictTaxFormBundleChecks
{
    public static void StrictTaxFormBundleStartsInIdle()
    {
        var item = StrictTaxFormBundle.NewStrictTaxFormBundle("code-625", "name-193");
        Check.Equal(item.Status, StrictTaxFormBundleStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictTaxFormBundleMovesToReady()
    {
        var item = StrictTaxFormBundle.NewStrictTaxFormBundle("code-625", "name-193");
        try
        {
            item.MoveTo(StrictTaxFormBundleStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictTaxFormBundleStatus.Ready, "status");
    }

    public static void StrictTaxFormBundleRejectsMovesFromStopped()
    {
        var item = StrictTaxFormBundle.NewStrictTaxFormBundle("code-625", "name-193");
        item.Status = StrictTaxFormBundleStatus.Stopped;
        Check.Fails(() => item.MoveTo(StrictTaxFormBundleStatus.Idle), "move");
    }

    public static void RunAll()
    {
        StrictTaxFormBundleStartsInIdle();
        StrictTaxFormBundleMovesToReady();
        StrictTaxFormBundleRejectsMovesFromStopped();
    }
}
