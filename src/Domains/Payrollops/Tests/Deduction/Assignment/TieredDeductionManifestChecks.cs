namespace Acme.Workforce.Payrollops.Tests.Deduction.Assignment;

using Acme.Workforce.Payrollops.Deduction.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class TieredDeductionManifestChecks
{
    public static void TieredDeductionManifestStartsInNew()
    {
        var item = TieredDeductionManifest.NewTieredDeductionManifest("code-614", "label-816");
        Check.Equal(item.Status, TieredDeductionManifestStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredDeductionManifestMovesToFailed()
    {
        var item = TieredDeductionManifest.NewTieredDeductionManifest("code-614", "label-816");
        try
        {
            item.MoveTo(TieredDeductionManifestStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredDeductionManifestStatus.Failed, "status");
    }

    public static void TieredDeductionManifestRejectsMovesFromFailed()
    {
        var item = TieredDeductionManifest.NewTieredDeductionManifest("code-614", "label-816");
        item.Status = TieredDeductionManifestStatus.Failed;
        Check.Fails(() => item.MoveTo(TieredDeductionManifestStatus.New), "move");
    }

    public static void RunAll()
    {
        TieredDeductionManifestStartsInNew();
        TieredDeductionManifestMovesToFailed();
        TieredDeductionManifestRejectsMovesFromFailed();
    }
}
