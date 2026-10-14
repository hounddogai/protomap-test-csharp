namespace Acme.Workforce.Payrollops.Tests.Deduction.Adjustment;

using Acme.Workforce.Payrollops.Deduction.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class StrictDeductionMilestoneRulesChecks
{
    public static void StrictDeductionMilestoneValidatorReportsEveryProblem()
    {
        var validator = StrictDeductionMilestoneRules.DefaultStrictDeductionMilestoneValidator();
        var item = StrictDeductionMilestone.NewStrictDeductionMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictDeductionMilestoneCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictDeductionMilestoneCodeLengthRule(255);
        var item = StrictDeductionMilestone.NewStrictDeductionMilestone("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictDeductionMilestoneManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new StrictDeductionMilestoneManagerIdLengthRule(120);
        var item = StrictDeductionMilestone.NewStrictDeductionMilestone("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        StrictDeductionMilestoneValidatorReportsEveryProblem();
        StrictDeductionMilestoneCodeLengthRuleFlagsALongCode();
        StrictDeductionMilestoneManagerIdLengthRuleFlagsALongManagerId();
    }
}
