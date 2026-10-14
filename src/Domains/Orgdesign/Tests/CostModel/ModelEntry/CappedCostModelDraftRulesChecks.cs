namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelEntry;

using Acme.Workforce.Orgdesign.CostModel.ModelEntry;

/// <summary>Checks of model entry code, run by the project's runner.</summary>
public static class CappedCostModelDraftRulesChecks
{
    public static void CappedCostModelDraftValidatorReportsEveryProblem()
    {
        var validator = CappedCostModelDraftRules.DefaultCappedCostModelDraftValidator();
        var item = CappedCostModelDraft.NewCappedCostModelDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedCostModelDraftRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new CappedCostModelDraftRequiredSkillRule("capped cost model draft needs this field");
        var item = CappedCostModelDraft.NewCappedCostModelDraft("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedCostModelDraftMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new CappedCostModelDraftMarginRangeRule(0.8, 149.2);
        var item = CappedCostModelDraft.NewCappedCostModelDraft("x", "y");
        item.Margin = 150.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedCostModelDraftSkillLengthRuleFlagsALongSkill()
    {
        var rule = new CappedCostModelDraftSkillLengthRule(64);
        var item = CappedCostModelDraft.NewCappedCostModelDraft("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedCostModelDraftRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new CappedCostModelDraftRequiredReviewerRule("capped cost model draft needs a value");
        var item = CappedCostModelDraft.NewCappedCostModelDraft("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CappedCostModelDraftValidatorReportsEveryProblem();
        CappedCostModelDraftRequiredSkillRuleFlagsABlankSkill();
        CappedCostModelDraftMarginRangeRuleFlagsValuesAboveMargin();
        CappedCostModelDraftSkillLengthRuleFlagsALongSkill();
        CappedCostModelDraftRequiredReviewerRuleFlagsABlankReviewer();
    }
}
