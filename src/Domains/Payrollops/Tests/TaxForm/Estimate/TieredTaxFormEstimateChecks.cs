namespace Acme.Workforce.Payrollops.Tests.TaxForm.Estimate;

using Acme.Workforce.Payrollops.TaxForm.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class TieredTaxFormEstimateChecks
{
    public static void TieredTaxFormEstimateStartsInProposed()
    {
        var item = TieredTaxFormEstimate.NewTieredTaxFormEstimate("code-415", "name-118");
        Check.Equal(item.Status, TieredTaxFormEstimateStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredTaxFormEstimateMovesToApproved()
    {
        var item = TieredTaxFormEstimate.NewTieredTaxFormEstimate("code-415", "name-118");
        try
        {
            item.MoveTo(TieredTaxFormEstimateStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredTaxFormEstimateStatus.Approved, "status");
    }

    public static void TieredTaxFormEstimateRejectsMovesFromWithdrawn()
    {
        var item = TieredTaxFormEstimate.NewTieredTaxFormEstimate("code-415", "name-118");
        item.Status = TieredTaxFormEstimateStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(TieredTaxFormEstimateStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        TieredTaxFormEstimateStartsInProposed();
        TieredTaxFormEstimateMovesToApproved();
        TieredTaxFormEstimateRejectsMovesFromWithdrawn();
    }
}
