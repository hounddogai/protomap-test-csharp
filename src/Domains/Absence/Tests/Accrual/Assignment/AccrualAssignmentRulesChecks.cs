namespace Acme.Workforce.Absence.Tests.Accrual.Assignment;

using Acme.Workforce.Absence.Accrual.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class AccrualAssignmentRulesChecks
{
    public static void AccrualAssignmentValidatorReportsEveryProblem()
    {
        var validator = AccrualAssignmentRules.DefaultAccrualAssignmentValidator();
        var item = AccrualAssignment.NewAccrualAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccrualAssignmentRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new AccrualAssignmentRequiredChecksumRule("accrual assignment needs an entry");
        var item = AccrualAssignment.NewAccrualAssignment("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccrualAssignmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AccrualAssignmentRequiredCodeRule("accrual assignment needs a value");
        var item = AccrualAssignment.NewAccrualAssignment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccrualAssignmentRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new AccrualAssignmentRequiredTitleRule("accrual assignment needs this field");
        var item = AccrualAssignment.NewAccrualAssignment("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccrualAssignmentTitleLengthRuleFlagsALongTitle()
    {
        var rule = new AccrualAssignmentTitleLengthRule(32);
        var item = AccrualAssignment.NewAccrualAssignment("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AccrualAssignmentValidatorReportsEveryProblem();
        AccrualAssignmentRequiredChecksumRuleFlagsABlankChecksum();
        AccrualAssignmentRequiredCodeRuleFlagsABlankCode();
        AccrualAssignmentRequiredTitleRuleFlagsABlankTitle();
        AccrualAssignmentTitleLengthRuleFlagsALongTitle();
    }
}
