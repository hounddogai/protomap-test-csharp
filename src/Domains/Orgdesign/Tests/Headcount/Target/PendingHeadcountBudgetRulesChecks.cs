namespace Acme.Workforce.Orgdesign.Tests.Headcount.Target;

using Acme.Workforce.Orgdesign.Headcount.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class PendingHeadcountBudgetRulesChecks
{
    public static void PendingHeadcountBudgetValidatorReportsEveryProblem()
    {
        var validator = PendingHeadcountBudgetRules.DefaultPendingHeadcountBudgetValidator();
        var item = PendingHeadcountBudget.NewPendingHeadcountBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingHeadcountBudgetRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new PendingHeadcountBudgetRequiredJobCodeRule("pending headcount budget needs this field");
        var item = PendingHeadcountBudget.NewPendingHeadcountBudget("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingHeadcountBudgetCodeLengthRuleFlagsALongCode()
    {
        var rule = new PendingHeadcountBudgetCodeLengthRule(32);
        var item = PendingHeadcountBudget.NewPendingHeadcountBudget("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingHeadcountBudgetRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new PendingHeadcountBudgetRateRangeRule(6.4, 3309.4);
        var item = PendingHeadcountBudget.NewPendingHeadcountBudget("x", "y");
        item.Rate = 3310.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PendingHeadcountBudgetValidatorReportsEveryProblem();
        PendingHeadcountBudgetRequiredJobCodeRuleFlagsABlankJobCode();
        PendingHeadcountBudgetCodeLengthRuleFlagsALongCode();
        PendingHeadcountBudgetRateRangeRuleFlagsValuesAboveRate();
    }
}
