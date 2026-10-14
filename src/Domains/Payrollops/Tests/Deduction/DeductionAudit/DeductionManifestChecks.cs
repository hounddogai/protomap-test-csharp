namespace Acme.Workforce.Payrollops.Tests.Deduction.DeductionAudit;

using Acme.Workforce.Payrollops.Deduction.DeductionAudit;

/// <summary>Checks of deduction audit code, run by the project's runner.</summary>
public static class DeductionManifestChecks
{
    public static void DeductionManifestStartsInOpen()
    {
        var item = DeductionManifest.NewDeductionManifest("code-561", "name-54");
        Check.Equal(item.Status, DeductionManifestStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeductionManifestMovesToInReview()
    {
        var item = DeductionManifest.NewDeductionManifest("code-561", "name-54");
        try
        {
            item.MoveTo(DeductionManifestStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeductionManifestStatus.InReview, "status");
    }

    public static void DeductionManifestRejectsMovesFromArchived()
    {
        var item = DeductionManifest.NewDeductionManifest("code-561", "name-54");
        item.Status = DeductionManifestStatus.Archived;
        Check.Fails(() => item.MoveTo(DeductionManifestStatus.Open), "move");
    }

    public static void RunAll()
    {
        DeductionManifestStartsInOpen();
        DeductionManifestMovesToInReview();
        DeductionManifestRejectsMovesFromArchived();
    }
}
