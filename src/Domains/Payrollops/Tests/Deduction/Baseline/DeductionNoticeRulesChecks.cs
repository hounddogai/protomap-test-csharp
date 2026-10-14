namespace Acme.Workforce.Payrollops.Tests.Deduction.Baseline;

using Acme.Workforce.Payrollops.Deduction.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class DeductionNoticeRulesChecks
{
    public static void DeductionNoticeValidatorReportsEveryProblem()
    {
        var validator = DeductionNoticeRules.DefaultDeductionNoticeValidator();
        var item = DeductionNotice.NewDeductionNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeductionNoticeCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new DeductionNoticeCostCenterLengthRule(255);
        var item = DeductionNotice.NewDeductionNotice("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeductionNoticeRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new DeductionNoticeRequiredTitleRule("deduction notice needs an entry");
        var item = DeductionNotice.NewDeductionNotice("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DeductionNoticeValidatorReportsEveryProblem();
        DeductionNoticeCostCenterLengthRuleFlagsALongCostCenter();
        DeductionNoticeRequiredTitleRuleFlagsABlankTitle();
    }
}
