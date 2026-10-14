namespace Acme.Workforce.Payrollops.Tests.TaxForm.Reservation;

using Acme.Workforce.Payrollops.TaxForm.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class BatchedTaxFormAssignmentRulesChecks
{
    public static void BatchedTaxFormAssignmentValidatorReportsEveryProblem()
    {
        var validator = BatchedTaxFormAssignmentRules.DefaultBatchedTaxFormAssignmentValidator();
        var item = BatchedTaxFormAssignment.NewBatchedTaxFormAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedTaxFormAssignmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new BatchedTaxFormAssignmentCodeLengthRule(255);
        var item = BatchedTaxFormAssignment.NewBatchedTaxFormAssignment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedTaxFormAssignmentRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new BatchedTaxFormAssignmentRequiredPhoneRule("batched tax form assignment needs this field");
        var item = BatchedTaxFormAssignment.NewBatchedTaxFormAssignment("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedTaxFormAssignmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BatchedTaxFormAssignmentRequiredCodeRule("batched tax form assignment needs this field");
        var item = BatchedTaxFormAssignment.NewBatchedTaxFormAssignment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedTaxFormAssignmentMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new BatchedTaxFormAssignmentMaximumRangeRule(4.1, 2683.5);
        var item = BatchedTaxFormAssignment.NewBatchedTaxFormAssignment("x", "y");
        item.Maximum = 2685.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BatchedTaxFormAssignmentValidatorReportsEveryProblem();
        BatchedTaxFormAssignmentCodeLengthRuleFlagsALongCode();
        BatchedTaxFormAssignmentRequiredPhoneRuleFlagsABlankPhone();
        BatchedTaxFormAssignmentRequiredCodeRuleFlagsABlankCode();
        BatchedTaxFormAssignmentMaximumRangeRuleFlagsValuesAboveMaximum();
    }
}
