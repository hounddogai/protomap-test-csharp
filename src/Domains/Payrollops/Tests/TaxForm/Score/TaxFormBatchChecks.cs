namespace Acme.Workforce.Payrollops.Tests.TaxForm.Score;

using Acme.Workforce.Payrollops.TaxForm.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class TaxFormBatchChecks
{
    public static void TaxFormBatchStartsInCreated()
    {
        var item = TaxFormBatch.NewTaxFormBatch("code-897", "label-465");
        Check.Equal(item.Status, TaxFormBatchStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TaxFormBatchMovesToValidated()
    {
        var item = TaxFormBatch.NewTaxFormBatch("code-897", "label-465");
        try
        {
            item.MoveTo(TaxFormBatchStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TaxFormBatchStatus.Validated, "status");
    }

    public static void TaxFormBatchRejectsMovesFromRetired()
    {
        var item = TaxFormBatch.NewTaxFormBatch("code-897", "label-465");
        item.Status = TaxFormBatchStatus.Retired;
        Check.Fails(() => item.MoveTo(TaxFormBatchStatus.Created), "move");
    }

    public static void RunAll()
    {
        TaxFormBatchStartsInCreated();
        TaxFormBatchMovesToValidated();
        TaxFormBatchRejectsMovesFromRetired();
    }
}
