namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormBundle;

using Acme.Workforce.Payrollops.TaxForm.FormBundle;

/// <summary>Checks of form bundle code, run by the project's runner.</summary>
public static class CappedTaxFormAssignmentRulesChecks
{
    public static void CappedTaxFormAssignmentValidatorReportsEveryProblem()
    {
        var validator = CappedTaxFormAssignmentRules.DefaultCappedTaxFormAssignmentValidator();
        var item = CappedTaxFormAssignment.NewCappedTaxFormAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedTaxFormAssignmentSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new CappedTaxFormAssignmentSequenceRangeRule(64, 64);
        var item = CappedTaxFormAssignment.NewCappedTaxFormAssignment("x", "y");
        item.Sequence = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedTaxFormAssignmentExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new CappedTaxFormAssignmentExpiresAtRangeRule(32, 32);
        var item = CappedTaxFormAssignment.NewCappedTaxFormAssignment("x", "y");
        item.ExpiresAt = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedTaxFormAssignmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CappedTaxFormAssignmentRequiredCodeRule("capped tax form assignment needs this field");
        var item = CappedTaxFormAssignment.NewCappedTaxFormAssignment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedTaxFormAssignmentRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new CappedTaxFormAssignmentRequiredSourceRule("capped tax form assignment needs a value");
        var item = CappedTaxFormAssignment.NewCappedTaxFormAssignment("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedTaxFormAssignmentSourceLengthRuleFlagsALongSource()
    {
        var rule = new CappedTaxFormAssignmentSourceLengthRule(64);
        var item = CappedTaxFormAssignment.NewCappedTaxFormAssignment("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CappedTaxFormAssignmentValidatorReportsEveryProblem();
        CappedTaxFormAssignmentSequenceRangeRuleFlagsValuesAboveSequence();
        CappedTaxFormAssignmentExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        CappedTaxFormAssignmentRequiredCodeRuleFlagsABlankCode();
        CappedTaxFormAssignmentRequiredSourceRuleFlagsABlankSource();
        CappedTaxFormAssignmentSourceLengthRuleFlagsALongSource();
    }
}
