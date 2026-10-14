namespace Acme.Workforce.Orgdesign.Tests.Scenario.Rule;

using Acme.Workforce.Orgdesign.Scenario.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class ScenarioRuleRulesChecks
{
    public static void ScenarioRuleValidatorReportsEveryProblem()
    {
        var validator = ScenarioRuleRules.DefaultScenarioRuleValidator();
        var item = ScenarioRule.NewScenarioRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScenarioRuleEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new ScenarioRuleEndsAtRangeRule(80, 3600);
        var item = ScenarioRule.NewScenarioRule("x", "y");
        item.EndsAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScenarioRuleRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new ScenarioRuleRequiredCategoryRule("scenario rule needs this field");
        var item = ScenarioRule.NewScenarioRule("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScenarioRuleRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new ScenarioRuleRequiredReviewerRule("scenario rule needs this field");
        var item = ScenarioRule.NewScenarioRule("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScenarioRuleReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new ScenarioRuleReviewerLengthRule(64);
        var item = ScenarioRule.NewScenarioRule("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScenarioRuleValidatorReportsEveryProblem();
        ScenarioRuleEndsAtRangeRuleFlagsValuesAboveEndsAt();
        ScenarioRuleRequiredCategoryRuleFlagsABlankCategory();
        ScenarioRuleRequiredReviewerRuleFlagsABlankReviewer();
        ScenarioRuleReviewerLengthRuleFlagsALongReviewer();
    }
}
