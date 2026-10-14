namespace Acme.Workforce.Payrollops.Tests.Deduction.Profile;

using Acme.Workforce.Payrollops.Deduction.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class RegionalDeductionBatchChecks
{
    public static void RegionalDeductionBatchStartsInCreated()
    {
        var item = RegionalDeductionBatch.NewRegionalDeductionBatch("code-563", "name-304");
        Check.Equal(item.Status, RegionalDeductionBatchStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalDeductionBatchMovesToValidated()
    {
        var item = RegionalDeductionBatch.NewRegionalDeductionBatch("code-563", "name-304");
        try
        {
            item.MoveTo(RegionalDeductionBatchStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalDeductionBatchStatus.Validated, "status");
    }

    public static void RegionalDeductionBatchRejectsMovesFromRetired()
    {
        var item = RegionalDeductionBatch.NewRegionalDeductionBatch("code-563", "name-304");
        item.Status = RegionalDeductionBatchStatus.Retired;
        Check.Fails(() => item.MoveTo(RegionalDeductionBatchStatus.Created), "move");
    }

    public static void RunAll()
    {
        RegionalDeductionBatchStartsInCreated();
        RegionalDeductionBatchMovesToValidated();
        RegionalDeductionBatchRejectsMovesFromRetired();
    }
}
