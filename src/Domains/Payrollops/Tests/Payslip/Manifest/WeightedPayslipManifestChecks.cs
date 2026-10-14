namespace Acme.Workforce.Payrollops.Tests.Payslip.Manifest;

using Acme.Workforce.Payrollops.Payslip.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class WeightedPayslipManifestChecks
{
    public static void WeightedPayslipManifestStartsInOpen()
    {
        var item = WeightedPayslipManifest.NewWeightedPayslipManifest("code-471", "name-886");
        Check.Equal(item.Status, WeightedPayslipManifestStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedPayslipManifestMovesToInReview()
    {
        var item = WeightedPayslipManifest.NewWeightedPayslipManifest("code-471", "name-886");
        try
        {
            item.MoveTo(WeightedPayslipManifestStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedPayslipManifestStatus.InReview, "status");
    }

    public static void WeightedPayslipManifestRejectsMovesFromArchived()
    {
        var item = WeightedPayslipManifest.NewWeightedPayslipManifest("code-471", "name-886");
        item.Status = WeightedPayslipManifestStatus.Archived;
        Check.Fails(() => item.MoveTo(WeightedPayslipManifestStatus.Open), "move");
    }

    public static void RunAll()
    {
        WeightedPayslipManifestStartsInOpen();
        WeightedPayslipManifestMovesToInReview();
        WeightedPayslipManifestRejectsMovesFromArchived();
    }
}
