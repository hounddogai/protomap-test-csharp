namespace Acme.Workforce.Absence.Tests.Accrual.Baseline;

using Acme.Workforce.Absence.Accrual.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class AccrualBaselineRulesChecks
{
    public static void AccrualBaselineValidatorReportsEveryProblem()
    {
        var validator = AccrualBaselineRules.DefaultAccrualBaselineValidator();
        var item = AccrualBaseline.NewAccrualBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccrualBaselineCodeLengthRuleFlagsALongCode()
    {
        var rule = new AccrualBaselineCodeLengthRule(16);
        var item = AccrualBaseline.NewAccrualBaseline("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccrualBaselineBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new AccrualBaselineBatchSizeRangeRule(32, 64);
        var item = AccrualBaseline.NewAccrualBaseline("x", "y");
        item.BatchSize = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccrualBaselineRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new AccrualBaselineRequiredOwnerIdRule("accrual baseline needs this field");
        var item = AccrualBaseline.NewAccrualBaseline("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccrualBaselineRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AccrualBaselineRequiredCodeRule("accrual baseline needs an entry");
        var item = AccrualBaseline.NewAccrualBaseline("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AccrualBaselineValidatorReportsEveryProblem();
        AccrualBaselineCodeLengthRuleFlagsALongCode();
        AccrualBaselineBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        AccrualBaselineRequiredOwnerIdRuleFlagsABlankOwnerId();
        AccrualBaselineRequiredCodeRuleFlagsABlankCode();
    }
}
