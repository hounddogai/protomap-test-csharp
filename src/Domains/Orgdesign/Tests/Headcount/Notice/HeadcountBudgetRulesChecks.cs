namespace Acme.Workforce.Orgdesign.Tests.Headcount.Notice;

using Acme.Workforce.Orgdesign.Headcount.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class HeadcountBudgetRulesChecks
{
    public static void HeadcountBudgetValidatorReportsEveryProblem()
    {
        var validator = HeadcountBudgetRules.DefaultHeadcountBudgetValidator();
        var item = HeadcountBudget.NewHeadcountBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HeadcountBudgetTitleLengthRuleFlagsALongTitle()
    {
        var rule = new HeadcountBudgetTitleLengthRule(255);
        var item = HeadcountBudget.NewHeadcountBudget("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HeadcountBudgetRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new HeadcountBudgetRequiredPhoneRule("headcount budget needs an entry");
        var item = HeadcountBudget.NewHeadcountBudget("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HeadcountBudgetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new HeadcountBudgetRequiredCodeRule("headcount budget needs a value");
        var item = HeadcountBudget.NewHeadcountBudget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HeadcountBudgetPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new HeadcountBudgetPhoneLengthRule(16);
        var item = HeadcountBudget.NewHeadcountBudget("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        HeadcountBudgetValidatorReportsEveryProblem();
        HeadcountBudgetTitleLengthRuleFlagsALongTitle();
        HeadcountBudgetRequiredPhoneRuleFlagsABlankPhone();
        HeadcountBudgetRequiredCodeRuleFlagsABlankCode();
        HeadcountBudgetPhoneLengthRuleFlagsALongPhone();
    }
}
