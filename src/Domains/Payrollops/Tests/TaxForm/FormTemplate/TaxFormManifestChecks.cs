namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormTemplate;

using Acme.Workforce.Payrollops.TaxForm.FormTemplate;

/// <summary>Checks of form template code, run by the project's runner.</summary>
public static class TaxFormManifestChecks
{
    public static void TaxFormManifestStartsInOpen()
    {
        var item = TaxFormManifest.NewTaxFormManifest("code-314", "title-785");
        Check.Equal(item.Status, TaxFormManifestStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TaxFormManifestMovesToInReview()
    {
        var item = TaxFormManifest.NewTaxFormManifest("code-314", "title-785");
        try
        {
            item.MoveTo(TaxFormManifestStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TaxFormManifestStatus.InReview, "status");
    }

    public static void TaxFormManifestRejectsMovesFromArchived()
    {
        var item = TaxFormManifest.NewTaxFormManifest("code-314", "title-785");
        item.Status = TaxFormManifestStatus.Archived;
        Check.Fails(() => item.MoveTo(TaxFormManifestStatus.Open), "move");
    }

    public static void RunAll()
    {
        TaxFormManifestStartsInOpen();
        TaxFormManifestMovesToInReview();
        TaxFormManifestRejectsMovesFromArchived();
    }
}
