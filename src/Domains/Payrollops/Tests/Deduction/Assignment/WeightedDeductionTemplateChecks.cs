namespace Acme.Workforce.Payrollops.Tests.Deduction.Assignment;

using Acme.Workforce.Payrollops.Deduction.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class WeightedDeductionTemplateChecks
{
    public static void WeightedDeductionTemplateStartsInCreated()
    {
        var item = WeightedDeductionTemplate.NewWeightedDeductionTemplate("code-851", "label-271");
        Check.Equal(item.Status, WeightedDeductionTemplateStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedDeductionTemplateMovesToValidated()
    {
        var item = WeightedDeductionTemplate.NewWeightedDeductionTemplate("code-851", "label-271");
        try
        {
            item.MoveTo(WeightedDeductionTemplateStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedDeductionTemplateStatus.Validated, "status");
    }

    public static void WeightedDeductionTemplateRejectsMovesFromRetired()
    {
        var item = WeightedDeductionTemplate.NewWeightedDeductionTemplate("code-851", "label-271");
        item.Status = WeightedDeductionTemplateStatus.Retired;
        Check.Fails(() => item.MoveTo(WeightedDeductionTemplateStatus.Created), "move");
    }

    public static void RunAll()
    {
        WeightedDeductionTemplateStartsInCreated();
        WeightedDeductionTemplateMovesToValidated();
        WeightedDeductionTemplateRejectsMovesFromRetired();
    }
}
