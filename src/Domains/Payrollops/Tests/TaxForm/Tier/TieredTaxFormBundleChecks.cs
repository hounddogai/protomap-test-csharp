namespace Acme.Workforce.Payrollops.Tests.TaxForm.Tier;

using Acme.Workforce.Payrollops.TaxForm.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class TieredTaxFormBundleChecks
{
    public static void TieredTaxFormBundleStartsInRequested()
    {
        var item = TieredTaxFormBundle.NewTieredTaxFormBundle("code-297", "name-726");
        Check.Equal(item.Status, TieredTaxFormBundleStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredTaxFormBundleMovesToExpired()
    {
        var item = TieredTaxFormBundle.NewTieredTaxFormBundle("code-297", "name-726");
        try
        {
            item.MoveTo(TieredTaxFormBundleStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredTaxFormBundleStatus.Expired, "status");
    }

    public static void TieredTaxFormBundleRejectsMovesFromExpired()
    {
        var item = TieredTaxFormBundle.NewTieredTaxFormBundle("code-297", "name-726");
        item.Status = TieredTaxFormBundleStatus.Expired;
        Check.Fails(() => item.MoveTo(TieredTaxFormBundleStatus.Requested), "move");
    }

    public static void RunAll()
    {
        TieredTaxFormBundleStartsInRequested();
        TieredTaxFormBundleMovesToExpired();
        TieredTaxFormBundleRejectsMovesFromExpired();
    }
}
