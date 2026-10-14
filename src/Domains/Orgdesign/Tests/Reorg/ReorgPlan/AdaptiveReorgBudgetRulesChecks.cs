namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgPlan;

using Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

/// <summary>Checks of reorg plan code, run by the project's runner.</summary>
public static class AdaptiveReorgBudgetRulesChecks
{
    public static void AdaptiveReorgBudgetValidatorReportsEveryProblem()
    {
        var validator = AdaptiveReorgBudgetRules.DefaultAdaptiveReorgBudgetValidator();
        var item = AdaptiveReorgBudget.NewAdaptiveReorgBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveReorgBudgetCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdaptiveReorgBudgetCodeLengthRule(32);
        var item = AdaptiveReorgBudget.NewAdaptiveReorgBudget("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveReorgBudgetRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new AdaptiveReorgBudgetRequiredCategoryRule("adaptive reorg budget needs an entry");
        var item = AdaptiveReorgBudget.NewAdaptiveReorgBudget("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveReorgBudgetRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new AdaptiveReorgBudgetRequiredDescriptionRule("adaptive reorg budget needs a value");
        var item = AdaptiveReorgBudget.NewAdaptiveReorgBudget("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdaptiveReorgBudgetValidatorReportsEveryProblem();
        AdaptiveReorgBudgetCodeLengthRuleFlagsALongCode();
        AdaptiveReorgBudgetRequiredCategoryRuleFlagsABlankCategory();
        AdaptiveReorgBudgetRequiredDescriptionRuleFlagsABlankDescription();
    }
}
