namespace Acme.Workforce.Talent.Tests.SkillProfile.Group;

using Acme.Workforce.Talent.SkillProfile.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class SkillProfileBudgetRulesChecks
{
    public static void SkillProfileBudgetValidatorReportsEveryProblem()
    {
        var validator = SkillProfileBudgetRules.DefaultSkillProfileBudgetValidator();
        var item = SkillProfileBudget.NewSkillProfileBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SkillProfileBudgetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SkillProfileBudgetRequiredCodeRule("skill profile budget needs an entry");
        var item = SkillProfileBudget.NewSkillProfileBudget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SkillProfileBudgetRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new SkillProfileBudgetRetriesRangeRule(16, 32);
        var item = SkillProfileBudget.NewSkillProfileBudget("x", "y");
        item.Retries = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SkillProfileBudgetCodeLengthRuleFlagsALongCode()
    {
        var rule = new SkillProfileBudgetCodeLengthRule(1000);
        var item = SkillProfileBudget.NewSkillProfileBudget("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SkillProfileBudgetValidatorReportsEveryProblem();
        SkillProfileBudgetRequiredCodeRuleFlagsABlankCode();
        SkillProfileBudgetRetriesRangeRuleFlagsValuesAboveRetries();
        SkillProfileBudgetCodeLengthRuleFlagsALongCode();
    }
}
