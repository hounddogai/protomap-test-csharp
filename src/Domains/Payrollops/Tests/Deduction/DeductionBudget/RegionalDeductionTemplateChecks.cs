namespace Acme.Workforce.Payrollops.Tests.Deduction.DeductionBudget;

using Acme.Workforce.Payrollops.Deduction.DeductionBudget;

/// <summary>Checks of deduction budget code, run by the project's runner.</summary>
public static class RegionalDeductionTemplateChecks
{
    public static void RegionalDeductionTemplateStartsInPlanned()
    {
        var item = RegionalDeductionTemplate.NewRegionalDeductionTemplate("code-423", "name-942");
        Check.Equal(item.Status, RegionalDeductionTemplateStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalDeductionTemplateMovesToCompleted()
    {
        var item = RegionalDeductionTemplate.NewRegionalDeductionTemplate("code-423", "name-942");
        try
        {
            item.MoveTo(RegionalDeductionTemplateStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalDeductionTemplateStatus.Completed, "status");
    }

    public static void RegionalDeductionTemplateRejectsMovesFromCancelled()
    {
        var item = RegionalDeductionTemplate.NewRegionalDeductionTemplate("code-423", "name-942");
        item.Status = RegionalDeductionTemplateStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RegionalDeductionTemplateStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RegionalDeductionTemplateStartsInPlanned();
        RegionalDeductionTemplateMovesToCompleted();
        RegionalDeductionTemplateRejectsMovesFromCancelled();
    }
}
