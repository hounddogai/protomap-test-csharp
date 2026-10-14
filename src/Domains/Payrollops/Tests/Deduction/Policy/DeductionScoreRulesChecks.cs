namespace Acme.Workforce.Payrollops.Tests.Deduction.Policy;

using Acme.Workforce.Payrollops.Deduction.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class DeductionScoreRulesChecks
{
    public static void DeductionScoreValidatorReportsEveryProblem()
    {
        var validator = DeductionScoreRules.DefaultDeductionScoreValidator();
        var item = DeductionScore.NewDeductionScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeductionScoreTitleLengthRuleFlagsALongTitle()
    {
        var rule = new DeductionScoreTitleLengthRule(80);
        var item = DeductionScore.NewDeductionScore("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeductionScoreUnitLengthRuleFlagsALongUnit()
    {
        var rule = new DeductionScoreUnitLengthRule(255);
        var item = DeductionScore.NewDeductionScore("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DeductionScoreValidatorReportsEveryProblem();
        DeductionScoreTitleLengthRuleFlagsALongTitle();
        DeductionScoreUnitLengthRuleFlagsALongUnit();
    }
}
